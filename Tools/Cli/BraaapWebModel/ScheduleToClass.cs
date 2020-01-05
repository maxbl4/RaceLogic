﻿using System;

 namespace Cli.BraaapWebModel
{
    public class ScheduleToClass: ITimestamp
    {
        public Guid ScheduleId { get; set; }
        public Schedule? Schedule { get; set; }
        public Guid ClassId { get; set; }
        public Class? Class { get; set; }
        public DateTime Created { get; set; }
        public DateTime Updated { get; set; }
    }
}