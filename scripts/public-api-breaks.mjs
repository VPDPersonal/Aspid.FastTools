// Finds the public API that a pull request takes out of the snapshot Tests/Editor/PublicApi/PublicApi.txt. A removed or
// changed member line is a break for the users of the package, so pr-checks.yml asks for `!` in the title then.
// The functions are pure: the workflow reads the pull request through the API and passes the text in.
//
// The snapshot holds `#` comment lines, blank lines, `[Assembly]` lines, a type name per type (no indent) and the
// members of that type (indented). Only a comment or a blank line is not a member line.

export const PUBLIC_API_PATH = 'Aspid.FastTools/Packages/tech.aspid.fasttools/Tests/Editor/PublicApi/PublicApi.txt';

const isMemberLine = (line) => line.trim() !== '' && !line.startsWith('#');

// The member lines that the `patch` field of the files API removes. A changed line is a removed line plus an added one.
// The field holds hunks only (no `---` header), and context lines start with a space, so only "-" lines count.
export function removedMemberLines(patch) {
  return patch
    .split('\n')
    .filter((line) => line.startsWith('-') && isMemberLine(line.slice(1)))
    .map((line) => line.slice(1).trim());
}

// The member lines of `before` that `after` no longer has, for a file whose patch GitHub left out (a large diff).
// A line counts with the type (and the assembly) it sits in: `public int Count { get; }` is shared by many types, so
// a plain line set would hide the loss from one of them.
export function removedMemberLinesBetween(before, after) {
  const kept = new Set(snapshotEntries(after).map(({ key }) => key));
  return snapshotEntries(before)
    .filter(({ key }) => !kept.has(key))
    .map(({ line }) => line);
}

function snapshotEntries(text) {
  const entries = [];
  let assembly = '';
  let type = '';

  for (const raw of text.split('\n')) {
    const line = raw.trimEnd();
    if (!isMemberLine(line)) continue;

    if (/^\[.*\]$/.test(line)) {
      assembly = line;
      type = '';
      entries.push({ line, key: line });
    } else if (/^\s/.test(line)) {
      entries.push({ line: line.trim(), key: `${assembly}\n${type}\n${line.trim()}` });
    } else {
      type = line;
      entries.push({ line, key: `${assembly}\n${line}` });
    }
  }

  return entries;
}

// The failure text of the title check, or an empty string when the title already has `!` or no member line was lost.
export function breakingTitleError(removed, hasBang) {
  if (hasBang || removed.length === 0) return '';

  const shown = removed.slice(0, 3).map((line) => `\`${line}\``).join(', ');
  const more = removed.length > 3 ? `, and ${removed.length - 3} more` : '';
  return `PublicApi.txt lost ${removed.length} member line${removed.length === 1 ? '' : 's'} (${shown}${more}): ` +
    'a removed or changed public member breaks the code of the package users. ' +
    'Put `!` before the colon (`type(scope)!: summary`) and add a migration note under Changed in CHANGELOG.md and ' +
    'CHANGELOG.ru.md.';
}
