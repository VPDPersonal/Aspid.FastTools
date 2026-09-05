import React from 'react';
import { Redirect } from '@docusaurus/router';
import useBaseUrl from '@docusaurus/useBaseUrl';

/** The site root is the documentation index; there is no separate landing page yet. */
export default function Home() {
  return <Redirect to={useBaseUrl('/docs')} />;
}
