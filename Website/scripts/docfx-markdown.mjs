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
