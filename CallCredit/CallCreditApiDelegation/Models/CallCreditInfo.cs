using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace CallCreditApiDelegation.Models
{
    [Table("CallCreditInfo")]
    public class CallCreditInfo
    {
        public long Id { get; set; }
        public string Password { get; set; }
        public DateTime passwordLastChangeDate { get; set; }
    }
}