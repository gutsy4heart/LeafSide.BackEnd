# LeafSide Backend API

REST API для интернет-магазина книг LeafSide, построенный на ASP.NET Core 8.0 с использованием Clean Architecture.

## 🏗 Архитектура

Проект следует принципам Clean Architecture и разделен на следующие слои:

- **LeafSide.API** - Web API слой с контроллерами и middleware
- **LeafSide.Application** - Слой приложения с бизнес-логикой и сервисами
- **LeafSide.Domain** - Доменный слой с сущностями и интерфейсами
- **LeafSide.Infrastructure** - Инфраструктурный слой с реализациями репозиториев и сервисов

## 🚀 Технологический стек

- **ASP.NET Core 8.0** - основной фреймворк
- **Entity Framework Core 8.0** - ORM для работы с PostgreSQL
- **PostgreSQL 16** - база данных
- **JWT Bearer Authentication** - аутентификация и авторизация
- **Swagger/OpenAPI** - документация API
- **Docker** - контейнеризация базы данных

## 📁 Структура проекта

```
LeafSide-backend/
├── LeafSide.API/                    # Web API
│   ├── Controllers/                 # API контроллеры
│   │   ├── AccountController.cs     # Аутентификация
│   │   ├── BooksController.cs       # Управление книгами
│   │   ├── CartController.cs        # Корзина покупок
│   │   ├── OrdersController.cs      # Заказы
│   │   ├── AdminBooksController.cs  # Админ: книги
│   │   ├── AdminUsersController.cs  # Админ: пользователи
│   │   └── AdminOrdersController.cs # Админ: заказы
│   ├── Requests/                    # DTO для входящих запросов
│   │   ├── Account/                 # Запросы аутентификации
│   │   ├── Books/                   # Запросы для книг
│   │   ├── Cart/                    # Запросы корзины
│   │   └── Orders/                  # Запросы заказов
│   ├── Responses/                   # DTO для ответов
│   └── Services/                    # API сервисы
├── LeafSide.Application/            # Слой приложения
│   ├── DTOs/                        # Data Transfer Objects
│   └── Services/                    # Бизнес-логика
│       ├── Abstract/                # Интерфейсы сервисов
│       └── Concrete/                # Реализации сервисов
├── LeafSide.Domain/                 # Доменный слой
│   ├── Entities/                    # Сущности предметной области
│   │   ├── Book.cs                  # Книга
│   │   ├── Cart.cs                  # Корзина
│   │   ├── CartItem.cs              # Элемент корзины
│   │   ├── Order.cs                 # Заказ
│   │   └── OrderItem.cs             # Элемент заказа
│   ├── Enums/                       # Перечисления
│   │   └── UserRole.cs              # Роли пользователей
│   ├── Repositories/                # Интерфейсы репозиториев
│   └── Services/                    # Доменные сервисы
└── LeafSide.Infrastructure/         # Инфраструктурный слой
    ├── Data/                        # Контекст БД и репозитории
    │   ├── AppDbContext.cs          # Контекст Entity Framework
    │   └── Repository/               # Реализации репозиториев
    ├── Identity/                    # Пользователи и роли
    │   └── AppUser.cs               # Пользователь системы
    ├── Migrations/                  # Миграции базы данных
    └── Services/                    # Реализации сервисов
        └── JwtTokenService.cs       # JWT токены
```

## 🗄 Модель данных

### Основные сущности

#### Book (Книга)
- `Id` - уникальный идентификатор
- `Title` - название книги
- `Author` - автор
- `Description` - описание
- `Genre` - жанр
- `Publishing` - издательство
- `Created` - год издания
- `Price` - цена
- `ImageUrl` - URL изображения
- `Isbn` - ISBN код
- `Language` - язык
- `PageCount` - количество страниц
- `IsAvailable` - доступность для покупки

#### Order (Заказ)
- `Id` - уникальный идентификатор
- `UserId` - ID пользователя
- `Status` - статус заказа (Pending, Processing, Shipped, Delivered, Cancelled)
- `TotalAmount` - общая сумма
- `ShippingAddress` - адрес доставки
- `CustomerName` - имя клиента
- `CustomerEmail` - email клиента
- `CustomerPhone` - телефон клиента
- `Notes` - заметки
- `Items` - элементы заказа

#### Cart (Корзина)
- `Id` - уникальный идентификатор
- `UserId` - ID пользователя
- `Items` - элементы корзины

## 🔧 Настройка и запуск

### Предварительные требования
- .NET 8.0 SDK
- Docker и Docker Compose
- PostgreSQL (опционально, можно использовать Docker)

