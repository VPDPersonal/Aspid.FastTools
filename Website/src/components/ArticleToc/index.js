import React from 'react';
import clsx from 'clsx';
import {translate} from '@docusaurus/Translate';
import {ThemeClassNames, useCollapsible, Collapsible, usePluralForm} from '@docusaurus/theme-common';
import {useDoc} from '@docusaurus/plugin-content-docs/client';
import TOCItems from '@theme/TOCItems';

function useSectionsLabel(count) {
  const {selectMessage} = usePluralForm();
  return selectMessage(count, translate({
    id: 'venom.articleToc.sections',
    message: '{count} section|{count} sections',
    description: 'Section count shown next to the in-article table of contents toggle',
  }, {count}));
}

/**
 * Collapsed one-line table of contents placed under the article title by remark/articleToc.js.
 * Shows the section count while closed; the list opens in two columns on wide articles (custom.css).
 */
export default function ArticleToc() {
  const {toc, frontMatter} = useDoc();
  const {collapsed, toggleCollapsed} = useCollapsible({initialState: true});
  const min = frontMatter.toc_min_heading_level ?? 2;
  const max = frontMatter.toc_max_heading_level ?? 3;
  const sections = toc.filter((item) => item.level >= min && item.level <= max);
  const top = sections.filter((item) => item.level === min).length || sections.length;
  const label = useSectionsLabel(top);
  if (frontMatter.hide_table_of_contents || sections.length === 0) return null;
  return (
    <nav className={clsx(ThemeClassNames.docs.docTocMobile, 'article-toc', !collapsed && 'article-toc--open')}>
      <button type="button" className="clean-btn article-toc__toggle" aria-expanded={!collapsed} onClick={toggleCollapsed}>
        <span className="article-toc__title">
          {translate({id: 'theme.TOCCollapsible.toggleButtonLabel', message: 'On this page'})}
        </span>
        <span className="article-toc__count">{label}</span>
      </button>
      <Collapsible lazy collapsed={collapsed} className="article-toc__list">
        <TOCItems toc={toc} minHeadingLevel={min} maxHeadingLevel={max} />
      </Collapsible>
    </nav>
  );
}
