// @ts-check
/**
 * Generated doc ids are `<folder>/readme`, the folder name run through `samplePrefixParser` in docusaurus.config.js
 * (`SerializeReferences` → `serialize-references`). `README.md` is the folder's index document, so the route is
 * /tutorials/<folder>. Source files live under each sample's `Documentation/` folder; the sync script flattens
 * that implementation detail for Docusaurus. Each sample is a single page; there is no separate TUTORIAL.md.
 * @type {import('@docusaurus/plugin-content-docs').SidebarsConfig}
 */
export default {
  tutorials: [
    { type: 'doc', id: 'index', label: 'Overview' },
    { type: 'doc', id: 'enum-values/readme', label: 'EnumValues' },
    { type: 'doc', id: 'types/readme', label: 'Types' },
    { type: 'doc', id: 'serialize-references/readme', label: 'SerializeReferences' },
    { type: 'doc', id: 'editor-tools/readme', label: 'EditorTools' },
    { type: 'doc', id: 'profiler-markers/readme', label: 'ProfilerMarkers' },
  ],
};
