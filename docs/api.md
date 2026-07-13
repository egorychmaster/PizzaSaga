Документация публичного HTTP API

# 1. Назначение
Основным способом взаимодействия с системой является HTTP REST API через API Gateway.
Для внешнего клиента система представляет собой единый API независимо от количества внутренних сервисов, участвующих в обработке запроса.
Все обращения выполняются по HTTPS с использованием JSON.

# 2. Общие соглашения
## 2.1 Базовый URL и Версионирование
Базовый URL
https://{host}/api/v1/

Версионирование
Используется версионирование через URL.
/api/v1/orders
/api/v2/orders
Новые версии публикуются без нарушения обратной совместимости существующих контрактов.

## 2.2 Формат Данных и Типы
Формат данных
Все запросы и ответы используют JSON.
Content-Type: application/json
Accept: application/json

Идентификаторы
Все ресурсы идентифицируются GUID.
Пример:
2fbb95d2-0672-4b2f-b3fd-447d2a02d5f8

Дата и время
Все значения времени передаются в формате ISO-8601 (UTC).
Пример:
2026-07-13T14:35:18Z

Валюта
Используются коды ISO 4217.

Например:
EUR
USD

# 3. HTTP Headers
Поддерживаются следующие заголовки.

Header				Назначение
Authorization		JWT Bearer Token
Content-Type		application/json
Accept				application/json
CorrelationId		Корреляция запросов
Idempotency-Key		Идемпотентность операций создания

Каждый входящий HTTP-запрос получает TraceId, используемый для распределённой трассировки. Для объединения всех операций, относящихся к одному пользовательскому запросу, используется CorrelationId. Если клиент не передаёт CorrelationId, API Gateway генерирует его автоматически.

# 4. Аутентификация
Защита периметра осуществляется на базе централизованной JWT-аутентификации.

## 4.1 Аутентификация пользователя
Endpoint:
POST /auth/login
Request
{
  "username": "user",
  "password": "password"
}

Response 200
{
  "accessToken": "...",
  "expiresIn": 3600,
  "refreshToken": "..."
}

Ошибки:
401 Unauthorized — неверные учётные данные
400 Bad Request — некорректный формат запроса (ProblemDetails)

Все защищённые методы требуют заголовок:
Authorization: Bearer <token>

# 5. Заказ (Orders)
## 5.1 Создание заказа
Endpoint:
POST /api/v1/orders
Создаёт новый заказ и запускает его асинхронную обработку.

Request
{
  "customerId": "...",
  "items": [
    {
      "productId": "...",
      "quantity": 2
    }
  ],
  "paymentMethod": "Card",
  "currency": "EUR"
}

Response 201 Created
Location /api/v1/orders/{orderId}
{
  "orderId": "...",
  "status": "Pending",
  "createdAt": "2026-07-13T12:00:00Z"
}

Ошибки:
400 Bad Request — валидация (например, пустой список товаров)
401 Unauthorized
409 Conflict — если первый запрос еще в обработке (Concurrent Request)
500 Internal Server Error — неожиданные ошибки (ProblemDetails)

После возврата ответа обработка продолжается асинхронно.

## 5.2 Получение заказа
Endpoint:
GET /api/v1/orders/{orderId}
Response 200
{
  "orderId": "...",
  "customerId": "...",
  "status": "Completed",
  "totalAmount": 2450,
  "currency": "EUR",
  "createdAt": "...",
  "updatedAt": "..."
}

# 5.3 Список заказов
Endpoint:
GET /api/v1/orders

Query-параметры:
Параметр	Описание
status	 	фильтр по статусу (опционально)
page 		номер страницы (по умолчанию 1)
pageSize 	размер страницы (по умолчанию 20)
sort		поле сортировки
order		asc / desc

Response 200
{
  "items": [],
  "page": 1,
  "pageSize": 20,
  "totalCount": 100
}

# 5.4 Отмена заказа
Endpoint:
POST /api/v1/orders/{orderId}/cancel
Инициирует отмену заказа.

Response
202 Accepted
{
    "orderId":"...",
    "status":"Cancelling"
}

