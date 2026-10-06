/**
 * GitHub layout for the generated README. GitHub renders the introduction without the site's components, so its
 * site-only blocks get the closest GitHub equivalent in plain HTML (src/remark/introBanner.js builds the site's):
 * - links to doc pages and to the samples overview open the site, which renders them in full;
 * - the link row's link to the site lists the site's languages: `Documentation: EN, RU`;
 * - the install instruction becomes a card like the site's install panel (src/components/InstallPanel): the recording
 *   of its Package Manager walk-through beside its numbered steps, the URL to copy below;
 * - the note under the install URL links the site's install panel for versions and channels;
 * - every feature becomes a card: a one-row table with the preview on the left, the title and summary on the right;
 *   a code preview gives way to the recording of the site's animated preview (`docs/images/readme-previews`);
 * - a section that is only a list of `[Link](…) — summary` items becomes a row of link tiles;
 * - the help section becomes the site's call to action (src/components/SupportPanel), without the licence line;
 * - an image with a `-light` sibling becomes a <picture> per GitHub theme.
 * All file URLs are already relative to the README.
 */

// The site's English strings: FeatureCardMore in src/theme/MDXComponents, InstallPanel and SupportPanel.
const TEXT = {
  more: 'Read more',
  steps: [
    'Open <b>Window → Package Manager</b>',
    'Choose <b>+ → Install package from git URL…</b>',
    'Paste the URL and press <b>Install</b>',
  ],
  walkthrough: 'Package Manager installs Aspid.FastTools from its git URL',
  install: ['To pin a version or switch channels, see ', 'Installation', ' in the documentation.'],
  support: 'Found a bug or have a question?',
  hint: 'Include your Unity version, package version and steps to reproduce.',
  issue: 'Open an issue',
  star: 'Star on GitHub',
};

