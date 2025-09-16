# Библиотека для взаимодействия с сервером заказов

Библиотека предоставляет функционал для взаимодействия с сервером заказов через HTTP с Basic-аутентификацией или через gRPC.

## Функциональность
- Получение меню с сервера
- Отправка заказов на сервер
- Поддержка двух протоколов: HTTP и gRPC
- Работа с базой данных для сохранения меню

## Установка

Добавьте ссылку на сборку в ваш проект:

```xml
<Reference Include="HttpGrpcClientLib.dll" />
```

## Конфигурация

Добавьте в файл `appsettings.json` следующие настройки:

```json
{
  "Api": {
    "Endpoint": "http://your-server-endpoint",
    "User": "username",
    "Password": "password"
  },
  "ConnectionStrings": {
    "Postgres": "Host=localhost;Port=5432;Database=smsdb;Username=postgres;Password=password"
  },
  "Logging": {
    "LogFilePath": "logs/app_"
  }
}
```

## Использование

### Инициализация клиента

```csharp
using HttpGrpcClientLib.Http;

// Создание HTTP клиента
var client = new HttpApiClient(
    "http://your-server-endpoint",
    "username",
    "password"
);
```

### Получение меню

```csharp
try
{
    var menu = await client.GetMenuAsync();
    
    foreach (var item in menu)
    {
        Console.WriteLine($"{item.Name} - {item.Price}");
    }
}
catch (HttpApiException ex)
{
    Console.WriteLine($"Ошибка: {ex.Message}");
}
```

### Отправка заказа

```csharp
var order = new HttpGrpcClientLib.Models.Order 
{ 
    Id = Guid.NewGuid().ToString() 
};

order.MenuItems.Add(new HttpGrpcClientLib.Models.OrderItem 
{ 
    Id = "5979224", 
    Quantity = 1 
});

order.MenuItems.Add(new HttpGrpcClientLib.Models.OrderItem 
{ 
    Id = "9084246", 
    Quantity = 0.408 
});

await client.SendOrderAsync(order);
```

## Модели данных

### Блюдо (Dish)

```csharp
public class Dish
{
    public string Id { get; set; }
    public string Article { get; set; }
    public string Name { get; set; }
    public double Price { get; set; }
    public bool IsWeighted { get; set; }
    public string FullPath { get; set; }
    public List<string> Barcodes { get; set; }
}
```

### Заказ (Order)

```csharp
public class Order
{
    public string Id { get; set; }
    public List<OrderItem> MenuItems { get; set; } = new List<OrderItem>();
}
```

### Элемент заказа (OrderItem)

```csharp
public class OrderItem
{
    public string Id { get; set; }
    public double Quantity { get; set; }
}
```

## Обработка ошибок

Библиотека выбрасывает исключение `HttpApiException` в случае ошибок взаимодействия с сервером.

## База данных

Библиотека автоматически создает и применяет миграции для базы данных PostgreSQL. Меню сохраняется в таблицу `Dishes`.

## gRPC поддержка

Для использования gRPC вместо HTTP, используйте соответствующую реализацию клиента (реализация должна быть предоставлена отдельно).

## Примеры запросов и ответов

### Запрос меню

**Запрос:**
```json
{
  "Command": "GetMenu",
  "CommandParameters": {
    "WithPrice": true
  }
}
```

**Ответ:**
```json
{
  "Command": "GetMenu",
  "Success": true,
  "ErrorMessage": "",
  "Data": {
    "MenuItems": [
      {
        "Id": "5979224",
        "Article": "A1004292",
        "Name": "Каша гречневая",
        "Price": 50,
        "IsWeighted": false,
        "FullPath": "ПРОИЗВОДСТВО\\Гарниры",
        "Barcodes": ["57890975627974236429"]
      }
    ]
  }
}
```

### Отправка заказа

**Запрос:**
```json
{
  "Command": "SendOrder",
  "CommandParameters": {
    "OrderId": "62137983-1117-4D10-87C1-EF40A4348250",
    "MenuItems": [
      {
        "Id": "5979224",
        "Quantity": "1"
      },
      {
        "Id": "9084246",
        "Quantity": "0.408"
      }
    ]
  }
}
```

**Ответ:**
```json
{
  "Command": "SendOrder",
  "Success": true,
  "ErrorMessage": ""
}
```

## Требования

- .NET 8.0 или выше
- PostgreSQL (для работы с базой данных)
- Доступ к серверу заказов

## Логирование

Логи сохраняются в файлы в формате `logs/app_YYYYMMDD.log`.
