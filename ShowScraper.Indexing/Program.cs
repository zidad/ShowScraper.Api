using System;
using System.IO;
using System.Threading.Tasks;
using Elasticsearch.Net;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Nest;
using Serilog;
using ShowScraper.TvMazeClient;
using ShowScraper.TvMazeClient.Models;

namespace ShowScraper.Indexer
{
    class Program
    {
        // please make sure docker-compose is running Elastic
        static async Task Main(string[] args)
        {
            Log.Logger = new LoggerConfiguration()
                .WriteTo.LiterateConsole()
                .CreateLogger();

            var configBuilder = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json")
                .AddCommandLine(args)
                .AddEnvironmentVariables();

            var configuration = configBuilder.Build();
            var services = new ServiceCollection();

            using (var provider = ConfigureServices(services))
            {
                var scraper = provider.GetService<ShowScraper>();

                await scraper.StartScraping(configuration);
            }
        }

        static ServiceProvider ConfigureServices(ServiceCollection services)
        {
            services
                .AddHttpClient<TvMazeService>()
                .AddPolicyHandler(TvMazeService.RetryAfterIncreasingDelayOnTooManyRequests);

            services.AddLogging(configure => configure.AddSerilog(Log.Logger));

            services.AddScoped<IElasticClient>(_ =>
            {
                var node = new Uri("http://localhost:9200");

                var connectionPool = new SniffingConnectionPool(new[] {node}) {SniffedOnStartup = true};

                var connectionSettingsValues = new ConnectionSettings(connectionPool)
                    .DefaultMappingFor<ShowWithCast>(s => s.IndexName(nameof(ShowWithCast).ToLowerInvariant()));

                return new ElasticClient(connectionSettingsValues);
            });

            services.AddTransient<ShowsWithCastEnricher>();
            services.AddTransient<ShowsElasticIndexer>();
            services.AddTransient<ElasticTemplateInitializer>();
            services.AddTransient<ShowScraper>();

            var provider = services.BuildServiceProvider();

            return provider;
        }

    }


}
