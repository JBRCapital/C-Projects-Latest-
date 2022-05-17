using Salesforce.Common.Models;
using Salesforce.Common.Models.Json;
using Salesforce.Force;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Dynamic;
using System.Threading;
using System.Threading.Tasks;
using Salesforce.Common.Models.Xml;

namespace UpdateSalesforceData
{
    class ProposalUpdater
    {

        public static void UpdateProposalData()
        {
            Console.WriteLine(string.Concat("Started Sync of Proposals"));

            Program.salesforceClient.BulkUpsertFromSQL("Proposal__c", "id",
                GetProposalDataFromSQL, proposalData =>
                    new SObject()
                    {
                        { "sentinalID__c", proposalData["ProposalId"].ToString().Trim()},
                        { "dateTimeProposalFirstAccepted__c", DataHelper.GetDateTime(proposalData["FirstDateTimeAccepted"])},
                        { "firstAcceptedStatus__c", proposalData["FirstAcceptedStatus"].ToString().Trim()},
                        { "introducer_commissionAmount__c", DataHelper.GetDouble(proposalData["IntroducerCommission"])},
                        { "supplier_commissionAmount__c", DataHelper.GetDouble(proposalData["SupplierCommission"])},
                        { "firstDecisionedDateTime__c", DataHelper.GetDateTime(proposalData["FirstDecisionedDateTime"])},
                        { "firstDecisionedStatus__c", proposalData["FirstDecisionedStatus"].ToString().Trim()},
                        { "timeTakenIn_NewProposals__c", DataHelper.GetDouble(proposalData["Active Proposals"]) + 
                                                         DataHelper.GetDouble(proposalData["New Proposals"])},
                        { "timeTakenIn_ReferredForInformation__c", DataHelper.GetDouble(proposalData["Referred for Information"])},
                        { "timeTakenIn_CreditManager__c", DataHelper.GetDouble(proposalData["Credit Manager"])},
                        { "timeTakenIn_SeniorCreditManager__c", DataHelper.GetDouble(proposalData["Senior Credit Manager"])},
                        { "timeTakenIn_CreditCommittee__c", DataHelper.GetDouble(proposalData["Credit Committee"])},
                        { "timeTakenIn_Funder__c", DataHelper.GetDouble(proposalData["Funder"])},
                        { "timeTakenIn_Decisioned__c", DataHelper.GetDouble(proposalData["Decisioned"])},
                        { "timeTakenIn_TotalCredit__c", DataHelper.GetDouble(proposalData["Credit Manager"]) + 
                                                        DataHelper.GetDouble(proposalData["Credit Committee"]) +
                                                        DataHelper.GetDouble(proposalData["Funder"])},
                        { "timeTakenIn_CreditPlusReferred__c", DataHelper.GetDouble(proposalData["Referred for Information"]) + 
                                                               DataHelper.GetDouble(proposalData["Active Proposals"]) +
                                                               DataHelper.GetDouble(proposalData["New Proposals"])},
                        { "timeTakenIn_CreditPlusSalesSupport__c", DataHelper.GetDouble(proposalData["Active Proposals"]) +
                                                                   DataHelper.GetDouble(proposalData["New Proposals"]) +
                                                                   DataHelper.GetDouble(proposalData["Credit Manager"]) + 
                                                                   DataHelper.GetDouble(proposalData["Credit Committee"]) + 
                                                                   DataHelper.GetDouble(proposalData["Funder"])},
                        { "timeTakenIn_AwaitingPayout__c", DataHelper.GetDouble(proposalData["Awaiting Payout"])},
                        { "timeTaIn_CreditPlusSalesSupportPlusRefer__c", DataHelper.GetDouble(proposalData["Active Proposals"]) +
                                                                         DataHelper.GetDouble(proposalData["New Proposals"]) + 
                                                                         DataHelper.GetDouble(proposalData["Referred for Information"]) + 
                                                                         DataHelper.GetDouble(proposalData["Credit Manager"]) + 
                                                                         DataHelper.GetDouble(proposalData["Credit Committee"]) +
                                                                         DataHelper.GetDouble(proposalData["Funder"])}
                    });

            Console.WriteLine(string.Concat("Ended Sync of Proposals"));
        }

        private static SqlDataReader GetProposalDataFromSQL(SqlConnection sqlConn)
        {
            return new SqlCommand("ServiceLevelAgreement", sqlConn) { CommandTimeout = 0 }.ExecuteReader();
        }

