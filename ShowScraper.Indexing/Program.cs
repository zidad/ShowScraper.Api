using System;
using System.Collections.Immutable;
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
        static async Task Main(string[] args)
        {
            Log.Logger = new LoggerConfiguration()
                .WriteTo.LiterateConsole()
                .CreateLogger();

            var configBuilder = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json")
                .AddEnvironmentVariables();

            Configuration = configBuilder.Build();
        
            var services = new ServiceCollection();

            services
                .AddHttpClient<TvMazeService>()
                .AddPolicyHandler(TvMazeService.RetryAfterIncreasingDelayOnTooManyRequests);

            services.AddLogging(configure => configure.AddSerilog(Log.Logger));

            services.AddScoped<IElasticClient>(_ =>
            {
                var node = new Uri("http://localhost:9200");
            
                var connectionPool = new SniffingConnectionPool(new[] {node}) {SniffedOnStartup = true };
            
                var connectionSettingsValues = new ConnectionSettings(connectionPool)
                    .DefaultMappingFor<ShowWithCast>(s => s.IndexName(nameof(ShowWithCast).ToLowerInvariant()));

                return new ElasticClient(connectionSettingsValues);
            });

            services.AddTransient<ShowsWithCastEnricher>();
            services.AddTransient<ShowsElasticIndexer>();
            services.AddTransient<CreateIndexTemplateIfNeeded>();

            var provider = services.BuildServiceProvider();

            var templateCreator = provider.GetService<CreateIndexTemplateIfNeeded>();
            await templateCreator.CreateIndexTemplate();

            var reader = provider.GetService<ShowsWithCastEnricher>();
            var indexer = provider.GetService<ShowsElasticIndexer>();

            bool hasMore = true;
            int page  = 0;

            while(hasMore)
            {
                var shows = (await reader.ReadShowsAsync(page++)).ToImmutableList();

                await indexer.SaveShowsAsync(shows);

                hasMore = shows.Count > 0;
            }
        }

        public static IConfigurationRoot Configuration { get; set; }
    }

}
