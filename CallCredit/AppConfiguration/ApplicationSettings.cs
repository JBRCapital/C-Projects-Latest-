using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AppConfiguration
{
    public class ApplicationSettings
    {
        public string SQLServerDatabase { get; set; }
        public string Table { get; set; }
        public string PasswordField { get; set; }
        public string PasswordLastChangeDateField { get; set; }
        public string connectionString { get; set; }
        public string CompanyName { get; set; }
        //public string TESTCompanyName { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }
        public DateTime PasswordLastChangeDate { get; set; }
        public string DaysBeforeCleaningUp { get; set; }
    }
}
