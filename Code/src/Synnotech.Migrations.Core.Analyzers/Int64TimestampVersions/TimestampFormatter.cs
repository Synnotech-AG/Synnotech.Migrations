using System;
using System.Globalization;

namespace Synnotech.Migrations.Core.Analyzers.Int64TimestampVersions
{
    /// <summary>
    /// Provides a method to format a datetime in ISO 8601 format.
    /// </summary>
    public static class TimestampFormatter
    {
        /// <summary>
        /// Converts the date time to UTC format if necessary and creates a
        /// string with the format "yyyy-MM-ddTHH:mm:ssZ".
        /// </summary>
        public static string ToIso8601UtcString(this DateTime dateTime)
        {
            if (dateTime.Kind != DateTimeKind.Utc)
                dateTime = dateTime.ToUniversalTime();

            return dateTime.ToString("yyyy-MM-ddTHH:mm:ssZ", CultureInfo.InvariantCulture);
        }

        /// <summary>
        /// Converts the date time to UTC format if necessary and creates a
        /// long value with the format "yyyyMMddHHmmss".
        /// </summary>
        public static long ToInt64Timestamp(this DateTime dateTime)
        {
            if (dateTime.Kind != DateTimeKind.Utc)
                dateTime = dateTime.ToUniversalTime();
            return ConvertToInt64(dateTime.Year,
                                  dateTime.Month,
                                  dateTime.Day,
                                  dateTime.Hour,
                                  dateTime.Minute,
                                  dateTime.Second);
        }

        private static long ConvertToInt64(int year, int month, int day, int hour, int minute, int second)
        {
            var value = 0L;
            value += year * 1_00_00_00_00_00;
            value += month * 1_00_00_00_00;
            value += day * 1_00_00_00;
            value += hour * 1_00_00;
            value += minute * 1_00;
            value += second;
            return value;
        }
    }
}