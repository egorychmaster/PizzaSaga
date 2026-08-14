var builder = DistributedApplication.CreateBuilder(args);

var jwtSecret = builder.AddParameter("JwtSecretKey", secret: true);

// --- 1. ОПИСАНИЕ ИНФРАСТРУКТУРЫ (КОНТЕЙНЕРЫ) ---

// База данных PostgreSQL для Auth.Service и Order.Service (Саги)
var postgres = builder.AddPostgres("postgres")
    .WithPgAdmin();     // Добавит удобную панель управления БД

var orderDb = postgres.AddDatabase("OrderDb");
var stockDb = postgres.AddDatabase("StockDb");

// Брокер сообщений RabbitMQ для MassTransit (общение между сервисами)
var rabbitMq = builder.AddRabbitMQ("rabbitmq")
    .WithManagementPlugin();    // Панель управления RabbitMQ

// --- 2. ОПИСАНИЕ МИКРОСЕРВИСОВ И ЗАВИСИМОСТЕЙ ---

/*
 * WaitFor - это метод, который управляет порядком запуска ресурсов в вашем приложении. 
 * Он гарантирует, что один ресурс не запустится, пока другой не будет готов.
 * Aspire считает ресурс готовым, когда:
 * 1. Контейнер запущен (для контейнерных ресурсов)
 * 2. Health check прошел успешно (если он есть)
 * 3. Ресурс в состоянии Running
 * 
 * Порядок запуска:
 * 1. Запускается контейнер PostgreSQL
 * 2. PostgreSQL становится здоровым (health check успешен)
 * 3. Только после этого запускается order-api
 */

// Сервис аутентификации
var authService = builder.AddProject<Projects.Auth_Api>("auth-api")
    .WithEnvironment("Jwt__SecretKey", jwtSecret);

// Сервис склада (управление остатками)
var stockService = builder.AddProject<Projects.Stock_Api>("stock-api")
    .WithReference(stockDb)
    .WithReference(rabbitMq)
    .WaitFor(stockDb)   // Сервис не запустится, пока БД не готова. Можно ждать несколько ресурсов.
    .WaitFor(rabbitMq);

// Сервис оплаты
var paymentService = builder.AddProject<Projects.Payment_Api>("payment-api")
    .WithReference(rabbitMq)
    .WaitFor(rabbitMq);

// Сервис заказов (содержит бизнес-логику и MassTransit State Machine)
var orderService = builder.AddProject<Projects.Order_Api>("order-api")
    .WithEnvironment("Jwt__SecretKey", jwtSecret)
    .WithReference(orderDb)
    .WithReference(rabbitMq)
    .WaitFor(orderDb)       // Сервис не запустится, пока БД не готова. Будет использовать встроенный health check PostgreSQL.
    .WaitFor(rabbitMq);     // Сервис не запустится, пока брокер сообщений не готов. Будет использовать встроенный health check RabbitMQ.

// --- 3. ШЛЮЗ МАРШРУТИЗАЦИИ (API GATEWAY) ---
// Шлюз YARP должен знать адреса других сервисов, чтобы проксировать запросы
builder.AddProject<Projects.PizzaSaga_ApiGateway>("api-gateway")
    .WithEnvironment("Jwt__SecretKey", jwtSecret)
    .WithReference(authService)
    .WithReference(orderService)
    .WithReference(stockService)
    .WithReference(paymentService)
    .WaitFor(authService)
    .WaitFor(orderService)
    .WaitFor(stockService)
    .WaitFor(paymentService);

builder.Build().Run();