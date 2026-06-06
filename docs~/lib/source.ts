import { docs } from '@/.source/server';
import { loader } from 'fumadocs-core/source';

// 문서를 사이트 루트(/)에서 서비스한다. (Docusaurus routeBasePath '/' 와 동일)
export const source = loader({
  baseUrl: '/',
  source: docs.toFumadocsSource(),
});
