// @ts-check
/** @type {import('@docusaurus/plugin-content-docs').SidebarsConfig} */
export default {
  docs: [
    { type: 'doc', id: 'README', label: 'Introduction' },
    { type: 'doc', id: 'getting-started' },
    { type: 'category', label: 'Serialization', className: 'doc-menu-group', collapsible: false, items: [
      'serializable-types', 'serialize-reference-selector', 'serialize-reference-tooling', 'enum-values',
    ] },
    { type: 'category', label: 'Editor & tooling', className: 'doc-menu-group', collapsible: false, items: [
      'profiler-markers', 'visual-element-extensions', 'serialized-property-extensions', 'editor-helpers', 'claude-code-plugin',
    ] },
  ],
};
