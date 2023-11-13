using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Amper.nemesis3.Common.Util
{
    public class TimeUtils
    {       

        public static Int64 DateTimeToUnix(DateTime dateTime)
        {
            return Convert.ToInt64((dateTime - new DateTime(1970, 1, 1)).TotalSeconds);
        }

        public static Int64 DateTimeToUnix_Milliseconds(DateTime dateTime)
        {
            return Convert.ToInt64((dateTime - new DateTime(1970, 1, 1)).TotalMilliseconds);
        }

        public static DateTime UnixToDateTimeUTC(Int64 seconds)
        {
            return new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc).AddSeconds(seconds);            
        }
    }
}
