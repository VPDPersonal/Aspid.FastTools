/**
 * The stateless transforms of `docfx-postprocess.mjs`, split out so that `docfx-markdown.test.mjs` can run them on small
 * inputs: DocFX identifiers, Markdown fixes for MDX and the conversion of `toc.yml` into the sidebar.
 */

/** `Aspid.FastTools.Types.SerializableType%601` → `Aspid.FastTools.Types.SerializableType-1` (the file name DocFX uses for generic types). */
export const uidToFile = (uid) => uid.replace(/%60(\d+)/g, '-$1');
export const uidToAnchor = (uid) => uid.replace(/%60/g, '_').replace(/[^A-Za-z0-9]/g, '_');

/** `Aspid.FastTools.Types.SerializableType%602` → `IBinder<T1, T2>`, `Aspid.FastTools.Types.TypeAllow.All` → `BindMode.TwoWay`. */
export function shortName(uid, keepParent) {
  const withoutParams = uid.replace(/\(.*$/, '');
  const parts = withoutParams.split('.');
  const name = keepParent ? parts.slice(-2).join('.') : parts[parts.length - 1];
  // A type's arity is `%60N`, a method's `%60%60N`.
  return name.replace(/(?:%60){1,2}(\d+)/g, (_, n) => {
    const count = Number(n);
    return count === 1 ? '<T>' : `<${Array.from({ length: count }, (_, i) => `T${i + 1}`).join(', ')}>`;
  });
}

/** `<pre><code class="lang-csharp">…</code></pre>` from `<example>` XML tags → fenced code, which MDX leaves alone and Prism highlights. */
export function preToFence(markdown) {
  const decode = (html) => html.replace(/&lt;/g, '<').replace(/&gt;/g, '>').replace(/&quot;/g, '"').replace(/&#39;/g, "'").replace(/&amp;/g, '&');
  return markdown.replace(/<pre><code(?: class="lang-(\w+)")?>([\s\S]*?)<\/code><\/pre>/g, (_, lang, code) => `\n\`\`\`${lang ?? ''}\n${decode(code).trimEnd()}\n\`\`\`\n`);
}

/** DocFX backslash-escapes `#`, `_` and `-` inside link destinations; Docusaurus resolves them literally. */
export function unescapeLinkTargets(markdown) {
  return markdown.replace(/\]\(([^)\s]+)\)/g, (_, target) => `](${target.replace(/\\([#_\-])/g, '$1')})`);
}

