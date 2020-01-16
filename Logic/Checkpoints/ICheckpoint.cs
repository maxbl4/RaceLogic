using maxbl4.Race.Logic.LogManagement.EntryTypes;

namespace maxbl4.Race.Logic.Checkpoints
{
    public interface ICheckpoint : IEntry
    {
        string RiderId { get; set; }
    }
}