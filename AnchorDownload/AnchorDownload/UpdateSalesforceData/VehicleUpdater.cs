using System;
using System.Threading;
using System.Threading.Tasks;

using Salesforce.Common.Models.Json;
using Salesforce.Force;

using UpworkPlatesLookupAutomationDLL;

namespace UpdateSalesforceData
{
    class VehicleUpdater
    {
        static PlateLookupAutomation lookUp;

        public static void UpdateVehicleData(ForceClient salesforceClient)
        {
            Console.WriteLine(string.Concat("Started Sync of Vehicles"));

            lookUp = new PlateLookupAutomation();
            lookUp.Login();
            while (!lookUp.IsLoggedIn) Thread.Sleep(1000);

            updateVehicleRecordInSalesforce(salesforceClient);

            Console.WriteLine(string.Concat("Ended Sync of Vehicles"));
        }

        private static void updateVehicleRecordInSalesforce(ForceClient salesforceClient)
        {
            //try
            //{
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

            QueryResult<VehicleData> anchorWebServices = null;

            Task.Run(async () =>
            {
                anchorWebServices = await salesforceClient.QueryAsync<VehicleData>(queryString);
            }).Wait(Timeout.InfiniteTimeSpan);

            Result LookupResult = null;

            if (anchorWebServices.Records.Count == 0) return;

            foreach (var proposalVehicle in anchorWebServices.Records)
            {
                if (!proposalVehicle.insuranceDateLastChecked__c.HasValue && proposalVehicle.proposal__r.primary_agreement__r.payoutDate__c.Value.AddDays(14) <= DateTime.Now)
                    //||
                    //(proposalVehicle.insuranceDateLastChecked__c.HasValue && proposalVehicle.insuranceDateLastChecked__c.Value.AddDays(30) < DateTime.Now)
                    //)
                {
                    Console.WriteLine(string.Concat("Checking Vehicle:", proposalVehicle.registrationPlate__c));

                    lookUp.Lookup((result) => LookupResult = result, proposalVehicle.registrationPlate__c.Replace(" ", string.Empty).Trim(), proposalVehicle.proposal__r.primary_agreement__r.approval_agreementNumber__c, DateTime.Now);
                    while (lookUp.IsLookingUp) Thread.Sleep(1000);

                    SuccessResponse successResponse = null;
                    object updatedVehicle = null;

                    if (LookupResult == null)
                    {
                        Console.WriteLine("Vehicle " + proposalVehicle.registrationPlate__c + " insurance NOT found");
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
                        Console.WriteLine("Vehicle " + proposalVehicle.registrationPlate__c + " insurance found: " + LookupResult.PolicyNumber);
                        updatedVehicle = new
                        {
                            insuranceDateLastChecked__c = DateTime.Now,
                            insurancePolicyNumber__c = LookupResult.PolicyNumber.Substring(0,Math.Min(LookupResult.PolicyNumber.Length, 99)),
                            insuranceInsurer__c = LookupResult.Insurer.Substring(0, Math.Min(LookupResult.Insurer.Length, 99)),
                            insuranceClaimsContact__c = LookupResult.ClaimsContact.Substring(0, Math.Min(LookupResult.ClaimsContact.Length, 254)),
                            lastUpdatedFromSentinel__c = DateTime.Now
                    };
                    }

                    Task.Run(async () =>
                    {
                        Console.WriteLine(string.Concat("Upserting Vehicle:", proposalVehicle.id));
                        successResponse = await salesforceClient.UpdateAsync("ProposalVehicle__c", proposalVehicle.id, updatedVehicle);
                        successResponse = await salesforceClient.UpdateAsync("Vehicle__c", proposalVehicle.vehicle__c, updatedVehicle);
                        Console.WriteLine(successResponse != null && successResponse.Success ? "Success" : "Failed");
                    }).Wait(Timeout.InfiniteTimeSpan);
                }
            }
        }
            
    }
}
