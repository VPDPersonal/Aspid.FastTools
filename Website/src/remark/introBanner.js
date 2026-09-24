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
    const tables = [];
    (function collect(node) {
      if (node.type === 'table') tables.push(node);
      else node.children?.forEach(collect);
    })(tree);
    for (const table of tables) {
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
        // The link row repeats the navigation the site already has, and points at this very page.
        const links = tree.children[badges + 2];
        if (links?.type === 'paragraph' && links.children.every((part) => part.type === 'link'
          || (part.type === 'text' && /^[\s·|\-–—]*$/.test(part.value)))) {
          tree.children.splice(badges + 2, 1);
        }
      }
      // The install section — the steps, the URL block and the note on versions — becomes one interactive panel.
      const install = tree.children.findIndex((node, index) => node.type === 'heading' && node.depth === 2
        && tree.children[index + 1]?.type === 'paragraph'
        && tree.children[index + 2]?.type === 'code' && /\.git#upm/.test(tree.children[index + 2].value)
        && tree.children[index + 3]?.type === 'paragraph');
      if (install !== -1) {
        tree.children.splice(install + 1, 3, {type: 'mdxJsxFlowElement', name: 'InstallPanel',
          attributes: [{type: 'mdxJsxAttribute', name: 'url', value: tree.children[install + 2].value.trim()}], children: []});
      }
      // On GitHub the features are grouped sections: a linked heading, a sentence and a preview.
      // The site drops the group headings and shows every feature in one card grid, like the samples overview.
      const features = tree.children.findIndex((node, index) => node.type === 'heading' && node.depth === 2
        && tree.children[index + 1]?.type === 'heading' && tree.children[index + 1].depth === 3
        && tree.children[index + 2]?.type === 'heading' && tree.children[index + 2].depth === 4);
      if (features !== -1) {
        let end = features + 1;
        while (end < tree.children.length && !(tree.children[end].type === 'heading' && tree.children[end].depth === 2)) end++;
        const section = tree.children.slice(features + 1, end);
        const jsx = (name, className, children) => ({
          type: 'mdxJsxFlowElement',
          name,
          attributes: [{type: 'mdxJsxAttribute', name: 'className', value: className}],
          children,
        });
        const paragraph = (className, children) => ({type: 'paragraph', data: {hProperties: {className}}, children});
        const result = [];
        let list;
        for (let i = 0; i < section.length; i++) {
          const node = section[i];
          if (node.type === 'heading' && node.depth === 3) {
            if (!list) {
              list = jsx('div', 'feature-cards', []);
              result.push(list);
            }
            continue;
          }
          if (!list || node.type !== 'heading' || node.depth !== 4) continue;
          const summary = section[i + 1];
          let preview = section[i + 2];
          // GitHub reads a sized <img>; the site lays the same capture out as a Markdown image.
          if (preview?.type === 'mdxJsxFlowElement' && preview.name === 'img') {
            const attribute = (name) => preview.attributes.find((item) => item.name === name)?.value;
            preview = {type: 'paragraph', children: [{type: 'image', url: attribute('src'), alt: attribute('alt') ?? ''}]};
          }
          if (summary?.type !== 'paragraph' || !preview
            || !(preview.type === 'code' || (preview.type === 'paragraph' && preview.children[0]?.type === 'image'))) continue;
          // `05-profiler-markers.md` → `profiler-markers`: the site component picks a live preview by page.
          const link = node.children.find((part) => part.type === 'link');
          const url = link?.url ?? '';
          const doc = url.replace(/^.*\//, '').replace(/^\d+-/, '').replace(/\.md$/, '');
          const livePreview = {...jsx('FeaturePreview', undefined, [preview]),
            attributes: [{type: 'mdxJsxAttribute', name: 'doc', value: doc}]};
          list.children.push(jsx('article', 'feature-card', [
            jsx('div', 'feature-card__preview', [livePreview]),
            jsx('div', 'feature-card__body', [
              paragraph('feature-card__title', node.children),
              paragraph('feature-card__text', summary.children),
              ...(link ? [paragraph('feature-card__more', [{...structuredClone(link),
                children: [{type: 'mdxJsxTextElement', name: 'FeatureCardMore', attributes: [], children: []}]}])] : []),
            ]),
          ]));
          i += 2;
        }
        if (result.some((node) => node.children?.length && node.name === 'div')) {
          tree.children.splice(features + 1, end - features - 1, ...result);
        }
      }
      // Keep absolute site links on GitHub, and native local links on the site.
      function visit(node) {
        if (node.type === 'link') {
          const href = node.url.startsWith(`${siteUrl}${baseUrl}`) ? node.url.slice(siteUrl.length) : undefined;
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
