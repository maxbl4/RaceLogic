using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Cli.BraaapWebModel
{
    public class Checkpoint : ITimestamp
    {
        public Guid CheckpointId { get; set; }
        /// <summary>
        /// Time of rider crossing checkpoint
        /// </summary>
        public DateTime Timestamp { get; set; }

        public int Number { get; set; }
        public int Order { get; set; }
        public bool Rejected { get; set; }
        public bool LoggedAfterFinish { get; set; }
        public string? TagSimple { get; set; }
        public string? TagId { get; set; }
        public Tag? Tag { get; set; }
        /// <summary>
        /// Operator or RFID. When RFID is set, TagId should be also filled in.
        /// </summary>
        public string? AssignedBy { get; set; }
        public Guid RiderRegistrationId { get; set; }
        public RiderRegistration? RiderRegistration { get; set; }
        [Required]
        public Schedule? Schedule { get; set; }
        [Required]
        public Guid ScheduleId { get; set; }
        

        /// <summary>
        /// Time of the current record created. The record may be created manually after the race, so this
        /// stamp may be quite different from Timestamp
        /// </summary>
        public DateTime Created { get; set; }
        public DateTime Updated { get; set; }

        [NotMapped] public Guid RiderId => RiderRegistrationId;
    }
}