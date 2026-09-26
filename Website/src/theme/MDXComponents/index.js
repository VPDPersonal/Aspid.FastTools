import React from 'react';
import MDXComponents from '@theme-original/MDXComponents';
import Link from '@docusaurus/Link';
import Translate from '@docusaurus/Translate';
import useDocusaurusContext from '@docusaurus/useDocusaurusContext';
import AnimatedPreview from '@site/src/components/FeaturePreview';
import IntroBanner from '@site/src/components/IntroBanner';
import InstallPanel from '@site/src/components/InstallPanel';
import ProfilerHierarchy from '@site/src/components/ProfilerHierarchy';
import SupportPanel from '@site/src/components/SupportPanel';
import StatusBadge from '@site/src/components/StatusBadge';

function ReadmeLink(props) {
  return <Link {...props} autoAddBaseUrl={false} target="_self" />;
}

function DocTable(props) {
  return <div className="doc-table-scroll"><table {...props} /></div>;
}

// EnumValues and the tooling features get an animated preview; the rest keep their README capture.
function FeaturePreview(props) {
  const {i18n} = useDocusaurusContext();
  return <AnimatedPreview {...props} ru={i18n.currentLocale === 'ru'} />;
}

function FeatureCardMore() {
  return <Translate id="featureCard.more">Read more</Translate>;
}

export default {...MDXComponents, table: DocTable, IntroBanner, ReadmeLink, FeaturePreview, FeatureCardMore, InstallPanel, SupportPanel, StatusBadge, ProfilerHierarchy};
