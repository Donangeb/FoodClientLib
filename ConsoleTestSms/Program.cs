using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.EntityFrameworkCore;
using HttpGrpcClientLib.Http;
using ConsoleAppSms.SimpleLogger;
using ConsoleAppSms.Data;

class Program
{
    private static IConfiguration _config = null!;
    private static ILogger<Program> _logger = null!;

    static async Task Main()
    {
        // Загружаем конфигурацию и настраиваем логирование
        Configure();

        try
        {
            // Инициализация БД с миграциями
            await InitDatabaseAsync();

            // Получение меню с сервера
            using var client = new HttpApiClient(
                _config["Api:Endpoint"]!,
                _config["Api:User"]!,
                _config["Api:Password"]!
            );

            var menu = await client.GetMenuAsync();
            if (menu.Count == 0)
            {
                Console.WriteLine("Сервер вернул пустое меню");
                return;
            }

            // Записываем в БД
            await SaveMenuToDbAsync(menu);

            Console.WriteLine("=== Меню ===");
            foreach (var item in menu)
            {
                Console.WriteLine($"{item.Name} – {item.Article} – {item.Price}");
            }

            // Создаём заказ
            var order = new HttpGrpcClientLib.Models.Order { Id = Guid.NewGuid().ToString() };

            // Ввод позиций
            while (true)
            {
                Console.WriteLine("Введите заказ (формат: Код:Кол-во;Код2:Кол-во2):");
                var input = Console.ReadLine();
                if (string.IsNullOrWhiteSpace(input)) continue;

                var parts = input.Split(';', StringSplitOptions.RemoveEmptyEntries);
                bool valid = true;
                order.MenuItems.Clear();

                foreach (var p in parts)
                {
                    var kv = p.Split(':');
                    if (kv.Length != 2 ||
                        !double.TryParse(kv[1], out var qty) ||
                        qty <= 0)
                    {
                        valid = false;
                        break;
                    }

                    var code = kv[0];
                    var menuItem = menu.FirstOrDefault(m => m.Article == code);
                    if (menuItem == null)
                    {
                        valid = false;
                        break;
                    }

                    order.MenuItems.Add(new HttpGrpcClientLib.Models.OrderItem { Id = menuItem.Id, Quantity = qty });
                }

                if (!valid)
                {
                    Console.WriteLine("Ошибка ввода, попробуйте снова.");
                    continue;
                }

                break;
            }

            // Отправляем заказ
            await client.SendOrderAsync(order);

            // Успех
            Console.WriteLine("УСПЕХ");
        }
        catch (HttpApiException ex)
        {
            Console.WriteLine($"Ошибка API: {ex.Message}");
            _logger.LogError(ex, "Ошибка при работе с сервером");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Ошибка: {ex.Message}");
            _logger.LogError(ex, "Необработанное исключение");
        }
    }

    static void Configure()
    {
        _config = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json", optional: false)
            .Build();

        var logFile = _config["Logging:LogFilePath"] + DateTime.Now.ToString("yyyyMMdd") + ".log";

        using var loggerFactory = LoggerFactory.Create(builder =>
        {
            builder.AddConsole();
            builder.AddProvider(new SimpleFileLoggerProvider(logFile));
        });

        _logger = loggerFactory.CreateLogger<Program>();
    }

    static async Task InitDatabaseAsync()
    {
        var optionsBuilder = new DbContextOptionsBuilder<ApplicationDbContext>();
        optionsBuilder.UseNpgsql(_config.GetConnectionString("Postgres"));

        using var context = new ApplicationDbContext(optionsBuilder.Options);

        // Применяем миграции
        await context.Database.MigrateAsync();
    }

    public static ConsoleAppSms.Data.Dish ConvertToDataDish(HttpGrpcClientLib.Models.Dish source)
    {
        return new ConsoleAppSms.Data.Dish
        {
            Id = source.Id,
            Article = source.Article,
            Name = source.Name,
            Price = source.Price,
            IsWeighted = source.IsWeighted,
            FullPath = source.FullPath,
            Barcodes = source.Barcodes?.ToList() ?? new List<string>()
        };
    }

    static async Task SaveMenuToDbAsync(System.Collections.Generic.IReadOnlyList<HttpGrpcClientLib.Models.Dish> menu)
    {
        var optionsBuilder = new DbContextOptionsBuilder<ApplicationDbContext>();
        optionsBuilder.UseNpgsql(_config.GetConnectionString("Postgres"));

        using var context = new ApplicationDbContext(optionsBuilder.Options);

        foreach (var item in menu)
        {
            var existingDish = await context.Dishes.FindAsync(item.Id);
            if (existingDish != null)
            {
                // Обновляем существующую запись
                existingDish.Article = item.Article;
                existingDish.Name = item.Name;
                existingDish.Price = item.Price;
                context.Dishes.Update(existingDish);
            }
            else
            {
                var httpDish = new HttpGrpcClientLib.Models.Dish();
                var dataDish = ConvertToDataDish(httpDish);
                // Добавляем новую запись
                context.Dishes.Add(dataDish);
            }
        }

        await context.SaveChangesAsync();
    }
}