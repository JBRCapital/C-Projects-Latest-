using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace CallCreditApiDelegation.Models
{
    public class LimitationForDevelopingPurposesModel
    {
        public DateTime Today { get; set; }
        public int NumberOfRequestsToday { get; set; }
        public List<ClientsCountsForDevelopingPurposesModel> ClientsCounts { get; set; }
    }
}