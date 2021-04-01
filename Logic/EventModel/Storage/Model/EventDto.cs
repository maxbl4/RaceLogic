using System;
using maxbl4.Race.Logic.EventModel.Storage.Identifier;
using maxbl4.Race.Logic.EventStorage.Storage.Traits;

namespace maxbl4.Race.Logic.EventStorage.Storage.Model
{
    public class EventDto : IHasId<EventDto>, IHasName, IHasTimestamp, IHasPublished, IHasSeed
    {
        public string Date { get; set; }
        public string Regulations { get; set; }
        public string ResultsTemplate { get; set; }
        public Id<ChampionshipDto> ChampionshipId { get; set; }

        public DateTime StartOfRegistration { get; set; }
        public DateTime EndOfRegistration { get; set; }

        public decimal BasePrice { get; set; }
        public decimal PaymentMultiplier { get; set; }
        public Id<EventDto> Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public bool Published { get; set; }
        public bool IsSeed { get; set; }
        public DateTime Created { get; set; }
        public DateTime Updated { get; set; }
    }
}