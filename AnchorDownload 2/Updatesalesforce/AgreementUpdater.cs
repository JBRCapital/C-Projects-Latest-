using System;
using System.Data.SqlClient;
using System.Collections.Generic;
using System.Linq;
using SalesforceDataLibrary;

namespace UpdateSalesforceData
{
    public static class AgreementUpdater
    {
        private static Dictionary<string , IntroducerAndDealerData> introducersAndDealers = null;

        public static void Run()
        {
            LogHelper.Logger.WriteOutput(string.Concat("-----------------------------------", 
                              Environment.NewLine,
                              "Started Querying for Sync of Agreements - Step 1"), Program.EmailTransactionLog);

            introducersAndDealers = SalesforceDataHelper.SalesforceClient.GetRecordsFromSalesforce<IntroducerAndDealerData>("IntroducerAndDealer",
              @"SELECT id, sentinalCompanyId__c, introducerType__c, dealerType__c 
                FROM Account 
                WHERE sentinalCompanyId__c LIKE 'I%' OR sentinalCompanyId__c LIKE 'S%'").ToDictionary(x => x.sentinalCompanyId__c, x => x);
            
            var salesforceUserIds = SalesforceDataHelper.SalesforceClient.GetRecordsFromSalesforce<UserNumberAndIdData>("UserNumberAndIdData",
              @"SELECT Id,jbrPersonNumber__c FROM User WHERE jbrPersonNumber__c != ''")
                .ToLookup(x => x.jbrPersonNumber__c, y => y.Id);

            var dateTimeSyncStarted = DateTime.Now;
            
            SalesforceDataHelper.SalesforceClient.BulkUpsertFromSQL("Agreements","Agreement__c", "sentinalProposal_Id__c",
                true, GetAgreementDataFromSQL, agreementData =>
                    new CustomSObject()
                    {
                        { "sentinalProposal_Id__c", SQLDataHelper.Helper.GetStringMaxLength(agreementData["AgreementProposalID"].ToString().Trim(),18) },
                        { "payoutDate__c", SQLDataHelper.Helper.GetDateTime(agreementData["AgreementAgreementDate"]) },
                        { "settledDate__c", SQLDataHelper.Helper.GetDateTime(agreementData["AgreementSettledDate"]) },
                        { "approval_agreementNumber__c", SQLDataHelper.Helper.GetStringMaxLength(agreementData["AgreementNumber"].ToString().Trim(),20) },
                        { "totalCashPrice__c", SQLDataHelper.Helper.GetDouble(agreementData["TotalCashPrice"].ToString().Trim()) },
                        { "calcVehicleValue__c", SQLDataHelper.Helper.GetDouble(agreementData["CalcVehicleValue"].ToString().Trim()) },
                        { "netInstalment__c", SQLDataHelper.Helper.GetDouble(agreementData["NetInstalment"]) },
                        { "residualValue__c", SQLDataHelper.Helper.GetDouble(agreementData["ResidualValue"]) },
                        { "balancePayable__c", SQLDataHelper.Helper.GetDouble(agreementData["ProposalBalancePayable"]) },
                        { "balanceIncludingFuturePayments__c", SQLDataHelper.Helper.GetDouble(agreementData["BalanceIncludingFuturePayments"]) },
                        { "docFees__c", SQLDataHelper.Helper.GetDouble(agreementData["DocFees"]) },
                        { "totalFees__c", SQLDataHelper.Helper.GetDouble(agreementData["FeesTotal"]) },
                        { "originalAdvance__c", SQLDataHelper.Helper.GetDouble(agreementData["OriginalAdvance"]) },
                        { "originalCharges__c", SQLDataHelper.Helper.GetDouble(agreementData["OriginalCharges"]) },
                        { "originalTotalFees__c", SQLDataHelper.Helper.GetDouble(agreementData["OriginalTotalFees"]) },
                        { "originalBalancePayable__c", SQLDataHelper.Helper.GetDouble(agreementData["OriginalBalancePayable"]) },
                        { "originalTotalPayable__c", SQLDataHelper.Helper.GetDouble(agreementData["OriginalTotalPayable"]) },
                        { "outstandingPrinciple__c", SQLDataHelper.Helper.GetDouble(agreementData["OutstandingPrinciple"]) },
                        { "outstandingCharges__c", SQLDataHelper.Helper.GetDouble(agreementData["OutstandingCharges"]) },
                        { "outstandingFees__c", SQLDataHelper.Helper.GetDouble(agreementData["OutstandingFees"]) },
                        { "outstandingTotalBalance__c", SQLDataHelper.Helper.GetDouble(agreementData["OutstandingTotalBalance"]) },
                        { "arrearsAmount__c", SQLDataHelper.Helper.GetDouble(agreementData["ArrearsAmount"]) },
                        { "arrearsDateWasDue__c", SQLDataHelper.Helper.GetDate(agreementData["ArrearsDateWasDue"]) },
                        { "arrearsNumberOfDaysOverdue__c", SQLDataHelper.Helper.GetInt32(agreementData["ArrearsNumberOfDaysOverdue"]) },
                        { "firstPaymentDate__c", SQLDataHelper.Helper.GetDateTime(agreementData["FirstPaymentDate"]) },
                        { "contractEndDate__c" , SQLDataHelper.Helper.GetDateTime(agreementData["ContractEndDate"])},
                        { "latestContractEndDate__c" , SQLDataHelper.Helper.GetDate(agreementData["LatestContractEndDate"])},
                        { "lastPayProfileDate__c" , SQLDataHelper.Helper.GetDate(agreementData["lastPayProfileDate"])},
                        { "lastPaymentDateDue__c" , SQLDataHelper.Helper.GetDate(agreementData["lastPaymentDateDue"])},
                        { "lastPaymentAmountDue__c" , SQLDataHelper.Helper.GetDouble(agreementData["lastPaymentAmountDue"])},
                        { "lastPaymentDatePaid__c" , SQLDataHelper.Helper.GetDate(agreementData["lastPaymentDatePaid"])},
                        { "lastPaymentAmountPaid__c" , SQLDataHelper.Helper.GetDouble(agreementData["lastPaymentAmountPaid"])},
                        { "nextPaymentDueDate__c" , SQLDataHelper.Helper.GetDate(agreementData["nextPaymentDueDate"])},
                        { "nextPaymentAmountDue__c" , SQLDataHelper.Helper.GetDouble(agreementData["nextPaymentAmountDue"])},
                        { "nextPaymentInterestAmountDue__c" , SQLDataHelper.Helper.GetDouble(agreementData["nextPaymentInterestAmountDue"])},
                        { "workDayPayDate__c" , SQLDataHelper.Helper.GetDate(agreementData["WorkDayPayDate"])},
                        { "workDayPayDatePlusOneWorkingDay__c" , SQLDataHelper.Helper.GetDate(agreementData["WorkDayPayDatePlusOneWorkingDay"])},
                        { "outstandingPrincipleAtSettlement__c" , SQLDataHelper.Helper.GetDouble(agreementData["outstandingPrincipleAtSettlement"])},
                        { "outstandingPaymentAtSettlement__c" , SQLDataHelper.Helper.GetDouble(agreementData["outstandingPaymentAtSettlement"])},
                        { "introducerType__c" , getSalesforceAccountTypeFromSentinel(SQLDataHelper.Helper.GetInt32(agreementData["IntroducerNumber"]), "I")},
                        { "introduction_introducer_id__c" , SQLDataHelper.Helper.GetInt32(agreementData["IntroducerNumber"])},
                        { "introducerSalesforceId__c" , getSalesforceAccountIdFromSentinelId(SQLDataHelper.Helper.GetInt32(agreementData["IntroducerNumber"]), "I")},
                        { "introduction_introducer_name__c" , SQLDataHelper.Helper.GetStringMaxLength(agreementData["Introducer"].ToString().Trim(),255)},
                        { "introducer_commissionAmount__c" , SQLDataHelper.Helper.GetDouble(agreementData["IntroducerCommission"])},
                        { "introduction_supplier_id__c" , SQLDataHelper.Helper.GetInt32(agreementData["DealerNumber"])},
                        { "supplierSalesforceId__c" , getSalesforceAccountIdFromSentinelId(SQLDataHelper.Helper.GetInt32(agreementData["DealerNumber"]), "S")},
                        { "dealerType__c" , getSalesforceAccountTypeFromSentinel(SQLDataHelper.Helper.GetInt32(agreementData["DealerNumber"]), "S")},
                        { "introduction_supplier_name__c" , SQLDataHelper.Helper.GetStringMaxLength(agreementData["Dealer"].ToString().Trim(),255)},
                        { "supplier_commissionAmount__c" , SQLDataHelper.Helper.GetDouble(agreementData["DealerCommission"])},
                        { "introduction_introducerChannel__c" , SQLDataHelper.Helper.GetStringMaxLength(agreementData["IntroducerChannel"].ToString().Trim(),255)},
                        { "grossYield__c" , SQLDataHelper.Helper.GetDouble(agreementData["GrossYield"])},
                        { "netYield__c" , SQLDataHelper.Helper.GetDouble(agreementData["NetYield"])},
                        { "numberOfVehicles__c" , SQLDataHelper.Helper.GetInt32(agreementData["NoOfAgreementVehicles"])},
                        { "make__c" , SQLDataHelper.Helper.GetStringMaxLength(agreementData["Make"].ToString().Trim(),255)},
                        { "model__c" , SQLDataHelper.Helper.GetStringMaxLength(agreementData["Model"].ToString().Trim(),255)},
                        { "vehicleType__c" , SQLDataHelper.Helper.GetStringMaxLength(agreementData["VehicleType"].ToString().Trim(),255)},
                        { "introduction_jbrSalesman__c" , salesforceUserIds[agreementData["JBRSalesmanNumber"].ToString().Trim()].FirstOrDefault()},
                        { "introduction_jbrSalesmanName__c" , SQLDataHelper.Helper.GetStringMaxLength(agreementData["JBRSalesmanName"].ToString().Trim(),255)},
                        { "payProfileOutstanding__c" , SQLDataHelper.Helper.GetDouble(agreementData["PayProfileOutstanding"])},
                        { "payProfileOSLessPrincipleOS__c" , SQLDataHelper.Helper.GetDouble(agreementData["PayProfileOSLessPrincipleOS"])},
                        { "lastPaymentPrincipleAmountLessDifference__c" , SQLDataHelper.Helper.GetDouble(agreementData["LastPaymentPrincipleAmountLessDifference"])},
                        { "totalCharges__c" , SQLDataHelper.Helper.GetDouble(agreementData["TotalCharges"])},
                        { "customersAndCompanyNames__c" , SQLDataHelper.Helper.GetStringMaxLength(agreementData["CustomerAndCompanyNames"].ToString().Trim(),255)},
                        { "fundingDetails_loanTypeIsRegulated__c" , SQLDataHelper.Helper.GetBoolean(agreementData["AgreementRegulated"])},
                        { "contractLength__c" , SQLDataHelper.Helper.GetInt32(agreementData["ContractLength"])},
                        { "noOfFullMonthsSinceAgreementStarted__c" , SQLDataHelper.Helper.GetInt32(agreementData["NoOfFullMonthsSinceAgreementStarted"])},
                        { "agreementPresentMonth__c" , SQLDataHelper.Helper.GetInt32(agreementData["CurrentMonth"])},
                        { "capitalPaidOrSettled__c" , SQLDataHelper.Helper.GetDouble(agreementData["CapitalSettled"])},
                        { "optionToPurchaseFeeRemaining__c" , SQLDataHelper.Helper.GetDouble(agreementData["optionToPurchaseFee"])},
                        { "optionToPurchaseFeeVATRemaining__c" , SQLDataHelper.Helper.GetDouble(agreementData["optionToPurchaseFeeVAT"])},
                        { "numberOfPaymentsMade__c" , SQLDataHelper.Helper.GetInt32(agreementData["noOfPaymentsMade"])},
                        { "totalNumberOfPayments__c" , SQLDataHelper.Helper.GetInt32(agreementData["totalNoOfPayments"])},
                        { "lastUpdatedFromSentinel__c" , SQLDataHelper.Helper.GetDateTime(DateTime.Now) }
                    }, null);

            //UpdateLastTimeAgreementsWereUpdated(dateTimeSyncStarted);

            introducersAndDealers = null;
            LogHelper.Logger.WriteOutput("Ended Sync of Agreements", Program.EmailTransactionLog);
        }

        private static string getSalesforceAccountTypeFromSentinel(int? id, string prefix)
        {
            var account = getSalesforceAccountFromSentinel(id, prefix);
            return account != null ? account.introducerType__c : string.Empty;
        }

        private static IntroducerAndDealerData getSalesforceAccountFromSentinel(int? id, string prefix)
        {
            if (!id.HasValue) { return null; }
            if (!introducersAndDealers.TryGetValue(string.Concat(prefix, id), out IntroducerAndDealerData introducerAndDealerData)) { return null; };
            return introducerAndDealerData;
        }

        private static string getSalesforceAccountIdFromSentinelId(int? id, string prefix)
        {
            var introducerAndDealerData = getSalesforceAccountFromSentinel(id, prefix);
            return introducerAndDealerData == null ? string.Empty : introducerAndDealerData.Id;
        }

        private static SqlDataReader GetAgreementDataFromSQL()
        {
            return new SqlCommand("SELECT * FROM SQL_SF_Sync_Agreements() Order By AgreementNumber", SQLDataConnectionHelper.SqlConnection) { CommandTimeout = 0 }.ExecuteReader();
        }

        //private static void UpdateLastTimeAgreementsWereUpdated(DateTime lastTimeAgreementsWereUpdated)
        //{
        //    DataHelper.RunCommand("UpdateLastTimeAgreementsWereUpdated", CommandType.StoredProcedure,
        //        new List<SqlParameter>()
        //        {
        //            new SqlParameter("DateOfLastUpdate", lastTimeAgreementsWereUpdated)
        //        });

        //}

    }
}
