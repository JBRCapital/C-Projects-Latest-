using CallCreditApiDelegation.Models;
using System;
using System.Configuration;
using System.Linq;

namespace CallCreditAutoPasswordChanger
{
    /// <summary>
    /// a helper class that is used to encapsulate the logic of reading and writting to the configuration file
    /// </summary>
    public static class ConfigurationHelper
    {
        public static Configuration config = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);

        public static string GetSQLServerDatabase() => ReadConfig("SQLServerDatabase");

        public static string GetTableName() => ReadConfig("Table");

        public static string GetPasswordFieldName() => ReadConfig("PasswordField");

        public static string GetpasswordLastChangeDateFieldName() => ReadConfig("passwordLastChangeDateField");

        public static string GetConnectionString() => ReadConfig("connectionString");

        public static string GetCompanyName() => ReadConfig("CompanyName");

        public static string GetTestCompanyName() => ReadConfig("TESTCompanyName");

        public static string GetUserName() => ReadConfig("Username");

        public static string GetCurrentPassword()
        {
            using (var apicontext = new ApiContext())
            {
                return apicontext.CallCreditInformation.First().Password;
            }
        }

        public static DateTime GetCurrentPasswordLastChangeDate()
        {
            using (var apicontext = new ApiContext())
            {
                return apicontext.CallCreditInformation.First().passwordLastChangeDate;
            }
        }

        public static string DaysBeforeCleaningUp() => ReadConfig("DaysBeforeCleaningUp");

        public static bool SetCurrentPassword(string NewPassword)
        {
            try
            {
                config.AppSettings.Settings.Remove("Password");
                config.AppSettings.Settings.Add("Password", NewPassword);
                config.Save(ConfigurationSaveMode.Modified);
                return true;
            }
            catch (Exception e)
            {
                return false;
            }
        }

        public static string ReadConfig(string keyName) => config.AppSettings.Settings[keyName].Value ?? "Not Found";
    }
}
