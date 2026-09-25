import prismIncludeLanguages from '@theme-original/prism-include-languages';

export default function includeLanguages(Prism) {
  prismIncludeLanguages(Prism);

  const preprocessor = Prism.languages.csharp.preprocessor;
  // Keep directive arguments in the normal text color and include # in the keyword.
  delete preprocessor.alias;
  delete preprocessor.inside.directive.lookbehind;

  // Prism colours an event's delegate type only before `;` or `=`, and only the first part of `class Outer.Inner`.
  Prism.languages.insertBefore('csharp', 'class-name', {
    'nested-type-declaration': {
      pattern: /(\b(?:class|enum|interface|record|struct)\s+)[A-Za-z_]\w*(?:\.[A-Za-z_]\w*)+/,
      lookbehind: true,
      alias: 'class-name',
      inside: {punctuation: /\./},
    },
    'event-type': {
      pattern: /(\bevent\s+)[A-Za-z_][\w.]*(?:<[^<>;{}]*>)?(?=\s+[A-Za-z_]\w*)/,
      lookbehind: true,
      alias: 'class-name',
      inside: {punctuation: /[.<>,]/},
    },
  });
}
