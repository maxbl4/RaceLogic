using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using ServiceBase;

namespace maxbl4.Race.WsHub
{
    public class Program
    {
        public static async Task<int> Main(string[] args)
        {
            using var svc = new ServiceRunner<Startup>();
            return await svc.Start(args);
        }
    }
}