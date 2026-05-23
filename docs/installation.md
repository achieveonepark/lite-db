# 설치

## Unity Package Manager

Git URL로 설치할 수 있습니다.

```text
https://github.com/achieveonepark/lite-db.git
```

Unity Package Manager의 `Add package from git URL...` 메뉴에 위 주소를 입력합니다.

## 의존성

패키지 manifest 기준 의존성은 다음과 같습니다.

| 패키지 | 용도 |
| --- | --- |
| `com.cysharp.unitask` | 비동기 DB 파일 복사 및 초기화 |
| `com.unity.test-framework` | Unity 테스트 환경 |

## 지원 Unity 버전

`package.json` 기준 Unity `2022.3` 이상을 대상으로 합니다.

## 패키지 구성

| 경로 | 설명 |
| --- | --- |
| `Runtime/` | 런타임 API와 SQLite 직접 호출 구현 |
| `Runtime/Plugins/lib/` | 플랫폼별 네이티브 SQLite 바이너리 |
| `Editor/` | CSV Importer와 코드 생성 도구 |
| `docs/` | GitBook 문서 소스 |
