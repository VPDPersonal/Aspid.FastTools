import React from 'react';
import {Highlight} from 'prism-react-renderer';
import {usePrismTheme} from '@docusaurus/theme-common';

/**
 * One line of code highlighted with the site's Prism theme, keeping the inline-code chip.
 * `string` and `class-name` are token types, not languages: the whole text takes that token's colour
 * (a Profiler marker name, a lone type parameter such as `T`).
 */
export default function InlineCode({code, language}) {
  const theme = usePrismTheme();
  if (language === 'string' || language === 'class-name') {
    const color = theme.styles.find((style) => style.types.includes(language))?.style.color;
    return <code style={{color}}>{code}</code>;
  }
  return (
    <Highlight theme={theme} code={code} language={language}>
      {({tokens, getTokenProps}) => (
        <code className={`language-${language}`} style={{color: theme.plain.color}}>
          {tokens.flat().map((token, index) => {
            const {key, ...props} = getTokenProps({token});
            return <span key={index} {...props} />;
          })}
        </code>
      )}
    </Highlight>
  );
}
