
# LiteDB

## Adding Data to the Database

1. Click **GameFramework > Data > CsvImporter** at the top of the editor.
2. Cache the db file and csv file, then hit **Insert!**

## Quick Start

```csharp
LiteDB.Initialize($"{Application.dataPath}/Resources/data.db"); // Path

var a = LiteDB.Get<Quest>(1);
if (LiteDB.TryGetValue<Quest, int>("Quest", 1, out var quest))
{
    var reward = quest.reward;
}
```
