---
name: docs-review
description: Review one Aspid.FastTools documentation page (English file and its ru twin) against the docs-site rules and the source, and report the findings in chat in a fixed, readable format. - «посмотрим страницу X», «ревью страницы», «проверь страницу на правила», a localhost …/docs/<page> link with a request to look at it.
metadata:
  internal: true
---

# Docs page review

A review checks one page and proposes changes. The first pass never edits anything.

## Scope

- Review feature pages (`Website/docs/NN-*.md`) and sample pages (`Website/tutorials/<Sample>/README.md`).
- Never review the Introduction (`Website/docs/README.md`). It is the reference for all other pages.

## Steps

1. Load the `docs-site` skill. Its rules are the checklist.
2. Read the English page and its Russian twin in `Website/i18n/ru/`.
3. Run `Website/scripts/serve-all.sh`. Open the Russian page of this checkout in the built-in browser.
4. Look at every section on screen.
5. Extract frames from each GIF. Look for empty UI, encoder residue and cursor jumps.
6. Check the page against the source. If the `docs-verifier` agent exists, start it in the background and give it the page pair,
   the source folders and the claims to check. If not, follow "Check without the agent".
7. Compare the page with the Introduction and ProfilerMarkers: lead, quick start, density and visuals.
8. Send the review in the format below. Do not edit yet.
9. When the user answers, apply what they accepted to both languages. Rebuild and check the page.
10. Then the user goes section by section. A stated decision is a "yes": edit at once and rebuild.
11. If a remark is ambiguous, state your reading in one line and apply it. Ask only on «что думаешь».
12. A user remark can apply to other pages too, and no rule may cover it yet. Then propose a rule in one line.
13. Add the rule to `docs-site` only after the user's yes. A remark about one page only stays a page edit.

## Check without the agent

The `docs-verifier` agent is not in this repository. Without it, do the check yourself. The check changes no files.

1. List the claims on the page: type, member and attribute names, signatures, namespaces, `using` lines, sample paths,
   menu paths, default values, versions and install URLs.
2. Find the names in one call: `grep -rnE 'Name1|Name2'` over `Aspid.FastTools/Packages/tech.aspid.fasttools`.
3. Read the declarations, not the usages. A code sample must compile: check every call and every argument.
4. For a feature list, grep the public types and compare them with the list on the page.
5. Compare the English and Russian pages section by section.
6. Note each mismatch as `<page>:<line> — <claim> → <what the code says> (<file>:<line>)`. Use the notes in the review (step 8).

## Review format

Write the review in Russian.

1. Start with one line: do the facts match the source, and do the English and Russian pages agree.
2. Put the items under bold group names: **Нарушения правил**, **Мелочи**, **Оставляю как есть**.
3. Number the items through all groups, so that the user can refer to an item by its number.
4. Give each item a bold title of a few words.
5. Describe the problem in one or two sentences. Name the rule it breaks.
6. Give the fix. For new or changed text, write «Станет: …» with the Russian text only.
7. Show text as the site renders it, not as Markdown source:
   - code in backticks: `Allow = TypeAllow.None`, never `<code lang="csharp">…</code>`;
   - no HTML entities (`&#123;`, `&lt;`) and no `<pre>` cells;
   - Inspector and menu labels in bold: **Fix all**;
   - a link as its underlined text, `<u>Project References</u>`, without the file path or the anchor.
8. Never show the English wording. Translate it yourself when you apply the change.
9. For a picture, give the file, its size and the problem. A re-shoot or a cut needs the user's yes.
10. Under **Оставляю как есть**, list the checked parts that stay, in one or two lines.
11. End with one line that tells the user what to answer.