/** An "Extension Methods" header left without entries is removed. */
export function dropObjectExtensions(markdown) {
  return markdown.replace(/\n#### Extension Methods\n\s*(?=\n#{1,4} |$)/g, '\n');
}

/** Removes a `#### Heading` block up to the next heading. */
export function dropSection(markdown, heading) {
  return markdown.replace(new RegExp(`\\n#### ${heading}\\n[\\s\\S]*?(?=\\n#{1,4} |$)`, 'g'), '\n');
}

/**
 * Escapes what MDX would read as JSX outside code: `{`/`}` expressions and `<T`-style generic brackets
 * (DocFX escapes only the closing `>`). Fenced and inline code are left alone, and so is the real HTML DocFX
 * emits from XML doc tags (`<p>`, `<a id>`, `<code class="paramref">`, lists).
 */
const HTML_TAGS = 'a|p|pre|code|br|ul|ol|li|em|strong|b|i|table|thead|tbody|tr|td|th|blockquote|span|div|h[1-6]';
const JSX_BRACKET = new RegExp(`(?<!\\\\)<(?!/|(?:${HTML_TAGS})[\\s>/])(?=[A-Za-z\\[])`, 'g');

export function escapeMdx(markdown) {
  const parts = markdown.split(/(```[\s\S]*?```|`[^`\n]*`)/);
  return parts
    .map((part, i) => (i % 2 ? part : part.replace(/(?<!\\)([{}])/g, '\\$1').replace(JSX_BRACKET, '\\<')))
    .join('');
}

/** `### <a id="X"></a> Title` → `### Title {#X}`: Docusaurus only knows heading ids, not inline anchors. */
export function headingAnchors(markdown) {
  return markdown.replace(/^(#+) <a id="([^"]+)"><\/a> (.+)$/gm, '$1 $3 {#$2}');
}

export function titleOf(markdown, file) {
  const h1 = markdown.match(/^# (.+?)(?: \{#[^}]+\})?$/m)?.[1] ?? file;
  return h1.replace(/\\([<>()])/g, '$1');
}

export function parseToc(yaml) {
  const root = { items: [] };
  const stack = [{ indent: -1, node: root }];
  for (const line of yaml.split('\n')) {
    const match = line.match(/^(\s*)- name: (.*)$/);
    const hrefMatch = line.match(/^\s*href: (.*)$/);
    if (match) {
      const indent = match[1].length;
      const node = { name: match[2].trim(), href: null, items: [] };
      while (stack[stack.length - 1].indent >= indent) stack.pop();
      stack[stack.length - 1].node.items.push(node);
      stack.push({ indent, node });
    } else if (hrefMatch) {
      stack[stack.length - 1].node.href = hrefMatch[1].trim();
    }
  }
  return root.items;
}

export const docId = (href) => href.replace(/\.md$/, '');

/** Namespace → category linked to the namespace page; "Classes"/"Interfaces"/… headers → collapsed sub-categories. */
export function namespaceCategory(node) {
  const groups = [];
  let current = null;
  for (const item of node.items) {
    if (!item.href) {
      // `key` keeps the translation key unique: every namespace has its own "Classes", "Enums", …
      current = { type: 'category', key: `${node.name}.${item.name}`, label: item.name, collapsed: true, items: [] };
      groups.push(current);
    } else if (current) {
      current.items.push({ type: 'doc', id: docId(item.href), label: item.name });
    } else {
      groups.push({ type: 'doc', id: docId(item.href), label: item.name });
    }
  }
  return {
    type: 'category',
    key: node.name,
    label: node.name,
    collapsed: true,
    link: { type: 'doc', id: docId(node.href) },
    items: groups.filter((g) => g.type === 'doc' || g.items.length > 0),
  };
}

/**
 * Docusaurus derives one translation key per doc label in a sidebar, so two namespaces holding a type of the same
 * name (`VisualElementExtensions` in `Aspid.FastTools.UIElements` and `Aspid.FastTools.UIElements.Editors`) break
 * the i18n build. Give the duplicates a namespace suffix.
 */
export function disambiguateLabels(sidebar) {
  const docItems = [];
  const collect = (items, namespace) => {
    for (const item of items) {
      if (item.type === 'doc') docItems.push({ item, namespace });
      else if (item.items) collect(item.items, namespace); // "Classes"/"Enums" groups stay inside their namespace
    }
  };
  for (const category of sidebar) collect(category.items, category.label);
  const counts = new Map();
  for (const { item } of docItems) counts.set(item.label, (counts.get(item.label) ?? 0) + 1);
  for (const { item, namespace } of docItems) {
    if (counts.get(item.label) > 1) item.label = `${item.label} (${namespace.split('.').slice(-1)[0]})`;
  }
}

/**
 * `<p>`, the text and `</p>` spread over several lines → one line. MDX reads the lines between the tags as a Markdown paragraph
 * and wraps it in its own `<p>`, so the page holds a `<p>` inside a `<p>`: the browser closes the outer one while
 * parsing and React reports a hydration mismatch (error #418). On one line MDX leaves the element alone. A block with
 * a code fence stays as it is: joining the lines would break the fence.
 */
export function joinParagraphs(markdown) {
  return markdown.replace(/^<p>\n([\s\S]*?)\n<\/p>$/gm, (block, text) =>
    text.includes('```') ? block : `<p>${text.replace(/\s*\n\s*/g, ' ')}</p>`);
}

/** Splits `A, B<C, D>, E` at the commas outside angle brackets. */
function splitConstraints(clause) {
  const items = [];
  let depth = 0;
  let start = 0;
  for (let i = 0; i < clause.length; i++) {
    if (clause[i] === '<') depth++;
    else if (clause[i] === '>') depth--;
    else if (clause[i] === ',' && depth === 0) {
      items.push(clause.slice(start, i).trim());
      start = i + 1;
    }
  }
  items.push(clause.slice(start).trim());
  return items;
}

/**
 * Finds the extension methods whose receiver constraint DocFX cannot check, from a raw DocFX page.
 *
 * `SetMaxLength<TField, TValue>(this TField element, …) where TField : TextInputBaseField<TValue>` constrains the
 * receiver by a type that mentions another type parameter. DocFX does not evaluate such a constraint and lists the
 * method as an extension of every type. Returns member anchor → the generic types the receiver must derive from
 * (`['TextInputBaseField']`) for every method like that on the page.
 */
export function collectReceiverBases(markdown) {
  const bases = new Map();
  for (const member of markdown.split(/^### /m).slice(1)) {
    const anchor = member.match(/^<a id="([^"]+)"><\/a>/)?.[1];
    const signature = member.match(/```csharp\n([^\n]+)\n```/)?.[1];
    const receiver = signature?.match(/\(this (\w+) /)?.[1];
    const typeParameters = signature?.match(/\w+<([\w, ]+)>\(this /)?.[1].split(/,\s*/);
    if (!anchor || !receiver || !typeParameters) continue;

    const others = typeParameters.filter((name) => name !== receiver);
    const clause = signature.match(new RegExp(`\\swhere ${receiver} : (.+?)(?= where \\w+ :|$)`))?.[1];
    const required = splitConstraints(clause ?? '')
      .map((constraint) => constraint.match(/^([\w.]+)<(.+)>$/))
      .filter((match) => match && others.some((name) => new RegExp(`\\b${name}\\b`).test(match[2])))
      .map((match) => match[1].split('.').pop());
    if (required.length > 0) bases.set(anchor, required);
  }
  return bases;
}

/** The text of a `#### Heading` block up to the next heading, or an empty string. */
function sectionOf(markdown, heading) {
  return markdown.match(new RegExp(`\\n#### ${heading}\\n([\\s\\S]*?)(?=\\n#{1,4} |$)`))?.[1] ?? '';
}

/**
 * Drops from the "Extension Methods" list of a page the methods whose receiver constraint (see
 * `collectReceiverBases`) names a generic type the page's type neither inherits nor implements:
 * `SliderExtensions.SetHighValue` is no extension of an enum or of a `BaseField<Type>`. A list left empty stays
 * as a bare header for `dropObjectExtensions` to remove. Run it after the link targets are unescaped.
 */
export function dropForeignExtensions(markdown, receiverBases) {
  const ancestors = `${sectionOf(markdown, 'Inheritance')}\n${sectionOf(markdown, 'Implements')}`;
  // `BaseField<Type>` counts, `TextInputBaseField<string>` and the nested `BaseField<Type>.UxmlSerializedData` do not.
  const derivesFrom = (base) => new RegExp(`(?:^|[\\s\\[])${base}\\\\?<.*?\\\\?>(?=[\\s,\\]]|$)`, 'm').test(ancestors);

  return markdown.replace(/(\n#### Extension Methods\n\n)([\s\S]*?)(?=\n#{1,4} |$)/, (_, header, list) => {
    const kept = list
      .split('\n')
      .filter((line) => line.trim())
      .filter((line) => {
        const anchor = line.match(/\]\([^)#]*#([^)]+)\)/)?.[1];
        return (receiverBases.get(anchor) ?? []).every(derivesFrom);
      })
      .map((line) => line.replace(/,\s*$/, ''));
    return kept.length > 0 ? `${header}${kept.join(',\n')}\n` : header;
  });
}
