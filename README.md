# LiteDB

LiteDB는 Unity 프로젝트에서 SQLite DB 파일을 가볍게 읽기 위한 패키지입니다. 게임 밸런스 데이터, 퀘스트, 아이템, 지역화 텍스트처럼 정적인 테이블 데이터를 C# 모델로 매핑해서 조회할 수 있습니다.

이 패키지는 sqlite-net 또는 SQLite Asset 패키지를 래핑하지 않습니다. 기존 사용부와 비슷하게 `SQLiteConnection`, `[Table]`, `[PrimaryKey]` 같은 인터페이스를 제공하되 내부에서는 네이티브 SQLite C API를 직접 호출합니다.

## Documentation

GitBook 가이드는 GitHub Pages로 배포됩니다.

https://somiri.dev/lite-db/

## Quick Start

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
```

```csharp
await LiteDB.Initialize();

var quest = LiteDB.Get<Quest>(1);

if (LiteDB.TryGetValue<Quest>(2, out var nextQuest))
{
    Debug.Log(nextQuest.Name);
}

var quests = LiteDB.GetList<Quest>(1, 10);

if (LiteDB.Exist<Quest>(1))
{
    Debug.Log("Quest exists.");
}
```

## DB File

기본 초기화 API는 다음 경로를 사용합니다.

| 용도 | 경로 |
| --- | --- |
| 원본 DB | `Application.streamingAssetsPath/data/file.db` |
| 런타임 복사본 | `Application.persistentDataPath/data/localdatasqlite.db` |

직접 DB 파일 경로를 지정할 수도 있습니다.

```csharp
LiteDB.Initialize($"{Application.persistentDataPath}/data/game.db");
```

## Editor Tools

CSV Importer는 Unity Editor 메뉴에서 열 수 있습니다.

```text
GameFramework/Data/CsvImporter
```

CSV 파일을 SQLite 테이블로 삽입하고, CSV 헤더를 기반으로 C# 데이터 모델 클래스를 생성할 수 있습니다.
