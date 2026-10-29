# RetailCatalog

RetailCatalog — тестовый проект.
Проект представляет собой веб-приложение для работы с каталогом товаров и включает frontend и backend части.

## Технологии

### Frontend

- Angular
- TypeScript
- HTML / CSS
- HTTP Client для взаимодействия с backend API

### Backend

- .NET 10
- C#
- ASP.NET Core Web API
- Entity Framework Core
- PostgreSQL
- MediatR
- Clean Architecture
- xUnit — модульное тестирование

## Архитектура

Backend организован по слоям, каждый из которых отвечает за свою область:

- **API** — HTTP endpoints и обработка запросов.
- **Application** — сценарии использования и прикладная логика.
- **Domain** — сущности и бизнес-правила.
- **Infrastructure** — доступ к данным и реализация инфраструктурных зависимостей.
- **Unit Tests** — проверка поведения отдельных компонентов.


## Основные решения

- Разделение frontend и backend позволяет независимо развивать клиентскую и серверную части.
- Слоистая архитектура отделяет бизнес-логику от инфраструктуры и HTTP-обработки.
- Entity Framework Core используется для доступа к данным PostgreSQL.
- MediatR организует обработку прикладных запросов и команд.


