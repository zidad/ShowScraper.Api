using System.Collections.Generic;

namespace ShowScraper.Api.Models
{
    public class ShowModel
    {
        public string id { get; set; }
        public string name { get; set; }
        public IList<CastModel> cast { get; set; }
    }
}