using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using ShowScraper.TvMazeClient;

namespace ShowScraper.Api.Controllers.v1
{
    [Route("api/v1/[controller]")]
    public class ShowsController : Controller
    {
        readonly ElasticShowsProvider _client;

        public ShowsController(ElasticShowsProvider client)
        {
            _client = client;
        }

        // GET api/values
        [HttpGet]
        public async Task<IActionResult> Get([FromQuery]int pageSize = 10, [FromQuery]int pageIndex = 0)
        {
            if (pageSize > 250)
                return BadRequest("page size can not exceed 250");

            if (pageIndex < 0)
                return BadRequest("page size can not be lower than 0");

            var shows = await _client.GetShows(pageSize, pageIndex);

            var showModels = shows.Select(s => new ShowModel
            {
                id = s.id.ToString(),
                name = s.name,
                cast = s._embedded.cast.OrderByDescending(c => c.person.birthday).Select(c => new CastModel
                {
                    id = c.person.id,
                    name = c.person.name,
                    birthday = c.person.birthday?.ToString("yyyy-MM-dd")
                })
            });

            return Ok(showModels);
        }
    }
}
