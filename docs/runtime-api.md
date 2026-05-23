# 런타임 API

런타임 진입점은 `Achieve.Database.LiteDB`입니다.

## Initialize

```csharp
LiteDB.Initialize(path);
```

전달한 SQLite 파일 경로로 즉시 연결합니다.

```csharp
await LiteDB.Initialize();
```

기본 DB 파일을 `StreamingAssets/data/file.db`에서 `persistentDataPath/data/localdatasqlite.db`로 복사한 뒤 연결합니다.

## Get

```csharp
var item = LiteDB.Get<ItemData>(1);
```

모델의 Primary Key 또는 `Id` 컬럼이 전달한 값과 같은 첫 번째 행을 반환합니다. 초기화되지 않은 상태에서는 `default`를 반환합니다.

## GetList

```csharp
var items = LiteDB.GetList<ItemData>(100, 199);
```

`Id >= startId AND Id <= endId` 범위의 행을 조회합니다.

## Exist

```csharp
if (LiteDB.Exist<ItemData>(10))
{
    // 존재함
}
```

해당 타입의 테이블에서 `Id`가 존재하는지 확인합니다.

## TryGetValue

```csharp
if (LiteDB.TryGetValue<ItemData>(10, out var item))
{
    Debug.Log(item.Name);
}
```

존재 여부를 확인한 뒤 데이터가 있으면 `out`으로 반환합니다. 초기화되지 않았거나 데이터가 없으면 `false`를 반환합니다.

## 직접 SQLiteConnection 사용

고급 사용이 필요하면 `SQLite.SQLiteConnection`을 직접 사용할 수 있습니다.

```csharp
using var db = new SQLiteConnection(path);
var rows = db.Query<ItemData>("SELECT * FROM ItemData WHERE Price >= ?", 100);
var count = db.ExecuteScalar<int>("SELECT COUNT(1) FROM ItemData");
```

현재 제공하는 직접 API는 조회 중심입니다. sqlite-net의 전체 ORM 기능, LINQ 테이블 쿼리, 비동기 커넥션 풀은 포함하지 않습니다.
