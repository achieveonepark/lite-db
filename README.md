
# LiteDB

게임에서 사용하는 테이블 데이터를 SQLite로 관리하면서 내부적으로 QUERY의 WHERE절을 이용해 데이터를 반환합니다.

## 사용하기 전 설정하기
1. [DB Browser](https://sqlitebrowser.org/)을 설치합니다.<br>![New Database](./Document~/img.png)
2. 새로운 데이터베이스를 생성합니다.
3. Create Table을 클릭하여 테이블 및 변수를 추가합니다. 변수 추가에 대한 상세는 아래를 참조해주세요.
4. 생성한 테이블에 데이터를 추가합니다.

### SQLite 테이블 생성하기
- Type

| SQLite Type | C# Type            |
|-------------|--------------------|
| INTEGER     | int, long          |
| REAL        | double, float      |
| TEXT        | string, enum, bool |
| BLOB        | byte[]             |

- **PK** : Primary Key, 기본 키이며 이 패키지에서는 `Id`를 PK로 설정
- **NN** : Not Null, bool 처럼 NULL 값을 허용하지 않는 값일 경우 설정
- **AI** : Auto Increment, 정수형 열에서 사용되며, 새로운 행이 추가될 때마다 값이 자동으로 1씩 증가함.<br>일반적으로 기본 키와 함께 사용.
- **U** : Unsigned, 숫자 데이터 타입에서 사용되며 음수를 허용하지 않는 0 이상의 값

### 테이블 데이터 클래스 생성하기

> CsvImporter에서 Code Generator를 지원하니 원클릭으로도 테이블데이터 클래스를 만들어보세요.

위와 같이 값을 설정했다면, 유니티에서도 받아와서 사용 할 클래스를 생성해주어야 합니다.

```csharp
using Achieve.Database;
using Unity.VisualScripting.Dependencies.Sqlite;

[Table("TowerData")]
public class UnitData : IDataBase
{
    // Id로 Query에서 조회하기 때문에 이 Attribute와 PK 체크가 꼭 되어야 함
    [PrimaryKey, AutoIncrement]
    public int Id { get; set; }
    public string Name { get; set; }
    public double Attack { get; set; }
    public double Defense { get; set; }
    public double HP { get; set; }
}
```

## Quick Start

```csharp
LiteDB.Initialize($"{Application.persistentDataPath}/secure/data.db"); // Path

var data = LiteDB.Get<Quest>(1);

if (LiteDB.TryGetValue<Quest, int>("Quest", 1, out var quest))
{
    var reward = quest.reward;
}

// 존재한다면 Id가 1~10인 값들을 List로 불러 옴
var list = LiteDB.GetList<Quest>(1, 10);

if(LiteDB.Exist<Quest>(1))
{
    // 존재한다!
}
```
