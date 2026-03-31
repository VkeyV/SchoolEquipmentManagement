# School Equipment Management

Веб-приложение для учета IT-оборудования в образовательной организации: реестр оборудования, инвентаризация, проблемные объекты, гарантийный контроль, аудит безопасности и управление учетными записями.

## Возможности

- реестр оборудования с фильтрами, карточкой объекта и историей изменений
- импорт оборудования из CSV и готовые примеры файлов для загрузки
- инвентаризационные сессии и фиксация расхождений
- отдельный экран проблемного оборудования
- гарантийный контроль и отдельный гарантийный отчет
- карточка локации со сводкой по статусам и интерактивной картой
- аутентификация, роли, блокировка после неудачных входов, 2FA по email
- журнал аудита безопасности
- управление учетными записями администратора

## Технологии

- `ASP.NET Core MVC`
- `.NET 8` 
- `Entity Framework Core`
- `SQL Server / SQL Server Express`
- `xUnit`
- `QuestPDF`

## Структура решения

- [SchoolEquipmentManagement.Web] UI, контроллеры, авторизация
- [SchoolEquipmentManagement.Application] — application layer, DTO, сервисы
- [SchoolEquipmentManagement.Domain] — доменные сущности и правила
- [SchoolEquipmentManagement.Infrastructure] — EF Core, БД, миграции, сидирование
- [SchoolEquipmentManagement.Tests] — unit и integration тесты
- [docs\sample-data] — примеры CSV для импорта

## Требования

- Windows 10/11
- Visual Studio 2022, .NET SDK `9.0.202`
- LocalDB, `SQLEXPRESS` или другой доступный экземпляр SQL Server

## Быстрый старт

1. Откройте решение [SchoolEquipmentManagement.sln]
2. Убедитесь, что startup project — [SchoolEquipmentManagement.Web.csproj]
3. Проверьте строку подключения в [appsettings.json]
4. Запустите приложение.

При первом старте приложение:

- применит миграции автоматически
- создаст базу `SchoolEquipmentManagementDb`, если она отсутствует
- добавит справочники, тестовые локации и тестовые учетные записи

## Настройка базы данных

По умолчанию используется:

```json
"DefaultConnection": "Server=localhost\\SQLEXPRESS;Database=SchoolEquipmentManagementDb;Trusted_Connection=True;TrustServerCertificate=True;"
```

Если у вас другой экземпляр SQL Server, измените `DefaultConnection` в [appsettings.json].

### Применение миграций вручную

Через `Package Manager Console`:

```powershell
Update-Database -Project SchoolEquipmentManagement.Infrastructure -StartupProject SchoolEquipmentManagement.Web -Context ApplicationDbContext
```

Через `dotnet ef`:

```powershell
dotnet ef database update --project .\SchoolEquipmentManagement.Infrastructure\SchoolEquipmentManagement.Infrastructure.csproj --startup-project .\SchoolEquipmentManagement.Web\SchoolEquipmentManagement.Web.csproj --context ApplicationDbContext
```

Миграции лежат в [Migrations].

## Запуск приложения

### Через Visual Studio

1. Откройте решение.
2. Назначьте `SchoolEquipmentManagement.Web` стартовым проектом.
3. Нажмите `F5` или `Ctrl+F5`.

### Через CLI

```powershell
dotnet run --project .\SchoolEquipmentManagement.Web\SchoolEquipmentManagement.Web.csproj
```

## Тестовые учетные записи

Создаются автоматически сидером в [DbSeeder.cs]:

- `admin / Admin123!`
- `tech / Tech123!`
- `responsible / Responsible123!`
- `viewer / Viewer123!`

## Email и 2FA

Настройки почты находятся в:

- [appsettings.json]
- [appsettings.Development.json]

Ключевые параметры:

- `Email.Enabled`
- `Email.SmtpHost`
- `Email.SmtpPort`
- `Email.UseSsl`
- `Email.UserName`
- `Email.Password`

## Импорт CSV

Готовые примеры файлов лежат в [docs\sample-data]
Импорт доступен из раздела оборудования через кнопку `Импорт CSV`.

## Сборка и тесты

### Обычная сборка

```powershell
dotnet build .\SchoolEquipmentManagement.sln
```

### Запуск тестов

```powershell
dotnet test .\SchoolEquipmentManagement.Tests\SchoolEquipmentManagement.Tests.csproj
```

## Минимальный сценарий проверки после запуска

1. Войдите под `admin`.
2. Откройте раздел `Оборудование`.
3. Проверьте карточку любого объекта.
4. Откройте `Локации` через ссылку из оборудования.
5. Проверьте `Проблемные объекты`, `Гарантии`, `Аудит`.
6. Зайдите в `Учетные записи` и убедитесь, что управление пользователями доступно админу.

## Лицензии и зависимости

Сторонние зависимости подключены через NuGet. Для генерации PDF используется `QuestPDF Community License`.
