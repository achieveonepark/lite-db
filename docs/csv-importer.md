# CSV Importer

Editor 메뉴에서 CSV를 DB 테이블로 넣고 모델 클래스를 생성할 수 있습니다.

```text
GameFramework/Data/CsvImporter
```

## CSV를 DB에 삽입

1. SQLite DB 파일을 선택하거나 새로 만듭니다.
2. CSV 파일을 선택합니다.
3. 테이블 이름은 CSV 파일 이름으로 자동 지정됩니다.
4. 미리보기로 헤더와 일부 행을 확인합니다.
5. `Insert!` 버튼을 눌러 DB에 삽입합니다.

첫 번째 CSV 행은 컬럼 헤더로 사용됩니다. 빈 헤더가 있거나 행의 필드 수가 헤더 수와 다르면 예외가 발생합니다.

## C# 클래스 생성

`Generate C# Class` 버튼을 누르면 CSV 컬럼을 기반으로 데이터 모델 클래스를 생성합니다.

생성 위치:

```text
Assets/Runtime/DataModel/{TableName}.cs
```

생성 규칙:

* `id` 컬럼은 `[PrimaryKey, AutoIncrement]`가 붙습니다.
* 정수로 모두 해석되면 `int`
* 정수가 아니지만 실수로 해석되면 `float`
* 그 외에는 `string`

생성된 클래스는 프로젝트 규칙에 맞게 namespace나 타입을 조정해서 사용하면 됩니다.
