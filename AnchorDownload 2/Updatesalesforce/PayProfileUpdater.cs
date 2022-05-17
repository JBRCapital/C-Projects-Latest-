using SalesforceDataLibrary;
using System.Data.SqlClient;

namespace UpdateSalesforceData
{
    public class PayProfileUpdater
    {
        static string slqObjectName = "AgreementPayProfile__c";
        private static SqlDataReader GetDataFromSQL()
        {
            return new SqlCommand("SELECT * FROM SQL_SF_Sync_Agreement_PayProfiles() Order By PayProAgreementNumber,PayDate",
                                   SQLDataConnectionHelper.SqlConnection) { CommandTimeout = 0 }.ExecuteReader();
        }

        public static void Run()
        {
            SalesforceDataHelper.SalesforceClient.BulkUpsertFromSQL("AgreementPayProfile", slqObjectName, "PayProID__c",
                true, GetDataFromSQL, agreementPayProfileData => 
                    !Helper.BasicAgreementData.ContainsKey(agreementPayProfileData["PayProAgreementNumber"].ToString())
                ? null : new CustomSObject()
                {
                    { "PayProID__c",                    SQLDataHelper.Helper.GetDouble(agreementPayProfileData["PayProID"]) },
                    { "Name",                           string.Concat(agreementPayProfileData["PayProAgreementNumber"].ToString() , " - " + SQLDataHelper.Helper.GetDate(agreementPayProfileData["PayDate"])) },
                    { "Agreement__c",                   Helper.BasicAgreementData[agreementPayProfileData["PayProAgreementNumber"].ToString()] },
                    { "Sentinel_Agreement_Number__c",   agreementPayProfileData["PayProAgreementNumber"].ToString() },
                    { "PayDate__c",                     SQLDataHelper.Helper.GetDate(agreementPayProfileData["PayDate"]) },
                    { "Instalment__c",                  SQLDataHelper.Helper.GetDouble(agreementPayProfileData["InstalmentValue"]) },
                    { "Principle__c",                   SQLDataHelper.Helper.GetDouble(agreementPayProfileData["Principle"]) },
                    { "Interest__c",                    SQLDataHelper.Helper.GetDouble(agreementPayProfileData["Interest"]) },
                    { "Fee__c",                         SQLDataHelper.Helper.GetDouble(agreementPayProfileData["Fee"]) },
                    { "VATonFee__c",                    SQLDataHelper.Helper.GetDouble(agreementPayProfileData["VATonFee"]) },
                    { "PayFallenDue__c",                SQLDataHelper.Helper.GetBoolean(agreementPayProfileData["PayFallenDue"]) },
                    { "UpdateBatchNumber__c", Helper.UpdateBatchNumber }
                }, null, 1000);

            //Delete old PayProfiles
            SalesforceDataHelper.SalesforceClient.DeleteByBatchId(slqObjectName, "UpdateBatchNumber__c", Helper.UpdateBatchNumber, Program.EmailTransactionLog, Program.EmailErrorLog);
        }

    }
}