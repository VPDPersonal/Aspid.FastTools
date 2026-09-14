import React from 'react';
import MDXComponents from '@theme-original/MDXComponents';
import {useDoc} from '@docusaurus/plugin-content-docs/client';
import DocItemTOCMobile from '@theme/DocItem/TOC/Mobile';
import Link from '@docusaurus/Link';

function ReadmeLink(props) {
  return <Link {...props} autoAddBaseUrl={false} target="_self" />;
}

function DocTable(props) {
  return <div className="doc-table-scroll"><table {...props} /></div>;
}

function IntroBanner(props) {
  const {toc, frontMatter} = useDoc();
  return <>
    <img {...props} />
    {!frontMatter.hide_table_of_contents && toc.length > 0 && <DocItemTOCMobile />}
  </>;
}

export default {...MDXComponents, table: DocTable, IntroBanner, ReadmeLink};
