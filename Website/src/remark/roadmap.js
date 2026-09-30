/**
 * The roadmap page (root ROADMAP.md) on the site. On GitHub it is plain sections: `## ` stages holding `### ` themes, each a
 * paragraph and a line of issue links. The site draws the stages as a track under the lede and each theme as a card with a
 * live preview of the feature and the states of its issues (`src/components/Roadmap`). The stage headings stay, so the
 * page keeps its table of contents; a theme keeps its heading's anchor on its card, which the sidebar links to.
 */
const ISSUE_URL = /\/issues\/(\d+)$/;

const attribute = (name, value) => ({type: 'mdxJsxAttribute', name, value});
const jsx = (name, attributes, children = []) => ({type: 'mdxJsxFlowElement', name, attributes, children});
const paragraph = (className, children) => ({type: 'paragraph', data: {hProperties: {className}}, children});

/** `Heading {#id}` → [id, heading children without the anchor]; sync-i18n adds the anchor to every stage and theme. */
function splitAnchor(heading) {
  const children = structuredClone(heading.children);
  const last = children.at(-1);
  let id;
  if (last?.type === 'text') {
    last.value = last.value.replace(/\s*\{#([^}]+)\}\s*$/, (match, anchor) => {
      id = anchor;
      return '';
    });
  } else if (last?.type === 'mdxTextExpression' && last.value.startsWith('#')) {
    id = last.value.slice(1);
    children.pop();
  }
  if (!id && heading.data?.hProperties?.id) id = heading.data.hProperties.id;
  return [id, children];
}

const plain = (nodes) => nodes.map((node) => node.value ?? plain(node.children ?? [])).join('').trim();

/** A paragraph made only of issue links and the commas between them. */
function issueNumbers(node) {
  if (node?.type !== 'paragraph') return undefined;
  const links = node.children.filter((part) => part.type === 'link');
  const rest = node.children.filter((part) => part.type !== 'link');
  if (!links.length || !links.every((link) => ISSUE_URL.test(link.url))
    || !rest.every((part) => part.type === 'text' && /^[\s,]*$/.test(part.value))) return undefined;
  return links.map((link) => Number(link.url.match(ISSUE_URL)[1]));
}

export default function remarkRoadmap() {
  return (tree) => {
    const title = tree.children.findIndex((node) => node.type === 'heading' && node.depth === 1);
    if (title === -1) return;
    const lede = tree.children[title + 1];
    if (lede?.type === 'paragraph') lede.data = {...lede.data, hProperties: {...lede.data?.hProperties, className: 'readme-lede'}};

    const stages = [];
    const result = tree.children.slice(0, title + 2);
    let cards;
    for (let i = title + 2; i < tree.children.length; i++) {
      const node = tree.children[i];
      if (node.type === 'heading' && node.depth === 2) {
        const [id, children] = splitAnchor(node);
        stages.push({id, label: plain(children), themes: 0, issues: []});
        cards = undefined;
        result.push({...node, data: {...node.data, hProperties: {...node.data?.hProperties, className: 'roadmap-stage'}}});
        continue;
      }
      const stage = stages.at(-1);
      if (stage && node.type === 'heading' && node.depth === 3) {
        const [id, children] = splitAnchor(node);
        const body = [];
        let issues = [];
        while (tree.children[i + 1] && tree.children[i + 1].type !== 'heading') {
          const next = tree.children[++i];
          const numbers = issueNumbers(next);
          if (numbers) issues = numbers;
          else body.push(next.type === 'paragraph' ? paragraph('feature-card__text', next.children) : next);
        }
        stage.themes++;
        stage.issues.push(...issues);
        if (!cards) {
          cards = jsx('div', [attribute('className', 'feature-cards roadmap-cards')]);
          result.push(cards);
        }
        cards.children.push(jsx('RoadmapTheme', [attribute('id', id), attribute('issues', issues.join(','))], [
          paragraph('feature-card__title', children),
          ...body,
        ]));
        continue;
      }
      // A stage without themes that is one paragraph with a link to a new issue is the call for ideas.
      const link = node.type === 'paragraph' && node.children.find((part) => part.type === 'link' && /\/issues\/new/.test(part.url));
      if (stage && !stage.themes && link) {
        stage.idea = true;
        result.push(jsx('RoadmapIdea', [attribute('href', link.url)], [paragraph('roadmap-idea__text', node.children)]));
        continue;
      }
      result.push(node);
    }

    const planned = stages.flatMap((stage) => stage.issues).join(',');
    result.filter((node) => node.name === 'RoadmapIdea').forEach((node) => node.attributes.push(attribute('exclude', planned)));
    const track = jsx('RoadmapTrack', [attribute('stages', JSON.stringify(stages.filter((stage) => !stage.idea)))]);
    result.splice(title + 2, 0, track);
    tree.children = result;
  };
}
