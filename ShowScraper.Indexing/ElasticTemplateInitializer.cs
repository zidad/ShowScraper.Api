using System;
using System.Threading.Tasks;
using Nest;
using ShowScraper.TvMazeClient.Models;

namespace ShowScraper.Indexer
{
    public class ElasticTemplateInitializer
    {
        readonly IElasticClient _client;

        public ElasticTemplateInitializer(IElasticClient elasticClient)
        {
            _client = elasticClient ?? throw new ArgumentNullException(nameof(elasticClient));
        }
        
        /// <summary>
        ///     Register the template.
        /// </summary>
        public async Task CreateIndexTemplate()
        {            
            var createIndexResponse = await _client.PutIndexTemplateAsync(nameof(ShowWithCast).ToLowerInvariant() + "-template", c => c
                .Create(false)
                .IndexPatterns(nameof(ShowWithCast).ToLowerInvariant() + "*")
                .Mappings(ms => ms.Map<ShowWithCast>(m => m.AutoMap<ShowWithCast>())
                ));

            if (!createIndexResponse.Acknowledged)
            {
                if (createIndexResponse.ServerError.Status == 400) // resource_already_exists
                    return;

                throw new Exception($"Error saving index: {createIndexResponse.ServerError.Error}");
            }
        }
    }
}