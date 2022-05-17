/*using System;

namespace UpdateSalesforceData
{
    class Program
    {
        static void Main(string[] args)
        {
            try
            {
                Console.WriteLine(string.Concat("Started - ", DateTime.Now.ToString()));

                //Salesforce.Force.ForceClient 
                SalesforceHttpClient salesforceClient = null;

                //Task.Run(async () =>
                //{

                    salesforceClient = *//*await*//* DataHelper.GetSalesforceConnection();
                    



                //}).Wait(Timeout.InfiniteTimeSpan);

                var sqlConn = DataHelper.GetOpenConnection();
                var sqlConn1 = DataHelper.GetOpenConnection();

                //AgreementUpdater.UpdateAgreementData(sqlConn, salesforceClient);

                //Switch off triggers
                //TriggerControlUpdater.UpdateTriggerControlData(salesforceClient, false);

                //ProposalUpdater.UpdateProposalData(sqlConn, salesforceClient);

                
                ////IntroducerUpdater.UpdateAccountData(sqlConn, salesforceClient);
                ////DealerUpdater.UpdateAccountData(sqlConn, salesforceClient);
                
                //CustomerUpdater.UpdateCustomerData(sqlConn, salesforceClient);
                ////CustomerCompanyUpdater.UpdateCustomerCompanyData(sqlConn, salesforceClient);

                //PayProfileUpdater.UpdateAgreementPayProfile(sqlConn, sqlConn1, salesforceClient);
                //TransactionUpdater.UpdateAgreementTransactions(sqlConn, sqlConn1, salesforceClient);

                /////AmortisationUpdater.UpdateAmortisationData(sqlConn, salesforceClient);
                /////VehicleUpdater.UpdateVehicleData(salesforceClient);

                //Switch on triggers
                //TriggerControlUpdater.UpdateTriggerControlData(salesforceClient, true);

                //sqlConn.Close();
                //sqlConn1.Close();

                //if (salesforceClient != null) { salesforceClient.Dispose(); }

                //Console.WriteLine(string.Concat("Finished - ", DateTime.Now.ToString()));

                //Environment.Exit(1);
                //Console.ReadLine();
            }

            catch (Exception ex)
            {
                ExceptionLogging.ExceptionLogging.Write(ex);
            }
        }

    }
}
*/