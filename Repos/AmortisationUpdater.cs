using Salesforce.Common.Models;
using Salesforce.Force;
using System;
using System.Threading.Tasks;
using System.Data.SqlClient;
using System.Data;
using Salesforce.Common.Models.Json;
using System.Threading;

namespace UpdateSalesforceData
{
    class AmortisationUpdater
    {

        public static void UpdateAgreementExtraDetailsData()
        {

            QueryResult<AgreementData> agreementDataResults = null;

            Task.Run(async () =>
            {
                Console.WriteLine(string.Concat("Quering AgreementData for AgreementExtraDetails"));

                agreementDataResults = await Program.salesforceClient.QueryAsync<AgreementData>(
                    @"SELECT sentinalProposal_Id__c,
                             introducer_CommissionBalanceOutstanding__c,
                             introducer_CommissionTotalRefunds__c,
                             dealer_CommissionBalanceOutstanding__c,
                             dealer_CommissionTotalRefunds__c,
                             mcLarenCommission_CommissionBalanceOutst__c,
                             mcLarenCommission_CommissionTotalRefunds__c,
                             mcLarenVolumeBonus_CommissionBalanceOuts__c,
                             optionToPurchaseFee__c,
                             optionToPurchaseVat__c FROM Agreement__c
                      WHERE payoutDate__c != null
                      ORDER BY sentinalProposal_Id__c ASC NULLS FIRST");
            }).Wait(Timeout.InfiniteTimeSpan);

            var finished = false;
            do
            {
                finished = agreementDataResults.Done;

                // add any records
                if (agreementDataResults.Records.Count > 0)
                {
                    foreach (var agreement in agreementDataResults.Records)
                    {
                        SqlCommand cmd = new SqlCommand(@"INSERT INTO AgreementExtraDetails (AgreementProposalID,
                                                                             Introducer_CommissionBalanceOutstanding,
                                                                             Introducer_CommissionTotalRefunds,
                                                                             Dealer_CommissionBalanceOutstanding,
                                                                             Dealer_CommissionTotalRefunds,
                                                                             McLarenCommission_CommissionBalanceOutstanding,
                                                                             McLarenCommission_CommissionTotalRefunds,
                                                                             McLarenVolumeBonus_CommissionBalanceOutstanding,
                                                                             OptionToPurchaseAmount,
                                                                             OptionToPurchaseVat) 
                                                                             VALUES 
                                                                             (@AgreementProposalID,
                                                                             @Introducer_CommissionBalanceOutstanding,
                                                                             @Introducer_CommissionTotalRefunds,
                                                                             @Dealer_CommissionBalanceOutstanding,
                                                                             @Dealer_CommissionTotalRefunds,
                                                                             @McLarenCommission_CommissionBalanceOutstanding,
                                                                             @McLarenCommission_CommissionTotalRefunds,
                                                                             @McLarenVolumeBonus_CommissionBalanceOutstanding,
                                                                             @OptionToPurchaseAmount,
                                                                             @OptionToPurchaseVat)", Program.sqlConn);

                        cmd.Parameters.AddWithValue("@AgreementProposalID", agreement.sentinalProposal_Id__c);
                        cmd.Parameters.AddWithValue("@Introducer_CommissionBalanceOutstanding", agreement.introducer_CommissionBalanceOutstanding__c);
                        cmd.Parameters.AddWithValue("@Introducer_CommissionTotalRefunds", agreement.introducer_CommissionTotalRefunds__c);
                        cmd.Parameters.AddWithValue("@Dealer_CommissionBalanceOutstanding", agreement.dealer_CommissionBalanceOutstanding__c);
                        cmd.Parameters.AddWithValue("@Dealer_CommissionTotalRefunds", agreement.dealer_CommissionTotalRefunds__c);
                        cmd.Parameters.AddWithValue("@McLarenCommission_CommissionBalanceOutstanding", agreement.mcLarenCommission_CommissionBalanceOutst__c);
                        cmd.Parameters.AddWithValue("@McLarenCommission_CommissionTotalRefunds", agreement.mcLarenCommission_CommissionTotalRefunds__c);
                        cmd.Parameters.AddWithValue("@McLarenVolumeBonus_CommissionBalanceOutstanding", agreement.mcLarenVolumeBonus_CommissionBalanceOuts__c);
                        cmd.Parameters.AddWithValue("@OptionToPurchaseAmount", agreement.optionToPurchaseFee__c);
                        cmd.Parameters.AddWithValue("@OptionToPurchaseVat", agreement.optionToPurchaseVat__c);

                        cmd.ExecuteNonQuery();
                    }
                }

                // if more
                if (!finished)
                {
                    Task.Run(async () =>
                    {
                        // get next batch
                        agreementDataResults = await Program.salesforceClient.QueryContinuationAsync<AgreementData>(agreementDataResults.NextRecordsUrl);
                    }).Wait(Timeout.InfiniteTimeSpan);
                }
            } while (!finished);
        }

        public static void UpdateConstantsData()
        {
            {
                QueryResult<SageData> sageDataResults = null;

                Task.Run(async () =>
                {
                    Console.WriteLine(string.Concat("Querying AgreementData for AgreementExtraDetails"));

                    sageDataResults = await Program.salesforceClient.QueryAsync<SageData>(
                        "SELECT sage_TrialBalanceNL1160__c,sage_TrialBalanceNL1170__c FROM SageData__c");
                }).Wait(Timeout.InfiniteTimeSpan);

                if (sageDataResults.Records.Count > 0)
                {
                    foreach (var sageData in sageDataResults.Records)
                    {
                        SqlCommand cmd = new SqlCommand(@"UPDATE Constants  
                                                          SET SageTrackerAsEndOfMonth_TrialBalanceNL1170 = @SageTrackerAsEndOfMonth_TrialBalanceNL1170, 
                                                              SageCommissionAsEndOfMonth_TrialBalanceNL1160 = @SageCommissionAsEndOfMonth_TrialBalanceNL1160, ", Program.sqlConn);

                        cmd.Parameters.AddWithValue("@SageTrackerAsEndOfMonth_TrialBalanceNL1170", sageData.sage_TrialBalanceNL1160__c);
                        cmd.Parameters.AddWithValue("@SageCommissionAsEndOfMonth_TrialBalanceNL1160", sageData.sage_TrialBalanceNL1160__c);

                        cmd.ExecuteNonQuery();
                    }
                }
            }
        }
        
        public static void UpdateEthosVolumeCommission()
        {
            QueryResult<EthosVolumeCommissionData> ethosVolumeCommissionData = null;

            Task.Run(async () =>
            {
                Console.WriteLine(string.Concat("Quering EthosVolumeCommissionData for EthosVolumeCommission"));

                ethosVolumeCommissionData = await Program.salesforceClient.QueryAsync<EthosVolumeCommissionData>(
                    @"SELECT agreement__r.sentinalProposal_Id__c,
                             ethosVolume_TransactionAmountInvoiced__c, 
                             ethosVolume_TransactionAmountPaid__c, 
                             ethosVolume_TransactionDate__c 
                      FROM AgreementEthosVolumeTransaction__c
                      ORDER BY ethosVolume_TransactionDate__c ASC NULLS FIRST");
            }).Wait(Timeout.InfiniteTimeSpan);

            if (ethosVolumeCommissionData.Records.Count > 0)
            {
                foreach (var ethosVolumeCommission in ethosVolumeCommissionData.Records)
                {
                    SqlCommand cmd = new SqlCommand(@"INSERT INTO EthosVolumeCommission (ProposalId,
                                                                                        InvoiceDate,
                                                                                        InvoiceAmount) 
                                                                             VALUES 
                                                                             (@ProposalId,
                                                                             @InvoiceDate,
                                                                             @InvoiceAmount)", Program.sqlConn);

                    cmd.Parameters.AddWithValue("@ProposalId", ethosVolumeCommission.agreement__r.sentinalProposal_Id__c);
                    cmd.Parameters.AddWithValue("@InvoiceDate", ethosVolumeCommission.ethosVolume_TransactionDate__c);
                    cmd.Parameters.AddWithValue("@InvoiceAmount", ethosVolumeCommission.ethosVolume_TransactionAmountInvoiced__c);

                    cmd.ExecuteNonQuery();
                }
            }
        }

        public static void UpdateFacebookCommission()
        {
            QueryResult<FacebookCommissionData> facebookCommissionData = null;

            Task.Run(async () =>
            {
                Console.WriteLine(string.Concat("Quering FacebookCommissionData for FacebookCommission"));

                facebookCommissionData = await Program.salesforceClient.QueryAsync<FacebookCommissionData>(
                    @"SELECT agreement__r.sentinalProposal_Id__c,
                             facebook_TransactionAmountInvoiced__c,
                             facebook_TransactionDate__c
                       FROM AgreementFacebookTransaction__c
                       ORDER BY facebook_TransactionDate__c ASC NULLS FIRST");
            }).Wait(Timeout.InfiniteTimeSpan);

            if (facebookCommissionData.Records.Count > 0)
            {
                foreach (var facebookCommission in facebookCommissionData.Records)
                {
                    SqlCommand cmd = new SqlCommand(@"INSERT INTO FacebookCommission (ProposalId,
                                                                                        InvoiceDate,
                                                                                        InvoiceAmount) 
                                                                                         VALUES 
                                                                                         (@ProposalId,
                                                                                         @InvoiceDate,
                                                                                         @InvoiceAmount)", Program.sqlConn);

                    cmd.Parameters.AddWithValue("@ProposalId", facebookCommission.agreement__r.sentinalProposal_Id__c);
                    cmd.Parameters.AddWithValue("@InvoiceDate", facebookCommission.facebook_TransactionDate__c);
                    cmd.Parameters.AddWithValue("@InvoiceAmount", facebookCommission.facebook_TransactionAmountInvoiced__c);

                    cmd.ExecuteNonQuery();
                }
            }
        }

        public static void UpdateMcLarenCommission()
        {

            QueryResult<McLarenCommissionData> mcLarenCommissionData = null;

            Task.Run(async () =>
            {
                Console.WriteLine(string.Concat("Quering UpdateMcLarenCommissionData for UpdateMcLarenCommission"));

                mcLarenCommissionData = await Program.salesforceClient.QueryAsync<McLarenCommissionData>(
                    @"SELECT agreement__r.sentinalProposal_Id__c,
                             mcLarenCommission_CommissionTransactionD__c,
                             mcLarenCommission_CommissionAmountInvoic__c
                     FROM AgreementMcLarenCommissionTransaction__c
                     ORDER BY mcLarenCommission_CommissionTransactionD__c ASC NULLS FIRST");
            }).Wait(Timeout.InfiniteTimeSpan);

            if (mcLarenCommissionData.Records.Count > 0)
            {
                foreach (var mcLarenCommission in mcLarenCommissionData.Records)
                {
                    SqlCommand cmd = new SqlCommand(@"INSERT INTO McLarenCommission (ProposalId,
                                                                                     InvoiceDate,
                                                                                     InvoiceAmount) 
                                                                             VALUES 
                                                                             (@ProposalId,
                                                                             @InvoiceDate,
                                                                             @InvoiceAmount)", Program.sqlConn);

                    cmd.Parameters.AddWithValue("@ProposalId", mcLarenCommission.agreement__r.sentinalProposal_Id__c);
                    cmd.Parameters.AddWithValue("@InvoiceDate", mcLarenCommission.mcLarenCommission_CommissionTransactionD__c);
                    cmd.Parameters.AddWithValue("@InvoiceAmount", mcLarenCommission.mcLarenCommission_CommissionAmountInvoic__c);

                    cmd.ExecuteNonQuery();
                }
            }

        }

        public static void UpdateMcLarenVolumeBonus()
        {
            QueryResult<McLarenVolumeBonusCommissionData> mcLarenVolumeBonusCommissionData = null;

            Task.Run(async () =>
            {
                Console.WriteLine(string.Concat("Quering UpdateMcLarenVolumeBonusCommissionData for UpdateMcLarenVolumeBonusCommission"));

                mcLarenVolumeBonusCommissionData = await Program.salesforceClient.QueryAsync<McLarenVolumeBonusCommissionData>(
                    @"SELECT agreement__r.sentinalProposal_Id__c,
                             mcLarenVolumeBonus_CommissionTransacDate__c,
                             mcLarenVolumeBonus_CommissionAmountInv__c
                     FROM AgreementMcLarenVolumeBonusCommissionTra__c
                     ORDER BY mcLarenVolumeBonus_CommissionTransacDate__c ASC NULLS FIRST");
            }).Wait(Timeout.InfiniteTimeSpan);

            if (mcLarenVolumeBonusCommissionData.Records.Count > 0)
            {
                foreach (var mcLarenCommission in mcLarenVolumeBonusCommissionData.Records)
                {
                    SqlCommand cmd = new SqlCommand(@"INSERT INTO McLarenVolumeBonus (ProposalId,
                                                                                     InvoiceDate,
                                                                                     InvoiceAmount) 
                                                                             VALUES 
                                                                             (@ProposalId,
                                                                             @InvoiceDate,
                                                                             @InvoiceAmount)", Program.sqlConn);

                    cmd.Parameters.AddWithValue("@ProposalId", mcLarenCommission.agreement__r.sentinalProposal_Id__c);
                    cmd.Parameters.AddWithValue("@InvoiceDate", mcLarenCommission.mcLarenVolumeBonus_CommissionTransacDate__c);
                    cmd.Parameters.AddWithValue("@InvoiceAmount", mcLarenCommission.mcLarenVolumeBonus_CommissionAmountInv__c);

                    cmd.ExecuteNonQuery();
                }
            }

        }

        public static void UpdatePistonheadsPayment()
        {
            QueryResult<PistonheadsTransactionData> pistonheadsTransactionData = null;

            Task.Run(async () =>
            {
                Console.WriteLine(string.Concat("Quering PistonheadsTransactionData for PistonheadsPayment"));

                pistonheadsTransactionData = await Program.salesforceClient.QueryAsync<PistonheadsTransactionData>(
                    @"SELECT pistonheads_TransactionDate__c,
                             pistonheads_TransactionAmountInvoiced__c
                      FROM PistonheadsTransaction__c
                      ORDER BY pistonheads_TransactionDate__c ASC NULLS FIRST");
            }).Wait(Timeout.InfiniteTimeSpan);

            if (pistonheadsTransactionData.Records.Count > 0)
            {
                foreach (var pistonheadsPayment in pistonheadsTransactionData.Records)
                {
                    SqlCommand cmd = new SqlCommand(@"INSERT INTO PistonheadsPayment (PistonheadsPaymentDate,
                                                                          PistonheadsPaymentAmount) 
                                                                          VALUES 
                                                                          (@PistonheadsPaymentDate,
                                                                           @PistonheadsPaymentAmount)", Program.sqlConn);

                    cmd.Parameters.AddWithValue("@PistonheadsPaymentDate", pistonheadsPayment.pistonheads_TransactionDate__c);
                    cmd.Parameters.AddWithValue("@PistonheadsPaymentAmount", pistonheadsPayment.pistonheads_TransactionAmountInvoiced__c);

                    cmd.ExecuteNonQuery();
                }
            }

        }

        public static void UpdateTracker()
        {
            QueryResult<TrackerData> trackers = null;

            Task.Run(async () =>
            {
                Console.WriteLine(string.Concat("Quering TrackerData for Tracker"));

                trackers = await Program.salesforceClient.QueryAsync<TrackerData>(
                    @"SELECT TrackerStartDate__c,
                             tracker_TransactionAmountInvoicedVAT__c,
                             tracker_TransactionAmountInvoiced__c,
                             tracker_TransactionAmountPaid__c,
                             tracker_TransactionDate__c 
                      FROM AgreementTrackerTransaction__c
                      ORDER BY tracker_TransactionDate__c ASC NULLS FIRST");
            }).Wait(Timeout.InfiniteTimeSpan);

            if (trackers.Records.Count > 0)
            {
                foreach (var tracker in trackers.Records)
                {
                    SqlCommand cmd = new SqlCommand(@"INSERT INTO Tracker (AgreementProposalID,
                                                                           InvoiceDate,
                                                                           StartDate,
                                                                           InvoiceAmount,
                                                                           InvoiceAmountVAT,
                                                                           AmountPaid) 
                                                                           VALUES 
                                                                           (@AgreementProposalID,
                                                                           @InvoiceDate,
                                                                           @StartDate,
                                                                           @InvoiceAmount,
                                                                           @InvoiceAmountVAT,
                                                                           @AmountPaid)", Program.sqlConn);

                    cmd.Parameters.AddWithValue("@AgreementProposalID", tracker.agreement__r.sentinalProposal_Id__c);
                    cmd.Parameters.AddWithValue("@InvoiceDate", tracker.tracker_TransactionDate__c);
                    cmd.Parameters.AddWithValue("@StartDate", tracker.TrackerStartDate__c);
                    cmd.Parameters.AddWithValue("@InvoiceAmount", tracker.tracker_TransactionAmountInvoiced__c);
                    cmd.Parameters.AddWithValue("@InvoiceAmountVAT", tracker.tracker_TransactionAmountInvoicedVAT__c);
                    cmd.Parameters.AddWithValue("@AmountPaid", tracker.trackerSubscriptions_TranAmountPaid__c);

                    cmd.ExecuteNonQuery();
                }
            }

        }

        public static void UpdateTrackerSubscriptions()
        {
            QueryResult<TrackerSubscriptionsData> trackers = null;

            Task.Run(async () =>
            {
                Console.WriteLine(string.Concat("Quering TrackerSubscriptionsData for TrackerSubscriptions"));

                trackers = await Program.salesforceClient.QueryAsync<TrackerSubscriptionsData>(
                    @"SELECT TrackerStartDate__c,
                             trackerSubscriptions_TranAmountInv__c,
                             trackerSubscriptions_TransAmountInvVAT__c,
                             trackerSubscriptions_TranAmountPaid__c,
                             trackerSubscriptions_TransactionDate__c 
                      FROM AgreementTrackerSubscriptionsTransaction__c
                      ORDER BY trackerSubscriptions_TransactionDate__c ASC NULLS FIRST");
            }).Wait(Timeout.InfiniteTimeSpan);

            if (trackers.Records.Count > 0)
            {
                foreach (var tracker in trackers.Records)
                {
                    SqlCommand cmd = new SqlCommand(@"INSERT INTO TrackerSubscriptions (ProposalId,
                                                                           InvoiceDate,
                                                                           StartDate,
                                                                           InvoiceAmount,
                                                                           InvoiceAmountVAT,
                                                                           AmountPaid) 
                                                                           VALUES 
                                                                           (@ProposalID,
                                                                           @InvoiceDate,
                                                                           @StartDate,
                                                                           @InvoiceAmount,
                                                                           @InvoiceAmountVAT,
                                                                           @AmountPaid)", Program.sqlConn);

                    cmd.Parameters.AddWithValue("@ProposalID", tracker.agreement__r.sentinalProposal_Id__c);
                    cmd.Parameters.AddWithValue("@InvoiceDate", tracker.trackerSubscriptions_TransactionDate__c);
                    cmd.Parameters.AddWithValue("@StartDate", tracker.TrackerStartDate__c);
                    cmd.Parameters.AddWithValue("@InvoiceAmount", tracker.trackerSubscriptions_TranAmountInv__c);
                    cmd.Parameters.AddWithValue("@InvoiceAmountVAT", tracker.trackerSubscriptions_TransAmountInvVAT__c);
                    cmd.Parameters.AddWithValue("@AmountPaid", tracker.trackerSubscriptions_TranAmountPaid__c);

                    cmd.ExecuteNonQuery();
                }
            }


        }


        internal static void UpdateAmortisationData()
        {
            DeleteAmmortisationData();

            UpdateAgreementExtraDetailsData();

            UpdateConstantsData();
            UpdateEthosVolumeCommission();
            UpdateFacebookCommission();
            UpdateMcLarenCommission();
            UpdateMcLarenVolumeBonus();
            UpdatePistonheadsPayment();
            UpdateTracker();
            UpdateTrackerSubscriptions();
        }

        private static void DeleteAmmortisationData()
        {
            using (var command = new SqlCommand("ClearAmortisationData", Program.sqlConn) { CommandType = CommandType.StoredProcedure })
            { command.ExecuteNonQuery(); }
        }
    }
}
