using SalesforceDataLibrary;
using System;
using System.Text;
using System.Threading;

using UpworkPlatesLookupAutomationDLL;

namespace VehicleUpdater
{
    class VehicleUpdater
    {

        public static StringBuilder EmailErrorLog = new StringBuilder();
        public static StringBuilder EmailTransactionLog = new StringBuilder();

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
        static PlateLookupAutomation LookUp
        {
            get
            {

                if (lookUp == null) { lookUp = new PlateLookupAutomation(); }

                if (lookUp.IsLoggedIn) return lookUp;

                lookUp.Login();
                while (!lookUp.IsLoggedIn)
                {
                    Thread.Sleep(1000);
                }

                return lookUp;
            }
        }

        public static void UpdateVehicleData()
        {
            Console.WriteLine(string.Concat("Started Sync of Vehicles"));

            updateVehicleRecordInSalesforce();

            Console.WriteLine(string.Concat("Ended Sync of Vehicles"));
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
                                                AND registrationPlate__c != null"); // and insuranceDateLastChecked__c = null

            var vehicles = SalesforceDataHelper.SalesforceClient.GetRecordsFromSalesforce<VehicleData>("VehicleData", queryString);

            Result LookupResult = null;

            foreach (var proposalVehicle in vehicles)
            {
                if (!proposalVehicle.insuranceDateLastChecked__c.HasValue && proposalVehicle.proposal__r.primary_agreement__r.payoutDate__c.Value.AddDays(14) <= DateTime.Now
                   ||
                   (proposalVehicle.insuranceDateLastChecked__c.HasValue && proposalVehicle.insuranceDateLastChecked__c.Value.AddDays(0) < DateTime.Now.Date)
                   )
                {
                    Console.WriteLine(string.Concat("Checking Vehicle:", proposalVehicle.registrationPlate__c));

                    LookUp.Lookup((result) => LookupResult = result, proposalVehicle.registrationPlate__c.Replace(" ", string.Empty).Trim(), proposalVehicle.proposal__r.primary_agreement__r.approval_agreementNumber__c, DateTime.Now);
                    while (LookUp.IsLookingUp) Thread.Sleep(1000);
                    CustomSObject updatedVehicle = null;
                    CustomSObject updatedProposalVehicle = null;

                    if (LookupResult == null)
                    {
                        Console.WriteLine("Vehicle " + proposalVehicle.registrationPlate__c + " insurance NOT found");
                        updatedVehicle = new CustomSObject()
                        {
                           // { "id",  proposalVehicle.vehicle__c },
                            { "insuranceDateLastChecked__c", DataHelper.GetDateTime(DateTime.Now) },
                            { "insurancePolicyNumber__c", "Not Found" },
                            { "insuranceInsurer__c", string.Empty },
                            { "insuranceClaimsContact__c", string.Empty },
                            //{ "lastUpdatedFromSentinel__c", DataHelper.GetDateTime(DateTime.Now) }
                        };
                        updatedProposalVehicle = new CustomSObject()
                        {
                          //  { "id",  proposalVehicle.id },
                            { "insuranceDateLastChecked__c", DataHelper.GetDateTime(DateTime.Now) },
                            { "insurancePolicyNumber__c", "Not Found" },
                            { "insuranceInsurer__c", string.Empty },
                            { "insuranceClaimsContact__c", string.Empty },
                        };
                    }
                    else
                    {
                        Console.WriteLine("Vehicle " + proposalVehicle.registrationPlate__c + " insurance found: " + LookupResult.PolicyNumber);
                        updatedVehicle = new CustomSObject()
                        {
                           // { "id",  proposalVehicle.id },
                            { "insuranceDateLastChecked__c", DataHelper.GetDateTime(DateTime.Now) },
                            { "insurancePolicyNumber__c", LookupResult.PolicyNumber.Substring(0, Math.Min(LookupResult.PolicyNumber.Length, 99))},
                            { "insuranceInsurer__c",  LookupResult.Insurer.Substring(0, Math.Min(LookupResult.Insurer.Length, 99))},
                            { "insuranceClaimsContact__c", LookupResult.ClaimsContact.Substring(0, Math.Min(LookupResult.ClaimsContact.Length, 254))},
                            //{ "lastUpdatedFromSentinel__c", DataHelper.GetDateTime(DateTime.Now) }
                        };
                        updatedProposalVehicle = new CustomSObject()
                        {
                           // { "id",  proposalVehicle.vehicle__c },
                            { "insuranceDateLastChecked__c",  DataHelper.GetDateTime(DateTime.Now) },
                            { "insurancePolicyNumber__c",LookupResult.PolicyNumber.Substring(0, Math.Min(LookupResult.PolicyNumber.Length, 99))},
                            { "insuranceInsurer__c", LookupResult.Insurer.Substring(0, Math.Min(LookupResult.Insurer.Length, 99))},
                            { "insuranceClaimsContact__c", LookupResult.ClaimsContact.Substring(0, Math.Min(LookupResult.ClaimsContact.Length, 254))},
                        };
                    }


                    string error;
                    Console.WriteLine(string.Concat("Upserting Vehicle:", proposalVehicle.id));
                    
                    var successResponseProposalVehicle__c = SalesforceDataHelper.SalesforceClient.UpdateAsync("ProposalVehicle__c", proposalVehicle.id, updatedProposalVehicle);
                    if (proposalVehicle.vehicle__c != null)
                    {
                        var successResponseVehicle__c = SalesforceDataHelper.SalesforceClient.UpdateAsync("Vehicle__c", proposalVehicle.vehicle__c, updatedVehicle);
                        Console.WriteLine(successResponseProposalVehicle__c != null && successResponseProposalVehicle__c.Success &&
                                            successResponseVehicle__c != null && successResponseVehicle__c.Success ? "Success" : "Failed");
                    }
                }
            }
        }

    }
   
    class Program
    {
        public static SalesforceClient SalesforceClient { get => DataHelper.GetSalesforceConnection(); }
        public static StringBuilder EmailLog = new StringBuilder();

        static void Main(string[] args)
        {
            VehicleUpdater.UpdateVehicleData();
        }
    }
}
