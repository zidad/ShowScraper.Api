using System;

namespace ShowScraper.TvMazeClient.Models
{
    public class Person
    {
        public int id { get; set; }
        public string name { get; set; }
        public DateTime? birthday { get; set; }

        /*
         unmapped properties, uncomment to include in indexing
            public string url { get; set; }
            public Country country { get; set; }
            public string gender { get; set; }
            public Image image { get; set; }
        */
    }
}