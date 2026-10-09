/**
 * Markdown fixes for the DocFX output that `docfx-postprocess.mjs` applies. They are pure functions of the page text,
 * so `docfx-markdown.test.mjs` checks them without running DocFX.
 */

/**
 * `<p>`, the text and `</p>` on three lines → one line. MDX reads the lines between the tags as a Markdown paragraph
 * and wraps it in its own `<p>`, so the page holds a `<p>` inside a `<p>`: the browser closes the outer one while
 * parsing and React reports a hydration mismatch (error #418). On one line MDX leaves the element alone.
 */
export function joinParagraphs(markdown) {
  return markdown.replace(/^<p>\n([\s\S]*?)\n<\/p>$/gm, (_, text) => `<p>${text.replace(/\s*\n\s*/g, ' ')}</p>`);
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
  const derivesFrom = (base) => new RegExp(`(?:^|[\\s\\[])${base}\\\\?<.*?\\\\?>(?=[\\s,]|$)`, 'm').test(ancestors);

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
