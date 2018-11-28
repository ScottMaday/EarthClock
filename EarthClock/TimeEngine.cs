using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Foundation;
using UIKit;

namespace EarthClock
{
    public static class TimeEngine
    {
        public static float DegreesToRadians(float Degrees)
        {
            return Degrees * (float)(Math.PI / 180);
        }

        private static DateTime LastSummerSolstice(DateTime UNow)
        {
            if(UNow.Month < 6) // june
            {
                return new DateTime(UNow.Year - 1, 6, 21);
            }
            else
            {
                return new DateTime(UNow.Year, 6, 21);
            }
        }

        private static TimeSpan TimeSinceSolstice(DateTime UNow)
        {
            return UNow - LastSummerSolstice(UNow);
        }

        public static float YearAngle()
        {
            DateTime UNow = DateTime.UtcNow;
            float YearPortion = (float)TimeSinceSolstice(UNow).TotalMinutes / 525600;
            return (float)Math.PI * 2 * YearPortion;
        }

        public static float DayAngle()
        {
            DateTime UNow = DateTime.UtcNow;
            TimeSpan TimeSinceMorning = UNow - DateTime.Today.AddHours(12);
            float DayPortion = (float)TimeSinceMorning.TotalSeconds / 86400;
            float SiderealAdjust = (float)TimeSinceSolstice(UNow).TotalDays * 0.00273032f;
            DayPortion -= SiderealAdjust;
            return (float)Math.PI * 2 * DayPortion;
        }
    }
}