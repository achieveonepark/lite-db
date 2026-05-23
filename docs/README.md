# LiteDB Guide

LiteDB는 Unity 프로젝트에서 SQLite 데이터베이스를 읽기 전용 테이블 데이터 저장소처럼 다루기 위한 경량 패키지입니다. 게임 밸런스 데이터, 퀘스트, 아이템, 지역화 텍스트처럼 빌드에 포함되는 정적 데이터를 C# 모델로 매핑해서 조회할 수 있습니다.

이 패키지는 sqlite-net 또는 SQLite Asset 패키지를 래핑하지 않습니다. `SQLiteConnection`, `[Table]`, `[PrimaryKey]`처럼 기존 사용부와 비슷한 인터페이스를 제공하되 내부에서는 네이티브 SQLite C API를 직접 호출합니다.

## 제공 기능

* StreamingAssets에 포함된 DB 파일을 런타임 저장소로 복사하고 초기화
* Primary Key 기반 단일 데이터 조회
* Id 범위 조회, 존재 여부 확인, 안전한 조회
* Editor CSV Importer를 통한 CSV 테이블 삽입
* CSV 헤더 기반 데이터 모델 코드 생성
* Localization 헬퍼

## 기본 흐름

1. SQLite DB 파일을 준비합니다.
2. Unity의 `StreamingAssets/data/file.db` 위치에 포함합니다.
3. 데이터 모델 클래스를 작성합니다.
4. 게임 시작 시 `await LiteDB.Initialize()` 또는 `LiteDB.Initialize(path)`를 호출합니다.
5. `LiteDB.Get<T>(id)` 같은 API로 데이터를 읽습니다.

```csharp
using Achieve.Database;
using SQLite;

[Table("Quest")]
public sealed class Quest : IDataBase
{
    [PrimaryKey]
    public int Id { get; set; }
    public string Name { get; set; }
    public int RewardGold { get; set; }
}

await LiteDB.Initialize();

var quest = LiteDB.Get<Quest>(1);
if (LiteDB.TryGetValue<Quest>(2, out var nextQuest))
{
    Debug.Log(nextQuest.Name);
}
```
