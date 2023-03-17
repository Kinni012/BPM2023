using System;

namespace CobotAssignmentAndJobShopSchedulingProblem
{
    public static class ConversionMethods
    {
        public static DateTime StringToDate(string s)
        {
            DateTime dtDateTime = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);
            dtDateTime = dtDateTime.AddSeconds(Convert.ToInt32(s)).ToLocalTime();
            return dtDateTime;
        }
    }
}
