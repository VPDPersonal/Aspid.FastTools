import React from 'react';
import Head from '@docusaurus/Head';
import { Redirect } from '@docusaurus/router';
import useBaseUrl from '@docusaurus/useBaseUrl';

/**
 * The site root is the documentation index; there is no separate landing page yet. `<Redirect>` runs only after
 * hydration, so the static page also redirects by itself, points crawlers at /docs and stays out of the sitemap.
 */
export default function Home() {
  const docs = useBaseUrl('/docs');
  const canonical = useBaseUrl('/docs', { absolute: true });
  return (
    <>
      <Head>
        <meta httpEquiv="refresh" content={`0; url=${docs}`} />
        <meta name="robots" content="noindex" />
        <link rel="canonical" href={canonical} />
      </Head>
      <Redirect to={docs} />
    </>
  );
}
