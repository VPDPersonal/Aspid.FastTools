import {existsSync} from 'node:fs';
import path from 'node:path';

/** A sibling `image-light.png` supplies the light theme; Markdown stays usable outside the site. */
export default function remarkThemedImages() {
  return (tree, file) => {
    if (!file.path) return;
    function visit(node) {
      // Existing sample captions repeat the image alt text in a following paragraph.
      node.children?.forEach((child, index, siblings) => {
        const image = child.type === 'paragraph' && child.children?.length === 1 && child.children[0];
        const caption = siblings[index + 1];
        if (image?.type === 'image' && caption?.type === 'paragraph'
          && caption.children?.every((part) => part.type === 'text')
          && caption.children.map((part) => part.value).join('') === image.alt) {
          caption.data = {...caption.data, hProperties: {...caption.data?.hProperties, className: 'doc-media-caption'}};
        }
      });
      if ((node.type === 'paragraph' || node.type === 'tableCell') && node.children?.length === 1) {
        const image = node.children[0];
        if (image.type === 'image' && !/^(?:[a-z]+:|\/|#)/i.test(image.url)) {
          const lightUrl = image.url.replace(/(?<!-light)(\.(?:png|gif|jpe?g|webp|svg))$/i, '-light$1');
          if (lightUrl !== image.url && existsSync(path.resolve(path.dirname(file.path), decodeURIComponent(lightUrl)))) {
            const sceneCapture = /(?:^|\/)(?:demo|scene)\.(?:gif|png)$/i.test(image.url);
            const sceneSample = sceneCapture && /[/\\](?:enum-?values|types|serialize-?references|profiler-?markers)[/\\]/i.test(file.path);
            // A scene sample's footage linked from a doc page keeps its own background, which matches the article.
            const sceneFootage = sceneCapture && /Samples~\/(?:EnumValues|Types|SerializeReferences|ProfilerMarkers)\//.test(image.url);
            // Any other sample's footage linked from a doc page is an editor window with its own edge: no frame around it.
            const windowFootage = sceneCapture && !sceneFootage && /Samples~\//.test(image.url);
            const sceneClass = sceneSample ? ' sample-scene' : sceneFootage ? ' scene-footage' : windowFootage ? ' window-footage' : '';
            // Docusaurus replaces image nodes; put the theme class on a stable wrapper.
            const wrap = (child, theme) => ({
              type: 'mdxJsxTextElement',
              name: 'span',
              attributes: [{type: 'mdxJsxAttribute', name: 'className', value: `theme-image--${theme}${sceneClass}`}],
              children: [child],
            });
            node.children = [wrap(image, 'dark'), wrap({...image, url: lightUrl}, 'light')];
          }
        }
      }
      node.children?.forEach(visit);
    }
    visit(tree);
  };
}
