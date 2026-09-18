import {readFile} from 'node:fs/promises';
import path from 'node:path';

// Use Docusaurus' resolved sources and permalinks so translations and versions stay aligned.
export function plainText(markdown) {
  return markdown
    .replace(/^---\r?\n[\s\S]*?\r?\n---\r?\n/, '')
    .replace(/```[^\n]*\n/g, ' ').replace(/```/g, ' ')
    .replace(/!\[[^\]]*\]\([^)]*\)/g, '')
    .replace(/\[([^\]]+)\]\([^)]*\)/g, '$1')
    .replace(/<\/?(?:img|a|div|span|p|br|details|summary|table|tr|td|th|pre|code)\b[^>]*>/gi, ' ')
    .replace(/\\([<>_{}])/g, '$1')
    .replace(/\{#[^}]+\}/g, '')
    .replace(/^\s*(?:#{1,6}|>)\s+/gm, ' ')
    .replace(/[*`|]/g, ' ')
    .replace(/\s+/g, ' ').trim();
}

export default function searchPlugin(context) {
  let indexPath;
  return {
    name: 'fasttools-search',
    async allContentLoaded({allContent, actions}) {
      const entries = [];
      for (const [id, content] of Object.entries(allContent['docusaurus-plugin-content-docs'] ?? {})) {
        for (const version of content.loadedVersions) {
          for (const doc of version.docs) {
            if (doc.draft || doc.unlisted) continue;
            const source = path.resolve(context.siteDir, doc.source.replace(/^@site\//, ''));
            const markdown = await readFile(source, 'utf8');
            const heading = markdown.match(/^# (.+)$/m)?.[1];
            entries.push({
              title: plainText(heading || doc.title),
              url: doc.permalink,
              section: {default: 'Docs', tutorials: 'Samples', api: 'API', changelog: 'Changelog'}[id] ?? id,
              description: plainText(doc.description || ''),
              text: plainText(markdown),
            });
          }
        }
      }
      indexPath = await actions.createData('index.json', JSON.stringify(entries));
    },
    configureWebpack() {
      return {resolve: {alias: {'@fasttools-search-index': indexPath}}};
    },
  };
}
