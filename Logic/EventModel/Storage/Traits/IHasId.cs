﻿using maxbl4.Race.Logic.EventModel.Storage.Identifier;

namespace maxbl4.Race.Logic.EventStorage.Storage.Traits
{
    public interface IHasId<T> : IHasGuidId, IHasTraits
    {
        new Id<T> Id { get; set; }
        IGuidValue IHasGuidId.Id
        {
            get => Id;
            set => Id = new Id<T>(value.Value);
        }
    }
}