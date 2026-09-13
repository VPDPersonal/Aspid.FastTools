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
    // Translated copies can start with generated front matter.
    const banner = tree.children.find((node) => node.type === 'mdxJsxFlowElement' && node.name === 'img');
    if (banner?.type === 'mdxJsxFlowElement' && banner.name === 'img'
      && banner.attributes.some((attribute) => attribute.name === 'src'
        && typeof attribute.value === 'string'
        && attribute.value.endsWith('/aspid_fasttools_readme_banner.gif'))) {
      banner.name = 'IntroBanner';
      const features = tree.children.find((node) => node.type === 'table');
      if (features) {
        features.data = {...features.data, hProperties: {...features.data?.hProperties, className: 'readme-feature-table'}};
        enhanceTableCode(features);
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
