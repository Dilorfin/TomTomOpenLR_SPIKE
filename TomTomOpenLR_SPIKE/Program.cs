using System.Diagnostics;
using Microsoft.EntityFrameworkCore;
using Roadnet.Apex.Server.RoadNetwork.Traffic.TomTom;
using TomTomOpenLR_SPIKE;

TimeSpan delay = TimeSpan.FromMinutes(5);

await using (var context = new ApplicationContext())
{
    /*context.Database.EnsureDeleted();
    context.Database.EnsureCreated();*/
}
int i = 0;
while (true)
{
    using var trafficProvider = new TomTomTrafficProvider();
    var responses = trafficProvider.RetrieveTrafficData(CancellationToken.None);
    
    Stopwatch stopWatch = new Stopwatch();
    stopWatch.Start();

    Parallel.ForEach(responses, 
        new ParallelOptions { MaxDegreeOfParallelism = 8 },
        resp =>
    //foreach (var resp in responses)
    {
        var url = resp?.RequestMessage?.RequestUri?.ToString();
        if (url == null)
        {
            Console.WriteLine("Request was null");
            return;
        }

        //Console.WriteLine($"start {url}");
        using Stream stream = resp.Content.ReadAsStreamAsync().Result;

        var parsedData = TrafficFlowGroup.Parser.ParseFrom(stream);
        var locations = parsedData.TrafficFlowWithPredictionPerSection
            .Where(tf => tf.Location.HasOpenlr)
            .Select(tf => Convert.ToBase64String(tf.Location.Openlr.Span))
            .Distinct()
            .ToList();

        //Console.WriteLine($"Count: {locations.Count()}");

        using (var context = new ApplicationContext())
        {
            context.Database.SetCommandTimeout(TimeSpan.FromHours(1));
            var locEntities = context.Locations.Where(l => l.Url == url);

            var newObjs = locations.Except(locEntities.Select(x => x.Location));
            context.Locations.BulkInsert(newObjs.Select(l => new OpenLRLocation()
            { Location = l, Url = url, GotCount = 0 }));

            
            var list = locEntities.WhereBulkContains(locations, "Location");
            foreach (var el in list)
            {
                el.GotCount++;
            }

            context.BulkUpdate(list);
        }
    });

    stopWatch.Stop();

    Console.Clear();
    Console.WriteLine($"Iteration: {i}\n");
    TimeSpan ts = stopWatch.Elapsed;
    string elapsedTime = $"{ts.Hours:00}:{ts.Minutes:00}:{ts.Seconds:00}.{ts.Milliseconds / 10:00}";
    Console.WriteLine("RunTime " + elapsedTime);

    DisplayStatus();
    
    ++i;
    Thread.Sleep(delay);
}

void DisplayStatus()
{
    using (var context = new ApplicationContext())
    {
        var groups = context.Locations.GroupBy(l => l.Url).Select(g=>
            new
            {
                Key = g.Key,
                Count = g.Count(),
                Max = g.Max(l => l.GotCount),
                Min = g.Min(l => l.GotCount)
            }).ToList();

        Console.WriteLine($"All locations are unchanged: {groups.All(m => m.Max - m.Min == 0)}");

        foreach (var group in groups)
        {
            Console.WriteLine($"URL: {group.Key}");
            Console.WriteLine($"Locations count: {group.Count}");
            Console.WriteLine($"Max requests count: {group.Max}");
            Console.WriteLine($"Min requests count: {group.Min}");
            Console.WriteLine($"Locations are unchanged: {group.Max-group.Min == 0}");
            Console.WriteLine();
        }
        Console.WriteLine();
    }
}