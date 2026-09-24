/**
 * Replaces a static diagram with its animated site component. The Markdown keeps the SVG, so GitHub and Unity
 * still show the picture; only the site renders the live version. The caption paragraph that repeats the alt
 * text stays in the document and keeps its caption style.
 */
const COMPONENTS = {
  'profiler-markers-hierarchy.svg': 'ProfilerHierarchy',
};

export default function remarkLiveDiagrams() {
  return (tree) => {
    tree.children.forEach((node, index, siblings) => {
      const image = node.type === 'paragraph' && node.children?.length === 1 && node.children[0];
      const name = image?.type === 'image' && COMPONENTS[image.url.split('/').pop()];
      if (!name) return;
      siblings[index] = {
        type: 'mdxJsxFlowElement',
        name,
        attributes: [{type: 'mdxJsxAttribute', name: 'alt', value: image.alt || ''}],
        children: [],
      };
      const caption = siblings[index + 1];
      if (caption?.type === 'paragraph' && caption.children?.every((part) => part.type === 'text')
        && caption.children.map((part) => part.value).join('') === image.alt) {
        caption.data = {...caption.data, hProperties: {...caption.data?.hProperties, className: 'doc-media-caption'}};
      }
    });
  };
}
