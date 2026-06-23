var builder = DistributedApplication.CreateBuilder(args);


// --- 1. ОПИСАНИЕ ИНФРАСТРУКТУРЫ (КОНТЕЙНЕРЫ) ---

// База данных PostgreSQL для Auth.Service и Order.Service (Саги)
var postgres = builder.AddPostgres("postgres")
                      .WithPgAdmin(); // Добавит удобную панель управления БД

var authDb = postgres.AddDatabase("AuthDb");
var orderDb = postgres.AddDatabase("OrderDb");

// База данных MongoDB для Stock.Service
var mongo = builder.AddMongoDB("mongo")
                   .WithMongoExpress(); // Панель управления для Mongo

var stockDb = mongo.AddDatabase("StockDb");

// Брокер сообщений RabbitMQ для MassTransit (общение между сервисами)
var rabbitMq = builder.AddRabbitMQ("rabbitmq")
                       .WithManagementPlugin(); // Панель управления RabbitMQ


// --- 2. ОПИСАНИЕ МИКРОСЕРВИСОВ И ЗАВИСИМОСТЕЙ ---

// Сервис аутентификации
var authService = builder.AddProject<Projects.Auth_Service>("auth-service")
                         .WithReference(authDb);

// Сервис склада (управление остатками)
var stockService = builder.AddProject<Projects.Stock_Service>("stock-service")
                          .WithReference(stockDb)
                          .WithReference(rabbitMq);

// Сервис оплаты
var paymentService = builder.AddProject<Projects.Payment_Service>("payment-service")
                            .WithReference(rabbitMq);

// Сервис заказов (содержит бизнес-логику и MassTransit State Machine)
var orderService = builder.AddProject<Projects.Order_Service>("order-service")
                          .WithReference(orderDb)
                          .WithReference(rabbitMq);

// --- 3. ШЛЮЗ МАРШРУТИЗАЦИИ (API GATEWAY) ---
// Шлюз YARP должен знать адреса других сервисов, чтобы проксировать запросы
builder.AddProject<Projects.ApiGateway>("api-gateway")
       .WithReference(authService)
       .WithReference(orderService)
       .WithReference(stockService)
       .WithReference(paymentService)
       ;

builder.Build().Run();