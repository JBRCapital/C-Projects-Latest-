/*using System;
using System.Data.SqlClient;
using System.Threading.Tasks;
using System.Dynamic;
using System.Data;
using System.Collections.Generic;
using System.Linq;
using Salesforce.Common.Models.Json;
using System.Diagnostics;
using System.Threading;

namespace UpdateSalesforceData
{
    public static class AgreementUpdater
    {
        private static List<AccountData> introducersAndDealers = null;

        private static ILookup<string, string> GetSalesforceUserIds(SalesforceHttpClient salesforceClient)
        {
            var queryString = string.Concat(@"SELECT Id,jbrPersonNumber__c FROM User WHERE jbrPersonNumber__c != ''");

            QueryResult<UserNumberAndIdData> userNumberAndIdData = null;
            Task.Run(async () =>
            {
                userNumberAndIdData = await salesforceClient.QueryAllAsync<UserNumberAndIdData>(queryString);
            }).Wait(Timeout.InfiniteTimeSpan);
            return userNumberAndIdData.Records.ToLookup(x => x.jbrPersonNumber__c, y => y.Id);
        }

        public static void UpdateAgreementData(SqlConnection sqlConn, SalesforceHttpClient salesforceClient)
        {
            Console.WriteLine(string.Concat("Started Sync of Agreements"));

            introducersAndDealers = getIntroducerAndSupplierRecordsFromSalesforce(salesforceClient);
            ILookup<string, string> salesforceUserIds = GetSalesforceUserIds(salesforceClient);

            var agreementData = GetAgreementDataFromSQL(sqlConn);
            var dateTimeSyncStarted = DateTime.Now;

            string logfile = System.IO.Path.GetDirectoryName(Process.GetCurrentProcess().MainModule.FileName) + @"\" + DateTime.Now.ToString("yyyy-dd-M--HH-mm-ss") + "-detail.log";

            using (System.IO.StreamWriter file = new System.IO.StreamWriter(logfile, true))
            {

                while (agreementData.Read())
                {
                    file.WriteLine();
                    file.WriteLine("Raw Data" + ConvertToString(agreementData));

                    var agreementDataForSF = new AgreementData()
                    {
                        sentinalProposal_Id__c = agreementData["AgreementProposalID"].ToString(),
                        payoutDate__c = DataHelper.GetDateTime(agreementData["AgreementAgreementDate"]),
                        settledDate__c = DataHelper.GetDateTime(agreementData["AgreementSettledDate"]),

                        approval_agreementNumber__c = agreementData["AgreementNumber"].ToString(),

                        totalCashPrice__c = DataHelper.GetDouble(agreementData["TotalCashPrice"].ToString()),
                        calcVehicleValue__c = DataHelper.GetDouble(agreementData["CalcVehicleValue"].ToString()),

                        netInstalment__c = DataHelper.GetDouble(agreementData["NetInstalment"]),
                        residualValue__c = DataHelper.GetDouble(agreementData["ResidualValue"]),

                        balancePayable__c = DataHelper.GetDouble(agreementData["ProposalBalancePayable"]),
                        balanceIncludingFuturePayments__c = DataHelper.GetDouble(agreementData["BalanceIncludingFuturePayments"]),

                        docFees__c = DataHelper.GetDouble(agreementData["DocFees"]),
                        totalFees__c = DataHelper.GetDouble(agreementData["FeesTotal"]),

                        originalAdvance__c = DataHelper.GetDouble(agreementData["OriginalAdvance"]),
                        originalCharges__c = DataHelper.GetDouble(agreementData["OriginalCharges"]),
                        originalTotalFees__c = DataHelper.GetDouble(agreementData["OriginalTotalFees"]),
                        originalBalancePayable__c = DataHelper.GetDouble(agreementData["OriginalBalancePayable"]),
                        originalTotalPayable__c = DataHelper.GetDouble(agreementData["OriginalTotalPayable"]),

                        outstandingPrinciple__c = DataHelper.GetDouble(agreementData["OutstandingPrinciple"]),
                        outstandingCharges__c = DataHelper.GetDouble(agreementData["OutstandingCharges"]),
                        outstandingFees__c = DataHelper.GetDouble(agreementData["OutstandingFees"]),
                        outstandingTotalBalance__c = DataHelper.GetDouble(agreementData["OutstandingTotalBalance"]),

                        arrearsAmount__c = DataHelper.GetDouble(agreementData["ArrearsAmount"]),

                        arrearsDateWasDue__c = DataHelper.GetDateTime(agreementData["ArrearsDateWasDue"]),
                        arrearsNumberOfDaysOverdue__c = DataHelper.GetInt32(agreementData["ArrearsNumberOfDaysOverdue"]),

                        firstPaymentDate__c = DataHelper.GetDateTime(agreementData["FirstPaymentDate"]),

                        contractEndDate__c = DataHelper.GetDateTime(agreementData["ContractEndDate"]),
                        latestContractEndDate__c = DataHelper.GetDateTime(agreementData["LatestContractEndDate"]),

                        lastPayProfileDate__c = DataHelper.GetDateTime(agreementData["lastPayProfileDate"]),
                        lastPaymentDateDue__c = DataHelper.GetDateTime(agreementData["lastPaymentDateDue"]),
                        lastPaymentAmountDue__c = DataHelper.GetDouble(agreementData["lastPaymentAmountDue"]),

                        lastPaymentDatePaid__c = DataHelper.GetDateTime(agreementData["lastPaymentDatePaid"]),
                        lastPaymentAmountPaid__c = DataHelper.GetDouble(agreementData["lastPaymentAmountPaid"]),

                        nextPaymentDueDate__c = DataHelper.GetDateTime(agreementData["nextPaymentDueDate"]),
                        nextPaymentAmountDue__c = DataHelper.GetDouble(agreementData["nextPaymentAmountDue"]),
                        nextPaymentInterestAmountDue__c = DataHelper.GetDouble(agreementData["nextPaymentInterestAmountDue"]),
                        workDayPayDate__c = DataHelper.GetDateTime(agreementData["WorkDayPayDate"]),
                        workDayPayDatePlusOneWorkingDay__c = DataHelper.GetDateTime(agreementData["WorkDayPayDatePlusOneWorkingDay"]),

                        outstandingPrincipleAtSettlement__c = DataHelper.GetDouble(agreementData["outstandingPrincipleAtSettlement"]),
                        outstandingPaymentAtSettlement__c = DataHelper.GetDouble(agreementData["outstandingPaymentAtSettlement"]),

                        introducerType__c = getSalesforceAccountIntroducerTypeFromSentinel(DataHelper.GetInt32(agreementData["IntroducerNumber"]), "I"),
                        dealerType__c = getSalesforceAccountIntroducerTypeFromSentinel(DataHelper.GetInt32(agreementData["IntroducerNumber"]), "I"),

                        introduction_introducer_id__c = DataHelper.GetInt32(agreementData["IntroducerNumber"]),
                        introducerSalesforceId__c = getSalesforceAccountIdFromSentinelId(DataHelper.GetInt32(agreementData["IntroducerNumber"]), "I"),
                        introduction_introducer_name__c = agreementData["Introducer"].ToString(),
                        introducer_commissionAmount__c = DataHelper.GetDouble(agreementData["IntroducerCommission"]),

                        introduction_supplier_id__c = DataHelper.GetInt32(agreementData["DealerNumber"]),
                        supplierSalesforceId__c = getSalesforceAccountIdFromSentinelId(DataHelper.GetInt32(agreementData["DealerNumber"]), "S"),
                        introduction_supplier_name__c = agreementData["Dealer"].ToString(),
                        supplier_commissionAmount__c = DataHelper.GetDouble(agreementData["DealerCommission"]),

                        introduction_introducerChannel__c = agreementData["IntroducerChannel"].ToString(),
                        grossYield__c = DataHelper.GetDouble(agreementData["GrossYield"]),
                        netYield__c = DataHelper.GetDouble(agreementData["NetYield"]),
                        numberOfVehicles__c = DataHelper.GetInt32(agreementData["NoOfAgreementVehicles"]),
                        make__c = agreementData["Make"].ToString(),
                        model__c = agreementData["Model"].ToString(),
                        vehicleType__c = agreementData["VehicleType"].ToString(),
                        introduction_jbrSalesman__c = salesforceUserIds[agreementData["JBRSalesmanNumber"].ToString()].FirstOrDefault(),
                        //introduction_jbrSalesmanId__c = salesforceUserIds[agreementData["JBRSalesmanNumber"].ToString()].FirstOrDefault(),
                        introduction_jbrSalesmanName__c = agreementData["JBRSalesmanName"].ToString(),

                        payProfileOutstanding__c = DataHelper.GetDouble(agreementData["PayProfileOutstanding"]),
                        payProfileOSLessPrincipleOS__c = DataHelper.GetDouble(agreementData["PayProfileOSLessPrincipleOS"]),
                        lastPaymentPrincipleAmountLessDifference__c = DataHelper.GetDouble(agreementData["LastPaymentPrincipleAmountLessDifference"]),

                        totalCharges__c = DataHelper.GetDouble(agreementData["TotalCharges"]),

                        customersAndCompanyNames__c = agreementData["CustomerAndCompanyNames"].ToString(),

                        fundingDetails_loanTypeIsRegulated__c = DataHelper.GetBoolean(agreementData["AgreementRegulated"]),

                        contractLength__c = DataHelper.GetInt32(agreementData["ContractLength"]),

                        noOfFullMonthsSinceAgreementStarted__c = DataHelper.GetInt32(agreementData["NoOfFullMonthsSinceAgreementStarted"]),
                        agreementPresentMonth__c = DataHelper.GetInt32(agreementData["CurrentMonth"]),

                        capitalPaidOrSettled__c = DataHelper.GetDouble(agreementData["CapitalSettled"]),

                        optionToPurchaseFeeRemaining__c = DataHelper.GetDouble(agreementData["optionToPurchaseFee"]),
                        optionToPurchaseFeeVATRemaining__c = DataHelper.GetDouble(agreementData["optionToPurchaseFeeVAT"]),

                        numberOfPaymentsMade__c = DataHelper.GetInt32(agreementData["noOfPaymentsMade"]),
                        totalNumberOfPayments__c = DataHelper.GetInt32(agreementData["totalNoOfPayments"]),

                        lastUpdatedFromSentinel__c = DateTime.Now
                        //loanTypeRateType__c


                        //a.interestOutstanding__c
                        //a.settledCapital__c
                        //a.settledInterest__c
                        //a.settledTotalOptionToPurchaseFee__c
                        //a.settledTotalOptionToPurchaseVAT__c
                        //a.remainingAnnualFees__c
                        //a.remainingVATOnAnnualFees__c
                        //a.optionToPurchaseFeeRemaining__c
                        //a.balloonRemaining__c
                        //a.netInstalment__c


                        //IntroducerType__c

                        //capitalAvailableToSettle__c = DataHelper.GetDouble(agreementData["OutstandingPrinciple"]),
                        //capitalOutstanding__c = DataHelper.GetDouble(agreementData["OutstandingPrinciple"])
                    };

                    file.WriteLine("Agreement data ready for update");

                    updateAgreementRecordInSalesforce(salesforceClient, agreementDataForSF, file);

                }

                file.WriteLine("Finished reading from SQL..");
                file.WriteLine("Ended Sync of Agreements");
            }

            agreementData.Close();

            UpdateLastTimeAgreementsWereUpdated(dateTimeSyncStarted, sqlConn);

            Console.WriteLine(string.Concat("Ended Sync of Agreements"));
        }

        private static string getSalesforceAccountIntroducerTypeFromSentinel(int? id, string prefix)
        {
            var account = getSalesforceAccountFromSentinel(id, prefix);
            if (account != null) {
                return account.introducerType__c;
            }

            return string.Empty;
        }

        private static string getSalesforceAccountDealerTypeFromSentinel(int? id, string prefix)
        {
            var account = getSalesforceAccountFromSentinel(id, prefix);
            if (account != null)
            {
                return account.dealerType__c;
            }

            return string.Empty;
        }

        private static AccountData getSalesforceAccountFromSentinel(int? id, string prefix)
        {
            if (!id.HasValue) { return null; }
            if (!introducersAndDealers.Exists(item => item.sentinalCompanyId__c == string.Concat(prefix, id))) { return null; }

            return introducersAndDealers.Find(item => item.sentinalCompanyId__c == string.Concat(prefix, id));
        }

        private static string getSalesforceAccountIdFromSentinelId(int? id, string prefix)
        {
            if (!id.HasValue) { return string.Empty; }
            if (!introducersAndDealers.Exists(item => item.sentinalCompanyId__c == string.Concat(prefix, id))) { return string.Empty; }

            return introducersAndDealers.Find(item => item.sentinalCompanyId__c == string.Concat(prefix, id)).Id;
        }

        private static List<AccountData> getIntroducerAndSupplierRecordsFromSalesforce(SalesforceHttpClient salesforceClient)
        {
            List<AccountData> result = new List<AccountData>();
            //try
            //{
            var queryString = string.Concat(@"SELECT id, sentinalCompanyId__c, introducerType__c, dealerType__c FROM Account WHERE sentinalCompanyId__c LIKE 'I%' OR sentinalCompanyId__c LIKE 'S%'");

            QueryResult<AccountData> anchorWebServices = null;

            Task.Run(async () =>
            {
                anchorWebServices = await salesforceClient.QueryAsync<AccountData>(queryString);
                var finished = false;
                do
                {
                    finished = anchorWebServices.Done;

                    // add any records
                    if (anchorWebServices.Records.Any())
                        result.AddRange(anchorWebServices.Records);

                    // if more
                    if (!finished)
                    {
                        // get next batch
                        anchorWebServices = await salesforceClient.QueryContinuationAsync<AccountData>(anchorWebServices.NextRecordsUrl);
                    }
                } while (!finished);
            }).Wait(Timeout.InfiniteTimeSpan);

            return result;// anchorWebServices.Records;
        }

        //private static List<AccountData> getDealerRecordsFromSalesforce(ForceClient salesforceClient)
        //{
        //    //try
        //    //{
        //    var queryString = string.Concat(@"SELECT id, sentinalCompanyId__c FROM Account WHERE Type = 'Supplier'");

        //    QueryResult<AccountData> anchorWebServices = null;

        //    Task.Run(async () =>
        //    {
        //        anchorWebServices = await salesforceClient.QueryAsync<AccountData>(queryString);
        //    }).Wait(Timeout.InfiniteTimeSpan);

        //    return anchorWebServices.Records;
        //}


        private static void updateAgreementRecordInSalesforce(SalesforceHttpClient salesforceClient, AgreementData agreementData, System.IO.StreamWriter file)
        {
            file.WriteLine("Start of updateAgreementRecordInSalesforce");
            //try
            //{
            var queryString = string.Concat(@"SELECT id, approval_agreementNumber__c, payoutDate__c,payoutDateWithLive__c, totalCashPrice__c, netInstalment__c, 
                                                         residualValue__c,balanceIncludingFuturePayments__c,originalAdvance__c,
                                                         originalCharges__c, originalTotalFees__c, originalBalancePayable__c, originalTotalPayable__c, 
                                                         outstandingPrinciple__c, outstandingCharges__c, outstandingFees__c, outstandingTotalBalance__c, 
                                                         arrearsAmount__c, arrearsDateWasDue__c, arrearsNumberOfDaysOverdue__c, settledDate__c,
                                                         lastPaymentDateDue__c,lastPaymentAmountDue__c,lastPaymentDatePaid__c,lastPaymentAmountPaid__c,
                                                         nextPaymentDueDate__c,nextPaymentAmountDue__c, nextPaymentInterestAmountDue__c,
                                                         outstandingPrincipleAtSettlement__c,outstandingPaymentAtSettlement__c, 
                                                         proposal__r.sentinalWorkflowStageId__c, introducerSalesforceId__c, supplierSalesforceId__c,
                                                         payProfileOutstanding__c, payProfileOSLessPrincipleOS__c, lastPaymentPrincipleAmountLessDifference__c,
                                                         totalFees__c, docFees__c, totalCharges__c,
                                                         customersAndCompanyNames__c, fundingDetails_loanTypeIsRegulated__c,
                                                         contractLength__c, agreementPresentMonth__c, capitalPaidOrSettled__c,
                                                         workDayPayDate__c, workDayPayDatePlusOneWorkingDay__c,
                                                         optionToPurchaseFeeRemaining__c, optionToPurchaseFeeVATRemaining__c,
                                                         introduction_jbrSalesman__c, totalNumberOfPayments__c, numberOfPaymentsMade__c, lastUpdatedFromSentinel__c
                                                         FROM Agreement__c
                                                         WHERE sentinalProposal_Id__c = ", agreementData.sentinalProposal_Id__c + " order by ID");

            QueryResult<AgreementData> anchorWebServices = null;

            file.WriteLine("Query for " + agreementData.sentinalProposal_Id__c);

            bool failed = false;

            do
            {
                try
                {
                    failed = false;

                    Task.Run(async () =>
                    {
                        anchorWebServices = await salesforceClient.QueryAsync<AgreementData>(queryString);
                    }).Wait(Timeout.InfiniteTimeSpan);
                }
                catch (Exception ex)
                {
                    failed = true;
                    file.WriteLine("Error with salesforce: " + ex.Message + " - " + queryString);
                }
            } while (failed);


            var changed = false;

            file.WriteLine("anchorWebServices.Records.Count: " + anchorWebServices.Records.Count);

            if (anchorWebServices.Records.Count > 0)
            {
                Console.WriteLine(string.Concat("Checking Agreement:", agreementData.sentinalProposal_Id__c, " - ", agreementData.payoutDate__c));
                dynamic obj = getUpdateObject(agreementData, anchorWebServices, ref changed);

                file.WriteLine(string.Concat("sentinalProposal_Id__c: ", agreementData.sentinalProposal_Id__c));
                file.WriteLine(Newtonsoft.Json.JsonConvert.SerializeObject(obj));
                file.WriteLine(string.Concat("Agreement Changed: ", changed ? "Yes" : "No"));

                Console.WriteLine(string.Concat("Agreement Changed: ", changed ? "Yes" : "No" ));

                if (changed)
                {
                    SuccessResponse successResponse = null;
                    
                    Task.Run(async () =>
                    {
                        Console.WriteLine(string.Concat("Upserted Agreement:", agreementData.sentinalProposal_Id__c, " - ", agreementData.payoutDate__c));
                        successResponse = await salesforceClient.UpsertExternalAsync("Agreement__c", "sentinalProposal_Id__c", agreementData.sentinalProposal_Id__c, obj);
                        Console.WriteLine(successResponse != null && successResponse.Success ? "Success" : "Failed");
                    }).Wait(Timeout.InfiniteTimeSpan);
                }
            }
        }

        private static dynamic getUpdateObject(AgreementData agreementData, QueryResult<AgreementData> SFAgreementData, ref bool changed)
        {
            dynamic obj = new ExpandoObject() as IDictionary<string, object>;
            var SFAgreementRecord = SFAgreementData.Records[0];

            //payoutDate__c
            if ((SFAgreementRecord.proposal__r.sentinalWorkflowStageId__c == "5" || SFAgreementRecord.proposal__r.sentinalWorkflowStageId__c == "111") &&
                    !SFAgreementRecord.payoutDate__c.HasValue ||
                    (SFAgreementRecord.payoutDate__c.HasValue && SFAgreementRecord.payoutDate__c.Value.Date != agreementData.payoutDate__c.Value.Date))
            {
                obj.payoutDate__c = agreementData.payoutDate__c;
                changed = true;
            }
            
            //payoutDateWithLive__c
            //if (SFAgreementRecord.proposal__r.sentinalWorkflowStageId__c == "5" &&
            //    (!SFAgreementRecord.payoutDateWithLive__c.HasValue ||
            //    (SFAgreementRecord.payoutDateWithLive__c.HasValue && SFAgreementRecord.payoutDateWithLive__c.Value.Date != agreementData.payoutDate__c.Value.Date)))
            //{ obj.payoutDateWithLive__c = agreementData.payoutDate__c.Value.Date; changed = true; }


            //settledDate__c
            if ((!SFAgreementRecord.settledDate__c.HasValue && agreementData.settledDate__c.HasValue)  ||
                    (SFAgreementRecord.settledDate__c.HasValue && agreementData.settledDate__c.HasValue && SFAgreementRecord.settledDate__c.Value.Date != agreementData.settledDate__c.Value.Date))
            {
                obj.settledDate__c = agreementData.settledDate__c;
                changed = true;
            }

            //approval_agreementNumber__c
            DataHelper.setValue(SFAgreementRecord.approval_agreementNumber__c, agreementData.approval_agreementNumber__c, obj, "approval_agreementNumber__c", ref changed);

            //totalCashPrice__c
            DataHelper.setValue(SFAgreementRecord.totalCashPrice__c, agreementData.totalCashPrice__c, obj, "totalCashPrice__c", ref changed);

            //calcVehicleValue__c
            DataHelper.setValue(SFAgreementRecord.calcVehicleValue__c, agreementData.calcVehicleValue__c, obj, "calcVehicleValue__c", ref changed);

            //netInstalment__c
            DataHelper.setValue(SFAgreementRecord.netInstalment__c, agreementData.netInstalment__c, obj, "netInstalment__c", ref changed);

            //residualValue__c
            DataHelper.setValue(SFAgreementRecord.residualValue__c, agreementData.residualValue__c, obj, "residualValue__c", ref changed);

            //balancePayable__c
            DataHelper.setValue(SFAgreementRecord.balancePayable__c, agreementData.balancePayable__c, obj, "balancePayable__c", ref changed);

            //balanceIncludingFuturePayments__c
            DataHelper.setValue(SFAgreementRecord.balanceIncludingFuturePayments__c, agreementData.balanceIncludingFuturePayments__c, obj, "balanceIncludingFuturePayments__c", ref changed);
            
            //originalCharges__c
            DataHelper.setValue(SFAgreementRecord.originalAdvance__c, agreementData.originalAdvance__c, obj, "originalAdvance__c", ref changed);

            //originalCharges__c
            DataHelper.setValue(SFAgreementRecord.originalCharges__c, agreementData.originalCharges__c, obj, "originalCharges__c", ref changed);

            //originalTotalFees__c
            DataHelper.setValue(SFAgreementRecord.originalTotalFees__c, agreementData.originalTotalFees__c, obj, "originalTotalFees__c", ref changed);

            //docFees__c
            DataHelper.setValue(SFAgreementRecord.docFees__c, agreementData.docFees__c, obj, "docFees__c", ref changed);

            //totalFees__c
            DataHelper.setValue(SFAgreementRecord.totalFees__c, agreementData.totalFees__c, obj, "totalFees__c", ref changed);

            //originalBalancePayable__c
            DataHelper.setValue(SFAgreementRecord.originalBalancePayable__c, agreementData.originalBalancePayable__c, obj, "originalBalancePayable__c", ref changed);

            //originalTotalPayable__c
            DataHelper.setValue(SFAgreementRecord.originalTotalPayable__c, agreementData.originalTotalPayable__c, obj, "originalTotalPayable__c", ref changed);

            //outstandingPrinciple__c
            DataHelper.setValue(SFAgreementRecord.outstandingPrinciple__c, agreementData.outstandingPrinciple__c, obj, "outstandingPrinciple__c", ref changed);

            //outstandingCharges__c
            DataHelper.setValue(SFAgreementRecord.outstandingCharges__c, agreementData.outstandingCharges__c, obj, "outstandingCharges__c", ref changed);

            //outstandingFees__c
            DataHelper.setValue(SFAgreementRecord.outstandingFees__c, agreementData.outstandingFees__c, obj, "outstandingFees__c", ref changed);

            //outstandingTotalBalance__c
            DataHelper.setValue(SFAgreementRecord.outstandingTotalBalance__c, agreementData.outstandingTotalBalance__c, obj, "outstandingTotalBalance__c", ref changed);

            //arrearsAmount__c
            DataHelper.setValue(SFAgreementRecord.arrearsAmount__c, agreementData.arrearsAmount__c, obj, "arrearsAmount__c", ref changed);

            //firstPaymentDate__c
            DataHelper.setValue(SFAgreementRecord.firstPaymentDate__c, agreementData.firstPaymentDate__c, obj, "firstPaymentDate__c", ref changed);

            //contractEndDate__c
            DataHelper.setValue(SFAgreementRecord.contractEndDate__c, agreementData.contractEndDate__c, obj, "contractEndDate__c", ref changed);

            //latestContractEndDate__c
            DataHelper.setValue(SFAgreementRecord.latestContractEndDate__c, agreementData.latestContractEndDate__c, obj, "latestContractEndDate__c", ref changed);

            //arrearsDateWasDue__c
            DataHelper.setValue(SFAgreementRecord.arrearsDateWasDue__c, agreementData.arrearsDateWasDue__c, obj, "arrearsDateWasDue__c", ref changed);

            //arrearsNumberOfDaysOverdue__c
            DataHelper.setValue(SFAgreementRecord.arrearsNumberOfDaysOverdue__c, agreementData.arrearsNumberOfDaysOverdue__c, obj, "arrearsNumberOfDaysOverdue__c", ref changed);

            //lastPayProfileDate__c
            DataHelper.setValue(SFAgreementRecord.lastPayProfileDate__c, agreementData.lastPayProfileDate__c, obj, "lastPayProfileDate__c", ref changed); 

            //lastPaymentDateDue__c
            DataHelper.setValue(SFAgreementRecord.lastPaymentDateDue__c, agreementData.lastPaymentDateDue__c, obj, "lastPaymentDateDue__c", ref changed);

            //lastPaymentAmountDue__c
            DataHelper.setValue(SFAgreementRecord.lastPaymentAmountDue__c, agreementData.lastPaymentAmountDue__c, obj, "lastPaymentAmountDue__c", ref changed);

            //lastPaymentDatePaid__c
            DataHelper.setValue(SFAgreementRecord.lastPaymentDatePaid__c, agreementData.lastPaymentDatePaid__c, obj, "lastPaymentDatePaid__c", ref changed);

            //lastPaymentAmountPaid__c
            DataHelper.setValue(SFAgreementRecord.lastPaymentAmountPaid__c, agreementData.lastPaymentAmountPaid__c, obj, "lastPaymentAmountPaid__c", ref changed);

            //nextPaymentDueDate__c
            DataHelper.setValue(SFAgreementRecord.nextPaymentDueDate__c, agreementData.nextPaymentDueDate__c, obj, "nextPaymentDueDate__c", ref changed);

            //nextPaymentAmountDue__c
            DataHelper.setValue(SFAgreementRecord.nextPaymentAmountDue__c, agreementData.nextPaymentAmountDue__c, obj, "nextPaymentAmountDue__c", ref changed);

            //nextPaymentInterestAmountDue__c
            DataHelper.setValue(SFAgreementRecord.nextPaymentInterestAmountDue__c, agreementData.nextPaymentInterestAmountDue__c, obj, "nextPaymentInterestAmountDue__c", ref changed);

            //outstandingPrincipleAtSettlement__c
            DataHelper.setValue(SFAgreementRecord.outstandingPrincipleAtSettlement__c, agreementData.outstandingPrincipleAtSettlement__c, obj, "outstandingPrincipleAtSettlement__c", ref changed);

            //outstandingPaymentAtSettlement__c
            DataHelper.setValue(SFAgreementRecord.outstandingPaymentAtSettlement__c, agreementData.outstandingPaymentAtSettlement__c, obj, "outstandingPaymentAtSettlement__c", ref changed);

            //introducerType__c
            DataHelper.setValue(SFAgreementRecord.introducerType__c, agreementData.introducerType__c, obj, "introducerType__c", ref changed);

            //dealerType__c
            DataHelper.setValue(SFAgreementRecord.dealerType__c, agreementData.dealerType__c, obj, "dealerType__c", ref changed);

            //introduction_introducer_id__c
            DataHelper.setValue(SFAgreementRecord.introduction_introducer_id__c, agreementData.introduction_introducer_id__c, obj, "introduction_introducer_id__c", ref changed);

            //introducerSalesforceId__c
            DataHelper.setValue(SFAgreementRecord.introducerSalesforceId__c, agreementData.introducerSalesforceId__c, obj, "introducerSalesforceId__c", ref changed);

            //introduction_introducer_name__c
            DataHelper.setValue(SFAgreementRecord.introduction_introducer_name__c, agreementData.introduction_introducer_name__c, obj, "introduction_introducer_name__c", ref changed);

            //introducer_commissionAmount__c
            DataHelper.setValue(SFAgreementRecord.introducer_commissionAmount__c, agreementData.introducer_commissionAmount__c, obj, "introducer_commissionAmount__c", ref changed);

            //introduction_supplier_id__c
            DataHelper.setValue(SFAgreementRecord.introduction_supplier_id__c, agreementData.introduction_supplier_id__c, obj, "introduction_supplier_id__c", ref changed);

            //introduction_supplier_id__c
            DataHelper.setValue(SFAgreementRecord.supplierSalesforceId__c, agreementData.supplierSalesforceId__c, obj, "supplierSalesforceId__c", ref changed);
            
            //introduction_supplier_name__c
            DataHelper.setValue(SFAgreementRecord.introduction_supplier_name__c, agreementData.introduction_supplier_name__c, obj, "introduction_supplier_name__c", ref changed);

            //supplier_commissionAmount__c
            DataHelper.setValue(SFAgreementRecord.supplier_commissionAmount__c, agreementData.supplier_commissionAmount__c, obj, "supplier_commissionAmount__c", ref changed);

            //introduction_introducerChannel__c
            DataHelper.setValue(SFAgreementRecord.introduction_introducerChannel__c, agreementData.introduction_introducerChannel__c, obj, "introduction_introducerChannel__c", ref changed);

            //grossYield__c
            DataHelper.setValue(SFAgreementRecord.grossYield__c, agreementData.grossYield__c, obj, "grossYield__c", ref changed);

            //netYield__c
            DataHelper.setValue(SFAgreementRecord.netYield__c, agreementData.netYield__c, obj, "netYield__c", ref changed);

            //numberOfVehicles__c
            DataHelper.setValue(SFAgreementRecord.numberOfVehicles__c, agreementData.numberOfVehicles__c, obj, "numberOfVehicles__c", ref changed);

            //make__c
            DataHelper.setValue(SFAgreementRecord.make__c, agreementData.make__c, obj, "make__c", ref changed);

            //model__c
            DataHelper.setValue(SFAgreementRecord.model__c, agreementData.model__c, obj, "model__c", ref changed);

            //vehicleType__c
            DataHelper.setValue(SFAgreementRecord.vehicleType__c, agreementData.vehicleType__c, obj, "vehicleType__c", ref changed);
            
            //introduction_jbrSalesmanId__c
            //DataHelper.setValue(SFAgreementRecord.introduction_jbrSalesmanId__c, agreementData.introduction_jbrSalesmanId__c, obj, "introduction_jbrSalesmanId__c", ref changed);

            //introduction_jbrSalesman__c
            DataHelper.setValue(SFAgreementRecord.introduction_jbrSalesman__c, agreementData.introduction_jbrSalesman__c, obj, "introduction_jbrSalesman__c", ref changed);

            //introduction_jbrSalesmanName__c
            DataHelper.setValue(SFAgreementRecord.introduction_jbrSalesmanName__c, agreementData.introduction_jbrSalesmanName__c, obj, "introduction_jbrSalesmanName__c", ref changed);

            //payProfileOutstanding__c
            DataHelper.setValue(SFAgreementRecord.payProfileOutstanding__c, agreementData.payProfileOutstanding__c, obj, "payProfileOutstanding__c", ref changed);

            //payProfileOSLessPrincipleOS__c
            DataHelper.setValue(SFAgreementRecord.payProfileOSLessPrincipleOS__c, agreementData.payProfileOSLessPrincipleOS__c, obj, "payProfileOSLessPrincipleOS__c", ref changed);

            //lastPaymentPrincipleAmountLessDifference__c
            DataHelper.setValue(SFAgreementRecord.lastPaymentPrincipleAmountLessDifference__c, agreementData.lastPaymentPrincipleAmountLessDifference__c, obj, "lastPaymentPrincipleAmountLessDifference__c", ref changed);

            //totalCharges__c
            DataHelper.setValue(SFAgreementRecord.totalCharges__c, agreementData.totalCharges__c, obj, "totalCharges__c", ref changed);

            //customersAndCompanyNames__c
            DataHelper.setValue(SFAgreementRecord.customersAndCompanyNames__c, agreementData.customersAndCompanyNames__c, obj, "customersAndCompanyNames__c", ref changed);

            //fundingDetails_loanTypeIsRegulated__c
            DataHelper.setValue(SFAgreementRecord.fundingDetails_loanTypeIsRegulated__c, agreementData.fundingDetails_loanTypeIsRegulated__c, obj, "fundingDetails_loanTypeIsRegulated__c", ref changed);

            //contractLength__c
            DataHelper.setValue(SFAgreementRecord.contractLength__c, agreementData.contractLength__c, obj, "contractLength__c", ref changed);

            //noOfMonthlyPaymentsRequested__c
            DataHelper.setValue(SFAgreementRecord.noOfFullMonthsSinceAgreementStarted__c, agreementData.noOfFullMonthsSinceAgreementStarted__c, obj, "noOfFullMonthsSinceAgreementStarted__c", ref changed);

            //agreementPresentMonth__c
            DataHelper.setValue(SFAgreementRecord.agreementPresentMonth__c, agreementData.agreementPresentMonth__c, obj, "agreementPresentMonth__c", ref changed);

            //numberOfPaymentsMade__c
            DataHelper.setValue(SFAgreementRecord.numberOfPaymentsMade__c, agreementData.numberOfPaymentsMade__c, obj, "numberOfPaymentsMade__c", ref changed);
     
            //totalNumberOfPayments__c
            DataHelper.setValue(SFAgreementRecord.totalNumberOfPayments__c, agreementData.totalNumberOfPayments__c, obj, "totalNumberOfPayments__c", ref changed);

            //capitalPaidOrSettled__c
            DataHelper.setValue(SFAgreementRecord.capitalPaidOrSettled__c, agreementData.capitalPaidOrSettled__c, obj, "capitalPaidOrSettled__c", ref changed);

            //workDayPayDate__c
            DataHelper.setValue(SFAgreementRecord.workDayPayDate__c, agreementData.workDayPayDate__c, obj, "workDayPayDate__c", ref changed);

            //workDayPayDatePlusOneWorkingDay__c
            DataHelper.setValue(SFAgreementRecord.workDayPayDatePlusOneWorkingDay__c, agreementData.workDayPayDatePlusOneWorkingDay__c, obj, "workDayPayDatePlusOneWorkingDay__c", ref changed);

            //optionToPurchaseFeeRemaining__c
            DataHelper.setValue(SFAgreementRecord.optionToPurchaseFeeRemaining__c, agreementData.optionToPurchaseFeeRemaining__c, obj, "optionToPurchaseFeeRemaining__c", ref changed);

            //optionToPurchaseFeeVATRemaining__c
            DataHelper.setValue(SFAgreementRecord.optionToPurchaseFeeVATRemaining__c, agreementData.optionToPurchaseFeeVATRemaining__c, obj, "optionToPurchaseFeeVATRemaining__c", ref changed);

            ((IDictionary<string, object>)obj).Add("lastUpdatedFromSentinel__c", DateTime.Now);

            return obj;
        }

        private static SqlDataReader GetAgreementDataFromSQL(SqlConnection sqlConn)
        {
            return new SqlCommand("SELECT * FROM GetAgreementDataForSalesforce() where AgreementProposalID > 0", sqlConn){ CommandTimeout = 0 }.ExecuteReader();
        }

        private static void UpdateLastTimeAgreementsWereUpdated(DateTime lastTimeAgreementsWereUpdated, SqlConnection sqlConn)
        {
            using (SqlCommand cmd = new SqlCommand())
            {
                cmd.CommandText = "UpdateLastTimeAgreementsWereUpdated";
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("DateOfLastUpdate", lastTimeAgreementsWereUpdated);

                cmd.Connection = sqlConn;
                cmd.ExecuteNonQuery();
            }
        }

        private static string ConvertToString(this SqlDataReader rd) {

            string s = string.Empty;
            for (int i = 0; i < rd.FieldCount; i++)
            {
                if (!rd.IsDBNull(i))
                {
                    s = s+ " -- " + rd.GetName(i) + " : " + rd.GetValue(i);
                }
            }

            return s;
        }
    }
}
*/