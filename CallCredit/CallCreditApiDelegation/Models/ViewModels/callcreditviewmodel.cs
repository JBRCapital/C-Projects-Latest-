//using System.ComponentModel.DataAnnotations;

//namespace CallCreditApiDelegation.Controllers
//{
//    /// <summary>
//    /// the main model, used for clean binding from the post request
//    /// nullable enums are there so it wouldn't invalidate the model
//    /// required items are marked with required attributes
//    /// same goes for datatype and length attribute
//    /// </summary>
//    public class callcreditviewmodel
//    {
//        public accommodationOptions? accommodation { get; set; }

//        public string address { get; set; }

//        public string annualSalary { get; set; }

//        public string dob { get; set; }

//        public bool emailConsent { get; set; }

//        [Required]
//        [DataType(DataType.EmailAddress)]
//        public string email { get; set; }

//        public employercategoryOptions? employercategory { get; set; }

//        public employmentstatusOptions? employmentstatus { get; set; }

//        [Required]
//        public string forename { get; set; }

//        public genderOptions? gender { get; set; }

//        public maritalStatusOptions? maritalstatus { get; set; }

//        public homeownerOptions? homeowner { get; set; }

//        public string monthsAtAddress { get; set; }

//        public string monthsAtEmployment { get; set; }

//        public string othernames { get; set; }

//        public string postcode { get; set; }

//        [Required]
//        public string surname { get; set; }

//        public string title { get; set; }

//        public string totaldependants { get; set; }

//        public string yearsAtAddress { get; set; }

//        public string yearsAtEmployment { get; set; }

//        public string gRecaptchaResponse { get; set; }
//    }
//}
