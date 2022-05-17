using System;
using System.ComponentModel.DataAnnotations;
using System.Threading;

namespace CallCreditWrapper
{
    /// <summary>
    /// the main model, used for clean binding from the post request
    /// nullable enums are there so it wouldn't invalidate the model
    /// required items are marked with required attributes
    /// same goes for datatype and length attribute
    /// </summary>
    public class CallCreditModel
    {

        #region address 1
        public string address1 { get; set; }
        public string postcode1 { get; set; }
        public accommodationOptions? accommodation1 { get; set; }
        public string yearsAtAddress1 { get; set; }
        public string monthsAtAddress1 { get; set; }
        #endregion

        #region address 2
        public string address2 { get; set; }
        public string postcode2 { get; set; }
        public accommodationOptions? accommodation2 { get; set; }
        public string yearsAtAddress2 { get; set; }
        public string monthsAtAddress2 { get; set; }
        #endregion

        #region address 3
        public string address3 { get; set; }
        public string postcode3 { get; set; }
        public accommodationOptions? accommodation3 { get; set; }
        public string yearsAtAddress3 { get; set; }
        public string monthsAtAddress3 { get; set; }
        #endregion

        #region address 4
        public string address4 { get; set; }
        public string postcode4 { get; set; }
        public accommodationOptions? accommodation4 { get; set; }
        public string yearsAtAddress4 { get; set; }
        public string monthsAtAddress4 { get; set; } 
        #endregion

        public string annualSalary { get; set; }

        public string dob { set; get; }
        public DateTime DOBDate { get { return DateTime.Parse(dob); } }
        
        [Required]
        [DataType(DataType.EmailAddress)]
        public string email { get; set; }

        public bool emailConsent { get; set; }

        //public employercategoryOptions? EmployerCategory { get; set; }
        public employmentstatusOptions? employmentStatus { get; set; }
        public string monthsAtEmployment { get; set; }

        public genderOptions? gender { get; set; }

        public maritalStatusOptions? maritalstatus { get; set; }

        public homeownerOptions? homeowner { get; set; }
        

        private string Title;
        public string title { get {
                return Title ?? string.Empty; } set {
                Title = (value == null ? string.Empty : Thread.CurrentThread.CurrentCulture.TextInfo.ToTitleCase(value)); } }

        private string Forename;
        [Required]
        public string forename { get {
                return Forename ?? string.Empty; } set {
                Forename = (value == null ? string.Empty : Thread.CurrentThread.CurrentCulture.TextInfo.ToTitleCase(value)); } }

        private string Othernames;
        public string othernames { get { return Othernames ?? string.Empty; } set { Othernames = (value == null ? string.Empty : Thread.CurrentThread.CurrentCulture.TextInfo.ToTitleCase(value)); } }

        private string Surname;
        [Required]
        public string surname { get {
                return Surname ?? string.Empty; } set {
                Surname = (value == null ? string.Empty : Thread.CurrentThread.CurrentCulture.TextInfo.ToTitleCase(value)); } }

        public string totalDependants { get; set; }


        public string yearsAtEmployment { get; set; }

        public string gRecaptchaResponse { get; set; }

        public CallCreditPhoneNumber HomePhone { get { return new CallCreditPhoneNumber() { Std = "", No = "" };  } }
        public CallCreditPhoneNumber Mobile { get { return new CallCreditPhoneNumber() { Std = "", No = "" }; } }
    }
}
