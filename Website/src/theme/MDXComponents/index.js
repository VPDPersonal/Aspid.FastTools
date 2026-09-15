import React from 'react';
import MDXComponents from '@theme-original/MDXComponents';
import Link from '@docusaurus/Link';
import banner from '@site/../docs/images/aspid_fasttools_readme_banner.gif';

function ReadmeLink(props) {
  return <Link {...props} autoAddBaseUrl={false} target="_self" />;
}

function DocTable(props) {
  return <div className="doc-table-scroll"><table {...props} /></div>;
}

// The package README links the banner from GitHub so the GIF stays out of the UPM package;
// the site bundles the same file from the repository, so it renders offline too.
function IntroBanner(props) {
  return <img {...props} src={banner} className="readme-banner" />;
}

export default {...MDXComponents, table: DocTable, IntroBanner, ReadmeLink};
