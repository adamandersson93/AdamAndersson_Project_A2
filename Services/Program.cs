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

            List<Task<News>> Listtasks = new List<Task<News>>();
            Task<News>[] tasks = { null, null };
            Exception exception= null;

            try
            {
                for (NewsCategory nc = NewsCategory.business; nc <= NewsCategory.technology; nc++)
                {
                    var task1 = service.GetNewsAsync(nc);
                    task1.Wait();
                    Listtasks.Add(task1);

                }


                Console.WriteLine();
                for (NewsCategory nc = NewsCategory.business; nc <= NewsCategory.technology; nc++)
                {

                    var task2 = service.GetNewsAsync(nc);
                    task2.Wait();
                    Listtasks.Add(task2);

                }

                //Task.WaitAll(Listtasks[0], Listtasks[1]);
                Console.WriteLine();

                for (NewsCategory nc = NewsCategory.business; nc <= NewsCategory.technology; nc++)
                {
                   // Listtasks[0] = service.GetNewsAsync(nc);
                   // Listtasks[1] = service.GetNewsAsync(nc);

                    News news = await new NewsService().GetNewsAsync(nc);

                    Console.WriteLine($" \n News in category {nc}: \n ");

                    var groupedNewsList = news.Articles.GroupBy(n => (n.DateTime.DayOfWeek, n.DateTime.ToShortDateString()), n => n).Distinct().ToList();

                    foreach (var News in groupedNewsList)

                    {
                        News.ToList().ForEach(n => Console.WriteLine($"{n.DateTime}: - {n.Title} - {n.UrlToImage}"));

                    }

                    NewsService.Serialize(groupedNewsList, "NewsList");

                }

                Console.WriteLine();
                Console.WriteLine("----------------------------------------------------------------------");
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        static void NewsReportDataAvailable(object sender, string message)
        {
            Console.WriteLine($"Event message from news service: {message}");
        }
    }
}
