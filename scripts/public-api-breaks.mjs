// Finds the public API that a pull request takes out of the snapshot Tests/Editor/PublicApi/PublicApi.txt. A removed or
// changed member line is a break for the users of the package, so pr-checks.yml asks for `!` in the title then: a wrong
// `!` sets the breaking-change label, so a line that only gains text is not a loss (see isAdditiveChange).
// The functions are pure: the workflow reads the pull request through the API and passes the text in.
//
// The snapshot holds `#` comment lines, blank lines, `[Assembly]` lines, a type name per type (no indent) and the
// members of that type (indented). Only a comment or a blank line is not a member line.

export const PUBLIC_API_PATH = 'Aspid.FastTools/Packages/tech.aspid.fasttools/Tests/Editor/PublicApi/PublicApi.txt';

// The code that writes the snapshot. A PR that changes it can change the format of every line.
export const PUBLIC_API_FORMAT_PATH =
  'Aspid.FastTools/Packages/tech.aspid.fasttools/Tests/Editor/PublicApi/PublicApiSnapshot.cs';

const isMemberLine = (line) => line.trim() !== '' && !line.startsWith('#');

// The member lines that the `patch` field of the files API removes, without the ones that an added line only extends.
// A changed line is a removed line plus an added one. The field holds hunks only (no `---` header), and context lines
// start with a space, so only "-" and "+" lines count.
// An added line extends a removed one only in the same type. A line sits in the type of the last unindented line above
// it on its side of the diff. Above a hunk's first one, git names a type after the `@@`: the last line above the hunk,
// in the old file, that starts with a letter (only a type line does), cut at 80 bytes. If that is the type the old side
// was in at the end of the previous hunk, the lines between the hunks hold no type line, and both sides stay in their
// types: the new side may be in another type, which a previous hunk added. Otherwise the type line the header names
// sits between the hunks, so it is the type of both sides.
export function removedMemberLines(patch) {
  const removed = [];
  const added = [];
  let oldScope = '';
  let newScope = '';

  for (const line of patch.split('\n')) {
    const hunk = /^@@ [^@]*@@(.*)$/.exec(line);
    if (hunk) {
      const named = hunk[1].trim();
      if (named !== oldScope && !(named && oldScope.startsWith(named))) oldScope = newScope = named;
      continue;
    }

    const text = line.slice(1);
    if (!isMemberLine(text)) continue;
    const isTypeLine = !/^\s/.test(text);
    const marker = line[0];

    if (marker === '-') removed.push({ line: text.trim(), scope: oldScope });
    else if (marker === '+') added.push({ line: text.trim(), scope: newScope });
    else if (marker !== ' ') continue;

    if (isTypeLine && marker !== '+') oldScope = text.trim();
    if (isTypeLine && marker !== '-') newScope = text.trim();
  }

  return withoutExtended(removed, added).map(({ line }) => line);
}

// The member lines of `before` that `after` no longer has, for a file whose patch GitHub left out (a large diff).
// A line counts with the type (and the assembly) it sits in: `public int Count { get; }` is shared by many types, so
// a plain line set would hide the loss from one of them.
export function removedMemberLinesBetween(before, after) {
  const beforeEntries = snapshotEntries(before);
  const afterEntries = snapshotEntries(after);
  const beforeKeys = new Set(beforeEntries.map(({ key }) => key));
  const afterKeys = new Set(afterEntries.map(({ key }) => key));

  // A line that appeared in a type may extend a line that left the same type. An assembly or a type line has no scope,
  // so it extends nothing.
  const removed = beforeEntries.filter(({ key }) => !afterKeys.has(key));
  const added = afterEntries.filter(({ key, scope }) => scope !== undefined && !beforeKeys.has(key));

  return withoutExtended(removed, added).map(({ line }) => line);
}

// A removed line that an added line of the same scope extends is no loss. Each added line extends at most one removed
// line.
function withoutExtended(removed, added) {
  const pool = [...added];

  return removed.filter(({ line, scope }) => {
    const index = pool.findIndex((candidate) => candidate.scope === scope && isAdditiveChange(line, candidate.line));
    if (index < 0) return true;

    pool.splice(index, 1);
    return false;
  });
}

