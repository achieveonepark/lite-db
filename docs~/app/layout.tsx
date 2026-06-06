import './global.css';
import { RootProvider } from 'fumadocs-ui/provider/next';
import type { ReactNode } from 'react';

export default function RootLayout({ children }: { children: ReactNode }) {
  return (
    <html lang="ko" suppressHydrationWarning>
      <body className="flex min-h-screen flex-col">
        <RootProvider
          theme={{ defaultTheme: 'dark', enableSystem: false }}
          search={{ options: { type: 'static' } }}
        >
          {children}
        </RootProvider>
      </body>
    </html>
  );
}
