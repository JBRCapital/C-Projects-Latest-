using System;
using System.Threading;

using UpworkPlatesLookupAutomationDLL;

namespace UpdateSalesforceData
{
    class VehicleUpdater
    {

        #region Models

        public class VehicleData
        {
            public class c_proposal__r
            {
                public class c_primary_agreement__r
                {
                    public string approval_agreementNumber__c { get; set; }
                    public DateTime? payoutDate__c { get; set; }
                }

                public c_primary_agreement__r primary_agreement__r;
            }

            public string id { get; set; }
            public DateTime? insuranceDateLastChecked__c { get; set; }
            public string insuranceClaimsContact__c { get; set; }
            public string insuranceInsurer__c { get; set; }
            public string insurancePolicyNumber__c { get; set; }
            public string registrationPlate__c { get; set; }

            public c_proposal__r proposal__r { get; set; }
            public string vehicle__c { get; set; }

            public DateTime? lastUpdatedFromSentinel__c { get; set; }
        }

        #endregion

        static PlateLookupAutomation lookUp;
        static PlateLookupAutomation  LookUp { get {

                if (lookUp == null) { lookUp = new PlateLookupAutomation(); }
                
                if (lookUp.IsLoggedIn) return lookUp;

                lookUp.Login();
                while (!lookUp.IsLoggedIn)
                {   
                    Thread.Sleep(1000);
                }

                return lookUp;
         } }

        public static void UpdateVehicleData()
        {
            LogHelper.Logger.WriteOutput(string.Concat("Started Sync of Vehicles"), Program.EmailTransactionLog);

            updateVehicleRecordInSalesforce();

            LogHelper.Logger.WriteOutput(string.Concat("Ended Sync of Vehicles"), Program.EmailTransactionLog);
        }

        private static void updateVehicleRecordInSalesforce()
        {
            var queryString = string.Concat(@"SELECT Id,
                                                    proposal__r.primary_agreement__r.approval_agreementNumber__c,
                                                    proposal__r.primary_agreement__r.payoutDate__c,
                                                    insuranceDateLastChecked__c,
                                                    insuranceClaimsContact__c,
                                                    insuranceInsurer__c,
                                                    insurancePolicyNumber__c,
                                                    vehicle__c,
                                                    registrationPlate__c
                                                FROM ProposalVehicle__c
                                                WHERE proposal__r.IsLatest__c = True AND proposal__r.isDummyProposal__c = False
                                                AND proposal__r.primary_agreement__r.payoutDate__c != null 
                                                AND proposal__r.sentinalWorkflowStageId__c = 5
                                                AND proposal__r.primary_agreement__r.OutstandingPrinciple__c > 0
                                                AND registrationPlate__c != null and insuranceDateLastChecked__c = null");

            var vehicles = SalesforceDataHelper.SalesforceClient.GetRecordsFromSalesforce<VehicleData>("VehicleData",queryString);

            Result LookupResult = null;

            foreach (var proposalVehicle in vehicles)
            {
                if (!proposalVehicle.insuranceDateLastChecked__c.HasValue && proposalVehicle.proposal__r.primary_agreement__r.payoutDate__c.Value.AddDays(14) <= DateTime.Now
                    //||
                    //(proposalVehicle.insuranceDateLastChecked__c.HasValue && proposalVehicle.insuranceDateLastChecked__c.Value.AddDays(30) < DateTime.Now)
                   )
                {
                    LogHelper.Logger.WriteOutput(string.Concat("Checking Vehicle:", proposalVehicle.registrationPlate__c), Program.EmailTransactionLog);

                    LookUp.Lookup((result) => LookupResult = result, proposalVehicle.registrationPlate__c.Replace(" ", string.Empty).Trim(), proposalVehicle.proposal__r.primary_agreement__r.approval_agreementNumber__c, DateTime.Now);
                    while (LookUp.IsLookingUp) Thread.Sleep(1000);
                    object updatedVehicle = null;

                    if (LookupResult == null)
                    {
                        LogHelper.Logger.WriteOutput("Vehicle " + proposalVehicle.registrationPlate__c + " insurance NOT found", Program.EmailTransactionLog);
                        updatedVehicle = new
                        {
                            insuranceDateLastChecked__c = DateTime.Now,
                            insurancePolicyNumber__c = "Not Found",
                            insuranceInsurer__c = string.Empty,
                            insuranceClaimsContact__c = string.Empty,
                        };
                    }
                    else
                    {
                        LogHelper.Logger.WriteOutput("Vehicle " + proposalVehicle.registrationPlate__c + " insurance found: " + LookupResult.PolicyNumber, Program.EmailTransactionLog);
                        updatedVehicle = new
                        {
                            insuranceDateLastChecked__c = DateTime.Now,
                            insurancePolicyNumber__c = LookupResult.PolicyNumber.Substring(0,Math.Min(LookupResult.PolicyNumber.Length, 99)),
                            insuranceInsurer__c = LookupResult.Insurer.Substring(0, Math.Min(LookupResult.Insurer.Length, 99)),
                            insuranceClaimsContact__c = LookupResult.ClaimsContact.Substring(0, Math.Min(LookupResult.ClaimsContact.Length, 254)),
                            lastUpdatedFromSentinel__c = DateTime.Now
                    };
                    }

                    LogHelper.Logger.WriteOutput(string.Concat("Upserting Vehicle:", proposalVehicle.id), Program.EmailTransactionLog);
                    //var successResponseProposalVehicle__c = SalesforceDataHelper.SalesforceClient.UpsertAsync("ProposalVehicle__c", proposalVehicle.id, updatedVehicle);
                    //var successResponseVehicle__c = SalesforceDataHelper.SalesforceClient.UpsertAsync("Vehicle__c", proposalVehicle.vehicle__c, updatedVehicle);
                    //LogHelper.Logger.WriteOutput(successResponseProposalVehicle__c != null && successResponseProposalVehicle__c.Success &&
                    //                    successResponseVehicle__c != null && successResponseVehicle__c.Success ? "Success" : "Failed");
                }
            }
        }
            
    }
}
