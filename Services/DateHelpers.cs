using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace StellaTheStaffe.Services
{
    public static class DateHelpers
    {
        /// <summary>
        /// Get weeks from birth for a date
        /// </summary>
        /// <returns>Week the date is in (e.g. one day after birth is week 1)</returns>
        public static int GetWeeksFromBirth(this DateTime date)
        {
            return (int)(1 + Math.Ceiling((date - Constants.BirthDate).Days / 7f));
        }

        /// <summary>
        /// Get months from birth for a date
        /// </summary>
        /// <returns>Months since birth, rounded to nearest 0.5</returns>
        public static float GetMonthsFromBirth(this DateTime date)
        {
            return (float)Math.Round((date.GetWeeksFromBirth() / 4f) * 2, MidpointRounding.AwayFromZero) / 2;
        }

        public static (DateTime startDate, DateTime endDate) GetDatesInWeek(int weekNo)
        {
            var startDate = Constants.BirthDate.AddDays(7 * (weekNo - 1));
            var endDate = startDate.AddDays(6);
            return (startDate, endDate);
        }
    }
}
