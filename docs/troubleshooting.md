# 문제 해결

## DllNotFoundException

`gilzoide-sqlite-net` 네이티브 라이브러리를 찾지 못하면 SQLite 연결이 실패합니다.

확인할 것:

* `Runtime/Plugins/lib` 폴더가 패키지에 포함되어 있는지 확인합니다.
* 대상 플랫폼의 바이너리가 있는지 확인합니다.
* Unity Plugin Importer 설정이 플랫폼에 맞게 유지되어 있는지 확인합니다.

## Get<T>가 데이터를 찾지 못함

확인할 것:

* 테이블 이름과 `[Table("...")]` 이름이 일치하는지 확인합니다.
* `Id` 컬럼 또는 `[PrimaryKey]`가 붙은 컬럼이 있는지 확인합니다.
* DB 파일이 `StreamingAssets/data/file.db`에 있는지 확인합니다.
* `await LiteDB.Initialize()`가 조회보다 먼저 호출되었는지 확인합니다.

## CSV Importer에서 컬럼 오류

첫 번째 행은 헤더로 사용됩니다. 빈 헤더가 있거나 어떤 행의 필드 수가 헤더 수와 다르면 Importer가 중단됩니다.

## Unity 프로젝트가 아닌 환경에서 테스트할 때

Unity는 `Runtime/Plugins`의 네이티브 플러그인을 플랫폼별로 로드합니다. 일반 .NET 또는 Mono 콘솔에서 직접 실행하면 Unity의 플러그인 로더가 없기 때문에 DLL 탐색 경로를 별도로 맞춰야 합니다.
