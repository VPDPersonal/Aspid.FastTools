# Types — пошаговый туториал

Три взаимодополняющих способа выбрать `System.Type` из Inspector — все хранят assembly-qualified name и
резолвятся лениво:

- `SerializableType<T>` — строго типизированная обёртка; generic-аргумент ограничивает выбор типом `T` и его
  наследниками, а неявное приведение к `Type?` бесплатно.
- `[TypeSelector(typeof(Base))]` на «сыром» `string` / `string[]` — то же окно выбора на необёрнутом поле,
  когда хочется обойтись без обёртки или самому сузить список кандидатов.
- `ComponentTypeSelector` — пустой сериализуемый struct, который добавляет дропдаун выбора подтипа в Inspector
  `MonoBehaviour` и переписывает `m_Script` по месту при переключении подтипа.

Каждый урок соответствует одной секции `STEP` компонента `TypesTutorial`.

## Как открыть туториал

1. Откройте окно Welcome (**Tools → Aspid 🐍 → FastTools → Welcome**) и импортируйте сэмпл **Types**.
2. Откройте **`Scenes/TypesTutorial.unity`** и выделите GameObject **Types Tutorial**.

---

## Урок 1 — Выбор типа через `SerializableType<T>`

**Поле:** `SerializableType<Ability> _step1Ability` — предустановлено в `Dash`.

Всё объявление — одна строка, см. `Scripts/Tutorial/TypesTutorial.cs`:

```csharp
[SerializeField] private SerializableType<Ability> _step1Ability;
```

1. Кликните по полю — откроется окно выбора типа, ограниченное `Ability` и всем, что ему присваиваемо:
   сама абстрактная база `Ability` плюс её наследники `Dash`, `Fireball`, `Heal`.
2. Выберите другую запись, например `Fireball`.

- Generic-аргумент `T` ограничивает список типом `T` и его наследниками; посторонние типы не появляются.
- `SerializableType<T>` показывает кандидатов с `TypeAllow.All`, поэтому в списке есть и сам тип `T` — **в том
  числе когда он абстрактный или интерфейс**. Это универсальная ссылка на тип, поэтому база/абстракт — валидное
  значение; но если вы резолвите тип для создания экземпляра (`AddComponent` / `Activator`), выбирайте
  **конкретный** наследник, ведь абстрактный тип инстанцировать нельзя. `[TypeSelector]` на «сыром» `string`
  (урок 2) теперь использует тот же дефолт `TypeAllow.All`, так что оба ведут себя одинаково.
- Сериализуется строка **assembly-qualified name**; `.Type` выполняет `GetType()` при первом обращении и
  кеширует результат.
- `.Type` возвращает `null`, если сохранённое имя больше не резолвится (например, тип переименовали) — всегда
  проверяйте на null перед использованием.

---

## Урок 2 — `[TypeSelector]` на «сыром» `string[]`

**Поле:** `[TypeSelector(typeof(AbilityModifier))] string[] _step2ModifierTypes` — предзаполнено двумя модификаторами.

```csharp
[TypeSelector(typeof(AbilityModifier))]
[SerializeField] private string[] _step2ModifierTypes;
```

1. Каждый элемент массива рисуется как **свой отдельный picker**, ограниченный наследниками `AbilityModifier`.
2. Нажмите **+**, чтобы добавить элемент и выбрать модификатор; элементы можно переупорядочивать и удалять.

- Аннотирование обычного `string` (или `string[]`) даёт picker **без** обёртки `SerializableType<T>` —
  удобно, когда вы уже храните имя сами или хотите разные базовые ограничения для разных полей.
- `Allow` по умолчанию `TypeAllow.All`: абстрактные базы и интерфейсы показываются наравне с конкретными типами.
  Укажите `[TypeSelector(typeof(AbilityModifier), Allow = TypeAllow.None)]`, чтобы ограничить picker только конкретными типами.
- Тот же атрибут `[TypeSelector]` стоит и за полем `SerializableType` — строка это хранилище, атрибут это picker.

---

## Урок 3 — Generic-типы в пикере

**Поле:** `[TypeSelector(typeof(AbilityModifier))] string _step3GenericModifier` — пустое; выберите тип, чтобы заполнить.

```csharp
[TypeSelector(typeof(AbilityModifier))]
[SerializeField] private string _step3GenericModifier;
```

`StackModifier<T>` (см. `Scripts/Modifiers/StackModifier.cs`) — это **конкретный открытый generic** `AbilityModifier`.

1. Откройте picker — рядом с конкретными модификаторами в списке есть открытый generic **`StackModifier<T>`**.
2. Выберите `StackModifier<T>` — откроется **вторая страница** с запросом `T`. Выберите `float`.
3. Хранится **закрытый** тип `StackModifier<float>`; `Log Tutorial Lookups` его печатает
   (с `IsConstructedGenericType == true`).

- Выбор открытого generic никогда не сохраняет сам `StackModifier<>` — вы всегда приходите к закрытому,
  инстанцируемому типу после выбора `T`; `Type.GetType` резолвит это закрытое имя обратно в `Type`.
- Базовое ограничение по-прежнему действует для закрытых форм: на второй странице принимаются только те
  аргументы, при которых результат остаётся присваиваемым к `AbilityModifier`.