const escape = (text) => text.replace(/&/g, '&amp;').replace(/</g, '&lt;').replace(/>/g, '&gt;').replace(/"/g, '&quot;');
const html = (value) => ({type: 'html', value});
const attributes = (tag) => Object.fromEntries([...tag.matchAll(/([\w-]+)=(["'])(.*?)\2/g)].map(([, name, , value]) => [name, value]));
const isHeading = (node, depth) => node?.type === 'heading' && node.depth === depth;

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

/** `Website/docs/09-profiler-markers.md#x` → `<site>docs/profiler-markers#x`; the samples overview → `<site>tutorials`. */
function siteLinks(node, site) {
  if (node.type === 'link' || node.type === 'definition') {
    const doc = /^Website\/docs\/(?:\d+-)?([^/#]+)\.md(#.*)?$/.exec(node.url);
    if (doc && doc[1] !== 'README') node.url = `${site}docs/${doc[1]}${doc[2] ?? ''}`;
    else if (/\/Samples~\/README\.md$/.test(node.url)) node.url = `${site}tutorials`;
  }
  node.children?.forEach((child) => siteLinks(child, site));
}

/**
 * The link row: text links and separators only (the badge row links images). A link to the site becomes its label and
 * one link per site language, the first one the default: `Documentation: EN, RU`.
 */
function linkRow(tree, site, locales) {
  const row = tree.children.find((node) => node.type === 'paragraph'
    && node.children.some((part) => part.type === 'link')
    && node.children.every((part) => (part.type === 'link' && part.children.every((child) => child.type === 'text'))
      || (part.type === 'text' && /^[\s·|\-–—]*$/.test(part.value))));
  if (!row) return;
  row.children = row.children.flatMap((part) => {
    if (part.type !== 'link' || !part.url.startsWith(site)) return [part];
    const page = part.url.slice(site.length);
    const links = locales.map((locale, at) => ({type: 'link', url: `${site}${at ? `${locale}/` : ''}${page}`,
      children: [{type: 'text', value: locale.toUpperCase()}]}));
    return [{type: 'text', value: `${part.children.map((child) => child.value).join('')}: `},
      ...links.flatMap((link, at) => (at ? [{type: 'text', value: ', '}, link] : [link]))];
  });
}

/** The install section: an instruction, the URL block and a note. The instruction gives way to the install card. */
function installCard(tree, {previews, exists}) {
  const index = tree.children.findIndex((node, at) => isHeading(node, 2) && tree.children[at + 1]?.type === 'paragraph'
    && tree.children[at + 2]?.type === 'code' && /\.git#upm/.test(tree.children[at + 2].value));
  const file = `${previews}/install.webp`;
  if (index === -1 || !exists(file)) return;
  const steps = TEXT.steps.map((step) => `<li>${step}</li>`).join('\n');
  tree.children[index + 1] = html(`<table>\n<tr>\n<td width="56%">${themedImage(file, TEXT.walkthrough, exists, ' width="100%"')}</td>\n`
    + `<td width="44%">\n<ol>\n${steps}\n</ol>\n</td>\n</tr>\n</table>`);
}

/** The note under the install URL points to the site's install panel, which pins versions and switches channels. */
function installLink(tree, site) {
  const index = tree.children.findIndex((node, at) => node.type === 'code' && /\.git#upm/.test(node.value)
    && tree.children[at + 1]?.type === 'paragraph');
  if (index === -1) return;
  tree.children[index + 1].children.push({type: 'text', value: ` ${TEXT.install[0]}`},
    {type: 'link', url: `${site}docs#installation`, children: [{type: 'text', value: TEXT.install[1]}]},
    {type: 'text', value: TEXT.install[2]});
}

/** The recording of the site's animated preview for `doc`, if there is one. */
function recording(doc, {previews, exists}) {
  const file = `${previews}/${doc}.webp`;
  return exists(file) ? file : undefined;
}

function card(heading, summary, preview, options) {
  const link = heading.children.find((part) => part.type === 'link');
  const url = link ? escape(link.url) : undefined;
  const body = `</td>\n<td width="44%">\n<h4>${inline(heading.children)}</h4>\n<p>${inline(summary.children)}</p>\n`
    + (url ? `<p><a href="${url}">${TEXT.more} →</a></p>\n` : '')
    + '</td>\n</tr>\n</table>';
  const size = ' width="100%"';
  let image;
  if (preview.type === 'html') {
    const {src, alt = ''} = attributes(preview.value);
    image = themedImage(src, alt, options.exists, size);
  } else {
    // `…/docs/profiler-markers` → `profiler-markers`, the key of the site's FeaturePreview.
    const doc = link?.url.replace(/[#?].*$/, '').replace(/^.*\//, '').replace(/^\d+-/, '').replace(/\.md$/, '');
    const file = doc && recording(doc, options);
    if (file) image = themedImage(file, escape(link.children.map((part) => part.value ?? '').join('')), options.exists, size);
  }
  // The preview is not a link: GitHub keeps a <picture> working only outside one, as the banner is.
  const open = '<table>\n<tr>\n<td width="56%">';
  // Without a recording the code stays a fenced block; the blank lines around it let GitHub parse it inside the cell.
  return image ? [html(`${open}${image}${body}`)] : [html(open), preview, html(body)];
}

/** On GitHub the features are grouped sections: a linked heading, a sentence and a preview. */
function featureCards(tree, options) {
  const start = tree.children.findIndex((node, index) => isHeading(node, 2)
    && isHeading(tree.children[index + 1], 3) && isHeading(tree.children[index + 2], 4));
  if (start === -1) return;
  let end = start + 1;
  while (end < tree.children.length && !isHeading(tree.children[end], 2)) end++;
  const section = tree.children.slice(start + 1, end);
  const result = [];
  for (let i = 0; i < section.length; i++) {
    const [node, summary, preview] = section.slice(i, i + 3);
    const isImage = preview?.type === 'html' && /^<img\b/.test(preview.value.trim());
    if (isHeading(node, 4) && summary?.type === 'paragraph' && (isImage || preview?.type === 'code')) {
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
    if (!isHeading(node, 2) || list?.type !== 'list' || list.ordered) return;
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

/**
 * The help section: a paragraph linking the issues, one linking the repository and an optional licence line become
 * the site's call to action. The licence line goes: the badge and GitHub's License tab name it.
 */
function supportPanel(tree, repository) {
  const linkIn = (node, test) => node?.type === 'paragraph' && node.children.find((part) => part.type === 'link' && test(part.url));
  const isIssues = (url) => /\/issues\/?$/.test(url);
  const index = tree.children.findIndex((node, at) => isHeading(node, 2) && linkIn(tree.children[at + 1], isIssues)
    && linkIn(tree.children[at + 2], (url) => url.replace(/\/$/, '') === repository));
  if (index === -1) return;
  const issues = escape(linkIn(tree.children[index + 1], isIssues).url);
  const licence = linkIn(tree.children[index + 3], (url) => /\/LICENSE$/.test(url));
  tree.children.splice(index + 1, licence ? 3 : 2,
    html(`<p>${TEXT.support} ${TEXT.hint}</p>\n<p><a href="${issues}"><b>${TEXT.issue}</b></a> · <a href="${repository}">${TEXT.star}</a></p>`));
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
 * @param tree the README's Markdown tree, its file URLs relative to the README
 * @param options.site the documentation site, ending in `/`
 * @param options.locales the site's languages, the default one first
 * @param options.repository the repository's GitHub URL, without a trailing `/`
 * @param options.previews the folder of the preview recordings, relative to the README
 * @param options.exists whether a URL relative to the README names a file
 */
export function githubLayout(tree, options) {
  siteLinks(tree, options.site);
  linkRow(tree, options.site, options.locales);
  installCard(tree, options);
  installLink(tree, options.site);
  featureCards(tree, options);
  linkTiles(tree);
  supportPanel(tree, options.repository);
  themedImages(tree, options.exists);
}
