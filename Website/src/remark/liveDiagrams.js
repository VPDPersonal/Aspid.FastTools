/**
 * Replaces a static diagram with its animated site component. The Markdown keeps the SVG, so GitHub and Unity
 * still show the picture; only the site renders the live version. The caption paragraph that repeats the alt
 * text stays in the document and keeps its caption style.
 */
// File name → component, or [component, props] when one component draws several pictures.
const COMPONENTS = {
  'agent-skills-profiler-marker.svg': ['AgentSession', {skill: 'aspid-profiler-marker'}],
  'agent-skills-visual-element-fluent.svg': ['AgentSession', {skill: 'aspid-visual-element-fluent'}],
  'agent-skills-serializable-type.svg': ['AgentSession', {skill: 'aspid-serializable-type'}],
  'agent-skills-enum-values.svg': ['AgentSession', {skill: 'aspid-enum-values'}],
  'profiler-markers-hierarchy.svg': 'ProfilerHierarchy',
};

export default function remarkLiveDiagrams() {
  return (tree) => {
    tree.children.forEach((node, index, siblings) => {
      const image = node.type === 'paragraph' && node.children?.length === 1 && node.children[0];
      const entry = image?.type === 'image' && COMPONENTS[image.url.split('/').pop()];
      if (!entry) return;
      const [name, props = {}] = Array.isArray(entry) ? entry : [entry];
      siblings[index] = {
        type: 'mdxJsxFlowElement',
        name,
        attributes: [{alt: image.alt || '', ...props}].flatMap((all) => Object.entries(all))
          .map(([key, value]) => ({type: 'mdxJsxAttribute', name: key, value})),
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
