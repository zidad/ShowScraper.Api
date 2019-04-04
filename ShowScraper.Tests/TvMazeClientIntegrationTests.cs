using System;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using ShowScraper.TvMazeClient;
using Xunit;
using Xunit.Abstractions;

namespace ShowScraper.Tests
{
    public class TvMazeClientIntegrationTests
    {
        readonly ITestOutputHelper _logger;
        readonly IServiceProvider _provider;

        public TvMazeClientIntegrationTests(ITestOutputHelper logger)
        {
            _logger = logger;
            
            var services = new ServiceCollection();

            services
                .AddHttpClient<TvMazeService>()
                .AddPolicyHandler(TvMazeService.RetryAfterIncreasingDelayOnTooManyRequests);

            _provider = services.BuildServiceProvider();
        }

        [Fact]
        public async Task Can_get_shows()
        {
            var service = _provider.GetService<TvMazeService>();

            var shows = await service.GetShows(1);

            foreach (var tvMazeShow in shows)
            {
                _logger.WriteLine(tvMazeShow.name);
            }
        }

        [Fact]
        public async Task Can_get_shows_with_cast()
        {
            var service = _provider.GetService<TvMazeService>();

            var shows = await service.GetShows(1);

            foreach (var tvMazeShow in shows)
            {
                _logger.WriteLine(tvMazeShow.name);

                var show = await service.GetShowWithCast(tvMazeShow.id);

                show._embedded.cast.Should().NotBeNull();

                if (show._embedded.cast.Length == 0)
                    _logger.WriteLine($"warning: show without cast {tvMazeShow.name}");
                
            }
        }
    }


}
