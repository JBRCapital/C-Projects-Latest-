using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web;

namespace CallCreditApiDelegation.Models
{
    public class ApiContext : DbContext
    {
        public DbSet<IPMonitorEntry> iPMonitorEntries { get; set; }
        public DbSet<DailyMonitorReport> dailyMonitorReports { get; set; }
        public DbSet<CallCreditInfo> CallCreditInformation { get; set; }

        public ApiContext() : base("ApiContext")
        {

        }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
        }
    }
}