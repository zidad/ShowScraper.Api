using System.Collections.Generic;

namespace ShowScraper.Api.Controllers.v1
{
    public class ShowModel
    {
        public string id { get; set; }
        public string name { get; set; }
        public IEnumerable<CastModel> cast { get; set; }
    }
}