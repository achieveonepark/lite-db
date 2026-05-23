# DB 파일 준비

LiteDB는 SQLite DB 파일을 읽어서 C# 모델로 매핑합니다. DB 파일은 DB Browser for SQLite 같은 도구로 만들 수 있습니다.

## 권장 위치

기본 초기화 API는 다음 위치를 사용합니다.

| 용도 | 경로 |
| --- | --- |
| 원본 DB | `Application.streamingAssetsPath/data/file.db` |
| 런타임 복사본 | `Application.persistentDataPath/data/localdatasqlite.db` |

`await LiteDB.Initialize()`를 호출하면 플랫폼별로 원본 DB 파일을 런타임 경로에 복사한 뒤 연결합니다. Android에서는 `UnityWebRequest`로 `StreamingAssets` 파일을 읽고, 그 외 플랫폼에서는 파일 복사를 사용합니다.

직접 경로를 제어하고 싶다면 다음 API를 사용합니다.

```csharp
LiteDB.Initialize($"{Application.persistentDataPath}/data/game.db");
```

## 테이블 작성 규칙

LiteDB의 기본 조회 API는 `Id` 컬럼을 기준으로 동작합니다.

| SQLite 타입 | C# 타입 |
| --- | --- |
| `INTEGER` | `int`, `long`, `bool`, enum |
| `REAL` | `float`, `double` |
| `TEXT` | `string`, `DateTime`, `Guid` |
| `BLOB` | `byte[]` |

권장 규칙:

* 테이블에는 `Id` 컬럼을 둡니다.
* 단일 조회 대상 모델은 `Id`를 Primary Key로 관리합니다.
* C# 프로퍼티 이름과 DB 컬럼 이름을 같게 맞춥니다.
* 클래스 이름과 테이블 이름이 다르면 `[Table("TableName")]`을 사용합니다.
