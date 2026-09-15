import React from 'react';
import MDXComponents from '@theme-original/MDXComponents';
import Link from '@docusaurus/Link';
import ArticleToc from '../../components/ArticleToc';

function ReadmeLink(props) {
  return <Link {...props} autoAddBaseUrl={false} target="_self" />;
}

function DocTable(props) {
  return <div className="doc-table-scroll"><table {...props} /></div>;
}

// The introduction is short enough to skip the in-article table of contents.
function IntroBanner(props) {
  return <img {...props} />;
}

export default {...MDXComponents, table: DocTable, IntroBanner, ReadmeLink, ArticleToc};
