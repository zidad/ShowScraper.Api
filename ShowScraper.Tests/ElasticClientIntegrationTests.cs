using System;
using System.Threading.Tasks;
using Elasticsearch.Net;
using Microsoft.Extensions.DependencyInjection;
using Nest;
using ShowScraper.TvMazeClient;
using ShowScraper.TvMazeClient.Models;
using Xunit;
using Xunit.Abstractions;

namespace ShowScraper.Tests
{
    public class ElasticClientIntegrationTests
    {
        readonly ITestOutputHelper _logger;
        readonly IServiceProvider _provider;

        public ElasticClientIntegrationTests(ITestOutputHelper logger)
        {
            _logger = logger;
            
            var services = new ServiceCollection();

            services.AddScoped<IElasticClient>(_ =>
            {
                var node = new Uri("http://localhost:9200");
            
                var connectionPool = new SniffingConnectionPool(new[] {node}) {SniffedOnStartup = true };
            
                var connectionSettingsValues = new ConnectionSettings(connectionPool)
                    .DefaultMappingFor<ShowWithCast>(s => s.IndexName(nameof(ShowWithCast).ToLowerInvariant()));

                return new ElasticClient(connectionSettingsValues);
            });

            services.AddTransient<ElasticShowsProvider>();

            _provider = services.BuildServiceProvider();
        }

        [Fact]
        public async Task Can_get_shows_with_cast()
        {
            var service = _provider.GetService<ElasticShowsProvider>();

            var shows = await service.GetShows(10,3);

            foreach (var show in shows)
            {
                _logger.WriteLine(show.name);

                foreach (var cast in show._embedded.cast)
                {
                    _logger.WriteLine("Person: {0}, Birthday: {1}", cast.person.name, cast.person.birthday);
                }
            }
        }
        
    }
}