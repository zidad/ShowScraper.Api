using System.Collections.Generic;
using System.Collections.Immutable;
using System.Threading.Tasks;
using Serilog;
using ShowScraper.TvMazeClient;
using ShowScraper.TvMazeClient.Models;

namespace ShowScraper.Indexer
{
    /// <summary>
    /// Enriches the TV shows with cast information from the API
    /// </summary>
    public class ShowsWithCastEnricher
    {
        readonly ILogger _logger = Log.Logger.ForContext<ShowsWithCastEnricher>();
        readonly TvMazeService _service;

        public ShowsWithCastEnricher(TvMazeService service)
        {
            _service = service;
        }

        public async Task<IList<ShowWithCast>> ReadShowsAsync(int startPage)
        {
            var shows = (await _service.GetShows(startPage)).ToImmutableList();
            IList<ShowWithCast> showsWithCast = new List<ShowWithCast>();

            foreach (var show in shows)
            {
                _logger.Information("Page {page}: Enriching show {showid} {showname}", startPage, show?.id, show?.name);
                showsWithCast.Add(await _service.GetShowWithCast(show.id));
            }

            return showsWithCast;
        }
    }
}