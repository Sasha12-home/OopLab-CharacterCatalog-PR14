# Дизайн-документ проєкту: Каталог персонажів

**Практичні заняття 13–14.** Командний інтерфейс та Бізнес-логіка додатку  
**Тема проєкту:** Тема 1 — Графічний каталог персонажів  
**Зовнішнє джерело даних:** `https://rickandmortyapi.com/api/character`  
**Стек технологій:** C# / .NET 8.0, System.Text.Json, HttpClient

---

## 1. Загальна архітектура додатку

Проєкт побудований за **тришаровою архітектурою (Layered Architecture)** з інверсією залежностей (DIP), де ядро бізнес-логіки (`OopLab.Core`) повністю відокремлене від консолі та мережевого транспорту:

```
┌──────────────────────────────────────────────────────────┐
│           ШАР ПРЕДСТАВЛЕННЯ (CLI / майбутній GUI)        │
│  OopLab.Cli: CliFacade, ArgParser, ICommandStrategy,     │
│              ConsoleCharacterRenderer, CliGraphic        │
└────────────────────────────┬─────────────────────────────┘
                             │ споживає CharacterViewModel
                             ▼
┌──────────────────────────────────────────────────────────┐
│                 ШАР БІЗНЕС-ЛОГІКИ (CORE)                 │
│  OopLab.Core: CharacterProfile, CharacterViewModel,      │
│               RickAndMortyApiJsonParser,                 │
│               CharacterViewModelMapper,                  │
│               CharacterFilterCriteria,                   │
│               CharacterCatalogService                    │
└────────────────────────────┬─────────────────────────────┘
                             ▲ реалізує контракти Core
                             │
┌────────────────────────────┴─────────────────────────────┐
│                ШАР ПОСТІЙНОСТІ ТА МЕРЕЖІ (DATA)          │
│  OopLab.Data: JsonCharacterRepository (локальний JSON),  │
│               RickAndMortyApiClient (REST API клієнт)    │
└──────────────────────────────────────────────────────────┘
```

---

## 2. Шар бізнес-логіки (CORE) — Практичне заняття 14

### 2.1. Конвеєр обробки даних (JSON → Domain Model → ViewModel)

Бізнес-логіка перетворює пагіновані JSON-відповіді від `https://rickandmortyapi.com/api/character` у типізовану доменну модель `CharacterProfile`, а потім формує `CharacterViewModel` з усіма візуальними метаданими, необхідними для графічного інтерфейсу (GUI):

```
[JSON від rickandmortyapi.com/api/character]
          │
          ▼  (RickAndMortyApiJsonParser)
[CharacterProfile — Доменна модель]
          │
          ▼  (CharacterViewModelMapper + розрахунок рейтингу та кольорів статусу)
[CharacterViewModel — Модель для GUI / CLI]
```

### 2.2. Ключові класи та патерни ядра (`OopLab.Core`)

1. **`RickAndMortyApiJsonParser` (Парсер зовнішніх JSON-даних):**
   * Десеріалізує об'єкт пагінованої сторінки (`results[]`) та окремі картки персонажів (`GET /character/{id}` і `GET /character/?name=...`).
   * Витягує атрибути: `id`, `name`, `status` (`Alive` / `Dead` / `unknown`), `species`, `type`, `gender`, вкладені об'єкти `origin.name` та `location.name`, список появ в епізодах `episode[]` та пряме посилання на аватар у форматі JPEG (`image`: `https://rickandmortyapi.com/api/character/avatar/{id}.jpeg`).
   * Обчислює рідкість (від $1\star$ до $5\star$) та базові характеристики (`BaseHp`, `BaseAtk`) на основі кількості епізодів і біологічного виду.

2. **`CharacterViewModel` та `CharacterViewModelMapper` (Патерн Adapter / Mapper):**
   * Трансформує `CharacterProfile` у готову для UI модель `CharacterViewModel`:
     * визначає HEX-колір індикатора життєвого статусу (`StatusColorHex`: `#55CC44` для `Alive`, `#D63D2E` для `Dead`, `#9E9E9E` для `unknown`);
     * формує підзаголовки, зірочки рідкості (`★★★★★`) та бейджі джерела;
     * обчислює **рейтинг популярності та сили (`PopularityScore`)** і присвоює ранг тиру (`S+`, `S`, `A`, `B`) за формулою:
       $$\text{PopularityScore} = \text{round}\big((N_{\text{episodes}} \times 14 + HP \times 0.02 + ATK \times 0.6) \times K_{\text{status}}\big)$$
       де $K_{\text{status}} = 1.1$ для живих персонажів (`Alive`) та $1.0$ для інших.

3. **`CharacterFilterCriteria` (Патерн Specification / Query Object):**
   * Інкапсулює правила фільтрації каталогу за статусом (`Status`), видом (`Species`), статтю (`Gender`), мінімальною рідкістю (`MinRarity`), пошуковим рядком (`SearchText`) та сортуванням (`name`, `rarity`, `episodes`, `power`).

4. **`CharacterCatalogService` (Фасад бізнес-логіки):**
   * Об'єднує синхронізацію з віддаленим API (`SyncWithRemoteAsync`), точковий імпорт за іменем або ID (`ImportFromRemoteAsync`), створення користувацьких персонажів (`CreateCustomCharacter`) та надання готових `CharacterViewModel` для представлення.

---

## 3. Командний інтерфейс (CLI) — Практичне заняття 13

Консольний інтерфейс реалізований у `OopLab.Cli` з використанням патернів **Facade** (`CliFacade`), **Strategy** (`ICommandStrategy`), **Command** (`Command`) та інверсії залежностей для рендерингу (`IRenderable<T>` / `IRenderer<T>`).

### Доступні команди CLI:
* `sync [--limit=20]` — одним HTTP-запитом завантажує та парсить до 20 персонажів із `https://rickandmortyapi.com/api/character`.
* `import <ім'я або ID>` — шукає та імпортує персонажа з API за іменем (наприклад, `import "Birdperson"`) або числовим ID (`import 47`).
* `list [--status=Alive] [--species=Alien] [--sort=power]` — виводить таблицю персонажів із фільтрацією та сортуванням.
* `info <ім'я або ID>` — відображає повну `CharacterViewModel`-картку з кольором статусу та прямим посиланням `Avatar JPEG URL`.
* `create` — створює нового користувацького персонажа через інтерактивний діалог.
