import { RootProvider } from 'fumadocs-ui/provider/next';
import { i18n, i18nProvider } from 'ach-fumadocs-theme';
import type { ReactNode } from 'react';

export default async function LangLayout({
  params,
  children,
}: {
  params: Promise<{ lang: string }>;
  children: ReactNode;
}) {
  const { lang } = await params;
  return (
    <html lang={lang} suppressHydrationWarning>
      <body className="flex min-h-screen flex-col">
        <RootProvider
          i18n={i18nProvider(lang)}
          theme={{ defaultTheme: 'dark', enableSystem: false }}
          search={{ options: { type: 'static' } }}
        >
          {children}
        </RootProvider>
      </body>
    </html>
  );
}

export function generateStaticParams() {
  return i18n.languages.map((lang) => ({ lang }));
}
