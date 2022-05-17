using System;
using System.Data.SqlClient;
using System.Linq;

namespace UpdateSalesforceData
{
    public class Program
    {
        public static SqlConnection sqlConn, sqlConn1;
        public static SalesforceHttpClient salesforceClient;

        static void Main(string[] args)
        {
            //string n = null;
            //n = n.ToString();
            //try
            //{
                Console.WriteLine(string.Concat("Started - ", DateTime.Now.ToString()));

                salesforceClient = DataHelper.GetSalesforceConnection();
                sqlConn = DataHelper.GetOpenConnection();
                sqlConn1 = DataHelper.GetOpenConnection();

                AgreementUpdater.UpdateAgreementData();

                #region full update

                //if (args.ToList()
                //    .Any(arg => new[] { "fullupdate", "-f", "f", "full" }.Any(flag => string.Equals(arg, flag, StringComparison.CurrentCultureIgnoreCase))))
                //{
                //    //Switch off triggers
                //    TriggerControlUpdater.UpdateTriggerControlData(false);

                //    ProposalUpdater.UpdateProposalData();

                //    IntroducerUpdater.UpdateAccountData();
                //    DealerUpdater.UpdateAccountData();
                //    CustomerCompanyUpdater.UpdateCustomerCompanyData();

                //    CustomerUpdater.UpdateCustomerData();


                //    PayProfileUpdater.UpdateAgreementPayProfile();
                //    //TransactionUpdater.UpdateAgreementTransactions();

                //    AmortisationUpdater.UpdateAmortisationData();

                //    //VehicleUpdater.UpdateVehicleData();

                //    //Switch on triggers
                //    TriggerControlUpdater.UpdateTriggerControlData(true);
                //}
                #endregion

                sqlConn.Close();
                sqlConn1.Close();

                //if (salesforceClient != null) { salesforceClient.Dispose(); }

                Console.WriteLine(string.Concat("Finished - ", DateTime.Now.ToString()));

                Environment.Exit(1);
                //Console.ReadLine();
            }

            //catch (Exception ex)
            //{
            //    //ExceptionLogging.ExceptionLogging.Write(ex);
            //}
        //}

    }
}