- **Закрытый тип поля фиксирует `T`**: ограничьте поле напрямую до `StackModifier<float>`
  (`[TypeSelector(typeof(StackModifier<float>))]`) — и `T` уже известен, picker строит тип в один шаг, без второй
  страницы. То же верно для `SerializableType<StackModifier<float>>`.

---

## Урок 4 — Обязательный тип через `[TypeSelector(Required = true)]`

**Поле:** `[TypeSelector(typeof(Ability), Required = true)] string _step4RequiredAbility` — намеренно оставлено **пустым**.

```csharp
[TypeSelector(typeof(Ability), Required = true)]
[SerializeField] private string _step4RequiredAbility;
```

1. Пока поле пустое, оно рисует инлайн-предупреждение **«Required type is not set»**.
2. Выберите любой `Ability` из дропдауна — предупреждение исчезает.

- `Required = true` помечает **незаполненное** поле: пустой `string` (или null `[SerializeReference]`)
  показывает инлайн-notice **и** засчитывается как нарушение build/CI-гейта (Settings → *SerializeReferences* →
  гейт обязательных полей: Off / Warn / Fail).
- Гейт сканирует **top-level поля** типа — `_step4RequiredAbility` таковым является, поэтому покрыт. Обязательное
  поле, вложенное в сериализуемый контейнер или массив, всё равно показывает инлайн-notice, но гейтом **не**
  видится (известное ограничение).
- `Required` ортогонален `Allow`: он валидирует *пустоту*, а не *какие* типы предлагаются.

---

## Урок 5 — Подмена компонента через `ComponentTypeSelector`

**Поле:** `EnemyBase _step5Enemy` — ссылается на GameObject **Enemy** в сцене.

`EnemyBase` объявляет единственное поле `ComponentTypeSelector` (см. `Scripts/Enemies/EnemyBase.cs`):

```csharp
[SerializeField] private ComponentTypeSelector _enemyType;
```

1. Выделите GameObject **Enemy** в Hierarchy.
2. В верхней части его Inspector раскройте **дропдаун типа** — в нём перечислены наследники объявляющего
   класса (`FastEnemy`, `TankEnemy`).
3. Переключитесь с `FastEnemy` на `TankEnemy` — `m_Script` компонента переписывается **по месту**; поле
   `Health` (объявленное в общем `EnemyBase`) сохраняет значение при замене.

- Picker автоматически находит подтипы из объявляющего класса поля — конфигурация не нужна.
- Поля переживают замену там, где новый подтип объявляет поле с **совпадающим именем**; `_speed` (FastEnemy) и
  `_armor` (TankEnemy) уникальны для каждого, поэтому сбрасываются при переключении.
- Размещайте одно поле `ComponentTypeSelector` на корневой класс, обычно вверху Inspector.

---

## Урок 6 — Резолв типов в коде

Правый клик по шапке компонента → **Log Tutorial Lookups** (работает в Edit Mode). Console покажет, как каждая
форма резолвит свой сохранённый тип:

```csharp
Type ability = _step1Ability.Type;              // SerializableType<T> — лениво, кешируется, null при ошибке
Type modifier = Type.GetType(qualifiedName);    // сырая строка — резолв вручную
Type concrete = _step5Enemy.GetType();          // подтип, на который переключили компонент
```

Реальный компонент `Scripts/Abilities/AbilitySelector.cs` использует первые две формы в `Start()`: он
`AddComponent`-ит выбранный наследник `Ability` и `Activator.CreateInstance`-ит каждый `AbilityModifier` по
его assembly-qualified name.

- `SerializableType<T>` безопасно приводится неявно (`Type t = _step1Ability;`) — picker предлагает только
  наследников `T`.
- У формы с сырой строкой нет гарантии на этапе компиляции: `Type.GetType` возвращает `null` для
  нерезолвящегося имени, поэтому проверяйте каждый вызов.

---

## Когда тип переименован или удалён

Хранимое значение — строка **assembly-qualified name**, поэтому:

- **Переименование или перенос типа** (смена namespace/сборки) делает сохранённое имя недействительным —
  `.Type` / `Type.GetType` возвращают `null`. Перевыберите поле или обновите сериализованную строку в ассете.
- **Удаление типа** оставляет поля со ссылкой на нерезолвящееся имя; picker показывает его как отсутствующее,
  пока вы не выберете валидный тип.
- `ComponentTypeSelector` резолвит подтипы «вживую» из объявляющего класса, поэтому удалённый подтип просто
  выпадает из дропдауна.

---

## Где смотреть в коде

| Файл | Что показывает |
|---|---|
| `Scripts/Tutorial/TypesTutorial.cs` | Все шесть уроков как пронумерованные поля + контекстное меню `Log Tutorial Lookups` |
| `Scripts/Abilities/AbilitySelector.cs` | `SerializableType<T>` + `[TypeSelector]` в реальном компоненте, резолв на `Start()` |
| `Scripts/Modifiers/StackModifier.cs` | Конкретный открытый generic `AbilityModifier` (`StackModifier<T>`) для пикера из урока 3 |
| `Scripts/Enemies/EnemyBase.cs` | Однострочное объявление `ComponentTypeSelector` и подтипы для подмены |
| [README_RU.md](README_RU.md) | Компактный обзор демо-сцены |
