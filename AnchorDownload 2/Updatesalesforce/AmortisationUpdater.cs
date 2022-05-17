using System;
using System.Data;
using System.Data.SqlClient;

namespace UpdateSalesforceData
{
    class AmortisationUpdater
    {
        #region Models

        public class SageData // SageData__c
        {
            public double? sage_TrialBalanceNL1160__c { get; set; }
            public double? sage_TrialBalanceNL1170__c { get; set; }
        }

        public class PistonheadsTransactionData // PistonheadsTransaction__c
        {
            public DateTime? pistonheads_TransactionDate__c { get; set; }
            public double? pistonheads_TransactionAmountInvoiced__c { get; set; }
        }

        public class EthosVolumeCommissionData //AgreementEthosVolumeTransaction__c
        {
            public class c_agreement__r
            {
                public double? sentinalProposal_Id__c { get; set; }
            }

            public c_agreement__r agreement__r { get; set; }
            public DateTime? ethosVolume_TransactionDate__c { get; set; }
            public double? ethosVolume_TransactionAmountInvoiced__c { get; set; }
        }

        public class FacebookCommissionData //AgreementFacebookTransaction__c
        {
            public class c_agreement__r
            {
                public double? sentinalProposal_Id__c { get; set; }
            }

            public c_agreement__r agreement__r { get; set; }
            public DateTime? facebook_TransactionDate__c { get; set; }
            public double? facebook_TransactionAmountInvoiced__c { get; set; }
        }

        public class McLarenCommissionData //AgreementMcLarenCommissionTransaction__c
        {
            public class c_agreement__r
            {
                public double? sentinalProposal_Id__c { get; set; }
            }

            public c_agreement__r agreement__r { get; set; }
            public DateTime? mcLarenCommission_CommissionTransactionD__c { get; set; }
            public double? mcLarenCommission_CommissionAmountInvoic__c { get; set; }
        }

        public class McLarenVolumeBonusCommissionData //AgreementMcLarenVolumeBonusCommissionTra__c
        {
            public class c_agreement__r
            {
                public double? sentinalProposal_Id__c { get; set; }
            }

            public c_agreement__r agreement__r { get; set; }
            public DateTime? mcLarenVolumeBonus_CommissionTransacDate__c { get; set; }
            public double? mcLarenVolumeBonus_CommissionAmountInv__c { get; set; }
        }

        public class TrackerData // AgreementTrackerTransaction__c
        {
            public class c_agreement__r
            {
                public double? sentinalProposal_Id__c { get; set; }
            }

            public c_agreement__r agreement__r { get; set; }

            public DateTime? TrackerStartDate__c { get; set; }
            public double? tracker_TransactionAmountInvoiced__c { get; set; }
            public double? tracker_TransactionAmountInvoicedVAT__c { get; set; }
            public double? trackerSubscriptions_TranAmountPaid__c { get; set; }
            public DateTime? tracker_TransactionDate__c { get; set; }
        }

        public class TrackerSubscriptionsData //AgreementTrackerSubscriptionsTransaction__c
        {
            public class c_agreement__r
            {
                public double? sentinalProposal_Id__c { get; set; }
            }

            public c_agreement__r agreement__r { get; set; }

            public DateTime? TrackerStartDate__c { get; set; }
            public double? trackerSubscriptions_TranAmountInv__c { get; set; }
            public double? trackerSubscriptions_TransAmountInvVAT__c { get; set; }
            public double? trackerSubscriptions_TranAmountPaid__c { get; set; }
            public DateTime? trackerSubscriptions_TransactionDate__c { get; set; }

        }

        #endregion

        internal static void UpdateAmortisationData()
        {
            DeleteAmmortisationData();

            UpdateAgreementExtraDetails();

            UpdateConstants();
            UpdateEthosVolumeCommission();
            UpdateFacebookCommission();
            UpdateMcLarenCommission();
            UpdateMcLarenVolumeBonus();
            UpdatePistonheadsPayment();
            UpdateTracker();
            UpdateTrackerSubscriptions();
        }

