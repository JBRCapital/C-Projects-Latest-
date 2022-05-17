using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace UpdateSalesforceData
{
    public class Helper
    {
        static int updateBatchNumber; 
        public static int UpdateBatchNumber { get { 
                if (updateBatchNumber == 0) { updateBatchNumber = GetEpoc(); } return updateBatchNumber; } }

        public static int GetEpoc()
        {
            TimeSpan t = DateTime.UtcNow - new DateTime(1970, 1, 1);
            return (int)t.TotalSeconds;
        }

        private static Dictionary<string, string> basicAgreementData = null;
        public static Dictionary<string, string> BasicAgreementData { get { {
                    if (basicAgreementData != null) return basicAgreementData;

                    LogHelper.Logger.WriteOutput("Start - Get Agreement Data from Salesforce to Support Pay Profile and Transactions", Program.EmailTransactionLog);

                    var agreements = SalesforceDataHelper.SalesforceClient.GetRecordsFromSalesforce<AgreementData>("AgreementData",
                      @"SELECT id, approval_agreementNumber__c FROM Agreement__c where approval_agreementNumber__c <> null");

                    LogHelper.Logger.WriteOutput("End - Get Agreement Data from Salesforce to Support Pay Profile and Transactions", Program.EmailTransactionLog);

                    basicAgreementData = agreements.ToDictionary(p => p.approval_agreementNumber__c, p => p.id);

                    return basicAgreementData;
                } }
        }

        //public static bool IsValidEmailOld(string email, bool emptyIsValid)
        //{
        //    if (emptyIsValid && email.Length == 0) { return true; }
        //    if (email.EndsWith(".")) { return false; }

        //    const string expression = @"^((([a-z]|\d|[!#\$%&'\*\+\-\/=\?\^_`{\|}~]|[\u00A0-\uD7FF\uF900-\uFDCF\uFDF0-\uFFEF])+(\.([a-z]|\d|[!#\$%&'\*\+\-\/=\?\^_`{\|}~]|[\u00A0-\uD7FF\uF900-\uFDCF\uFDF0-\uFFEF])+)*)|((\x22)((((\x20|\x09)*(\x0d\x0a))?(\x20|\x09)+)?(([\x01-\x08\x0b\x0c\x0e-\x1f\x7f]|\x21|[\x23-\x5b]|[\x5d-\x7e]|[\u00A0-\uD7FF\uF900-\uFDCF\uFDF0-\uFFEF])|(\\([\x01-\x09\x0b\x0c\x0d-\x7f]|[\u00A0-\uD7FF\uF900-\uFDCF\uFDF0-\uFFEF]))))*(((\x20|\x09)*(\x0d\x0a))?(\x20|\x09)+)?(\x22)))@((([a-z]|\d|[\u00A0-\uD7FF\uF900-\uFDCF\uFDF0-\uFFEF])|(([a-z]|\d|[\u00A0-\uD7FF\uF900-\uFDCF\uFDF0-\uFFEF])([a-z]|\d|-|\.|_|~|[\u00A0-\uD7FF\uF900-\uFDCF\uFDF0-\uFFEF])*([a-z]|\d|[\u00A0-\uD7FF\uF900-\uFDCF\uFDF0-\uFFEF])))\.)+(([a-z]|[\u00A0-\uD7FF\uF900-\uFDCF\uFDF0-\uFFEF])|(([a-z]|[\u00A0-\uD7FF\uF900-\uFDCF\uFDF0-\uFFEF])([a-z]|\d|-|\.|_|~|[\u00A0-\uD7FF\uF900-\uFDCF\uFDF0-\uFFEF])*([a-z]|[\u00A0-\uD7FF\uF900-\uFDCF\uFDF0-\uFFEF])))\.?$";
        //    if (Regex.IsMatch(email, expression))
        //    {
        //        if (Regex.Replace(email, expression, string.Empty).Length == 0)
        //        {
        //            return true;
        //        }
        //        else
        //        {
        //            return false;
        //        }
        //    }
        //    else
        //    {
        //        return false;
        //    }
        //}

        

    }
}
