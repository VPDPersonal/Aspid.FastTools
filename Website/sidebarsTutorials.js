// @ts-check
/**
 * Doc ids are `<folder>/readme`, the folder name run through `samplePrefixParser` in docusaurus.config.js
 * (`SerializeReferences` → `serialize-references`). `README.md` is the folder's index document, so the route is
 * /tutorials/<folder>. Each sample is a single page; there is no separate TUTORIAL.md.
 * @type {import('@docusaurus/plugin-content-docs').SidebarsConfig}
 */
export default {
  tutorials: [
    { type: 'doc', id: 'types/readme', label: 'Types' },
    { type: 'doc', id: 'serialize-references/readme', label: 'SerializeReferences' },
    { type: 'doc', id: 'enum-values/readme', label: 'EnumValues' },
    { type: 'doc', id: 'profiler-markers/readme', label: 'ProfilerMarkers' },
    { type: 'doc', id: 'editor-tools/readme', label: 'EditorTools' },
  ],
};