// Whether `after` is `before` plus text that cannot break a caller or a derived type. Two additions are safe, nothing
// else: an entry in the base list of a class or a struct (a new interface, or a base class), and an attribute in front
// of the declaration (the `[Flags]` of an enum). The rest of the line must stay the same, so `sealed`, `abstract` or
// `static` on a type, `readonly` on a field, a lost `virtual`, a narrower access, a changed return or parameter type, a
// new constraint, a new base interface of an interface and a new underlying type of an enum all stay losses.
export function isAdditiveChange(before, after) {
  if (before === after) return false;

  const old = parseDeclaration(before);
  const now = parseDeclaration(after);
  if (!old || !now || old.head !== now.head || old.constraints !== now.constraints) return false;
  if (!isSubsequence(old.attributes, now.attributes)) return false;

  return old.kind === 'class' || old.kind === 'struct'
    ? isSubsequence(old.bases, now.bases)
    : old.bases.join(', ') === now.bases.join(', ');
}

// The parts of a type declaration line, or null for any other line. The snapshot writes it as
// `[Flags] public sealed class Name<T> : Base, IFace where T : struct`.
function parseDeclaration(line) {
  const attributes = [];
  let rest = line;
  for (let match; (match = /^(\[[^\]]*\])\s+/.exec(rest)); rest = rest.slice(match[0].length)) attributes.push(match[1]);

  const declaration = /^((?:public|protected) (?:(?:static|abstract|sealed) )?(class|struct|interface|enum) )(.*)$/.exec(rest);
  if (!declaration) return null;

  let tail = declaration[3];
  let constraints = '';
  const where = indexOfOutsideBrackets(tail, ' where ');
  if (where >= 0) {
    constraints = tail.slice(where);
    tail = tail.slice(0, where);
  }

  const colon = indexOfOutsideBrackets(tail, ' : ');
  return {
    attributes,
    kind: declaration[2],
    head: declaration[1] + (colon >= 0 ? tail.slice(0, colon) : tail),
    bases: colon >= 0 ? splitOutsideBrackets(tail.slice(colon + 3), ', ') : [],
    constraints,
  };
}

// The first place of `token` that is not inside <...>, (...) or [...], so a comma in `KeyValuePair<TEnum, TValue>` is no
// separator of the base list.
function indexOfOutsideBrackets(text, token) {
  let depth = 0;

  for (let i = 0; i < text.length; i++) {
    if ('<(['.includes(text[i])) depth++;
    else if ('>)]'.includes(text[i])) depth--;
    else if (depth === 0 && text.startsWith(token, i)) return i;
  }

  return -1;
}

function splitOutsideBrackets(text, separator) {
  const parts = [];
  let rest = text;

  for (let index; (index = indexOfOutsideBrackets(rest, separator)) >= 0; rest = rest.slice(index + separator.length))
    parts.push(rest.slice(0, index));

  parts.push(rest);
  return parts;
}

// Whether every item of `items` is in `list`, in the same order.
function isSubsequence(items, list) {
  let position = 0;
  for (const item of list) if (position < items.length && item === items[position]) position++;
  return position === items.length;
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
      entries.push({ line: line.trim(), key: `${assembly}\n${type}\n${line.trim()}`, scope: `${assembly}\n${type}` });
    } else {
      type = line;
      entries.push({ line, key: `${assembly}\n${line}` });
    }
  }

  return entries;
}

// What the title check does with the lost lines: `error` fails it, `warning` only tells. A PR that also changes the code
// that writes the snapshot may have changed the format of the lines, which says nothing about the API, so a maintainer
// decides by hand there.
export function breakingTitleVerdict(removed, hasBang, formatChanged) {
  if (hasBang || removed.length === 0) return { error: '', warning: '' };

  const shown = removed.slice(0, 3).map((line) => `\`${line}\``).join(', ');
  const more = removed.length > 3 ? `, and ${removed.length - 3} more` : '';
  const lost = `PublicApi.txt lost ${removed.length} member line${removed.length === 1 ? '' : 's'} (${shown}${more})`;

  if (formatChanged) {
    return {
      error: '',
      warning: `${lost}, and this PR changes PublicApiSnapshot.cs, which writes the format of the file. ` +
        'The snapshot format changed: a maintainer must judge by hand whether this is a break. ' +
        'If it is, put `!` before the colon and add a migration note under Changed.',
    };
  }

  return {
    error: `${lost}: a removed or changed public member breaks the code of the package users. ` +
      'Put `!` before the colon (`type(scope)!: summary`) and add a migration note under Changed in CHANGELOG.md and ' +
      'Website/i18n/ru/CHANGELOG.md.',
    warning: '',
  };
}
