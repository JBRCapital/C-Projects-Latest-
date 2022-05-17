using SalesforceDataLibrary;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;

namespace UpdateSalesforceData
{
    class ProposalUpdater
    {
        public static void Run()
        {
            var proposalSentinelNumberAndIds = getfromProposalRecordIsLatestIds();

           SalesforceDataHelper.SalesforceClient.BulkUpsertFromSQL("Proposals","Proposal__c", "id",
                true, GetProposalDataFromSQL, proposalData =>
                    !proposalSentinelNumberAndIds.ContainsKey(SQLDataHelper.Helper.GetDouble(proposalData["ProposalId"]).Value) ? null : new CustomSObject()
                    {
                        { "id", proposalSentinelNumberAndIds[SQLDataHelper.Helper.GetDouble(proposalData["ProposalId"]).Value]},
                        { "dateTimeProposalFirstAccepted__c",            SQLDataHelper.Helper.GetDateTime(proposalData["FirstDateTimeAccepted"])},
                        { "firstAcceptedStatus__c",                      SQLDataHelper.Helper.GetStringMaxLength(proposalData["FirstAcceptedStatus"].ToString().Trim(),255)},
                        //{ "introducer_commissionAmount__c",              DataHelper.GetDouble(proposalData["IntroducerCommission"])},
                        //{ "supplier_commissionAmount__c",                DataHelper.GetDouble(proposalData["SupplierCommission"])},
                        { "firstDecisionedDateTime__c",                  SQLDataHelper.Helper.GetDateTime(proposalData["FirstDecisionedDateTime"])},
                        { "firstDecisionedStatus__c",                    SQLDataHelper.Helper.GetStringMaxLength(proposalData["FirstDecisionedStatus"].ToString().Trim(),255)},
                        { "timeTakenIn_NewProposals__c",                 SQLDataHelper.Helper.GetDouble(proposalData["Active Proposals"]) +
                                                                         SQLDataHelper.Helper.GetDouble(proposalData["New Proposals"])},
                        { "timeTakenIn_ReferredForInformation__c",       SQLDataHelper.Helper.GetDouble(proposalData["Referred for Information"])},
                        { "timeTakenIn_CreditManager__c",                SQLDataHelper.Helper.GetDouble(proposalData["Credit Manager"])},
                        { "timeTakenIn_SeniorCreditManager__c",          SQLDataHelper.Helper.GetDouble(proposalData["Senior Credit Manager"])},
                        { "timeTakenIn_CreditCommittee__c",              SQLDataHelper.Helper.GetDouble(proposalData["Credit Committee"])},
                        { "timeTakenIn_Funder__c",                       SQLDataHelper.Helper.GetDouble(proposalData["Funder"])},
                        { "timeTakenIn_Decisioned__c",                   SQLDataHelper.Helper.GetDouble(proposalData["Decisioned"])},
                        { "timeTakenIn_TotalCredit__c",                  SQLDataHelper.Helper.GetDouble(proposalData["Credit Manager"]) +
                                                                         SQLDataHelper.Helper.GetDouble(proposalData["Credit Committee"]) +
                                                                         SQLDataHelper.Helper.GetDouble(proposalData["Funder"])},
                        { "timeTakenIn_CreditPlusReferred__c",           SQLDataHelper.Helper.GetDouble(proposalData["Referred for Information"]) +
                                                                         SQLDataHelper.Helper.GetDouble(proposalData["Active Proposals"]) +
                                                                         SQLDataHelper.Helper.GetDouble(proposalData["New Proposals"])},
                        { "timeTakenIn_CreditPlusSalesSupport__c",       SQLDataHelper.Helper.GetDouble(proposalData["Active Proposals"]) +
                                                                         SQLDataHelper.Helper.GetDouble(proposalData["New Proposals"]) +
                                                                         SQLDataHelper.Helper.GetDouble(proposalData["Credit Manager"]) +
                                                                         SQLDataHelper.Helper.GetDouble(proposalData["Credit Committee"]) +
                                                                         SQLDataHelper.Helper.GetDouble(proposalData["Funder"])},
                        { "timeTakenIn_AwaitingPayout__c",               SQLDataHelper.Helper.GetDouble(proposalData["Awaiting Payout"])},
                        { "timeTaIn_CreditPlusSalesSupportPlusRefer__c", SQLDataHelper.Helper.GetDouble(proposalData["Active Proposals"]) +
                                                                         SQLDataHelper.Helper.GetDouble(proposalData["New Proposals"]) +
                                                                         SQLDataHelper.Helper.GetDouble(proposalData["Referred for Information"]) +
                                                                         SQLDataHelper.Helper.GetDouble(proposalData["Credit Manager"]) +
                                                                         SQLDataHelper.Helper.GetDouble(proposalData["Credit Committee"]) +
                                                                         SQLDataHelper.Helper.GetDouble(proposalData["Funder"])},
                        { "lastUpdatedFromSentinel__c" , SQLDataHelper.Helper.GetDateTime(DateTime.Now)}
                    }, null, 40);

            proposalSentinelNumberAndIds = null;

        }

        private static Dictionary<double, string> getfromProposalRecordIsLatestIds()
        {
            return SalesforceDataHelper.SalesforceClient.GetRecordsFromSalesforce<ProposalSentinelNumberAndIdData>(
                 "ProposalSentinelNumberAndIdData",
                 @"SELECT id, sentinalID__c FROM proposal__c WHERE isLatest__c = true")
                 .ToDictionary(x => x.sentinalID__c, y => y.Id);
        }

        private static SqlDataReader GetProposalDataFromSQL()
        {
            return new SqlCommand("SELECT * FROM SQL_SF_Sync_Proposals() Where CreatedDateTime >= GETDATE()-90 order by ProposalId desc", SQLDataConnectionHelper.SqlConnection) { CommandTimeout = 0 }.ExecuteReader();
        }

        
    }
}

