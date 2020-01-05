﻿using System;

 namespace Cli.BraaapWebModel
{
    public interface ITimestamp
    {
        DateTime Created { get; set; }
        DateTime Updated { get; set; }
    }
}