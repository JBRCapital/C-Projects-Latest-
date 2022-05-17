using SalesforceDataLibrary;
using System.Data.SqlClient;

namespace UpdateSalesforceData
{
    public class TransactionUpdater
    {
        static string slqObjectName = "AgreementTransaction__c";

        private static SqlDataReader GetDataFromSQL()
        {
            return new SqlCommand("SELECT * FROM SQL_SF_Sync_Agreement_Transactions() Order By TransAgreementNumber, TransCounter",
                                   SQLDataConnectionHelper.SqlConnection) { CommandTimeout = 0 }.ExecuteReader();
        }

        public static void Run()
        {
            SalesforceDataHelper.SalesforceClient.BulkUpsertFromSQL("Transactions",slqObjectName, "TransactionId__c",
                true, GetDataFromSQL, agreementTransactionData =>
                    !Helper.BasicAgreementData.ContainsKey(agreementTransactionData["TransAgreementNumber"].ToString())
                ? null : new CustomSObject()
                {
                    { "TransactionId__c",               SQLDataHelper.Helper.GetDouble(agreementTransactionData["TransCounter"]) },
                    { "Name",                           string.Concat(agreementTransactionData["TransAgreementNumber"].ToString(), " - " , SQLDataHelper.Helper.GetDate(agreementTransactionData["TransDate"])) },
                    { "Agreement__c",                   Helper.BasicAgreementData[agreementTransactionData["TransAgreementNumber"].ToString()] },
                    { "SentinelAgreementNumber__c",     agreementTransactionData["TransAgreementNumber"].ToString() },
                    { "TransactionDate__c",             SQLDataHelper.Helper.GetDate(agreementTransactionData["TransDate"]) },
                    { "Amount__c",                      SQLDataHelper.Helper.GetDouble(agreementTransactionData["Amount"]) },
                    { "InstalmentDue__c",               SQLDataHelper.Helper.GetDouble(agreementTransactionData["InstalmentDue"]) },
                    { "StandingOrder__c",               SQLDataHelper.Helper.GetDouble(agreementTransactionData["StandingOrder"]) },
                    { "BankTransfer__c",                SQLDataHelper.Helper.GetDouble(agreementTransactionData["BankTransfer"]) },
                    { "DirectDebit__c",                 SQLDataHelper.Helper.GetDouble(agreementTransactionData["DirectDebit"]) },
                    { "CardPayment__c",                 SQLDataHelper.Helper.GetDouble(agreementTransactionData["CardPayment"]) },
                    { "Cheque__c",                      SQLDataHelper.Helper.GetDouble(agreementTransactionData["Cheque"]) },
                    { "Adjustment__c",                  SQLDataHelper.Helper.GetDouble(agreementTransactionData["Adjustment"]) },
                    { "SettlementPayment__c",           SQLDataHelper.Helper.GetDouble(agreementTransactionData["SettlementPayment"]) },
                    { "ArrearsAdjustment__c",           SQLDataHelper.Helper.GetDouble(agreementTransactionData["ArrearsAdjustment"]) },
                    { "WriteOff__c",                    SQLDataHelper.Helper.GetDouble(agreementTransactionData["WriteOff"]) },
                    { "TransactionReversal__c",         SQLDataHelper.Helper.GetDouble(agreementTransactionData["TransactionReversal"]) },
                    { "ReturnedCheque__c",              SQLDataHelper.Helper.GetDouble(agreementTransactionData["ReturnedCheque"]) },
                    { "Refund__c",                      SQLDataHelper.Helper.GetDouble(agreementTransactionData["Refund"]) },
                    { "SettlementPaymentReversal__c",   SQLDataHelper.Helper.GetDouble(agreementTransactionData["SettlementPaymentReversal"]) },
                    { "ReturnedDD_RTP__c",              SQLDataHelper.Helper.GetDouble(agreementTransactionData["ReturnedDD_RTP"]) },
                    { "ReturnedDD_IC__c",               SQLDataHelper.Helper.GetDouble(agreementTransactionData["ReturnedDD_IC"]) },
                    { "ReturnedDD_NI__c",               SQLDataHelper.Helper.GetDouble(agreementTransactionData["ReturnedDD_NI"]) },
                    { "ReturnedDD_NA__c",               SQLDataHelper.Helper.GetDouble(agreementTransactionData["ReturnedDD_NA"]) },
                    { "ReturnedDD_ANYD__c",             SQLDataHelper.Helper.GetDouble(agreementTransactionData["ReturnedDD_ANYD"]) },
                    { "ReturnedDD_PD__c",               SQLDataHelper.Helper.GetDouble(agreementTransactionData["ReturnedDD_PD"]) },
                    { "ReturnedDD_AC__c",               SQLDataHelper.Helper.GetDouble(agreementTransactionData["ReturnedDD_AC"]) },
                    { "RebateOfInterest__c",            SQLDataHelper.Helper.GetDouble(agreementTransactionData["RebateOfInterest"]) },
                    { "PenaltyInterest__c",             SQLDataHelper.Helper.GetDouble(agreementTransactionData["PenaltyInterest"]) },
                    { "EarlySettlementFee__c",          SQLDataHelper.Helper.GetDouble(agreementTransactionData["EarlySettlementFee"]) },
                    { "LatePaymentFee__c",              SQLDataHelper.Helper.GetDouble(agreementTransactionData["LatePaymentFee"]) },
                    { "ExcessMileageCharge__c",         SQLDataHelper.Helper.GetDouble(agreementTransactionData["ExcessMileageCharge"]) },
                    { "FairWearAndTearCharge__c",       SQLDataHelper.Helper.GetDouble(agreementTransactionData["FairWearAndTearCharge"]) },
                    { "OtherFeesVATable__c",            SQLDataHelper.Helper.GetDouble(agreementTransactionData["OtherFeesVATable"]) },
                    { "OtherFeesNoVAT__c",              SQLDataHelper.Helper.GetDouble(agreementTransactionData["OtherFeesNoVAT"]) },
                    { "RescheduleFee__c",               SQLDataHelper.Helper.GetDouble(agreementTransactionData["RescheduleFee"]) },
                    { "RebateOfInterestReversal__c",    SQLDataHelper.Helper.GetDouble(agreementTransactionData["RebateOfInterestReversal"]) },
                    { "Balance__c",                     SQLDataHelper.Helper.GetDouble(agreementTransactionData["Balance"]) },
                    { "UpdateBatchNumber__c", Helper.UpdateBatchNumber }
                }, null, 1000);

            //Delete old PayProfiles
            SalesforceDataHelper.SalesforceClient.DeleteByBatchId(slqObjectName, "UpdateBatchNumber__c", Helper.UpdateBatchNumber, Program.EmailTransactionLog, Program.EmailErrorLog);
        }

    }
}
