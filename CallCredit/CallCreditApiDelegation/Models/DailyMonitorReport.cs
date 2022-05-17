using System;

namespace CallCreditApiDelegation.Models
{
    public class DailyMonitorReport
    {
        public long Id { get; set; }
        public int DailyRequestsCounter { get; set; }
        public DateTime RequestDate { get; set; }
    }
}