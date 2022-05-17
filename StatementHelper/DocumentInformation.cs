using System;
using System.Collections.Generic;
using System.Data.SqlClient;

namespace AutoDocHelper
{
    public class DocumentInformation
    {
        public static Dictionary<string, string> GetDocumentInformation(SqlDataReader headerInfo) => new Dictionary<string, string>()
        {
            {"<Address>", getAddress(headerInfo)},
            {"<Date>", DateTime.Now.ToString("01/MM/yyyy")},
            {"<CustomerLetterName>", headerInfo["CustomerLetterName"].ToString()},
            {"<AgreementType>", headerInfo["AgreementType"].ToString()},
            {"<AgreementReference>", headerInfo["AgreementNumber"].ToString()},
            {"<AccountHolders>", headerInfo["CustomerNames"].ToString()},
            {"<StatementPeriodStart>", headerInfo["StatementStartDate"].ToString()},
            {"<StatementPeriodEnd>", headerInfo["StatementEndDate"].ToString()},
            {"<RegistrationPlates>", headerInfo["VehicleRegNo"].ToString()},
            {"<VehicleDetails>", headerInfo["VehicleDescription"].ToString()},
            {"<StartDateOfAgreement>", headerInfo["AgreementDate"].ToString()},
            {"<AgreementTermInMonths>", headerInfo["AgreementDuration"].ToString()},
            {"<Advance>", headerInfo["AgreementAdvance"].ToString()},
            {"<APR>", headerInfo["AgreementAPR"].ToString()},
            {"<GrossYield>",headerInfo["GrossYield"].ToString() },
            {"<DocFee>", Double.Parse(headerInfo["DocFee"].ToString()).ToString("N2")},
        };

        public static Dictionary<string, string> GetDocumentInformationForFirstStatement(SqlDataReader headerInfo) => new Dictionary<string, string>()
        {
            //{"<Address>", getAddress(headerInfo)},
            //{"<Date>", DateTime.Now.ToString("01/MM/yyyy")},
            //{"<CustomerLetterName>", headerInfo["CustomerLetterName"].ToString()},
            //{"<AgreementType>", headerInfo["AgreementType"].ToString()},
            {"<AgreementReference>", headerInfo["AgreementNumber"].ToString()},
            //{"<AccountHolders>", headerInfo["CustomerNames"].ToString()},
            //{"<StatementPeriodStart>", headerInfo["StatementStartDate"].ToString()},
            //{"<StatementPeriodEnd>", headerInfo["StatementEndDate"].ToString()},
            //{"<RegistrationPlates>", headerInfo["VehicleRegNo"].ToString()},
            {"<VehicleDetails>", headerInfo["VehicleDescription"].ToString()},
            //{"<StartDateOfAgreement>", headerInfo["AgreementDate"].ToString()},
            //{"<AgreementTermInMonths>", headerInfo["AgreementDuration"].ToString()},
            //{"<Advance>", headerInfo["AgreementAdvance"].ToString()},
            //{"<APR>", headerInfo["AgreementAPR"].ToString()},
            {"<DocFee>", Double.Parse(headerInfo["DocFee"].ToString()).ToString("N2")},
        };

        public static string getAddress(SqlDataReader reader)
        {

            var address = reader["CustomerShortName"].ToString();
            var lineBreak = "\v";

            address = StringFunctions.JoinWith(address, reader["CustomerAddress1"], lineBreak);
            address = StringFunctions.JoinWith(address, reader["CustomerAddress2"], lineBreak);
            address = StringFunctions.JoinWith(address, reader["CustomerAddress3"], lineBreak);
            address = StringFunctions.JoinWith(address, reader["CustomerAddress4"], lineBreak);
            address = StringFunctions.JoinWith(address, reader["CustomerAddress5"], lineBreak);
            address = StringFunctions.JoinWith(address, reader["CustomerPostcode"], lineBreak);

            return address;
        }
    }
}
