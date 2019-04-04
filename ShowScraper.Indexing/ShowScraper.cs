using System.Collections.Immutable;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Serilog;

namespace ShowScraper.Indexer
{
    /// <summary>
    ///  and 
    /// </summary>
    public class ShowScraper
    {
        ILogger logger = Log.Logger.ForContext<ShowScraper>();
        readonly ShowsWithCastEnricher _enricher;
        readonly ShowsElasticIndexer _indexer;
        readonly ElasticTemplateInitializer _elasticInitializer;

        public ShowScraper(ShowsWithCastEnricher enricher, ShowsElasticIndexer indexer, ElasticTemplateInitializer elasticInitializer)
        {
            _enricher = enricher ?? throw new System.ArgumentNullException(nameof(enricher));
            _indexer = indexer ?? throw new System.ArgumentNullException(nameof(indexer));
            _elasticInitializer = elasticInitializer ?? throw new System.ArgumentNullException(nameof(elasticInitializer));
        }

        public async Task StartScraping(IConfiguration configuration)
        {
            if (configuration == null)
                throw new System.ArgumentNullException(nameof(configuration));

            await _elasticInitializer.CreateIndexTemplate();

            bool hasMore = true;

            // allow continuation of indexing at later pages based on command line parameters
            int.TryParse(configuration["startPage"], out int startPage);

            while (hasMore)
            {
                var shows = (await _enricher.ReadShowsAsync(startPage++)).ToImmutableList();

                await _indexer.SaveShowsAsync(shows);

                hasMore = shows.Count > 0;
            }
        }
    }
}