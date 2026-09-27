/**
 * On the site, the reflection result table of SerializedProperty Extensions (the table whose header names
 * `GetDeclaringInstance()`) becomes `<PropertyExplorer/>`: the same results behind a clickable Inspector.
 * GitHub and Unity keep the table.
 */
// Inline code has already become `<InlineCode code="…"/>` (introBanner.js runs first), so read that attribute too.
const text = (node) => node.value
  ?? node.attributes?.find((attribute) => attribute.name === 'code')?.value
  ?? (node.children ?? []).map(text).join('');

export default function remarkPropertyExplorer() {
  return (tree) => {
    tree.children.forEach((node, index, siblings) => {
      if (node.type !== 'table' || !text(node.children?.[0] ?? {}).includes('GetDeclaringInstance()')) return;
      siblings[index] = {type: 'mdxJsxFlowElement', name: 'PropertyExplorer', attributes: [], children: []};
    });
  };
}
