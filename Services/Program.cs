using System;

using Assignment_A2_04.Models;
using Assignment_A2_04.ModelsSampleData;
using System.Net;

namespace Assignment_A2_04.Services
{
    class Program
    {
        static async Task Main(string[] args)
        {

            NewsService service = new NewsService();
            service.NewsReportAvailable += NewsReportDataAvailable;

            Task<News>[] tasks = { null };

            tasks[0] = service.GetNewsAsync(NewsCategory.sports);

            Task.WaitAll(tasks[0]);

            News news = await new NewsService().GetNewsAsync(NewsCategory.sports);


            Console.WriteLine("Headlines: ");
            var groupedNewsList = news.Articles.GroupBy(n => (n.DateTime.DayOfWeek, n.DateTime.ToShortDateString()), n => n).Distinct().ToList();

            foreach (var News in groupedNewsList)
            {
                News.ToList().ForEach(n => Console.WriteLine($"{n.DateTime}: - {n.Title} - {n.UrlToImage}"));

            }
            Console.WriteLine();
            Console.WriteLine("----------------------------------------------------------------------");


            void NewsReportDataAvailable(object sender, string message)
            {
                Console.WriteLine($"Event message from news service: {message}");
            }
        }
    }
}
