# 데이터 모델

데이터 모델은 public 기본 생성자를 가진 C# 클래스여야 합니다. `LiteDB.Get<T>()`와 `LiteDB.Query<T>()`는 조회 결과 컬럼 이름과 모델의 public set 가능한 프로퍼티 또는 public 필드를 매칭합니다.

```csharp
using Achieve.Database;
using SQLite;

[Table("ItemData")]
public sealed class ItemData : IDataBase
{
    [PrimaryKey]
    public int Id { get; set; }
    public string Name { get; set; }
    public int Price { get; set; }
    public float Weight { get; set; }
}
```

## IDataBase

`GetList`, `Exist`, `TryGetValue`는 `IDataBase`를 기준으로 `Id`와 `Name`을 기대합니다.

```csharp
public interface IDataBase
{
    int Id { get; set; }
    string Name { get; set; }
}
```

## 지원 Attribute

| Attribute | 설명 |
| --- | --- |
| `[Table("Name")]` | 클래스와 다른 테이블 이름을 지정 |
| `[Column("Name")]` | 멤버와 다른 컬럼 이름을 지정 |
| `[PrimaryKey]` | Primary Key 멤버를 지정 |
| `[AutoIncrement]` | 코드 생성 호환용 Attribute |
| `[Ignore]` | 매핑에서 제외 |
| `[NotNull]` | 모델 표현용 Attribute |

`[AutoIncrement]`와 `[NotNull]`은 현재 테이블 생성 API를 제공하지 않기 때문에 런타임 스키마 생성에는 사용되지 않습니다. CSV Importer나 외부 DB 도구에서 스키마를 만들 때 의미를 맞춰두는 용도입니다.

## 컬럼 매핑

기본 매핑은 대소문자를 구분하지 않습니다.

```csharp
[Column("display_name")]
public string DisplayName { get; set; }
```

set이 없는 프로퍼티와 `readonly` 필드는 매핑 대상에서 제외됩니다.
