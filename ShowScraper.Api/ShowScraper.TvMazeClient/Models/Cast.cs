﻿namespace ShowScraper.TvMazeClient.Models
{
    public class Cast
    {
        public Person person { get; set; }
        public Character character { get; set; }
        public bool self { get; set; }
        public bool voice { get; set; }
    }
}