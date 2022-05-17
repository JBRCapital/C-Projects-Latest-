using AppConfiguration;
using AutoPasswordChangerDBWrapper;
using CallCreditPasswordChangerWrapper;
using System;

namespace CallCreditAutoPasswordChanger
{
    class AutoPasswordChanger
    {
        /// <summary>
        /// the main application entry point, reads the settings, checks if a password change is due
        /// if it need to change the password, it does with updating the database and the config file
        /// then it exits
        /// </summary>
        /// <param name="args"></param>
        static void Main(string[] args)
        {
            var appSettings = InitializeApplicationSettings();
            var apiWrapper = new ApiPasswordChangerWrapper(appSettings);
            if (apiWrapper.CheckIfPasswordChangeIsDue())
            {
                var NewPassword = GenerateNewRandomPassword();
                if (apiWrapper.ChangePasswordIfDue(NewPassword))
                    ConfigurationHelper.SetCurrentPassword(NewPassword);
            }

            var dbWrapper = new PasswordChangerDBWrapper(appSettings);
            dbWrapper.DeleteOldMonitoringRecords();

            Environment.Exit(0);
        }

        /// <summary>
        /// read the config file and populate the ApplicationSettings properties from their corresponding fields
        /// </summary>
        /// <returns>the ApplicationSettings instance created</returns>
        static ApplicationSettings InitializeApplicationSettings()
        {
            return new ApplicationSettings()
            {
                SQLServerDatabase = ConfigurationHelper.GetSQLServerDatabase(),
                Table = ConfigurationHelper.GetTableName(),

                PasswordField = ConfigurationHelper.GetPasswordFieldName(),
                PasswordLastChangeDateField = ConfigurationHelper.GetpasswordLastChangeDateFieldName(),

                connectionString = ConfigurationHelper.GetConnectionString(),
                CompanyName = ConfigurationHelper.GetCompanyName(),
                //TESTCompanyName = ConfigurationHelper.GetTestCompanyName(),

                Username = ConfigurationHelper.GetUserName(),
                Password = ConfigurationHelper.GetCurrentPassword(),
                PasswordLastChangeDate = ConfigurationHelper.GetCurrentPasswordLastChangeDate(),

                DaysBeforeCleaningUp = ConfigurationHelper.DaysBeforeCleaningUp()
            };
        }

        /// <summary>
        /// generate a random password to be used as a replacment to the current password
        /// </summary>
        /// <returns>the generated password</returns>
        //static string GenerateNewRandomPassword() => Membership.GeneratePassword(8, 0);
        static string GenerateNewRandomPassword() => Guid.NewGuid().ToString("d").Substring(0, 8);
    }
}
