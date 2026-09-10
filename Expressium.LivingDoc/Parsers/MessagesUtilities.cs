using Io.Cucumber.Messages.Types;
using System;
using System.Text.RegularExpressions;

namespace Expressium.LivingDoc.Parsers
{
    internal static class MessagesUtilities
    {
        internal static DateTime ToDateTime(this Timestamp timestamp)
        {
            var epoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);

            var dateTime = epoch
                .AddSeconds(timestamp.Seconds)
                .AddTicks(timestamp.Nanos / 100);

            return DateTime.SpecifyKind(dateTime, DateTimeKind.Utc);
        }

        internal static TimeSpan ToTimeSpan(this Timestamp timestampStart, Timestamp timestampEnd)
        {
            var startTime = timestampStart.ToDateTime();
            var endTime = timestampEnd.ToDateTime();

            return endTime - startTime;
        }

        private static readonly Regex CapitalizeWordsRegex = new Regex(@"(^\w)|(\s\w)", RegexOptions.Compiled);

        internal static string CapitalizeWords(this string value)
        {
            if (string.IsNullOrEmpty(value))
                return value;

            return CapitalizeWordsRegex.Replace(value, m => m.Value.ToUpper());
        }
    }
}
