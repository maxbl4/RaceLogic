using System.Reflection;
using Xunit.Abstractions;

namespace maxbl4.RaceLogic.Tests.Ext
{
    public static class TestOutputHelperExt
    {
        public static ITest GetTest(this ITestOutputHelper outputHelper)
        {
            var field = outputHelper.GetType().GetField("test", BindingFlags.Instance | BindingFlags.NonPublic);
            return (ITest)field.GetValue(outputHelper);
        }

        public static string GetTestName(this ITestOutputHelper outputHelper)
        {
            return GetTest(outputHelper).DisplayName;
        }
    }
}