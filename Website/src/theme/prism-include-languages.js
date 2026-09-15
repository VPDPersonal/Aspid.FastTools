import prismIncludeLanguages from '@theme-original/prism-include-languages';

export default function includeLanguages(Prism) {
  prismIncludeLanguages(Prism);

  const preprocessor = Prism.languages.csharp.preprocessor;
  // Keep directive arguments in the normal text color and include # in the keyword.
  delete preprocessor.alias;
  delete preprocessor.inside.directive.lookbehind;
}
