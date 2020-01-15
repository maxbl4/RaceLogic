using System;
using System.IO;
using System.Linq;
using maxbl4.Infrastructure;
using maxbl4.Infrastructure.Extensions.TestOutputHelperExt;
using Xunit.Abstractions;

namespace maxbl4.Race.Tests.Extensions
{
    public static class ITestOutputHelpExt
    {
        static string GetNameForDbFile(this ITestOutputHelper outputHelper)
        {
            Directory.CreateDirectory("var/data");
            var parts = outputHelper.GetTest().DisplayName.Split(".");
            return "var/data/" + "_" + string.Join("-", parts.Skip(Math.Max(parts.Length - 2, 0))) + ".litedb";
        }
        
        public static string GetEmptyLiteDbForTest(this ITestOutputHelper outputHelper)
        {
            var fileName = GetNameForDbFile(outputHelper);
            new RollingFileInfo(fileName).Delete();
            return $"Filename={fileName};UtcDate=true";
        }
    }
}