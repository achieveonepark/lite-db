import { source } from '@/lib/source';
import { createFromSource } from 'fumadocs-core/search/server';

// 정적 export 환경에서 검색 인덱스를 정적 파일로 생성한다.
export const revalidate = false;

// Orama 는 ko/ja/zh stemmer 를 기본 지원하지 않으므로 영어 토크나이저로 통일한다.
export const { staticGET: GET } = createFromSource(source, {
  localeMap: {
    ko: 'english',
    en: 'english',
    ja: 'english',
    zh: 'english',
  },
});
