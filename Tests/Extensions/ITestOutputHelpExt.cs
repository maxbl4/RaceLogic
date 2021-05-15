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
        private static string GetNameForDbFile(this ITestOutputHelper outputHelper)
        {
            var parts = outputHelper.GetTest().DisplayName.Split(".");
            return "var/data/" + "_" + string.Join("-", parts.Skip(Math.Max(parts.Length - 2, 0))) + ".litedb";
        }
        
        public static string GetWorkingDirectory(this ITestOutputHelper outputHelper, string path)
        {
            path = Path.Combine("var", path);
            Directory.CreateDirectory(path);
            return path;
        }

        public static string GetEmptyLiteDbForTest(this ITestOutputHelper outputHelper)
        {
            var fileName = GetNameForDbFile(outputHelper);
            new RollingFileInfo(fileName).Delete();
            return $"Filename={fileName}";
        }
    }
}