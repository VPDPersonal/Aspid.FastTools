// @ts-check
/**
 * Doc ids are `<folder>/readme` and `<folder>/tutorial`, the folder name run through `samplePrefixParser`
 * in docusaurus.config.js (`SerializeReferences` → `serialize-references`). `README.md` is the folder's
 * index document, so the route is /tutorials/<folder>; TUTORIAL.md sits next to it as /tutorials/<folder>/tutorial.
 * @type {import('@docusaurus/plugin-content-docs').SidebarsConfig}
 */
export default {
  tutorials: [
    {
      type: 'category',
      label: 'Types',
      collapsed: false,
      link: { type: 'doc', id: 'types/readme' },
      items: ['types/tutorial'],
    },
    {
      type: 'category',
      label: 'SerializeReferences',
      collapsed: false,
      link: { type: 'doc', id: 'serialize-references/readme' },
      items: ['serialize-references/tutorial'],
    },
    {
      type: 'category',
      label: 'EnumValues',
      collapsed: false,
      link: { type: 'doc', id: 'enum-values/readme' },
      items: ['enum-values/tutorial'],
    },
    {
      type: 'category',
      label: 'Ids',
      collapsed: false,
      link: { type: 'doc', id: 'ids/readme' },
      items: ['ids/tutorial'],
    },
    {
      type: 'category',
      label: 'ProfilerMarkers',
      collapsed: false,
      link: { type: 'doc', id: 'profiler-markers/readme' },
      items: ['profiler-markers/tutorial'],
    },
    {
      type: 'category',
      label: 'VisualElements',
      collapsed: false,
      link: { type: 'doc', id: 'visual-elements/readme' },
      items: ['visual-elements/tutorial'],
    },
  ],
};
