using CallCreditApiDelegation.Controllers;
using CallCreditWrapper;
using Salesforce.Common.Models.Json;
using System;
using System.Threading.Tasks;
using System.Web.Configuration;

namespace CallCreditApiDelegation.Helpers
{
    /// <summary>
    /// sales force helper, used to follow the DRY and SRP principles 
    /// </summary>
    public class SalesForceHelper
    {

        internal static void SaveInformationOnSalesForce(CallCreditModel model, CallCreditResult creditScoreResult)
        {
            SalesforceCredentials salesforceCredentials = new SalesforceCredentials
            {
                SecurityToken = WebConfigurationManager.AppSettings["SecurityToken"],
                ConsumerKey = WebConfigurationManager.AppSettings["ConsumerKey"],
                ConsumerSecret = WebConfigurationManager.AppSettings["ConsumerSecret"],
                Username = WebConfigurationManager.AppSettings["Username"],
                Password = WebConfigurationManager.AppSettings["Password"]
            };

            string JoinWith(string text1, string text2, string joiner)
            {

                if (text1 == null) { text1 = string.Empty; }
                if (text2 == null) { text2 = string.Empty; }

                if (text1.Length > 0 && text2.Length > 0)
                {
                    return string.Concat(text1, joiner, text2);
                }

                return text1 + text2;
            }

            string getAccommodationTypeString(CallCreditWrapper.accommodationOptions? accOption)
            {
                if (accOption == null) { return null; }

                switch (accOption.Value)
                {
                    case CallCreditWrapper.accommodationOptions.Detached_House: { return "Detached House"; }
                    case CallCreditWrapper.accommodationOptions.Semi_Detached_House: { return "Semi - Detached House"; }
                    case CallCreditWrapper.accommodationOptions.Terraced_House: { return "Terraced House"; }
                    case CallCreditWrapper.accommodationOptions.Bungalow: { return "Bungalow"; }
                    case CallCreditWrapper.accommodationOptions.Flat_Apartment: { return "Flat / Apartment"; }
                    case CallCreditWrapper.accommodationOptions.Other: { return "Other"; }
                    case CallCreditWrapper.accommodationOptions.Not_Asked: { return "Other"; }
                    case CallCreditWrapper.accommodationOptions.Not_Given: { return "Other"; }
                }

                return "Other";
            }
            string getEmploymentStatusTypeString(CallCreditWrapper.employmentstatusOptions? employmentStatusOption)
            {
                if (employmentStatusOption == null) { return null; }

                switch (employmentStatusOption.Value)
                {
                    case CallCreditWrapper.employmentstatusOptions.Casual_Employment: { return "Casual Employment"; }
                    case CallCreditWrapper.employmentstatusOptions.Full_Time_Employee: { return "Full Time Employee"; }
                    case CallCreditWrapper.employmentstatusOptions.Other: { return "Other"; }
                    case CallCreditWrapper.employmentstatusOptions.Part_Time_Employee: { return "Part Time Employee"; }
                    case CallCreditWrapper.employmentstatusOptions.Retired: { return "Retired"; }
                    case CallCreditWrapper.employmentstatusOptions.Self_Employed: { return "Self Employed"; }
                    case CallCreditWrapper.employmentstatusOptions.Student: { return "Student"; }
                    case CallCreditWrapper.employmentstatusOptions.Temporary_Employee: { return "Temporary Employee"; }
                    case CallCreditWrapper.employmentstatusOptions.Trainee_Apprentice: { return "Trainee Apprentice"; }
                    case CallCreditWrapper.employmentstatusOptions.Unemployed: { return "Unemployed"; }
                }

                return "Other";
            }
            string getMartialStatusTypeString(CallCreditWrapper.maritalStatusOptions? maritalStatusOption)
            {
                if (maritalStatusOption == null) return null;
                
                switch (maritalStatusOption.Value)
                {
                    case CallCreditWrapper.maritalStatusOptions.Single: { return "Single"; }
                    case CallCreditWrapper.maritalStatusOptions.Married: { return "Married"; }
                    case CallCreditWrapper.maritalStatusOptions.Partnered: { return "Partnered"; }
                    case CallCreditWrapper.maritalStatusOptions.Cohabiting_but_not_married: { return "Cohabiting but not married"; }
                    case CallCreditWrapper.maritalStatusOptions.Separated: { return "Separated"; }
                    case CallCreditWrapper.maritalStatusOptions.Divorced: { return "Divorced"; }
                    case CallCreditWrapper.maritalStatusOptions.Widowed: { return "Widowed"; }
                }

                return "";
            }


            var leadDataForSF = new
            {
                RecordType = new { Name = "End User Prospect" },

                // Personal Details -----------------------------

                Title = model.title,

                FirstName = JoinWith(model.forename, model.othernames, " "),
                LastName = model.surname,

                Email = model.email,

                Company = "None",

                 dateOfBirth__c = model.dob.Replace('/','-'),

                Gender__c = model.gender == null ? null : model.gender == CallCreditWrapper.genderOptions.Male ? "Male" : "Female",

                maritalStatus__c = getMartialStatusTypeString(model.maritalstatus),

                dependants__c = model.totalDependants,

                homeOwner__c = model.homeowner,

                // Address History ////////////////////////////////////////////////////////

                buildingNumber__c = model.address1,
                postCode__c = model.postcode1,

                accommodationType__c = getAccommodationTypeString(model.accommodation1),

                yearsAtAddress__c =  model.yearsAtAddress1,
                monthsAtAddress__c = model.monthsAtAddress1,

                // Employment History ////////////////////////////////////////////////////////

                employmentstatus__c = getEmploymentStatusTypeString(model.employmentStatus),

                yearsAtCurrentEmployer__c = model.yearsAtEmployment,
                monthsAtCurrentEmployer__c = model.monthsAtEmployment,

                grossAnnualSalary__c = model.annualSalary,
                
                HasOptedOutOfEmail = !model.emailConsent,

                ///////model.employercategory;

                creditScoreCategory__c = creditScoreResult.creditScoreCategory
            };


            Salesforce.Force.ForceClient salesforceClient = null;
            SuccessResponse successResponse = null;

            Task.Run(async () => { salesforceClient = await DataHelper.GetSalesforceConnection(salesforceCredentials); }).Wait();
            Task.Run(async () =>
            {
                try
                {
                    successResponse = await salesforceClient.CreateAsync("Lead", leadDataForSF);
                }
                catch (Exception e)
                {
                }
            }).Wait();

        }
    }
}