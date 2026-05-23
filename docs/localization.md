# Localization

`Achieve.Database.Localization` 네임스페이스에는 SQLite 테이블 데이터를 이용한 간단한 지역화 헬퍼가 포함되어 있습니다.

## 언어 설정

```csharp
LocalizationManager.SetLanguage(SystemLanguage.Korean);
```

언어가 바뀌면 `LocalizationManager.onChangeLanguage` 이벤트가 호출됩니다.

## 문자열 조회

```csharp
var text = LocalizationManager.GetString(1001);
```

명시적으로 언어를 지정할 수도 있습니다.

```csharp
var english = LocalizationManager.GetString(1001, SystemLanguage.English);
```

## 컬렉션 조회

```csharp
var collection = LocalizationManager.GetCollection(1001);
var data = collection.GetLocalizedData(SystemLanguage.Japanese);
```

지역화 테이블 모델은 `LocalizationCollection`을 기준으로 `LiteDB.Get<LocalizationCollection>(key)`가 가능해야 합니다.
