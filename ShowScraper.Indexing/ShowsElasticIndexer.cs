using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Nest;
using Serilog;
using ShowScraper.TvMazeClient.Models;

namespace ShowScraper.Indexer
{
    /// <summary>
    /// saves lists of Shows to elastic search
    /// </summary>
    public class ShowsElasticIndexer
    {
        readonly ILogger _logger = Log.Logger.ForContext<ShowsWithCastEnricher>();
        readonly IElasticClient _client;

        public ShowsElasticIndexer(IElasticClient client)
        {
            _client = client;
        }

        public async Task SaveShowsAsync(IList<ShowWithCast> shows, CancellationToken cancellationToken=default)
        {
            if (shows == null) throw new ArgumentNullException(nameof(shows));
            _logger.Information("Indexing {amount} shows to Elastic", shows.Count());

            var result = await _client.IndexManyAsync(shows, cancellationToken: cancellationToken);

            foreach (var resultItemsWithError in result.ItemsWithErrors)
            {
                _logger.Error(
                    "Index failed for {showWithCast} with error {error}{error}", resultItemsWithError.Id, resultItemsWithError.Error, result);
            }
        }
    }
}