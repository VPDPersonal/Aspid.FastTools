import React from 'react';
import MDXComponents from '@theme-original/MDXComponents';
import Link from '@docusaurus/Link';
import useDocusaurusContext from '@docusaurus/useDocusaurusContext';
import AnimatedPreview from '@site/src/components/FeaturePreview';
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

// EnumValues and the tooling features get an animated preview; the rest keep their README capture.
function FeaturePreview(props) {
  const {i18n} = useDocusaurusContext();
  return <AnimatedPreview {...props} ru={i18n.currentLocale === 'ru'} />;
}

export default {...MDXComponents, table: DocTable, IntroBanner, ReadmeLink, FeaturePreview};
