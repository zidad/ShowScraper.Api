using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Nest;
using Serilog;
using ShowScraper.TvMazeClient.Models;

namespace ShowScraper.Indexer
{
    public class ShowsElasticIndexer
    {
        readonly ILogger _logger = Log.Logger.ForContext<ShowsWithCastEnricher>();
        readonly IElasticClient _client;

        public ShowsElasticIndexer(IElasticClient client)
        {
            _client = client;
        }

        public async Task SaveShowsAsync(IEnumerable<ShowWithCast> shows, CancellationToken cancellationToken=default(CancellationToken))
        {
            var result = await _client.IndexManyAsync(shows, cancellationToken: cancellationToken);

            foreach (var resultItemsWithError in result.ItemsWithErrors)
            {
                _logger.Error(
                    "Index failed for {showWithCast} with error {error}{error}", resultItemsWithError.Id, resultItemsWithError.Error, result);
            }
        }
    }
}