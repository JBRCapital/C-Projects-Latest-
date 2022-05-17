using Callcredit.CallReport7;
using System;
using System.Collections.Generic;
using System.Linq;

namespace CallCreditWrapper
{
    /// <summary>
    /// a wrapper for the call credit api, used to simplify the 
    /// request to call credit to retrive the data easily
    /// </summary>
    public class CallCredit
    {
        private CallReport7 apiProxy;
        private CT_SearchDefinition apiSD;

        /// <summary>
        /// the main constructor for the call credit api wrapper
        /// </summary>
        /// <param name="CompanyName">the company name</param>
        /// <param name="Username">the user name</param>
        /// <param name="Password">the password</param>
        /// <param name="endpoint">the api end point</param>
        public CallCredit(string CompanyName,
            string Username,
            string Password,
            string endpoint = "https://www.callcreditsecure.co.uk/Services/CallReport/CallReport7.asmx")
        {
            //string TESTendpoint = "https://ct.callcreditsecure.co.uk/Services/CallReport/CallReport7.asmx"
            apiProxy = new CallReport7
            {
                Url = endpoint,
                callcreditheadersValue = new callcreditheaders()
                {
                    company = CompanyName,
                    username = Username,
                    password = Password
                }
            };
        }

        /// <summary>
        /// the main function for the call credit api wrapper, used to
        /// delegate data to the actual api easily and call it then
        /// return the result of the api call
        /// </summary>
        /// <param name="Title">the user's title</param>
        /// <param name="Forname">the user's forname</param>
        /// <param name="Surename">the user's surename</param>
        /// <param name="dob">the user's date of birth</param>
        /// <param name="email">the user's email</param>
        /// <param name="HomePhone">the user's home phone number</param>
        /// <param name="Mobile">the user's mobile phone number</param>
        /// <param name="BuildingNumber">the user's building number</param>
        /// <param name="Postcode">the user's postal code</param>
        /// <returns>returns an object of type CallCreditResult that encapsulate
        /// the result, with error on failure and credit score on success</returns>
        public CallCreditResult Search(CallCreditModel model)
        {

            #region Preparing request

            CT_searchapplicant apiApplicant;

            apiSD = new CT_SearchDefinition
            {
                creditrequest = new CT_searchrequest
                {
                    purpose = "QS",
                    score = Convert.ToByte(true),
                    scoreSpecified = true,
                    transient = 0,
                    transientSpecified = true,
                    schemaversion = "7.2",
                    datasets = 511,

                    applicant = new CT_searchapplicant[] { apiApplicant = new CT_searchapplicant
                    {
                        name = new CT_inputname[] { new CT_inputname
                        {
                            title = model.title,
                            forename = model.forename,
                            surname = model.surname
                        }},
                        dob = model.DOBDate,
                        dobSpecified = true,
                        hho = Convert.ToByte(false),
                        hhoSpecified = true,
                        tpoptout = Convert.ToByte(false),
                        tpoptoutSpecified = true,
                        applicantdemographics = new CT_applicantdemographics
                        {
                            contact = new CT_applicantdemographicsContact() {
                                email = new CT_demographicsemail[] { new CT_demographicsemail { address = model.email, type = "03" } },
                                telephone = new CT_demographicstelephone[] {
                                        new CT_demographicstelephone
                                        {
                                            number = model.HomePhone?.No,
                                            std = model.HomePhone?.Std,
                                            type = "13"
                                        },
                                        new CT_demographicstelephone
                                        {
                                            number = model.Mobile?.No,
                                            std = model.Mobile?.Std,
                                            type = "15"
                                        } }}
                        }
                    }
                }
                }
            };

            var ValidAddresses = new List<KeyValuePair<string, string>>();
            if (!string.IsNullOrEmpty(model.address1) && !string.IsNullOrEmpty(model.postcode1))
                ValidAddresses.Add(new KeyValuePair<string, string>(model.address1, model.postcode1));
            if (!string.IsNullOrEmpty(model.address2) && !string.IsNullOrEmpty(model.postcode2))
                ValidAddresses.Add(new KeyValuePair<string, string>(model.address2, model.postcode2));
            if (!string.IsNullOrEmpty(model.address3) && !string.IsNullOrEmpty(model.postcode3))
                ValidAddresses.Add(new KeyValuePair<string, string>(model.address3, model.postcode3));
            if (!string.IsNullOrEmpty(model.address4) && !string.IsNullOrEmpty(model.postcode4))
                ValidAddresses.Add(new KeyValuePair<string, string>(model.address4, model.postcode4));

            var applicantAddresses = apiSD.creditrequest.applicant.First().address = new CT_inputaddress[ValidAddresses.Count];
            for (int i = 0; i < ValidAddresses.Count; i++)
                applicantAddresses[i] = new CT_inputaddress
                {
                    buildingno = ValidAddresses[i].Key,
                    postcode = ValidAddresses[i].Value
                };
            #endregion

            CT_SearchResult apiresult = new CT_SearchResult();
            CallCreditResult result = new CallCreditResult();

            try
            {
                /* Now that the proxy and credit request objects have been initialized,
                 * we can issue the request to the Callcredit API. */
                apiresult = apiProxy.Search07a(apiSD);
                var creditScore = apiresult.creditreport.applicant[0].creditscores[0].score.Value;
                string creditScoreCategory = string.Empty, creditScoreText = string.Empty, creditScoreExplanation = string.Empty;

                if (creditScore >= 628 && creditScore <= 710) {
                    creditScoreCategory = "Excellent";
                    creditScoreText = "Excellent";
                    creditScoreExplanation = "With an excellent credit score, assuming no other adverse issues, you would qualify for a good rate. Speak to one of our experts today on 020 3355 0035 to discuss your options and get a tailored quote to suit your needs.";
                }

                if (creditScore >= 604 && creditScore <= 627) {
                    creditScoreCategory = "Good";
                    creditScoreText = "Good";
                    creditScoreExplanation = "With a good credit score, assuming no other adverse issues, you should qualify for a good rate. We would advise using a Credit reporting service to review your credit, in order see if you can boost your Credit Score. Speak to one of our experts today to discuss your options and get a tailored quote to suit your needs. Our experts are on hand to discuss your requirements and can provide a fast, no obligation quote today. Call on 020 3355 0035.";
                }

                if (creditScore >= 566 && creditScore <= 603) {
                    creditScoreCategory = "Fair";
                    creditScoreText = "Fair";
                    creditScoreExplanation = "With a fair credit score, you may not qualify for the best rates. An underwriter may call to discuss your adverse credit profiles to understand the background of the issues. We would advise using a Credit reporting service to review your adverse credit. Speak to our expert team today on 020 3355 0035 – we would be happy to talk you through your options and bring you one step closer to your dream car.";
                }
                if (creditScore >= 551 && creditScore <= 565) {
                    creditScoreCategory = "Poor";
                    creditScoreText = "Poor";
                    creditScoreExplanation = "With a poor credit score, you unfortunately won’t qualify for the best rates. Though your application may not pass in the initial stages, an underwriter may call to discuss your adverse credit profiles to understand the background of the issues. We would advise using a Credit reporting service to review your adverse credit. If you would like to discuss finance options, call our dedicated team on 020 3355 0035 for a fast, tailored quote.";
                }

                if (creditScore >= 0 && creditScore <= 550) {
                    creditScoreCategory = "VeryPoor";
                    creditScoreText = "Very Poor";
                    creditScoreExplanation = "With a very poor credit score, you unfortunately won’t qualify for the best rates and may have difficulty obtaining credit. Though your application may not pass in the initial stages, an underwriter may call to discuss your adverse credit profiles to understand the background of the issues. We would advise using a Credit reporting service to review your adverse credit. Our friendly team is on hand to talk you through  your finance options – call today on 020 3355 0035.";
                }


                result.creditScoreCategory = creditScoreCategory;
                result.creditScoreText = creditScoreText;
                result.creditScoreExplanation = creditScoreExplanation;
                result.Succeeded = true;
            }
            catch (Exception e)
            {
                result.Succeeded = false;
                result.Error = e.Message;
            }
            finally
            {
                apiProxy.Dispose();
            }

            return result;
        }
    }
}