/**
 * Reads the repository's issues from GitHub at build time, so the roadmap shows each linked issue's state and 👍 count.
 * CI passes GITHUB_TOKEN; without it the public API still answers (60 requests an hour). Offline or rate-limited, the
 * build goes on without the data and the roadmap shows plain issue numbers.
 */
const REPO = 'VPDPersonal/Aspid.FastTools';
const PAGES = 5;

async function fetchIssues() {
  const headers = {Accept: 'application/vnd.github+json', 'User-Agent': 'aspid-fasttools-site'};
  if (process.env.GITHUB_TOKEN) headers.Authorization = `Bearer ${process.env.GITHUB_TOKEN}`;
  const issues = {};
  for (let page = 1; page <= PAGES; page++) {
    const response = await fetch(`https://api.github.com/repos/${REPO}/issues?state=all&per_page=100&page=${page}`, {
      headers,
      signal: AbortSignal.timeout(10_000),
    });
    if (!response.ok) throw new Error(`GitHub answered ${response.status}`);
    const batch = await response.json();
    for (const issue of batch) {
      if (issue.pull_request) continue;
      issues[issue.number] = {
        title: issue.title,
        state: issue.state,
        votes: issue.reactions?.['+1'] ?? 0,
        milestone: issue.milestone?.title ?? null,
      };
    }
    if (batch.length < 100) break;
  }
  return issues;
}

export default function githubIssuesPlugin() {
  return {
    name: 'fasttools-github-issues',
    async loadContent() {
      try {
        return await fetchIssues();
      } catch (error) {
        console.warn(`[github-issues] roadmap builds without issue states: ${error.message}`);
        return {};
      }
    },
    async contentLoaded({content, actions}) {
      actions.setGlobalData({issues: content});
    },
  };
}
