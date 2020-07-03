using System;
using System.Collections.Generic;
using System.Text;

namespace ZmaniosClock
{
    /// <summary>
    /// Represents a time-of-day in "degtime".
    /// This is an alternative time measure system where the day is divided into 360 degrees each of which is divided into 60 minutes, of 60 seconds of 60 thirds of 60 fourths and so on.
    /// The normal time system is called (here) "regTime" and it divides the day into 24 hours of 60 minutes of 60 seconds each. 
    /// Each Degree of degtime is 4 minutes of regtime. 
    /// Each hour of regtime is 15 Degrees of degtime.
    /// Each Minute of degtime is 4 seconds of regtime.
    /// Each minute of regtime is 15 Minutes of degtime.
    /// Each second of regtime is 15 Seconds of degtime.
    /// </summary>
    public class DegreeTime
    {
        /// <summary>
        /// Number of degrees in a "tick" (one ten-millionth of a regtime second)
        /// This number is the value of the equation: 360 / 24 / 60 / 60 / 1000 / 10000
        /// </summary>
        private const double DEGS_IN_TICK = 0.0000000004166666666666666667;
        private double _degrees;

        /// <summary>
        /// Gets the Degrees part of this degtime
        /// </summary>
        public int Degrees
        {
            get
            {
                return (int)this._degrees;
            }
        }

        /// <summary>
        /// Gets the Minutes part of this degtime
        /// </summary>
        public int Minutes
        {
            get
            {
                //No need to truncate as we know we are dealing only with positive numbers
                return (int)((this._degrees * 60) % 60);
            }
        }

        /// <summary>
        /// Gets the Seconds part of this degtime
        /// </summary>
        public int Seconds
        {
            get
            {
                //No need to truncate as we know we are dealing only with positive numbers
                return (int)((this._degrees * 3600) % 60);
            }
        }

        /// <summary>
        /// Gets the Thirds part of this degtime
        /// </summary>
        public int Thirds
        {
            get
            {
                //No need to truncate as we know we are dealing only with positive numbers
                return (int)((this._degrees * 216000) % 60);
            }
        }

        public DegreeTime() : this(DateTime.Now) { }

        public DegreeTime(int degrees, int minutes, int seconds, int thirds)
        {
            if (degrees < 0)
            {
                throw new ArgumentOutOfRangeException("degrees", degrees, "degrees can not be a negative number.");
            }
            if (minutes < 0)
            {
                throw new ArgumentOutOfRangeException("minutes", minutes, "minutes can not be a negative number.");
            }
            if (seconds < 0)
            {
                throw new ArgumentOutOfRangeException("seconds", seconds, "seconds can not be a negative number.");
            }
            if (thirds < 0)
            {
                throw new ArgumentOutOfRangeException("thirds", thirds, "thirds can not be a negative number.");
            }

            this._degrees = (double)(degrees +
                (minutes / 60M) +
                (seconds / 3600M) +
                (thirds / 216000M));
            /* Note: Double math was found to be not accurate enough (thirds were often off by one) 
               so we use decimals for the actual math. */
        }

        public DegreeTime(double degrees)
        {
            if (degrees < 0)
            {
                throw new ArgumentOutOfRangeException("degrees", degrees, "degrees can not be a negative number.");
            }

            this._degrees = degrees;
        }

        public DegreeTime(TimeSpan ts) : this(DEGS_IN_TICK * ts.Ticks) { }

        public DegreeTime(DateTime dt) : this(dt.TimeOfDay) { }

        public TimeSpan ToTimeSpan()
        {
            return new TimeSpan((long)(this._degrees / DEGS_IN_TICK));
        }

        public static DegreeTime FromTimeSpan(TimeSpan ts)
        {
            return new DegreeTime(DEGS_IN_TICK * ts.Ticks);
        }

        public static TimeSpan ToTimeSpan(DegreeTime dt)
        {
            return dt.ToTimeSpan();
        }


        /// <summary>
        /// Gets this degtime in "time" notation. For example, degtime 124° 52′ 9″ will be returned as 124:52:09
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return String.Format("{0}:{1:00}:{2:00}", Degrees, Minutes, Seconds);
        }

        /// <summary>
        /// Gets this degtime represented in full and proper degree/minute/second/third notation: 124° 52′ 9″ 41‴
        /// </summary>
        /// <returns></returns>
        public string ToLongString()
        {
            return String.Format("{0}° {1}′ {2}″ {3}‴", Degrees, Minutes, Seconds, Thirds);
        }

        /// <summary>
        /// Gets this degtime represented in degree/minute/second notation: 124° 52′ 9″
        /// </summary>
        /// <returns></returns>
        public string ToMediumString()
        {
            return String.Format("{0}° {1}′ {2}″", Degrees, Minutes, Seconds);
        }

        /// <summary>
        /// Gets this degtime represented in degree/minute notation: 177° 8′
        /// </summary>
        /// <returns></returns>        
        public string ToShortString()
        {
            return String.Format("{0}° {1}′", Degrees, Minutes);
        }

        /// <summary>
        /// Gets the full degtime in degrees
        /// </summary>
        public double GetFullDegrees()
        {
            return this._degrees;
        }
    }
}
