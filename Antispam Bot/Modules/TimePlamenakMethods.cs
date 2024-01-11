using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Plamenak_Bot.Modules
{
    class TimePlamenakMethods
    {
        static int[] months = { 31, 28, 31, 30, 31, 30, 31, 31, 30, 31, 30, 31 };

        public static void GetPlamenakTime(ref int hours, ref int minutes, DateTime LastPlamenak)
        {
            int lday = LastPlamenak.Day, lmonth = LastPlamenak.Month, lyear = LastPlamenak.Year, lhour = LastPlamenak.Hour, lmin = LastPlamenak.Minute,
                nday = DateTime.Now.Day, nmonth = DateTime.Now.Month, nyear = DateTime.Now.Year, nhour = DateTime.Now.Hour, nmin = DateTime.Now.Minute;

            int daysdiff = 0;
            bool ok = false;

            int currYear = lyear;

            for (int i = lmonth - 1; ok != true; i++)
            {
                if (lmonth == nmonth && lyear == nyear)
                {
                    hours = ((nday - lday) * 24) - (lhour - nhour);
                    ok = true;
                }
                else if (i == lmonth - 1 && currYear == lyear)
                    daysdiff += months[i] - lday;
                else if (i == nmonth - 1 && currYear == nyear)
                {
                    daysdiff += nday;
                    hours = daysdiff * 24 - (lhour - nhour);
                    ok = true;
                }
                else
                    daysdiff += months[i];

                if (i == 11)
                {
                    i = -1;
                    currYear += 1;
                }
            }

            minutes = (lmin - nmin);

            if (minutes > 0)
            {
                minutes = 60 - minutes;
                hours -= 1;
            }
            else
                minutes *= -1;
        }
    }
}