        #region Legacy code
        private static void updateProposalRecordInSalesforce(ProposalData proposalData)
        {

            var queryString = string.Concat(@"SELECT id,
                                                     sentinalID__c,
                                                     introducer_commissionAmount__c,
                                                     supplier_commissionAmount__c, 
                                                     dateTimeProposalFirstAccepted__c,
                                                     firstAcceptedStatus__c,
                                                     firstDecisionedDateTime__c,
                                                     firstDecisionedStatus__c,
                                                     timeTakenIn_NewProposals__c,
                                                     timeTakenIn_ReferredForInformation__c,
                                                     timeTakenIn_CreditManager__c,
                                                     timeTakenIn_CreditCommittee__c,
                                                     timeTakenIn_Funder__c,
                                                     timeTakenIn_Decisioned__c,
                                                     timeTakenIn_TotalCredit__c,
                                                     timeTakenIn_CreditPlusReferred__c,
                                                     timeTakenIn_CreditPlusSalesSupport__c,
                                                     timeTaIn_CreditPlusSalesSupportPlusRefer__c
                                              FROM proposal__c
                                              WHERE isLatest__c = true and sentinalID__c = ", proposalData.sentinalID__c);

            QueryResult<ProposalData> anchorWebServices = null;

            Task.Run(async () =>
            {
                anchorWebServices = await Program.salesforceClient.QueryAsync<ProposalData>(queryString);
            }).Wait(Timeout.InfiniteTimeSpan);

            var changed = false;

            if (anchorWebServices.Records.Count > 0)
            {
                Console.WriteLine(string.Concat("Checking Proposal:", proposalData.sentinalID__c));
                dynamic obj = getUpdateObject(proposalData, anchorWebServices, ref changed);
                Console.WriteLine(string.Concat("Proposal Changed: ", changed ? "Yes" : "No"));

                if (changed)
                {
                    SuccessResponse successResponse = null;

                    Task.Run(async () =>
                    {
                        Console.WriteLine(string.Concat("Upserted Proposal:", proposalData.sentinalID__c));
                        var awsR = anchorWebServices.Records[0];
                        successResponse = await Program.salesforceClient.UpsertExternalAsync("Proposal__c", "id", awsR.id, obj);
                        Console.WriteLine(successResponse != null && successResponse.Success ? "Success" : "Failed");
                    }).Wait(Timeout.InfiniteTimeSpan);
                }
            }
        }

        private static dynamic getUpdateObject(ProposalData proposalData, QueryResult<ProposalData> anchorWebServices, ref bool changed)
        {
            dynamic obj = new ExpandoObject() as IDictionary<string, object>;
            var awsR = anchorWebServices.Records[0];

            DataHelper.setValue(awsR.introducer_commissionAmount__c, proposalData.introducer_commissionAmount__c, obj, "introducer_commissionAmount__c", ref changed);
            DataHelper.setValue(awsR.supplier_commissionAmount__c, proposalData.supplier_commissionAmount__c, obj, "supplier_commissionAmount__c", ref changed);

            //dateTimeProposalFirstAccepted__c
            DataHelper.setValue(awsR.dateTimeProposalFirstAccepted__c, proposalData.dateTimeProposalFirstAccepted__c, obj, "dateTimeProposalFirstAccepted__c", ref changed);
            DataHelper.setValue(awsR.firstAcceptedStatus__c, proposalData.firstAcceptedStatus__c, obj, "firstAcceptedStatus__c", ref changed);

            DataHelper.setValue(awsR.firstDecisionedDateTime__c, proposalData.firstDecisionedDateTime__c, obj, "firstDecisionedDateTime__c", ref changed);
            DataHelper.setValue(awsR.firstDecisionedStatus__c, proposalData.firstDecisionedStatus__c, obj, "firstDecisionedStatus__c", ref changed);

            DataHelper.setValue(awsR.timeTakenIn_NewProposals__c, proposalData.timeTakenIn_NewProposals__c, obj, "timeTakenIn_NewProposals__c", ref changed);
            DataHelper.setValue(awsR.timeTakenIn_ReferredForInformation__c, proposalData.timeTakenIn_ReferredForInformation__c, obj, "timeTakenIn_ReferredForInformation__c", ref changed);
            DataHelper.setValue(awsR.timeTakenIn_CreditManager__c, proposalData.timeTakenIn_CreditManager__c, obj, "timeTakenIn_CreditManager__c", ref changed);
            DataHelper.setValue(awsR.timeTakenIn_SeniorCreditManager__c, proposalData.timeTakenIn_SeniorCreditManager__c, obj, "timeTakenIn_SeniorCreditManager__c", ref changed);

            DataHelper.setValue(awsR.timeTakenIn_CreditCommittee__c, proposalData.timeTakenIn_CreditCommittee__c, obj, "timeTakenIn_CreditCommittee__c", ref changed);
            DataHelper.setValue(awsR.timeTakenIn_Funder__c, proposalData.timeTakenIn_Funder__c, obj, "timeTakenIn_Funder__c", ref changed);
            DataHelper.setValue(awsR.timeTakenIn_Decisioned__c, proposalData.timeTakenIn_Decisioned__c, obj, "timeTakenIn_Decisioned__c", ref changed);

            DataHelper.setValue(awsR.timeTakenIn_TotalCredit__c, proposalData.timeTakenIn_TotalCredit__c, obj, "timeTakenIn_TotalCredit__c", ref changed);
            DataHelper.setValue(awsR.timeTakenIn_CreditPlusReferred__c, proposalData.timeTakenIn_CreditPlusReferred__c, obj, "timeTakenIn_CreditPlusReferred__c", ref changed);
            DataHelper.setValue(awsR.timeTakenIn_CreditPlusSalesSupport__c, proposalData.timeTakenIn_CreditPlusSalesSupport__c, obj, "timeTakenIn_CreditPlusSalesSupport__c", ref changed);

            DataHelper.setValue(awsR.timeTakenIn_AwaitingPayout__c, proposalData.timeTakenIn_AwaitingPayout__c, obj, "timeTakenIn_AwaitingPayout__c", ref changed);

            DataHelper.setValue(awsR.timeTaIn_CreditPlusSalesSupportPlusRefer__c, proposalData.timeTaIn_CreditPlusSalesSupportPlusRefer__c, obj, "timeTaIn_CreditPlusSalesSupportPlusRefer__c", ref changed);

            ((IDictionary<string, object>)obj).Add("lastUpdatedFromSentinel__c", DateTime.Now);

            return obj;
        }

        #endregion
    }
}

