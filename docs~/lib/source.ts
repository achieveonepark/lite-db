import { docs } from '@/.source/server';
import { loader } from 'fumadocs-core/source';
import { i18n } from 'ach-fumadocs-theme';

// 문서를 사이트 루트(/)에서 서비스한다. i18n 은 공통 테마 설정을 사용한다.
export const source = loader({
  baseUrl: '/',
  i18n,
  source: docs.toFumadocsSource(),
});
