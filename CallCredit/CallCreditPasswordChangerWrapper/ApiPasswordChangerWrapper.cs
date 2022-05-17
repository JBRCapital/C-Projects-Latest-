using AppConfiguration;
using AutoPasswordChangerDBWrapper;
using Callcredit.CallReport7;
using System;

namespace CallCreditPasswordChangerWrapper
{
    public class ApiPasswordChangerWrapper
    {
        private PasswordChangerDBWrapper dbwrapper;
        public ApplicationSettings ApplicationSettings { get; }

        public ApiPasswordChangerWrapper(ApplicationSettings applicationSettings)
        {
            dbwrapper = new PasswordChangerDBWrapper(applicationSettings);
            ApplicationSettings = applicationSettings;
        }

        /// <summary>
        /// checks if a password change is due
        /// </summary>
        /// <returns>boolean to indicate if a password change is due</returns>
        public bool CheckIfPasswordChangeIsDue() => DateTime.Today >= dbwrapper.NextChangeDateDue();

        /// <summary>
        /// checks if a password change is due, if it's due, it changes the password
        /// </summary>
        /// <param name="NewPassword">the new password to use</param>
        /// <returns>returns the result of the password change if a change was due, else return false</returns>
        public bool ChangePasswordIfDue(string NewPassword) => CheckIfPasswordChangeIsDue() ? ChangePassword(NewPassword) : false;

        /// <summary>
        /// changes the api password by composing the password change soap request
        /// and send it to the server
        /// </summary>
        /// <param name="newPassword">the new password to change to</param>
        /// <returns>returns a boolean to indicate if the password change request was successful</returns>
        private bool ChangePassword(string newPassword)
        {
            var apiProxy = new CallReport7()
            {
                Url = "https://www.callcreditsecure.co.uk/Services/CallReport/CallReport7.asmx"
            };

            callcreditheaders apiCredentials = new callcreditheaders
            {
                company = ApplicationSettings.CompanyName,
                username = ApplicationSettings.Username,
                password = ApplicationSettings.Password
            };
            apiProxy.callcreditheadersValue = apiCredentials;

            try
            {
                var changed = apiProxy.ChangePassword07a(newPassword, newPassword);
                if (changed)
                    dbwrapper.ChangecurrentPassword(newPassword);

                return changed;
            }
            catch (Exception e)
            {
                return false;
            }
            finally
            {
                apiProxy.Dispose();
            }
        }
    }


}
