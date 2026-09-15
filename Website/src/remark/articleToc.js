/**
 * Place the in-article table of contents right under the page title.
 * Docusaurus renders its collapsible TOC above the article; the site wants it after the `# H1`
 * so the disclosure reads as part of the article. Pages without an H1 (the introduction) get none.
 */
export default function remarkArticleToc() {
  return (tree) => {
    const index = tree.children.findIndex((node) => node.type === 'heading' && node.depth === 1);
    if (index === -1) return;
    tree.children.splice(index + 1, 0, {type: 'mdxJsxFlowElement', name: 'ArticleToc', attributes: [], children: []});
  };
}
