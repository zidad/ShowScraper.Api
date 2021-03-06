﻿using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Nest;
using ShowScraper.TvMazeClient.Models;

namespace ShowScraper.TvMazeClient
{
    public class ElasticShowsProvider
    {
        readonly IElasticClient _client;

        public ElasticShowsProvider(IElasticClient client)
        {
            _client = client;
        }

        public async Task<IEnumerable<ShowWithCast>> GetShows(int pageSize, int pageIndex)
        {
            var result = await _client.SearchAsync<ShowWithCast>(
                s => s.Index(nameof(ShowWithCast).ToLowerInvariant())
                    .Size(pageSize)
                    .Skip(pageIndex * pageSize));

            if (!result.IsValid)
                throw new ElasticException("Unable to query elastic, did you start 'docker-compose up --build'?");

            return result.Documents;
        }
    }

    public class ElasticException : Exception
    {
        public ElasticException(string message) : base(message)
        {
        }
    }
}