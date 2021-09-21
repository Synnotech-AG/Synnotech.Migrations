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
    }
}