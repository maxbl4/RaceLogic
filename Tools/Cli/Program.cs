using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Cli.BraaapWebModel;
using Cli.Serialization;
using Dapper;
using MySql.Data.MySqlClient;

namespace Cli
{
    class Program
    {
        private const string OutputDir = "out";
        static async Task Main(string[] args)
        {
            if (!Directory.Exists(OutputDir))
                Directory.CreateDirectory(OutputDir);
            
            var cb = new MySqlConnectionStringBuilder {Server = "localhost", UserID = "root", Password = "", Database = "racelog"};
            await using var con = new MySqlConnection(cb.ConnectionString);
            await con.OpenAsync();
            var events = await con.QueryAsync<Event>("select * from Events order by Date");
            var allLaps = (await con.QueryAsync<Lap>("select * from Laps order by Number")).ToList();
            foreach (var ev in events)
            {
                Console.WriteLine(ev.Name);
                var schedules = await con.QueryAsync<Schedule>("SELECT distinct s.* from Schedules s INNER join ScheduleToClass stc ON s.ScheduleId = stc.ScheduleId where s.EventId = @EventId and Published = 1", ev);
                foreach (var sch in schedules)
                {
                    var roundRating = (await con.QueryAsync<RoundRating>("SELECT * FROM RoundRatings WHERE ScheduleId = @ScheduleId", sch))
                        .OrderBy(x => x.ClassId != null).FirstOrDefault();
                    if (roundRating == null) continue;
                    
                    var checkpoints = await con.QueryAsync<Checkpoint>("SELECT * FROM Checkpoints WHERE ScheduleId = @ScheduleId ORDER BY `Timestamp`", sch);
                    var results = await con.QueryAsync<RoundRiderResult>("SELECT * FROM RoundRiderResults WHERE RoundRatingId = @RoundRatingId and LapsCount > 0 order by Position", roundRating);
                    var riders = checkpoints.GroupBy(x => x.RiderRegistrationId)
                        .ToDictionary(x => x.Key, x => x.First().Number.ToString());
                    Console.WriteLine($" {sch.ScheduleId} {sch.Name}, {sch.ActualStartTime}, {sch.Duration} Checkpoints: {checkpoints.Count()} Results: {results.Count()}");
                    var file = Path.Combine(OutputDir, $"{ev.Name} {sch.Name}.txt");
                    await using var sw = new StreamWriter(file, false);
                    sw.Write($"{RoundDefParser.Track} {sch.Duration.ToShortString()}");
                    sw.WriteLine(RoundDefParser.FormatCheckpoints(checkpoints, sch.ActualStartTime));
                    sw.WriteLine($"{RoundDefParser.Rating}");
                    foreach (var res in results)
                    {
                        if (res.Finished) sw.Write("F");
                        var laps = allLaps.Where(x => x.RoundRiderResultId == res.RoundRiderResultId).Select(x => x.AggDuration.ToShortString());
                        sw.WriteLine($"{riders[res.RiderRegistrationId]} {res.LapsCount} [{string.Join(" ", laps)}]");
                    }
                }
            }
        }
    }
}