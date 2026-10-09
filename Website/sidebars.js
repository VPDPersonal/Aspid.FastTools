// @ts-check
/** @type {import('@docusaurus/plugin-content-docs').SidebarsConfig} */
export default {
  docs: [
    { type: 'doc', id: 'README', label: 'Introduction' },
    { type: 'category', label: 'Serialization', className: 'doc-menu-group', collapsible: false, items: [
      'serializable-types', 'type-selector', 'serialize-reference-selector', 'component-type-selector',
      'serialize-reference-tooling', 'serialize-reference-validation', 'enum-values',
    ] },
    { type: 'category', label: 'Editor & tooling', className: 'doc-menu-group', collapsible: false, items: [
      'profiler-markers', 'visual-element-extensions', 'serialized-property-extensions', 'editor-helpers', 'theme-override', 'agent-skills',
    ] },
  ],
};