### 1. Запуск базы данных
```bash
# Запуск PostgreSQL в Docker
docker-compose up -d

# Проверка статуса
docker-compose ps
```

### 2. Настройка подключения к БД
Отредактируйте `appsettings.Development.json`:
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Port=5432;Database=leafsidedb;Username=leafuser;Password=leafpass"
  }
}
```

### 3. Восстановление зависимостей
```bash
dotnet restore
```

### 4. Применение миграций
```bash
# Создание миграции (если нужно)
dotnet ef migrations add InitialCreate --project LeafSide.Infrastructure --startup-project LeafSide.API

# Применение миграций
dotnet ef database update --project LeafSide.Infrastructure --startup-project LeafSide.API
```

### 5. Запуск приложения
```bash
dotnet run --project LeafSide.API
```

API будет доступен по адресам:
- HTTPS: `https://localhost:7000`
- HTTP: `http://localhost:5000`
- Swagger: `https://localhost:7000/swagger`

## 📚 API Документация

### Аутентификация
Все эндпоинты, кроме публичных, требуют JWT токен в заголовке `Authorization: Bearer <token>`

#### Публичные эндпоинты
- `GET /api/books` - получить все книги
- `GET /api/books/{id}` - получить книгу по ID
- `POST /api/account/register` - регистрация пользователя
- `POST /api/account/login` - авторизация

#### Эндпоинты для авторизованных пользователей
- `GET /api/cart` - получить корзину
- `POST /api/cart/add` - добавить товар в корзину
- `PUT /api/cart/update` - обновить количество товара
- `DELETE /api/cart/remove/{bookId}` - удалить товар из корзины
- `GET /api/orders` - получить заказы пользователя
- `POST /api/orders` - создать заказ
- `GET /api/orders/{id}` - получить заказ по ID
- `GET /api/profile` - получить профиль пользователя
- `PUT /api/profile` - обновить профиль

#### Эндпоинты для администраторов
- `POST /api/books` - создать книгу
- `PUT /api/books/{id}` - обновить книгу
- `DELETE /api/books/{id}` - удалить книгу
- `GET /api/admin/users` - получить всех пользователей
- `PUT /api/admin/users/{id}` - обновить пользователя
- `GET /api/admin/orders` - получить все заказы
- `PUT /api/admin/orders/{id}` - обновить статус заказа

### Примеры запросов

#### Регистрация
```bash
curl -X POST "https://localhost:7000/api/account/register" \
  -H "Content-Type: application/json" \
  -d '{
    "email": "user@example.com",
    "password": "Password123!",
    "firstName": "Иван",
    "lastName": "Иванов"
  }'
```

#### Авторизация
```bash
curl -X POST "https://localhost:7000/api/account/login" \
  -H "Content-Type: application/json" \
  -d '{
    "email": "user@example.com",
    "password": "Password123!"
  }'
```

#### Получение книг
```bash
curl -X GET "https://localhost:7000/api/books"
```

## 🧪 Тестирование

### Запуск тестов
```bash
dotnet test
```

### Запуск с покрытием кода
```bash
dotnet test --collect:"XPlat Code Coverage"
```

## 🔒 Безопасность

- JWT токены для аутентификации
- Роли пользователей (User, Admin)
- Валидация входных данных
- CORS настройки для фронтенда
- HTTPS в production

## 📝 Конфигурация

### appsettings.json
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Port=5432;Database=leafsidedb;Username=leafuser;Password=leafpass"
  },
  "JwtSettings": {
    "SecretKey": "your-secret-key-here",
    "Issuer": "LeafSide",
    "Audience": "LeafSide",
    "ExpiryMinutes": 60
  },
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning"
    }
  }
}
```

## 🐳 Docker

### Запуск только базы данных
```bash
docker-compose up -d db
```

### Остановка
```bash
docker-compose down
```

### Очистка данных
```bash
docker-compose down -v
```

## 📊 Мониторинг

- Health Check endpoint: `/health`
- Swagger UI: `/swagger`
- Логирование через встроенную систему .NET

## 🤝 Разработка

### Создание новой миграции
```bash
dotnet ef migrations add MigrationName --project LeafSide.Infrastructure --startup-project LeafSide.API
```

### Откат миграции
```bash
dotnet ef database update PreviousMigrationName --project LeafSide.Infrastructure --startup-project LeafSide.API
```

### Генерация контроллера из Entity
```bash
dotnet aspnet-codegenerator controller -name BooksController -api -m Book -dc AppDbContext -outDir Controllers
```
