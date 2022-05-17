﻿/*using Salesforce.Common.Models.Json;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace UpdateSalesforceData
{
    class TriggerControlUpdater
    {

        public static void UpdateTriggerControlData(SalesforceHttpClient salesforceClient, bool shouldTriggersRun)
        {
            Console.WriteLine(string.Concat("Started Update of Trigger Controller to ", shouldTriggersRun));

            var dateTimeSyncStarted = DateTime.Now;

                var triggerControlDataForSF = new TriggerControlData()
                {
                    ShouldTriggersRun__c = shouldTriggersRun
                };

            updateTriggerControlRecordInSalesforce(salesforceClient, triggerControlDataForSF);

            Console.WriteLine("Ended Update of Trigger Controller");
        }

        private static void updateTriggerControlRecordInSalesforce(SalesforceHttpClient salesforceClient, TriggerControlData triggerControlData)
        {
            //try
            //{
            var queryString = string.Concat(@"Select Id, ShouldTriggersRun__c from TriggerControl__c Limit 1");

            QueryResult<TriggerControlData> anchorWebServices = null;

            Task.Run(async () =>
            {
                anchorWebServices = await salesforceClient.QueryAsync<TriggerControlData>(queryString);
            }).Wait(Timeout.InfiniteTimeSpan);

            if (anchorWebServices.Records.Count > 0)
            {
                    SuccessResponse successResponse = null;

                    Task.Run(async () =>
                    {
                        successResponse = await salesforceClient.UpdateAsync("TriggerControl__c", anchorWebServices.Records[0].Id, triggerControlData);
                    }).Wait(Timeout.InfiniteTimeSpan);
                }
            }
        }
}
*/