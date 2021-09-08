using System;
using Light.GuardClauses;

namespace Synnotech.Migrations.Core.Int64TimestampVersions
{
    public static class TimestampParser
    {
        public static bool TryParseTimestamp(string timestamp, out long int64Timestamp)
        {
            // The formats that we accept are:
            // 2021-09-07T07:31Z (Length 17)
            // 2021-09-07T07:32:42Z
            if (!CheckStructureOfIso8601Timestamp(timestamp))
                goto ParsingFailed;
            if (!TryParseYear(timestamp, out var year))
                goto ParsingFailed;
            if (!TryParseMonth(timestamp, out var month))
                goto ParsingFailed;
            if (!TryParseDay(timestamp, out var day))
                goto ParsingFailed;
            if (!TryParseHour(timestamp, out var hour))
                goto ParsingFailed;
            if (!TryParseMinute(timestamp, out var minute))
                goto ParsingFailed;
            if (!TryParseSecond(timestamp, out var second))
                goto ParsingFailed;
            if (!IsValidTimestamp(year, month, day, hour, minute, second))
                goto ParsingFailed;

            int64Timestamp = ConvertToInt64(year, month, day, hour, minute, second);
            return true;

            ParsingFailed:
            int64Timestamp = default;
            return false;
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

        private static bool IsValidTimestamp(int year, int month, int day, int hour, int minute, int second)
        {
            if (month is < 1 or > 12)
                return false;
            if (day is < 1 or > 31)
                return false;
            if (hour is < 0 or > 23)
                return false;
            if (minute is < 0 or > 59)
                return false;
            if (second is < 0 or > 59)
                return false;

            if (day > DateTime.DaysInMonth(year, month))
                return false;

            return true;
        }

        private static bool CheckStructureOfIso8601Timestamp(string timestamp)
        {
            // ReSharper disable once ConditionIsAlwaysTrueOrFalse -- can be null for callers that have Nullable disabled
            if (timestamp is null)
                return false;
            if (timestamp.Length is not 17 and not 20)
                return false;
            if (timestamp[4] != '-' ||
                timestamp[7] != '-' ||
                timestamp[10] != 'T' ||
                timestamp[13] != ':')
            {
                return false;
            }

            if (timestamp.Length == 17 && timestamp[16] != 'Z')
                return false;

            if (timestamp.Length == 20 && (timestamp[16] != ':' || timestamp[19] != 'Z'))
                return false;

            return true;
        }

        private static bool TryParseYear(string timestamp, out int year)
        {
            if (!timestamp[0].TryParseDigit(out year))
                goto ParsingFailed;
            if (!timestamp[1].TryParseDigitAndCombine(ref year))
                goto ParsingFailed;
            if (!timestamp[2].TryParseDigitAndCombine(ref year))
                goto ParsingFailed;
            if (!timestamp[3].TryParseDigitAndCombine(ref year))
                goto ParsingFailed;

            return true;

            ParsingFailed:
            year = default;
            return false;
        }

        private static bool TryParseMonth(string timestamp, out int month) =>
            timestamp.TryParseTwoDigits(5, out month);

        private static bool TryParseDay(string timestamp, out int day) =>
            timestamp.TryParseTwoDigits(8, out day);

        private static bool TryParseHour(string timestamp, out int hour) =>
            timestamp.TryParseTwoDigits(11, out hour);

        private static bool TryParseMinute(string timestamp, out int minute) =>
            timestamp.TryParseTwoDigits(14, out minute);

        private static bool TryParseSecond(string timestamp, out int second)
        {
            if (timestamp.Length == 17)
            {
                second = 0;
                return true;
            }

            if (timestamp.Length == 20)
                return timestamp.TryParseTwoDigits(17, out second);

            second = default;
            return false;
        }

        private static bool TryParseTwoDigits(this string timestamp, int startingIndex, out int value)
        {
            if (!timestamp[startingIndex].TryParseDigit(out value))
                goto ParsingFailed;
            if (!timestamp[startingIndex + 1].TryParseDigitAndCombine(ref value))
                goto ParsingFailed;

            return true;

            ParsingFailed:
            value = default;
            return false;
        }

        private static bool TryParseDigitAndCombine(this char character, ref int existingValue)
        {
            if (!character.TryParseDigit(out var digit))
                return false;

            existingValue *= 10;
            existingValue += digit;
            return true;
        }

        private static bool TryParseDigit(this char character, out int digit)
        {
            if (!character.IsDigit())
            {
                digit = default;
                return false;
            }

            digit = character - '0';
            return true;
        }
    }
}