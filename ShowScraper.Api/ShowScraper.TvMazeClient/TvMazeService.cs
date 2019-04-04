using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Polly;
using Polly.Extensions.Http;
using ShowScraper.TvMazeClient.Models;

namespace ShowScraper.TvMazeClient
{
    public class TvMazeService
    {
        public HttpClient Client { get; }

        public TvMazeService(HttpClient client)
        {
            client.BaseAddress = new Uri("http://api.tvmaze.com");

            // While not required, we strongly recommend setting your client's HTTP User Agent to something that'll uniquely describe it.
            // This allows us to identify your application in case of problems, or to proactively reach out to you.
            client.DefaultRequestHeaders.Add("User-Agent", "RTL.ShowScraper");

            Client = client;
        }

        /// <summary>
        /// Show index
        /// A list of all shows in our database, with all primary information included. You can use this endpoint for example if you want to build a local cache of all shows contained in the TVmaze database.
        /// This endpoint is paginated, with a maximum of 250 results per page. The pagination is based on show ID, e.g. page 0 will contain shows with IDs between 0 and 250.
        /// This means a single page might contain less than 250 results, in case of deletions, but it also guarantees that deletions won't cause shuffling in the page numbering for other shows.
        ///    Because of this, you can implement a daily/weekly sync simply by starting at the page number where you last left off, and be sure you won't skip over any entries.
        /// For example, if the last show in your local cache has an ID of 1800, you would start the re-sync at page number floor(1800/250) = 7. After this,
        /// simply increment the page number by 1 until you receive a HTTP 404 response code, which indicates that you've reached the end of the list.
        ///    As opposed to the other endpoints, results from the show index are cached for up to 24 hours.
        ///    URL: /shows?page=:num
        ///    Example: http://api.tvmaze.com/shows
        /// Example: http://api.tvmaze.com/shows?page=1
        /// </summary>
        public async Task<IEnumerable<Show>> GetShows(int page)
        {
            var response = await Client.GetAsync(
                $"http://api.tvmaze.com/shows?page={page}");

            //simply increment the page number by 1 until you receive a HTTP 404 response code, which indicates that you've reached the end of the list.
            if (response.StatusCode == HttpStatusCode.NotFound)
                return Enumerable.Empty<Show>();

            response.EnsureSuccessStatusCode();

            var result = await response.Content
                .ReadAsAsync<IEnumerable<Show>>();

            return result;
        }

        /// <summary>
        /// Shows
        /// Show main information
        /// Retrieve all primary information for a given show. This endpoint allows embedding of additional information. See the section embedding for more information.
        ///
        /// URL: /shows/:id
        /// Example: http://api.tvmaze.com/shows/1
        /// Example: http://api.tvmaze.com/shows/1?embed=cast
        /// </summary>
        public async Task<ShowWithCast> GetShowWithCast(int showId)
        {
            var response = await Client.GetAsync(
                $"shows/{showId}?embed=cast").ConfigureAwait(true);

            response.EnsureSuccessStatusCode();

            var result = await response.Content
                .ReadAsAsync<ShowWithCast>().ConfigureAwait(true);

            if(result==null)
                throw new Exception($"Failed to load show with cast for {showId}");

            return result;
        }

        /// <summary>
        /// Rate limiting
        /// API calls are rate limited to allow at least 20 calls every 10 seconds per IP address.
        /// If you exceed this rate, you might receive an HTTP 429 error. We say at least, because rate limiting
        /// takes place on the backend but not on the edge cache. So if your client is only requesting common/popular
        /// endpoints like shows or episodes (as opposed to more unique endpoints like searches or embedding),
        /// you're likely to never hit the limit. For an optimal throughput, simply let your client back off for
        /// a few seconds when it receives a 429.
        /// Under special circumstances we may temporarily have to impose a stricter rate limit.
        /// So even if your client wouldn't normally exceed our rate limit, it's useful to gracefully
        /// handle HTTP 429 responses: simply retry the request after a small pause instead of treating it as a permanent failure.
        /// 
        /// While not required, we strongly recommend setting your client's HTTP User Agent to something that'll uniquely describe it.
        /// This allows us to identify your application in case of problems, or to proactively reach out to you.
        /// </summary>
        /// <value></value>
        public static IAsyncPolicy<HttpResponseMessage> RetryAfterIncreasingDelayOnTooManyRequests
        {
            get
            {
                return HttpPolicyExtensions
                    .HandleTransientHttpError()
                    .OrResult(msg => msg.StatusCode == (HttpStatusCode) 429) // Too many requests
                    .WaitAndRetryAsync(6, retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)));
            }
        }
    }


}
