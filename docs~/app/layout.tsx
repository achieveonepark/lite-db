import './global.css';
import type { ReactNode } from 'react';

// 루트 레이아웃은 통과만 한다. 실제 <html>/<body> 와 Provider 는 [lang]/layout 에서 처리.
export default function RootLayout({ children }: { children: ReactNode }) {
  return children;
}
