using Azure.Identity;
using EventStore.CosmosDb;
using EventStore.Domain;
using EventStore.Domain.Events;
using EventStore.Events;
using FoodInspector.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using static System.Net.Mime.MediaTypeNames;

namespace FoodInspectorTests
{
    [TestClass]
    public class EventStoreTests
    {
        private IServiceProvider _services = null;
        private IConfiguration _configuration;

        public static IConfiguration InitConfiguration()
        {
            var config = new ConfigurationBuilder()
                .AddJsonFile("appsettings.test.json")
                .AddEnvironmentVariables();

            var configRoot = config.Build();

            config.AddAzureKeyVault(
                new Uri($"https://{configRoot["KeyVault:VaultName"]}.vault.azure.net/"),
                new DefaultAzureCredential());

            var config2 = config.Build();

            return config2;
        }

        /*public EventStoreTests(
            IServiceProvider services,
            IConfiguration configuration)
        {
            _services = services;
            _configuration = configuration;
        }*/

        [TestInitialize]
        public void TestInitialize()
        {
            var config = InitConfiguration();

            IServiceCollection serviceCollection = new ServiceCollection();
            ContainerBuilder.ConfigureServices(serviceCollection, config);

            // Add the required connection strings and settings to the global ConfigurationManager
            _services = serviceCollection.BuildServiceProvider();
            _configuration = _services.GetRequiredService<IConfiguration>();
        }

        [TestMethod]
        public async Task WriteTestEvent()
        {
            Random rnd = new Random();
            long number = rnd.Next();

            Dictionary<string, string> dict = new Dictionary<string, string>();
            dict.Add("CreateTestEventOwner", "Ando");

            //TestEvent testevent = new TestEvent(Guid.NewGuid(), "Ando", number, DateTime.Now);

            Event<TestEvent> @event = new Event<TestEvent>();
            @event.EventType = "TestEvent";
            @event.EventNumber = 0;
            @event.Version = "1.0.0.0";
            @event.id = Guid.NewGuid();
            @event.OriginatingComponentVersion = "1.0.0.0";
            @event.OriginatingComponent = "EventStoreTests.WriteTestEvent";
            @event.Created = DateTime.Now;
            @event.StreamId = Guid.NewGuid();
            @event.Data = dict;

            CreateTestEvent createtestevent = new CreateTestEvent(@event);

            ICosmosClient<TestEvent> client = _services.GetRequiredService<ICosmosClient<TestEvent>>();

            await client.WriteEvent(createtestevent);
        }
    }
}
