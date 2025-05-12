using Microsoft.Extensions.Configuration;
using MongoDB.Bson;
using MongoDB.Bson.IO;
using MongoDB.Driver;
using Serilog;
using test_mongo_api_app.models;

class Program
{
	static async Task Main(string[] args)
	{
		// Загрузка конфигурации
		var configuration = new ConfigurationBuilder()
			.SetBasePath(AppContext.BaseDirectory)
			.AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
			.Build();

		// Настройка Serilog
		Log.Logger = new LoggerConfiguration()
			.ReadFrom.Configuration(configuration)
			.CreateLogger();

		try
		{
			// Получение настроек MongoDB
			var mongoSection = configuration.GetSection("MongoDbSettings");
			var connectionString = mongoSection["ConnectionString"];
			var databaseName = mongoSection["DatabaseName"];

			var client = new MongoClient(connectionString);
			var database = client.GetDatabase(databaseName);

			Log.Information("Подключено к MongoDB. База данных: {Database}", databaseName);

			// Список коллекций
			var collectionsCursor = await database.ListCollectionNamesAsync();
			var collections = await collectionsCursor.ToListAsync();

			Log.Information("Существующие коллекции:");
			foreach (var name in collections)
			{
				Console.WriteLine($" - {name}");
			}

			Console.WriteLine("Хотите создать новую коллекцию? (y/n):");
			var answer = Console.ReadLine()?.Trim().ToLower();

			string targetCollectionName;

			if (answer == "y")
			{
				Console.WriteLine("Введите имя новой коллекции:");
				targetCollectionName = Console.ReadLine()?.Trim();

				if (string.IsNullOrWhiteSpace(targetCollectionName))
				{
					Log.Warning("Имя коллекции не указано.");
					return;
				}

				if (collections.Contains(targetCollectionName))
				{
					Log.Warning("Коллекция '{Collection}' уже существует.", targetCollectionName);
				}
				else
				{
					await database.CreateCollectionAsync(targetCollectionName);
					Log.Information("Коллекция '{Collection}' создана.", targetCollectionName);
				}
			}
			else
			{
				Console.WriteLine("Введите имя коллекции, которую хотите просмотреть:");
				targetCollectionName = Console.ReadLine()?.Trim();

				if (!collections.Contains(targetCollectionName))
				{
					Log.Warning("Коллекция '{Collection}' не существует.", targetCollectionName);
					return;
				}

				var collection = database.GetCollection<BsonDocument>(targetCollectionName);
				var docs = await collection.Find(new BsonDocument()).ToListAsync();

				if (docs.Count == 0)
				{
					Log.Information("Коллекция пуста.");
				}
				else
				{
					Log.Information("Содержимое коллекции '{Collection}':", targetCollectionName);
					foreach (var doc in docs)
					{
						Console.WriteLine(doc.ToJson());
					}
				}
			}

			// Добавление документов в коллекцию
			var targetCollection = database.GetCollection<QueuesEntity>(targetCollectionName);

			while (true)
			{
				Console.WriteLine("Хотите добавить новый документ в коллекцию? (y/n):");
				var addAnswer = Console.ReadLine()?.Trim().ToLower();

				if (addAnswer != "y") break;

				Console.WriteLine("Введите имя входящей очереди (inQueueName):");
				var inQueue = Console.ReadLine()?.Trim();

				Console.WriteLine("Введите имя исходящей очереди (outQueueName):");
				var outQueue = Console.ReadLine()?.Trim();

				if (!string.IsNullOrWhiteSpace(inQueue) || !string.IsNullOrWhiteSpace(outQueue))
				{
					var entity = new QueuesEntity
					{
						InQueueName = inQueue,
						OutQueueName = outQueue,
						CreatedBy = Environment.UserName,
						IpAddress = "127.0.0.1", // при необходимости можно получить внешний IP
						UserAgent = "ConsoleApp/1.0",
						CorrelationId = Guid.NewGuid().ToString()
					};

					await targetCollection.InsertOneAsync(entity);
					Log.Information("Документ успешно добавлен в коллекцию '{Collection}'.", targetCollectionName);

					var allDocs = await targetCollection.Find(Builders<QueuesEntity>.Filter.Empty).ToListAsync();
					Console.WriteLine("Теперь коллекция состоит из:");
					foreach (var doc in allDocs)
					{
						Console.WriteLine(doc.ToJson(new JsonWriterSettings { Indent = true }));
					}
				}
				else
				{
					Log.Warning("Оба значения очередей пустые. Документ не добавлен.");
				}
			}
		}
		catch (Exception ex)
		{
			Log.Fatal(ex, "Произошла критическая ошибка.");
		}
		finally
		{
			Log.CloseAndFlush();
		}
	}
}