Ошибки:
404 Not Found — заказ не найден
409 Conflict — заказ в состоянии, недоступном для отмены (например, уже Completed)
401 Unauthorized

# 6. Продукты (Products)
## 6.1 Получение информации о товаре
Endpoint:
GET /api/v1/products/{productId}

## 6.2 Список товаров
Endpoint:
GET /api/v1/products

Query-параметры:
page
pageSize

Response 200
{
  "items": [],
  "page": 1,
  "pageSize": 20,
  "totalCount": 100
}

# 7. Платежи (Payments)
## 7.1 Получение информации о платеже заказа
Endpoint:
GET /api/v1/orders/{orderId}/payment

Response 200
{
  "paymentStatus":"Reserved",
  "amount":25.4,
  "orderId": "",
  "currency":"EUR",
  "method":"Card"
}

Ошибки:
404 Not Found — заказ или платёж не найден

# 8. Асинхронные операции
Некоторые операции требуют длительной обработки и не завершаются в рамках одного HTTP-запроса.
После создания заказа клиент получает идентификатор ресурса и его начальное состояние.
Для получения результата используется периодический опрос ресурса.

GET /orders/{id}

# 9. Жизненный цикл заказа
Для внешних клиентов (API/UI) жизненный цикл заказа намеренно упрощен до трех детерминированных состояний:

      ┌───────────────┐
      │    Pending    │ (Заказ создан, Сага запущена)
      └───────┬───────┘
              │
      ┌───────┴───────┐
      ▼               ▼
┌───────────┐   ┌───────────┐
│ Completed │   │ Cancelled │ (Финальные состояния)
└───────────┘   └___________┘

Соответствие Внешних статусов Внутренним состояниям Саги:
Pending: Соответствует внутренним состояниям Саги Submitted, InventoryReserved, PaymentAuthorized. Для UI это означает «Заказ обрабатывается системой».
Completed: Соответствует финальному состоянию Саги OrderCompleted. Все ресурсы успешно зафиксированы.
Cancelled: Соответствует состояниям Саги InventoryRollbackFailed, PaymentRefunded, OrderCancelled. Компенсирующие транзакции завершены, заказ аннулирован.

# 10. Идемпотентность
Для операций создания ресурсов клиент может передать заголовок
Idempotency-Key

При повторной отправке запроса с тем же ключом система возвращает ранее созданный результат без повторного выполнения операции.

# 11. Health
Endpoint:
GET /health/live
Проверка работоспособности процесса.

GET /health/ready
Проверка готовности принимать пользовательский трафик.

Response 200
{
  "status": "Healthy"
}

# 12. Наблюдаемость (Observability)
Каждый запрос получает уникальный CorrelationId.

Значение передаётся:
между HTTP-запросами;
в журналы;
в распределённые трассировки;
между внутренними сервисами.

Это позволяет восстановить полный путь выполнения пользовательской операции.

# 13. Обработка ошибок
Ошибки возвращаются в формате application/problem+json в соответствии с RFC 9457.

Пример
{
  "type": "https://httpstatuses.com/400",
  "title": "Validation error",
  "status": 400,
  "detail": "Validation failed.",
  "instance": "/api/v1/orders",
  "errors": {
    "items[0].quantity": [
      "Quantity must be greater than zero."
    ]
  }
}

Основные коды ответа:
Код	Назначение
200	OK
201	Created
202	Accepted
204	No Content
400	Bad Request валидация/некорректный запрос
401	Unauthorized аутентификация
403	Forbidden авторизация
404	Not Found ресурс не найден
409	Conflict конфликт (например, оптимистичная блокировка)
500	Internal Server Error внутренняя ошибка сервера

# 14. OpenAPI
Каждый микросервис публикует собственную OpenAPI-спецификацию (Swagger), используемую для разработки, тестирования и документирования REST API.
API Gateway не агрегирует OpenAPI-документацию сервисов.

# 15. Эволюция API
Публичный API развивается независимо от внутренней реализации системы.

Изменения механизмов обработки запросов, координации бизнес-процессов или обмена сообщениями не требуют изменения внешнего контракта, пока сохраняются структура ресурсов, их семантика и формат ответов.