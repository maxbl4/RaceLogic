using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using BraaapWeb.Client;
using Newtonsoft.Json;

namespace maxbl4.Race.Tests.Infrastructure
{
    public class FakeMainClient: IMainClient
    {
        private readonly string testDataDir;

        public FakeMainClient()
        {
            testDataDir = Path.GetFullPath("..\\TestData");
        }

        public Task<ICollection<Series>> SeriesAsync(string apiKey, DateTimeOffset? @from)
        {
            return Task.FromResult(Load<Series>());
        }

        public Task<ICollection<Series>> SeriesAsync(string apiKey, DateTimeOffset? @from, CancellationToken cancellationToken)
        {
            return Task.FromResult(Load<Series>());
        }

        public Task<ICollection<Championship>> ChampionshipsAsync(string apiKey, DateTimeOffset? @from)
        {
            return Task.FromResult(Load<Championship>());
        }

        public Task<ICollection<Championship>> ChampionshipsAsync(string apiKey, DateTimeOffset? @from, CancellationToken cancellationToken)
        {
            return Task.FromResult(Load<Championship>());
        }

        public Task<ICollection<Event>> EventsAsync(string apiKey, DateTimeOffset? @from)
        {
            return Task.FromResult(Load<Event>());
        }

        public Task<ICollection<Event>> EventsAsync(string apiKey, DateTimeOffset? @from, CancellationToken cancellationToken)
        {
            return Task.FromResult(Load<Event>());
        }

        public Task<ICollection<EventPrice>> EventPricesAsync(string apiKey, DateTimeOffset? @from)
        {
            return Task.FromResult(Load<EventPrice>());
        }

        public Task<ICollection<EventPrice>> EventPricesAsync(string apiKey, DateTimeOffset? @from, CancellationToken cancellationToken)
        {
            return Task.FromResult(Load<EventPrice>());
        }

        public Task<ICollection<Class>> ClassesAsync(string apiKey, DateTimeOffset? @from)
        {
            return Task.FromResult(Load<Class>());
        }

        public Task<ICollection<Class>> ClassesAsync(string apiKey, DateTimeOffset? @from, CancellationToken cancellationToken)
        {
            return Task.FromResult(Load<Class>());
        }

        public Task<ICollection<Schedule>> SchedulesAsync(string apiKey, DateTimeOffset? @from)
        {
            return Task.FromResult(Load<Schedule>());
        }

        public Task<ICollection<Schedule>> SchedulesAsync(string apiKey, DateTimeOffset? @from, CancellationToken cancellationToken)
        {
            return Task.FromResult(Load<Schedule>());
        }

        public Task<ICollection<ScheduleToClass>> ScheduleToClassAsync(string apiKey, DateTimeOffset? @from)
        {
            return Task.FromResult(Load<ScheduleToClass>());
        }

        public Task<ICollection<ScheduleToClass>> ScheduleToClassAsync(string apiKey, DateTimeOffset? @from, CancellationToken cancellationToken)
        {
            return Task.FromResult(Load<ScheduleToClass>());
        }

        public Task<ICollection<RiderProfile>> RiderProfilesAsync(string apiKey, DateTimeOffset? @from)
        {
            return Task.FromResult(Load<RiderProfile>());
        }

        public Task<ICollection<RiderProfile>> RiderProfilesAsync(string apiKey, DateTimeOffset? @from, CancellationToken cancellationToken)
        {
            return Task.FromResult(Load<RiderProfile>());
        }

        public Task<ICollection<RiderRegistration>> RiderRegistrationsAsync(string apiKey, DateTimeOffset? @from)
        {
            return Task.FromResult(Load<RiderRegistration>());
        }

        public Task<ICollection<RiderRegistration>> RiderRegistrationsAsync(string apiKey, DateTimeOffset? @from, CancellationToken cancellationToken)
        {
            return Task.FromResult(Load<RiderRegistration>());
        }

        public Task<ICollection<EventConfirmation>> EventConfirmationsAsync(string apiKey, DateTimeOffset? @from)
        {
            return Task.FromResult(Load<EventConfirmation>());
        }

        public Task<ICollection<EventConfirmation>> EventConfirmationsAsync(string apiKey, DateTimeOffset? @from, CancellationToken cancellationToken)
        {
            return Task.FromResult(Load<EventConfirmation>());
        }

        public Task<ICollection<RiderDisqualification>> RiderDisqualificationsAsync(string apiKey)
        {
            return Task.FromResult(Load<RiderDisqualification>());
        }

        public Task<ICollection<RiderDisqualification>> RiderDisqualificationsAsync(string apiKey, CancellationToken cancellationToken)
        {
            return Task.FromResult(Load<RiderDisqualification>());
        }
        
        public Task<FileResponse> StatusAsync(string apiKey)
        {
            throw new NotImplementedException();
        }

        public Task<FileResponse> StatusAsync(string apiKey, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public Task<ICollection<Tag>> TagsAsync(string apiKey, DateTimeOffset? @from)
        {
            throw new NotImplementedException();
        }

        public Task<ICollection<Tag>> TagsAsync(string apiKey, DateTimeOffset? @from, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public Task<bool> ObserveTagsAsync(string apiKey, IEnumerable<AlienTag> tags)
        {
            throw new NotImplementedException();
        }

        public Task<bool> ObserveTagsAsync(string apiKey, IEnumerable<AlienTag> tags, CancellationToken cancellationToken)
        {
            return null;
        }

        public Task<UploadCheckpointsResponse> UploadScheduleCheckpointsAsync(string apiKey, Guid? scheduleId, DateTimeOffset? startTime,
            IEnumerable<Checkpoint> checkpoints)
        {
            throw new NotImplementedException();
        }

        public Task<UploadCheckpointsResponse> UploadScheduleCheckpointsAsync(string apiKey, Guid? scheduleId, DateTimeOffset? startTime, IEnumerable<Checkpoint> checkpoints,
            CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        private ICollection<T> Load<T>([CallerMemberName]string name = null)
        {
            using var sr = new StreamReader($"{name}.json");
            var serializer = JsonSerializer.Create();
            var jr = new JsonTextReader(sr);
            return serializer.Deserialize<List<T>>(jr);
        }
    }
}