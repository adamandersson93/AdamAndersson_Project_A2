//#define UseNewsApiSample  // Remove or undefine to use your own code to read live data

using System;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Collections.Concurrent;
using System.Threading.Tasks;

using Assignment_A2_04.Models;
using Assignment_A2_04.ModelsSampleData;
using System.Net;
using System.Xml.Linq;
using System.Xml.Serialization;

namespace Assignment_A2_04.Services
{
    public class NewsService
    {

        // ConcurrentDictionary<(string, string), News> cachedNews = new ConcurrentDictionary<(string, string), News>();
        ConcurrentDictionary<string, (string, News)> cachedNews = new ConcurrentDictionary<string, (string, News)>();


        HttpClient httpClient = new HttpClient();

        // Your API Key
        readonly string apiKey = "1bd9f35626c54515b157bb3fcbc60f3a";


        public event EventHandler<string> NewsReportAvailable;
        protected virtual void OnNewsReportAvailable(string message)
        {
            NewsReportAvailable?.Invoke(this, message);
        }

        public NewsService()
        {
            httpClient = new HttpClient(new HttpClientHandler { AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate });
            httpClient.DefaultRequestHeaders.Add("user-agent", "News-API-csharp/0.1");
            httpClient.DefaultRequestHeaders.Add("x-api-key", apiKey);
        }

        public async Task<News> GetNewsAsync(NewsCategory category)
        {

            //https://newsapi.org/docs/endpoints/top-headlines
            var uri = $"https://newsapi.org/v2/top-headlines?country=se&category={category}";

            var key = category.ToString();
            var news = new News();
            var Date = DateTime.Now.ToString("yyyy, MM, dd: HH:mm");
            var value = (Date, news);

            if (!cachedNews.TryGetValue(key, out value))
            {
                news = await ReadWebApiAsync(uri);
                cachedNews[key] = value;
                NewsReportAvailable?.Invoke(news, $"News in category available: {category}");
            }
            else
                NewsReportAvailable?.Invoke(news, $"Cached News in category available: {category}");
            return news;

        }
        private async Task<News> ReadWebApiAsync(string uri)
        {
            // make the http request
            var httpRequest = new HttpRequestMessage(HttpMethod.Get, uri);
            var response = await httpClient.SendAsync(httpRequest);
            response.EnsureSuccessStatusCode();

            //Convert Json to Object
            NewsApiData nd = await response.Content.ReadFromJsonAsync<NewsApiData>();

            News news = new News()
            {
                Articles = nd.Articles.Select(n => new NewsItem
                {
                    DateTime = n.PublishedAt,
                    Title = n.Title,
                    Description = n.Description,
                    Url = n.Url,

                }).ToList()


            };

            return news;
        }

        public static void Serialize(News news, string fname)
        {
            var _locker = new object();
            lock (_locker)
            {
                var xs = new XmlSerializer(typeof(NewsApiData));
                using (Stream s = File.Create(fname))
                    xs.Serialize(s, news);
            }
        }
        public static NewsApiData Deserialize(string fname)
        {
            var _locker = new object();
            lock (_locker)
            {
                NewsApiData news;
                var xs = new XmlSerializer(typeof(NewsApiData));

                using (Stream s = File.OpenRead(fname))
                    news = (NewsApiData)xs.Deserialize(s);

                return news;
            }
        }
        static string fname(string key)
        {
            var documentPath = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
            documentPath = Path.Combine(documentPath, "A2", "NewsService");
            if (!Directory.Exists(documentPath)) Directory.CreateDirectory(documentPath);
            return Path.Combine(documentPath, key);
        }

    }

}
