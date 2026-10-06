# Rules for Claude in this repository

Both jobs in `.github/workflows/claude.yml` append this file to the system prompt:
the automatic review and the `@claude` replies. Edit the rules here, in one place.

## Always

- Follow AGENTS.md: it holds the boundaries, the layout and the code rules.
- Write replies, code, code comments and commit messages in English. Keep code identifiers as they are.

## When you review a pull request

Check, in this order:

1. Correctness: bugs and missed edge cases, with the input that breaks.
2. Boundaries from AGENTS.md:
   - no Unity APIs newer than 6000.0.0f1 without a version guard;
   - no editor-only APIs reachable from runtime code;
   - generator and analyzer code does not write to `Console` and does not reference `SourceGenerator.Foundations`;
   - no Unity API or language feature newer than C# 9 in the SerializeReference YAML engine
     that `Aspid.FastTools.YamlTests/` compiles.
3. Public API: a change needs a CHANGELOG entry, XML docs and, when a skill in `skills/` describes it,
   an updated skill in the same PR.
4. Generators and analyzers: a source change needs the rebuilt DLL committed into the package.
5. Version: a bump goes through `scripts/set-version.sh`, so `package.json`, the badge and both READMEs agree.

Skip style and naming that `.editorconfig` and AGENTS.md do not cover.

Format:

- At most 8 findings, most severe first.
- Put each specific issue in an inline comment on its line.
- Put only a short summary in the main comment: "No blocking issues" or the count of findings.
- Do not praise.
- Do not write the literal trigger phrase (at-sign + "claude") in your comments:
  it starts the workflow again.
