'use client';

import { useEffect } from 'react';
import { i18n } from 'ach-fumadocs-theme';

// 정적 export 환경에서 루트('/')를 기본 로케일로 보낸다. (미들웨어 없이 클라이언트 리다이렉트)
export default function RootRedirect() {
  const to = `./${i18n.defaultLanguage}/`;

  useEffect(() => {
    window.location.replace(to);
  }, [to]);

  return (
    <>
      <meta httpEquiv="refresh" content={`0; url=${to}`} />
      <a href={to}>Redirecting…</a>
    </>
  );
}
