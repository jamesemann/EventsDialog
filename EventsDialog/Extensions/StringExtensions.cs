using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Microsoft.Recognizers.Text.DateTime;

namespace EventsDialog.Extensions
{
    public static class StringExtensions
    {
        public static (int,int) GetYearAndMonth(this string textValue)
        {
            var year = -1;
            var month = -1;

            var dt = DateTimeRecognizer.RecognizeDateTime(textValue, "en-GB");

            var fromTimex = ((List<Dictionary<string, string>>) dt.FirstOrDefault()?.Resolution.FirstOrDefault().Value)?.FirstOrDefault()?["timex"];

            if (fromTimex != null)
            {
                var match = new Regex(@"(?<year>....)-(?<month>..)").Match(fromTimex);
                var monthGroup = match.Groups["month"];
                var yearGroup = match.Groups["year"];

                if (monthGroup.Success)
                {
                    month = int.Parse(monthGroup.Value);
                }

                if (yearGroup.Success && yearGroup.Value != "XXXX")
                {
                    year = int.Parse(yearGroup.Value);
                }
                else
                {
                    year = YearForNextNearestMonth(int.Parse(monthGroup.Value));
                }
            }

            return (year, month);
        }

        private static int YearForNextNearestMonth(int month)
        {
            var nextTwelveMonthsIncludingThisMonth = new List<DateTime>();
            for (var i = 0; i < 12; i++)
            {
                nextTwelveMonthsIncludingThisMonth.Add(DateTime.Now.AddMonths(i));
            }

            return nextTwelveMonthsIncludingThisMonth.Where(x => x.Month == month).Min().Year;
        }
    }
}