        public static void UpdateAgreementExtraDetails()
        {

            var agreementData =
                     SalesforceDataHelper.SalesforceClient.GetRecordsFromSalesforce<AgreementData>("Agreements",
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

                // add any records
                foreach (var agreement in agreementData)
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
                                                                            @OptionToPurchaseVat)", SQLDataConnectionHelper.SqlConnection);

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

        public static void UpdateConstants()
        {
            {
                var sageDataResults =
                    SalesforceDataHelper.SalesforceClient.GetRecordsFromSalesforce<SageData>("ConstantsData", 
                                                "SELECT sage_TrialBalanceNL1160__c,sage_TrialBalanceNL1170__c " +
                                                "FROM SageData__c");
                
                foreach (var sageData in sageDataResults)
                {
                    SqlCommand cmd = new SqlCommand(@"UPDATE Constants  
                                                        SET SageTrackerAsEndOfMonth_TrialBalanceNL1170 = @SageTrackerAsEndOfMonth_TrialBalanceNL1170, 
                                                            SageCommissionAsEndOfMonth_TrialBalanceNL1160 = @SageCommissionAsEndOfMonth_TrialBalanceNL1160, ", SQLDataConnectionHelper.SqlConnection);

                    cmd.Parameters.AddWithValue("@SageTrackerAsEndOfMonth_TrialBalanceNL1170", sageData.sage_TrialBalanceNL1160__c);
                    cmd.Parameters.AddWithValue("@SageCommissionAsEndOfMonth_TrialBalanceNL1160", sageData.sage_TrialBalanceNL1160__c);

                    cmd.ExecuteNonQuery();
                }
                
            }
        }

        public static void UpdateEthosVolumeCommission()
        {
            var ethosVolumeCommissionData =
                SalesforceDataHelper.SalesforceClient.GetRecordsFromSalesforce<EthosVolumeCommissionData>("EthosVolumeCommissionData",
                                    @"SELECT agreement__r.sentinalProposal_Id__c,
                                             ethosVolume_TransactionAmountInvoiced__c, 
                                             ethosVolume_TransactionAmountPaid__c, 
                                             ethosVolume_TransactionDate__c 
                                      FROM AgreementEthosVolumeTransaction__c
                                      ORDER BY ethosVolume_TransactionDate__c ASC NULLS FIRST");


            foreach (var ethosVolumeCommission in ethosVolumeCommissionData)
            {
                SqlCommand cmd = new SqlCommand(@"INSERT INTO EthosVolumeCommission (ProposalId,
                                                                                    InvoiceDate,
                                                                                    InvoiceAmount) 
                                                                            VALUES 
                                                                            (@ProposalId,
                                                                            @InvoiceDate,
                                                                            @InvoiceAmount)", SQLDataConnectionHelper.SqlConnection);

                cmd.Parameters.AddWithValue("@ProposalId", ethosVolumeCommission.agreement__r.sentinalProposal_Id__c);
                cmd.Parameters.AddWithValue("@InvoiceDate", ethosVolumeCommission.ethosVolume_TransactionDate__c);
                cmd.Parameters.AddWithValue("@InvoiceAmount", ethosVolumeCommission.ethosVolume_TransactionAmountInvoiced__c);

                cmd.ExecuteNonQuery();
            }    
        }

        public static void UpdateFacebookCommission()
        {
            var facebookCommissionData =
                       SalesforceDataHelper.SalesforceClient.GetRecordsFromSalesforce<FacebookCommissionData>("FacebookCommissionData",
                                @"SELECT agreement__r.sentinalProposal_Id__c,
                             facebook_TransactionAmountInvoiced__c,
                             facebook_TransactionDate__c
                       FROM AgreementFacebookTransaction__c
                       ORDER BY facebook_TransactionDate__c ASC NULLS FIRST");

                foreach (var facebookCommission in facebookCommissionData)
                {
                    SqlCommand cmd = new SqlCommand(@"INSERT INTO FacebookCommission (ProposalId,
                                                                                        InvoiceDate,
                                                                                        InvoiceAmount) 
                                                                                         VALUES 
                                                                                         (@ProposalId,
                                                                                         @InvoiceDate,
                                                                                         @InvoiceAmount)", SQLDataConnectionHelper.SqlConnection);

                    cmd.Parameters.AddWithValue("@ProposalId", facebookCommission.agreement__r.sentinalProposal_Id__c);
                    cmd.Parameters.AddWithValue("@InvoiceDate", facebookCommission.facebook_TransactionDate__c);
                    cmd.Parameters.AddWithValue("@InvoiceAmount", facebookCommission.facebook_TransactionAmountInvoiced__c);

                    cmd.ExecuteNonQuery();
                }
        }

        public static void UpdateMcLarenCommission()
        {
            var mcLarenCommissionData =
                SalesforceDataHelper.SalesforceClient.GetRecordsFromSalesforce<McLarenCommissionData>("UpdateMcLarenCommissionData",
                        @"SELECT agreement__r.sentinalProposal_Id__c,
                             mcLarenCommission_CommissionTransactionD__c,
                             mcLarenCommission_CommissionAmountInvoic__c
                        FROM AgreementMcLarenCommissionTransaction__c
                        ORDER BY mcLarenCommission_CommissionTransactionD__c ASC NULLS FIRST");

            foreach (var mcLarenCommission in mcLarenCommissionData)
            {
                SqlCommand cmd = new SqlCommand(@"INSERT INTO McLarenCommission (ProposalId,
                                                                                    InvoiceDate,
                                                                                    InvoiceAmount) 
                                                                            VALUES 
                                                                            (@ProposalId,
                                                                            @InvoiceDate,
                                                                            @InvoiceAmount)", SQLDataConnectionHelper.SqlConnection);

                cmd.Parameters.AddWithValue("@ProposalId", mcLarenCommission.agreement__r.sentinalProposal_Id__c);
                cmd.Parameters.AddWithValue("@InvoiceDate", mcLarenCommission.mcLarenCommission_CommissionTransactionD__c);
                cmd.Parameters.AddWithValue("@InvoiceAmount", mcLarenCommission.mcLarenCommission_CommissionAmountInvoic__c);

                cmd.ExecuteNonQuery();
            }
        }

        public static void UpdateMcLarenVolumeBonus()
        {
            var mcLarenVolumeBonusCommissionData = 
                SalesforceDataHelper.SalesforceClient.GetRecordsFromSalesforce<McLarenVolumeBonusCommissionData>("UpdateMcLarenVolumeBonusCommissionData",
                        @"SELECT agreement__r.sentinalProposal_Id__c,
                             mcLarenVolumeBonus_CommissionTransacDate__c,
                             mcLarenVolumeBonus_CommissionAmountInv__c
                     FROM AgreementMcLarenVolumeBonusCommissionTra__c
                     ORDER BY mcLarenVolumeBonus_CommissionTransacDate__c ASC NULLS FIRST");

                foreach (var mcLarenCommission in mcLarenVolumeBonusCommissionData)
                {
                    SqlCommand cmd = new SqlCommand(@"INSERT INTO McLarenVolumeBonus (ProposalId,
                                                                                     InvoiceDate,
                                                                                     InvoiceAmount) 
                                                                             VALUES 
                                                                             (@ProposalId,
                                                                             @InvoiceDate,
                                                                             @InvoiceAmount)", SQLDataConnectionHelper.SqlConnection);

                    cmd.Parameters.AddWithValue("@ProposalId", mcLarenCommission.agreement__r.sentinalProposal_Id__c);
                    cmd.Parameters.AddWithValue("@InvoiceDate", mcLarenCommission.mcLarenVolumeBonus_CommissionTransacDate__c);
                    cmd.Parameters.AddWithValue("@InvoiceAmount", mcLarenCommission.mcLarenVolumeBonus_CommissionAmountInv__c);

                    cmd.ExecuteNonQuery();
                }
            }

        public static void UpdatePistonheadsPayment()
        {
            var pistonheadsTransactionData = SalesforceDataHelper.SalesforceClient.GetRecordsFromSalesforce<PistonheadsTransactionData>("UpdatePistonheadsTransactionData",
                    @"SELECT pistonheads_TransactionDate__c,
                             pistonheads_TransactionAmountInvoiced__c
                      FROM PistonheadsTransaction__c
                      ORDER BY pistonheads_TransactionDate__c ASC NULLS FIRST");
   
                foreach (var pistonheadsPayment in pistonheadsTransactionData)
                {
                    SqlCommand cmd = new SqlCommand(@"INSERT INTO PistonheadsPayment (PistonheadsPaymentDate,
                                                                          PistonheadsPaymentAmount) 
                                                                          VALUES 
                                                                          (@PistonheadsPaymentDate,
                                                                           @PistonheadsPaymentAmount)", SQLDataConnectionHelper.SqlConnection);

                    cmd.Parameters.AddWithValue("@PistonheadsPaymentDate", pistonheadsPayment.pistonheads_TransactionDate__c);
                    cmd.Parameters.AddWithValue("@PistonheadsPaymentAmount", pistonheadsPayment.pistonheads_TransactionAmountInvoiced__c);

                    cmd.ExecuteNonQuery();
                }
        }

        public static void UpdateTracker()
        {
            var trackers = SalesforceDataHelper.SalesforceClient.GetRecordsFromSalesforce<TrackerData>("UpdateTrackerData",
                    @"SELECT TrackerStartDate__c,
                             tracker_TransactionAmountInvoicedVAT__c,
                             tracker_TransactionAmountInvoiced__c,
                             tracker_TransactionAmountPaid__c,
                             tracker_TransactionDate__c 
                      FROM AgreementTrackerTransaction__c
                      ORDER BY tracker_TransactionDate__c ASC NULLS FIRST");
 
                foreach (var tracker in trackers)
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
                                                                           @AmountPaid)", SQLDataConnectionHelper.SqlConnection);

                    cmd.Parameters.AddWithValue("@AgreementProposalID", tracker.agreement__r.sentinalProposal_Id__c);
                    cmd.Parameters.AddWithValue("@InvoiceDate", tracker.tracker_TransactionDate__c);
                    cmd.Parameters.AddWithValue("@StartDate", tracker.TrackerStartDate__c);
                    cmd.Parameters.AddWithValue("@InvoiceAmount", tracker.tracker_TransactionAmountInvoiced__c);
                    cmd.Parameters.AddWithValue("@InvoiceAmountVAT", tracker.tracker_TransactionAmountInvoicedVAT__c);
                    cmd.Parameters.AddWithValue("@AmountPaid", tracker.trackerSubscriptions_TranAmountPaid__c);

                    cmd.ExecuteNonQuery();
                }
        }

        public static void UpdateTrackerSubscriptions()
        {
            var trackerSubscriptions = SalesforceDataHelper.SalesforceClient.GetRecordsFromSalesforce<TrackerSubscriptionsData>("UpdateTrackerSubscriptions",
                    @"SELECT TrackerStartDate__c,
                             trackerSubscriptions_TranAmountInv__c,
                             trackerSubscriptions_TransAmountInvVAT__c,
                             trackerSubscriptions_TranAmountPaid__c,
                             trackerSubscriptions_TransactionDate__c 
                      FROM AgreementTrackerSubscriptionsTransaction__c
                      ORDER BY trackerSubscriptions_TransactionDate__c ASC NULLS FIRST");

                foreach (var trackerSubscription in trackerSubscriptions)
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
                                                                           @AmountPaid)", SQLDataConnectionHelper.SqlConnection);

                    cmd.Parameters.AddWithValue("@ProposalID", trackerSubscription.agreement__r.sentinalProposal_Id__c);
                    cmd.Parameters.AddWithValue("@InvoiceDate", trackerSubscription.trackerSubscriptions_TransactionDate__c);
                    cmd.Parameters.AddWithValue("@StartDate", trackerSubscription.TrackerStartDate__c);
                    cmd.Parameters.AddWithValue("@InvoiceAmount", trackerSubscription.trackerSubscriptions_TranAmountInv__c);
                    cmd.Parameters.AddWithValue("@InvoiceAmountVAT", trackerSubscription.trackerSubscriptions_TransAmountInvVAT__c);
                    cmd.Parameters.AddWithValue("@AmountPaid", trackerSubscription.trackerSubscriptions_TranAmountPaid__c);

                    cmd.ExecuteNonQuery();
                }
        }

        private static void DeleteAmmortisationData()
        {
            using (var command = new SqlCommand("ClearAmortisationData", SQLDataConnectionHelper.SqlConnection) { CommandType = CommandType.StoredProcedure })
            { command.ExecuteNonQuery(); }
        }
    }
}
