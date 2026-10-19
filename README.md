# ReservePoint — сервис бронирования

Микросервис бронирования корпоративных ресурсов (переговорные, рабочие места и т.д.) в составе системы **Reservio**.

## О системе

Reservio — система внутреннего бронирования ресурсов на микросервисной архитектуре. Каждый сервис в своём репозитории и со своей БД PostgreSQL:

| Сервис | Стек | Назначение |
|---|---|---|
| organizations | Go, Echo | организации и политики бронирования |
| users | C#, ASP.NET Core | пользователи, роли, права (RBAC) |
| resources | C#, ASP.NET Core | каталог ресурсов |
| **booking** (этот репозиторий) | C#, ASP.NET Core | бронирование ресурсов |

Единый фронтенд (React + TypeScript) для всех сервисов: [organization-bookings](https://github.com/MLeontev/organization-bookings). Аутентификация — Keycloak (JWT).

## Что делает сервис

- создаёт групповые бронирования (несколько ресурсов на один интервал) и отменяет их;
- ищет свободные ресурсы, показывает занятые слоты и расписание;
- проверяет конфликты по времени и политики организации: окно бронирования, лимит активных броней, максимальная длительность;
- проверяет права через сервис users, получает политики из organizations и данные ресурсов из resources.

## Стек и архитектура

.NET 10, EF Core + PostgreSQL 16, FluentResults, Swagger, Docker. Clean Architecture:

```
ReservePoint.API             — контроллеры, настройка приложения
ReservePoint.Application     — бизнес-логика, DTO, интерфейсы
ReservePoint.Domain          — BookingGroup, Booking, BookingPolicySnapshot
ReservePoint.Infrastructure  — БД, репозитории, клиенты других сервисов
```

`BookingGroup` (пользователь, организация, интервал, статус, снимок политики) содержит набор `Booking` — по одной брони на ресурс.

## API

Базовый путь `/v1`, все методы требуют JWT, время — в UTC.

| Метод | Путь | Описание |
|---|---|---|
| GET | `/v1/bookings` | бронирования организации (фильтры: период, статус) |
| GET | `/v1/bookings/my` | мои бронирования |
| GET | `/v1/bookings/{id}` | бронирование по id |
| POST | `/v1/bookings` | создать групповое бронирование |
| DELETE | `/v1/bookings/{id}` | отменить бронирование |
| GET | `/v1/resources/available` | свободные ресурсы на период |
| GET | `/v1/resources/{id}/busy-slots` | занятые слоты ресурса |
| GET | `/v1/resources/schedule` | расписание ресурсов |

Все методы принимают `organizationId`. Ошибки бизнес-правил — `400` с `{ "description": "..." }`.

## Запуск

```bash
docker network create booking-network   # один раз
cd docker
docker compose up -d --build
```

Параметры (БД, Keycloak, адреса сервисов, флаги `USE_FAKE_*` для заглушек) задаются в `docker/.env`. Миграции применяются при старте.

## CI/CD (моя часть)

Деплой настроен через GitHub Actions: при пуше в `main` workflow подключается к серверу по SSH, обновляет код и пересобирает контейнеры.

`.github/workflows/deploy.yml`:

```yaml
on:
  push:
    branches: [ main ]

jobs:
  deploy:
    runs-on: ubuntu-latest
    steps:
      - name: Deploy via SSH
        uses: appleboy/ssh-action@v1.2.5
        with:
          host: ${{ secrets.SSH_HOST }}
          username: ${{ secrets.SSH_USER }}
          password: ${{ secrets.SSH_PASSWORD }}
          script: |
            cd ${{ secrets.APP_PATH }}
            git pull
            docker compose up -d --build
```

**Секреты** (Settings → Secrets and variables → Actions): `SSH_HOST`, `SSH_USER`, `SSH_PASSWORD`, `APP_PATH`.

**Структура на сервере:** каждый сервис в своей папке, для booking `APP_PATH=/var/www/booking/booking`.

```
/var/www/booking/
├── booking/
├── users/
├── resources/
└── organizations/
```

**Перед первым деплоем** репозиторий нужно один раз клонировать на сервере вручную и создать `.env`:

```bash
cd /var/www/booking
git clone <url-репозитория> booking
```

Запросы к сервисам маршрутизируются через NGINX (HTTPS, Let's Encrypt): booking доступен по префиксу `/api/booking`.
