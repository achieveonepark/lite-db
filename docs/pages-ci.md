# GitHub Pages 배포

이 저장소는 GitBook 소스를 `docs/`에 두고 GitHub Actions로 정적 사이트를 빌드해 GitHub Pages에 배포합니다.

## 로컬 빌드

```bash
cd docs
npm install
npm run build
```

결과물은 `docs/_book`에 생성됩니다.

로컬 미리보기:

```bash
cd docs
npm run serve
```

## CI 흐름

`.github/workflows/pages.yml`은 다음 순서로 동작합니다.

1. 저장소 체크아웃
2. Node.js 설정
3. `docs/package-lock.json` 기준 의존성 설치
4. HonKit으로 GitBook 정적 사이트 빌드
5. `_book` 결과물을 Pages artifact로 업로드
6. GitHub Pages에 배포

## GitHub 설정

저장소의 GitHub Pages 설정에서 Source를 `GitHub Actions`로 선택해야 합니다.

배포 URL은 일반적으로 다음 형식입니다.

```text
https://somiri.dev/lite-db/
```
