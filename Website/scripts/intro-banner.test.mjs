import fs from 'node:fs';
import assert from 'node:assert/strict';
import {test} from 'node:test';
import remarkIntroBanner, {remarkInlineCode} from '../src/remark/introBanner.js';
import {attribute, element, find, transform} from './remark-tree.mjs';

const options = {baseUrl: '/Aspid.FastTools/', siteUrl: 'https://vpdpersonal.github.io'};
const introduction = {
  English: 'docs/README.md',
  Russian: 'i18n/ru/docusaurus-plugin-content-docs/current/README.md',
};
const banner = (markdown) => transform(remarkIntroBanner(options), markdown, 'README.md');

for (const [language, file] of Object.entries(introduction)) {
  const markdown = fs.readFileSync(new URL(`../${file}`, import.meta.url), 'utf8');
  const tree = banner(markdown);

  test(`${language} introduction: the banner, the status badges and the install panel`, () => {
    assert.equal(tree.children[0].name, 'IntroBanner');
    assert.match(attribute(tree.children[0], 'src'), /aspid_fasttools_readme_banner\.png$/);
    const kinds = [...markdown.matchAll(/status-badge-(\w+)\.svg/g)].map(match => match[1]);
    assert.ok(kinds.length > 0);
    assert.deepEqual(find(tree, element('StatusBadge')).map(node => attribute(node, 'kind')), kinds);
    const [panel, ...more] = find(tree, element('InstallPanel'));
    assert.equal(more.length, 0);
    assert.equal(attribute(panel, 'url'), markdown.match(/^(https:\/\/\S+\.git#upm\S*)$/m)[1]);
  });

  test(`${language} introduction: every feature heading becomes a card with a preview and a link`, () => {
    const docs = [...markdown.matchAll(/^#### \[[^\]]+\]\(([^)]+)\)/gm)]
      .map(match => match[1].replace(/^.*\//, '').replace(/^\d+-/, '').replace(/\.md$/, ''));
    assert.equal(docs.length, [...markdown.matchAll(/^#### /gm)].length, 'every feature heading links its page');
    assert.deepEqual(find(tree, element('FeaturePreview')).map(node => attribute(node, 'doc')), docs);
    assert.equal(find(tree, element('article', 'feature-card')).length, docs.length);
    assert.equal(find(tree, element('FeatureCardMore')).length, docs.length);
    assert.equal(find(tree, node => node.data?.hProperties?.className === 'feature-group').length,
      [...markdown.matchAll(/^### /gm)].length);
  });

  test(`${language} introduction: the resources become tiles and the help section a support panel`, () => {
    const tiles = find(tree, element('div', 'resource-card'));
    assert.ok(tiles.length > 0);
    for (const tile of tiles) {
      const [title] = tile.children;
      assert.equal(title.data.hProperties.className, 'resource-card__title');
      assert.ok(title.children[0].type === 'link' || title.children[0].name === 'ReadmeLink');
    }
    const [support] = find(tree, element('SupportPanel'));
    assert.match(attribute(support, 'issues'), /^https:\/\/github\.com\/[^/]+\/[^/]+\/issues\/?$/);
    assert.match(attribute(support, 'repo'), /^https:\/\/github\.com\/[^/]+\/[^/]+\/?$/);
  });

  test(`${language} introduction: absolute links to the site become site links`, () => {
    const site = `${options.siteUrl}${options.baseUrl}`;
    assert.ok(find(tree, node => node.type === 'link' && node.url.startsWith(site)).length === 0);
    assert.ok(find(tree, element('ReadmeLink')).every(node => attribute(node, 'href').startsWith(options.baseUrl)));
  });
}

test('a page without the banner image is left as it is', () => {
  const page = '# Title\n\n## Installation\n\nPaste the URL:\n\n```text\nhttps://example.test/repo.git#upm\n```\n\nThat is all.\n';
  assert.deepEqual(banner(page), transform(() => {}, page));
});

const comparison = (header, cell) => `| ${header} | After |\n|---|---|\n| <pre lang="csharp">${cell}</pre> | <pre lang="csharp">b();</pre> |\n`;

test('a two-column table of <pre> cells becomes a code comparison with code blocks', () => {
  const tree = banner(comparison('Before', 'a();'));
  const [table] = find(tree, node => node.type === 'table');
  assert.equal(table.data.hProperties.className, 'doc-code-comparison');
  assert.deepEqual(find(table, node => node.type === 'code').map(node => [node.lang, node.value]), [['csharp', 'a();'], ['csharp', 'b();']]);
  assert.deepEqual(table.children[1].children.map(cell => cell.data.hProperties['data-label']), ['Before', 'After']);
});

test('a <code> in the header leaves the table as it is', () => {
  const [table] = find(banner(comparison('`Before`', 'a();')), node => node.type === 'table');
  assert.equal(table.data, undefined);
  assert.equal(find(table, node => node.type === 'code').length, 0);
});

test('<code lang> becomes highlighted inline code', () => {
  const tree = transform(remarkInlineCode(), 'Call <code lang="csharp">Run&lt;T&gt;()</code> first.\n');
  const [code] = find(tree, element('InlineCode'));
  assert.equal(attribute(code, 'language'), 'csharp');
  assert.equal(attribute(code, 'code'), 'Run<T>()');
});
