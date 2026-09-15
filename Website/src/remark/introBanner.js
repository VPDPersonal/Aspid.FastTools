// Portable <pre><code> cells remain readable on GitHub. On the site, use the
// same code renderer as fenced blocks, including highlighting and copying.
function enhanceTableCode(node) {
  if (node.type === 'mdxJsxTextElement' && node.name === 'pre') {
    const language = node.attributes.find((attribute) => attribute.name === 'lang')?.value;
    const read = (part) => {
      if (part.type === 'text') return part.value;
      if (part.type !== 'mdxJsxTextElement') return null;
      if (part.name === 'br') return '\n';
      if (part.name !== 'pre' && part.name !== 'code') return null;
      const children = part.children.map(read);
      return children.includes(null) ? null : children.join('');
    };
    const value = read(node);
    if (typeof language === 'string' && value !== null) {
      delete node.name;
      delete node.attributes;
      delete node.children;
      Object.assign(node, {type: 'code', lang: language, value});
    }
  }
  node.children?.forEach(enhanceTableCode);
}

/** Keep the package README unchanged while placing the site's TOC after its banner. */
export function remarkStatusBadges() {
  return (tree) => {
    function visit(node) {
      if (node.type === 'mdxJsxTextElement' && node.name === 'img'
        && node.attributes.some((attribute) => attribute.name === 'src'
          && attribute.value?.value?.includes('status-badge-'))) {
        node.attributes.push({type: 'mdxJsxAttribute', name: 'className', value: 'readme-status-badge'});
      }
      node.children?.forEach(visit);
    }
    visit(tree);
  };
}

export default function remarkIntroBanner({baseUrl, siteUrl}) {
  return (tree) => {
    // Two-column tables made entirely of code are before/after comparisons.
    // Keep portable HTML in package Markdown and use native code blocks on the site.
    for (const table of tree.children.filter((node) => node.type === 'table')) {
      const [header, ...rows] = table.children;
      if (header.children.length !== 2 || rows.length === 0
        || !header.children.every((cell) => cell.children.every((part) => part.type === 'text'))
        || !rows.every((row) => row.children.length === 2 && row.children.every((cell) =>
          cell.children.length === 1 && cell.children[0].type === 'mdxJsxTextElement'
          && cell.children[0].name === 'pre'))) continue;
      enhanceTableCode(table);
      if (!rows.every((row) => row.children.every((cell) => cell.children[0].type === 'code'))) continue;
      table.data = {...table.data, hProperties: {...table.data?.hProperties, className: 'doc-code-comparison'}};
      const labels = header.children.map((cell) => cell.children.map((part) => part.value).join(''));
      rows.forEach((row) => row.children.forEach((cell, index) => {
        cell.data = {...cell.data, hProperties: {...cell.data?.hProperties, 'data-label': labels[index]}};
      }));
    }
    // Translated copies can start with generated front matter.
    const banner = tree.children.find((node) => node.type === 'mdxJsxFlowElement' && node.name === 'img');
    if (banner?.type === 'mdxJsxFlowElement' && banner.name === 'img'
      && banner.attributes.some((attribute) => attribute.name === 'src'
        && typeof attribute.value === 'string'
        && attribute.value.endsWith('/aspid_fasttools_readme_banner.gif'))) {
      banner.name = 'IntroBanner';
      // The paragraph after the badges is the project lede; the next one is the link row.
      const badges = tree.children.findIndex((node) => node.type === 'paragraph'
        && node.children.some((part) => part.type === 'link'
          && part.children.some((child) => child.type === 'image' && child.url.includes('status-badge-'))));
      const addClass = (node, className) => {
        if (node?.type !== 'paragraph') return;
        node.data = {...node.data, hProperties: {...node.data?.hProperties, className}};
      };
      if (badges !== -1) {
        addClass(tree.children[badges + 1], 'readme-lede');
        const links = tree.children[badges + 2];
        addClass(links, 'readme-links');
        // On the site the "Documentation" link points at this very page: drop it and its separator.
        const self = new RegExp(`^${siteUrl}${baseUrl}(?:[a-z-]+/)?docs/?$`);
        if (links?.type === 'paragraph' && links.children[0]?.type === 'link' && self.test(links.children[0].url)) {
          links.children.splice(0, 1);
          if (links.children[0]?.type === 'text') {
            links.children[0].value = links.children[0].value.replace(/^\s*[·|\-–—]\s*/, '');
          }
        }
      }
      // On GitHub the features are plain sections: a linked heading, a sentence and a preview.
      // The site shows the same sections as a card grid, like the samples overview.
      const features = tree.children.findIndex((node) => node.type === 'heading' && node.depth === 2);
      if (features !== -1) {
        let end = features + 1;
        while (end < tree.children.length && !(tree.children[end].type === 'heading' && tree.children[end].depth === 2)) end++;
        const section = tree.children.slice(features + 1, end);
        const cards = [];
        for (let i = 0; i < section.length; i++) {
          const heading = section[i];
          if (heading.type !== 'heading' || heading.depth !== 3) continue;
          const summary = section[i + 1];
          const preview = section[i + 2];
          if (summary?.type !== 'paragraph' || !preview
            || !(preview.type === 'code' || (preview.type === 'paragraph' && preview.children[0]?.type === 'image'))) continue;
          const jsx = (name, className, children) => ({
            type: 'mdxJsxFlowElement',
            name,
            attributes: [{type: 'mdxJsxAttribute', name: 'className', value: className}],
            children,
          });
          cards.push(jsx('article', 'feature-card', [
            jsx('div', 'feature-card__preview', [preview]),
            jsx('div', 'feature-card__body', [
              {type: 'paragraph', data: {hProperties: {className: 'feature-card__title'}}, children: [
                {type: 'mdxJsxTextElement', name: 'span', attributes: [{type: 'mdxJsxAttribute', name: 'className', value: 'feature-card__index'}],
                  children: [{type: 'text', value: `${String(cards.length + 1).padStart(2, '0')} /`}]},
                {type: 'text', value: ' '},
                ...heading.children,
              ]},
              {type: 'paragraph', data: {hProperties: {className: 'feature-card__text'}}, children: summary.children},
            ]),
          ]));
          i += 2;
        }
        if (cards.length > 0) {
          tree.children.splice(features + 1, end - features - 1, {
            type: 'mdxJsxFlowElement',
            name: 'div',
            attributes: [{type: 'mdxJsxAttribute', name: 'className', value: 'feature-cards'}],
            children: cards,
          });
        }
      }
      // Keep portable file links on GitHub, and native local links on the site.
      // pathname:// follows Docusaurus locale links across independently built locales.
      function visit(node) {
        if (node.type === 'link') {
          const href = node.url === 'ru/README.md' ? `pathname://${baseUrl}ru/docs`
            : node.url === '../README.md' ? `pathname://${baseUrl}docs`
            : node.url.startsWith(`${siteUrl}${baseUrl}`) ? node.url.slice(siteUrl.length)
            : undefined;
          if (href) {
            node.type = 'mdxJsxTextElement';
            node.name = 'ReadmeLink';
            node.attributes = [{type: 'mdxJsxAttribute', name: 'href', value: href}];
            delete node.url;
            delete node.title;
          }
        }
        node.children?.forEach(visit);
      }
      visit(tree);
    }
  };
}
