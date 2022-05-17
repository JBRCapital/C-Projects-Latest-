using AppConfiguration;
using System;
using System.Data.SqlClient;

namespace AutoPasswordChangerDBWrapper
{
    /// <summary>
    /// a database wrapper to encapsulate the reading and updating operation for the database
    /// </summary>
    public class PasswordChangerDBWrapper
    {
        public ApplicationSettings AppSettings { get; }

        public PasswordChangerDBWrapper(ApplicationSettings appSettings)
        {
            AppSettings = appSettings;
        }

        /// <summary>
        /// query the database for the password's last change date
        /// </summary>
        /// <returns>the password's last change date</returns>
        public DateTime LastChangeDate()
        {
            DateTime LastChangeDate = new DateTime();
            using (var DbConnection = new SqlConnection(AppSettings.connectionString))
            {
                if (DbConnection != null)
                {
                    DbConnection.Open();
                    using (var cmd = new SqlCommand($"Select * from {AppSettings.SQLServerDatabase}.dbo.{AppSettings.Table}", DbConnection))
                    using (var reader = cmd.ExecuteReader())
                    {
                        if (reader.HasRows)
                        {
                            reader.Read();
                            LastChangeDate = reader.GetDateTime(2);
                        }
                        reader.Close();
                    }
                    DbConnection.Close();
                }
            }
            return LastChangeDate;
        }

        /// <summary>
        /// calculate the next password change due date
        /// </summary>
        /// <returns>the next password change due date</returns>
        public DateTime NextChangeDateDue() => LastChangeDate().AddDays(60);

        /// <summary>
        /// check if the database has any records, i.e. already has a password
        /// </summary>
        /// <returns></returns>
        public bool NoPasswordExists()
        {
            bool hasRows = false;
            using (var DbConnection = new SqlConnection(AppSettings.connectionString))
            {
                if (DbConnection != null)
                {
                    DbConnection.Open();
                    using (var cmd = new SqlCommand($"Select * from {AppSettings.SQLServerDatabase}.dbo.{AppSettings.Table}", DbConnection))
                    using (var reader = cmd.ExecuteReader())
                    {
                        hasRows = reader.HasRows;
                        reader.Close();
                    }
                    DbConnection.Close();
                }
            }
            return hasRows;
        }

        /// <summary>
        /// query the database for the password currently in use by the application
        /// </summary>
        /// <returns>the current password used by the application</returns>
        public string CurrentPasswordInUse()
        {
            string CurrentPassword = string.Empty;
            using (var DbConnection = new SqlConnection(AppSettings.connectionString))
            {
                if (DbConnection != null)
                {
                    DbConnection.Open();
                    using (var cmd = new SqlCommand($"Select * from {AppSettings.SQLServerDatabase}.dbo.{AppSettings.Table}", DbConnection))
                    using (var reader = cmd.ExecuteReader())
                    {
                        if (reader.HasRows)
                        {
                            reader.Read();
                            CurrentPassword = reader.GetString(0);
                        }
                        reader.Close();
                    }
                    DbConnection.Close();
                }
            }
            return CurrentPassword;
        }

        /// <summary>
        /// change the current inuse password used by the application and updates
        /// the lastchangedate to the current date
        /// </summary>
        /// <param name="Password">the new password to replace the old one</param>
        /// <returns>a boolean to indicate if the database was updated successfully</returns>
        public bool ChangecurrentPassword(string Password)
        {
            try
            {
                //update the database
                using (var DbConnection = new SqlConnection(AppSettings.connectionString))
                {
                    if (DbConnection != null)
                    {
                        DbConnection.Open();
                        using (var cmd = new SqlCommand($"Update {AppSettings.SQLServerDatabase}.[dbo].[{AppSettings.Table}] set [{AppSettings.PasswordField}] = '{Password}', [{AppSettings.PasswordLastChangeDateField}] = '{DateTime.Now.Date.ToShortDateString()}'", DbConnection))
                        using (var adapter = new SqlDataAdapter())
                        {
                            adapter.UpdateCommand = cmd;
                            adapter.UpdateCommand.ExecuteNonQuery();
                        }
                        DbConnection.Close();
                    }
                }

                return true;
            }
            catch (Exception e)
            {
                return false;
            }
        }


        /// <summary>
        /// delete old records
        /// </summary>
        /// <returns>the current password used by the application</returns>
        public void DeleteOldMonitoringRecords()
        {
            using (var DbConnection = new SqlConnection(AppSettings.connectionString))
            {
                if (DbConnection != null)
                {
                    DbConnection.Open();
                    using (var cmd = new SqlCommand($"delete from {AppSettings.SQLServerDatabase}.dbo.DailyMonitorReports WHERE RequestDate < DATEADD(dd,-{AppSettings.DaysBeforeCleaningUp},GETDATE())", DbConnection))
                    {
                        cmd.ExecuteReader();
                    }
                    DbConnection.Close();
                }
            }
        }
    }
}
