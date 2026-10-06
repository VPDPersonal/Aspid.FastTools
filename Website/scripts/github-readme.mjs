/**
 * GitHub layout for the generated READMEs. GitHub renders the introduction without the site's components, so its
 * site-only blocks get the closest GitHub equivalent in plain HTML (src/remark/introBanner.js builds the site's):
 * - an image with a `-light` sibling becomes a <picture> per GitHub theme;
 * - every feature becomes a frameless card: the preview floats on the left, the title and summary sit on the right
 *   (a table cell would pad the preview); a code preview gives way to the recording of the site's animated preview
 *   (`docs/images/readme-previews`);
 * - a section that is only a list of `[Link](…) — summary` items becomes a row of link tiles;
 * - a line under the link row links the other translation.
 * All URLs are already relative to the README.
 */

const TEXT = {
  en: {name: 'English', more: 'Read more'},
  ru: {name: 'Русский', more: 'Подробнее'},
};

const escape = (text) => text.replace(/&/g, '&amp;').replace(/</g, '&lt;').replace(/>/g, '&gt;').replace(/"/g, '&quot;');
const html = (value) => ({type: 'html', value});
const attributes = (tag) => Object.fromEntries([...tag.matchAll(/([\w-]+)=(["'])(.*?)\2/g)].map(([, name, , value]) => [name, value]));

/** Inline Markdown as HTML: inside an HTML block GitHub does not parse Markdown. */
function inline(nodes) {
  return nodes.map((node) => {
    switch (node.type) {
      case 'text': return escape(node.value);
      case 'html': return node.value;
      case 'inlineCode': return `<code>${escape(node.value)}</code>`;
      case 'strong': return `<b>${inline(node.children)}</b>`;
      case 'emphasis': return `<i>${inline(node.children)}</i>`;
      case 'link': return `<a href="${escape(node.url)}">${inline(node.children)}</a>`;
      case 'break': return '<br>';
      default: throw new Error(`github-readme: no HTML for inline ${node.type} nodes`);
    }
  }).join('');
}

/** `alt` is HTML already. The light sibling follows the site's naming: `x.gif` and `x-light.gif`. */
function themedImage(src, alt, exists, size = '') {
  const image = `<img src="${src}" alt="${alt}"${size}>`;
  const light = src.replace(/(\.\w+)$/, '-light$1');
  if (!exists(light)) return image;
  return `<picture><source media="(prefers-color-scheme: dark)" srcset="${src}">`
    + `<source media="(prefers-color-scheme: light)" srcset="${light}">${image}</picture>`;
}

/** The recording of the site's animated preview for `doc`: the translated one first, then the English one. */
function recording(doc, {language, previews, exists}) {
  const files = language === 'en' ? [`${doc}.webp`] : [`${doc}-${language}.webp`, `${doc}.webp`];
  return files.map((file) => `${previews}/${file}`).find(exists);
}

function card(heading, summary, preview, options) {
  const link = heading.children.find((part) => part.type === 'link');
  const url = link ? escape(link.url) : undefined;
  const title = inline(heading.children);
  const body = `<h4>${title}</h4>\n<p>${inline(summary.children)}</p>\n`
    + (url ? `<p><a href="${url}">${TEXT[options.language].more} →</a></p>\n` : '')
    + '<br clear="all">\n</div>';
  // The preview takes 56% of the width, as on the site; GitHub pads a left-aligned image on its right.
  const size = ' width="56%" align="left"';
  let image;
  if (preview.type === 'html') {
    const {src, alt = ''} = attributes(preview.value);
    image = themedImage(src, alt, options.exists, size);
  } else {
    // `09-profiler-markers.md` → `profiler-markers`, the key of the site's FeaturePreview.
    const doc = link?.url.replace(/^.*\//, '').replace(/^\d+-/, '').replace(/\.md$/, '');
    const file = doc && recording(doc, options);
    if (file) image = themedImage(file, escape(link.children.map((part) => part.value ?? '').join('')), options.exists, size);
  }
  // The preview is not a link: GitHub keeps a <picture> working only outside one, as the banner is.
  // Without a recording the code stays a fenced block above the text; a <div> block would not parse it.
  return image ? [html(`<div>\n${image}\n${body}`)] : [preview, html(`<div>\n${body}`)];
}

/** On GitHub the features are grouped sections: a linked heading, a sentence and a preview. */
function featureCards(tree, options) {
  const start = tree.children.findIndex((node, index) => node.type === 'heading' && node.depth === 2
    && tree.children[index + 1]?.type === 'heading' && tree.children[index + 1].depth === 3
    && tree.children[index + 2]?.type === 'heading' && tree.children[index + 2].depth === 4);
  if (start === -1) return;
  let end = start + 1;
  while (end < tree.children.length && !(tree.children[end].type === 'heading' && tree.children[end].depth === 2)) end++;
  const section = tree.children.slice(start + 1, end);
  const result = [];
  for (let i = 0; i < section.length; i++) {
    const [node, summary, preview] = section.slice(i, i + 3);
    const isImage = preview?.type === 'html' && /^<img\b/.test(preview.value.trim());
    if (node.type === 'heading' && node.depth === 4 && summary?.type === 'paragraph' && (isImage || preview?.type === 'code')) {
      result.push(...card(node, summary, preview, options));
      i += 2;
    } else {
      result.push(node);
    }
  }
  tree.children.splice(start + 1, end - start - 1, ...result);
}

/** A section that is only a list of `[Link](…) — summary` items, as on the site. */
function linkTiles(tree) {
  tree.children.forEach((node, index) => {
    const list = tree.children[index + 1];
    if (node.type !== 'heading' || node.depth !== 2 || list?.type !== 'list' || list.ordered) return;
    const items = list.children.map((item) => (item.children.length === 1 && item.children[0].type === 'paragraph'
      ? item.children[0].children : null));
    if (!items.every((parts) => parts?.[0]?.type === 'link' && parts[1]?.type === 'text' && /^\s*—\s*/.test(parts[1].value))) return;
    const width = Math.floor(100 / items.length);
    const cells = items.map(([link, first, ...rest]) => {
      // The text after the dash starts lower-case; on its own line in a tile it starts a sentence.
      const lead = first.value.replace(/^\s*—\s*/, '');
      const summary = inline([{...first, value: lead.charAt(0).toUpperCase() + lead.slice(1)}, ...rest]);
      return `<td width="${width}%" valign="top"><b>${inline([link])}</b><br>${summary}</td>`;
    });
    tree.children[index + 1] = html(`<table>\n<tr>\n${cells.join('\n')}\n</tr>\n</table>`);
  });
}

/** Every other image with a light sibling: the README's `<img>` tags and Markdown images. */
function themedImages(node, exists) {
  node.children?.forEach((child, index) => {
    if (child.type === 'html' && !child.value.includes('<picture')) {
      child.value = child.value.replace(/<img\b[^>]*>/g, (tag) => {
        const {src, alt = ''} = attributes(tag);
        const light = src?.replace(/(\.\w+)$/, '-light$1');
        if (!light || !exists(light)) return tag;
        return `<picture><source media="(prefers-color-scheme: dark)" srcset="${src}">`
          + `<source media="(prefers-color-scheme: light)" srcset="${light}">${tag}</picture>`;
      });
    } else if (child.type === 'image' && exists(child.url.replace(/(\.\w+)$/, '-light$1'))) {
      node.children[index] = html(themedImage(escape(child.url), escape(child.alt ?? ''), exists));
    } else {
      themedImages(child, exists);
    }
  });
}

/**
 * @param tree the README's Markdown tree, its URLs relative to the README
 * @param options.language `en` or `ru`
 * @param options.translations README file per language, relative to this README
 * @param options.previews the folder of the preview recordings, relative to this README
 * @param options.exists whether a URL relative to the README names a file
 */
export function githubLayout(tree, options) {
  featureCards(tree, options);
  linkTiles(tree);
  themedImages(tree, options.exists);
  const switcher = Object.entries(options.translations).map(([language, file]) => (language === options.language
    ? `<b>${TEXT[language].name}</b>` : `<a href="${file}">${TEXT[language].name}</a>`));
  // The link row: text links and separators only (the badge row links images). Without one, the switch goes on top.
  const links = tree.children.findIndex((node) => node.type === 'paragraph'
    && node.children.some((part) => part.type === 'link')
    && node.children.every((part) => (part.type === 'link' && part.children.every((child) => child.type === 'text'))
      || (part.type === 'text' && /^[\s·|\-–—]*$/.test(part.value))));
  tree.children.splice(links + 1, 0, html(`<p>${switcher.join(' · ')}</p>`));
}
