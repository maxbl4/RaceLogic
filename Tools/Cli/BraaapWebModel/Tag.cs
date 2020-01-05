﻿using System;

 namespace Cli.BraaapWebModel
{
    public class Tag: ITimestamp
    {
        public Guid RiderRegistrationId { get; set; }
        public RiderRegistration? RiderRegistration { get; set; }
        public string? TagId { get; set; }
        public DateTime Created { get; set; }
        public DateTime Updated { get; set; }
    }
}