import { source } from '@/lib/source';
import { createFromSource } from 'fumadocs-core/search/server';

// 정적 export 환경에서 검색 인덱스를 정적 파일로 생성한다.
export const revalidate = false;

export const { staticGET: GET } = createFromSource(source);
