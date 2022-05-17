using System;

namespace ClickSend
{

    public class ProposalCustomerData
    {
        public string id { get; set; }

        public string sentinalID__c { get; set; }

        public ProposalData proposal__r { get; set; }
        public CustomerData customer__r { get; set; }
    }

    public class ProposalData
    {
        public string id { get; set; }

        public string sentinalID__c { get; set; }
        public VehicleData PrimaryVehicle__r { get; set; }
    }

    public class CustomerData
    {
        public string Id { get; set; }
        public string panther_id__c { get; set; }

        public string Title { get; set; }
        public string Name { get; set; }
        
        public string MailingStreet { get; set; }
        public string MailingCity { get; set; }
        public string MailingState { get; set; }
        public string MailingPostalCode { get; set; }
        public string MailingCountry { get; set; }

        public string Phone { get; set; }
        public string HomePhone { get; set; }
        public string OtherPhone { get; set; }

        public string MobilePhone { get; set; }
        public string mobileNumber__c { get; set; }

        public string Email { get; set; }

        public string fraudIdentityConfirmationNo__c { get; set; }
        public DateTime? fraudIdentityConfirmationLetterSent__c { get; set; }
    }

    public class VehicleData {
        public string make__c { get; set; }
        public string model__c { get; set; }
    }
   
}
