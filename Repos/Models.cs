using System;

namespace UpdateSalesforceData
{

    //Proposal

    public class ProposalData
    {
        public string id { get; set; }

        public string sentinalID__c { get; set; }
        
        public string sentinalWorkflowStageId__c { get; set; }

        public double? introducer_commissionAmount__c { get; set; }
        public double? supplier_commissionAmount__c { get; set; }

        public DateTime? dateTimeProposalFirstAccepted__c { get; set; }
        public string firstAcceptedStatus__c { get; set; }

        public DateTime? firstDecisionedDateTime__c { get; set; }
        public string firstDecisionedStatus__c { get; set; }

        public double? timeTakenIn_NewProposals__c { get; set; }
        public double? timeTakenIn_ReferredForInformation__c { get; set; }
        public double? timeTakenIn_CreditManager__c { get; set; }
        public double? timeTakenIn_SeniorCreditManager__c { get; set; }
        public double? timeTakenIn_CreditCommittee__c { get; set; }
        public double? timeTakenIn_Funder__c { get; set; }
        public double? timeTakenIn_Decisioned__c { get; set; }

        public double? timeTakenIn_TotalCredit__c { get; set; }
        public double? timeTakenIn_CreditPlusReferred__c { get; set; }
        public double? timeTakenIn_CreditPlusSalesSupport__c { get; set; }
        public double? timeTakenIn_AwaitingPayout__c { get; set; }
        public double? timeTaIn_CreditPlusSalesSupportPlusRefer__c { get; set; }

        public DateTime? lastUpdatedFromSentinel__c { get; set; }
    }


    //Agreement

    public class AgreementData
    {
        //public string id { get; set; }
        public string approval_agreementNumber__c { get; set; }

        public string sentinalProposal_Id__c { get; set; }
        public DateTime? payoutDate__c { get; set; }
        public DateTime? settledDate__c { get; set; }
        public DateTime? payoutDateWithLive__c { get; set; }

        public double? totalCashPrice__c { get; set; }
        public double? calcVehicleValue__c { get; set; }

        public double? netInstalment__c { get; set; }

        public double? residualValue__c { get; set; }

        public double? balancePayable__c { get; set; }
        public double? balanceIncludingFuturePayments__c { get; set; }

        public double? docFees__c { get; set; }
        public double? totalFees__c { get; set; }

        public double? originalAdvance__c { get; set; }
        public double? originalCharges__c { get; set; }
        public double? originalTotalFees__c { get; set; }
        public double? originalBalancePayable__c { get; set; }
        public double? originalTotalPayable__c { get; set; }
        public double? outstandingPrinciple__c { get; set; }
        public double? outstandingCharges__c { get; set; }
        public double? outstandingFees__c { get; set; }
        public double? outstandingTotalBalance__c { get; set; }
        public double? arrearsAmount__c { get; set; }

        public DateTime? firstPaymentDate__c { get; set; }
        public DateTime? contractEndDate__c { get; set; }

        public DateTime? arrearsDateWasDue__c { get; set; }
        public double? arrearsNumberOfDaysOverdue__c { get; set; }

        public DateTime? lastPaymentDateDue__c { get; set; }
        public double? lastPaymentAmountDue__c { get; set; }
        public DateTime? lastPaymentDatePaid__c { get; set; }
        public double? lastPaymentAmountPaid__c { get; set; }

        public DateTime? nextPaymentDueDate__c { get; set; }
        public double? nextPaymentAmountDue__c { get; set; }

        public double? nextPaymentInterestAmountDue__c { get; set; }

        public double? outstandingPrincipleAtSettlement__c { get; set; }
        public double? outstandingPaymentAtSettlement__c { get; set; }

        public ProposalData proposal__r { get; set; }

        public string introducerType__c { get; set; }
        public string dealerType__c { get; set; }

        public int? introduction_introducer_id__c { get; set; }
        public string introducerSalesforceId__c { get; set; }
        public string introduction_introducer_name__c { get; set; }
        public double? introducer_commissionAmount__c { get; set; }

        public int? introduction_supplier_id__c { get; set; }
        public string supplierSalesforceId__c { get; set; }
        public string introduction_supplier_name__c { get; set; }
        public double? supplier_commissionAmount__c { get; set; }

        public string introduction_introducerChannel__c { get; set; }
        public double? grossYield__c { get; set; }
        public double? netYield__c { get; set; }
        public int? numberOfVehicles__c { get; set; }
        public string make__c { get; set; }
        public string model__c { get; set; }
        public string vehicleType__c { get; set; }
        public string introduction_jbrSalesman__c { get; set; }
        //public string introduction_jbrSalesmanId__c { get; set; }
        public string introduction_jbrSalesmanName__c { get; set; }

        public double? payProfileOutstanding__c { get; set; }
        public double? payProfileOSLessPrincipleOS__c { get; set; }
        public double? lastPaymentPrincipleAmountLessDifference__c { get; set; }

        //Commission Related
        public double? introducer_CommissionBalanceOutstanding__c { get; set; }
        public double? introducer_CommissionTotalRefunds__c { get; set; }
        public double? dealer_CommissionBalanceOutstanding__c { get; set; }
        public double? dealer_CommissionTotalRefunds__c { get; set; }
        public double? mcLarenCommission_CommissionBalanceOutst__c { get; set; }
        public double? mcLarenCommission_CommissionTotalRefunds__c { get; set; }
        public double? mcLarenVolumeBonus_CommissionBalanceOuts__c { get; set; }
        public double? optionToPurchaseFee__c { get; set; }
        public double? optionToPurchaseVat__c { get; set; }

        //public double? feesTotal__C { get; set; }
        public double? totalCharges__c { get; set; }

        public string customersAndCompanyNames__c { get; set; }

        public bool? fundingDetails_loanTypeIsRegulated__c { get; set; }

        public double? contractLength__c { get; set; }

        public double? noOfFullMonthsSinceAgreementStarted__c { get; set; }

        public double? agreementPresentMonth__c { get; set; }

        public double? capitalPaidOrSettled__c { get; set; }

        public DateTime? workDayPayDate__c { get; set; }
        public DateTime? workDayPayDatePlusOneWorkingDay__c { get; set; }

        public double? optionToPurchaseFeeRemaining__c { get; set; }
        public double? optionToPurchaseFeeVATRemaining__c { get; set; }

        public double? numberOfPaymentsMade__c { get; set; }
        public double? totalNumberOfPayments__c { get; set; }

public DateTime? lastUpdatedFromSentinel__c { get; set; }

    }

    public class AgreementPayProfileData
    {

        public string Id { get; set; }
        public string approval_agreementNumber__c { get; set; }

        public PayProfileDataRecords PayProfiles__r { get; set; }

    }


    //Pay Profile

    public class PayProfileDataRecords
    {
        public PayProfileData[] Records { get; set; }
    }

    public class PayProfileData
    {
        public string Id { get; set; }

        public string Name { get; set; }
        public string Agreement__c { get; set; }
        public string Sentinel_Agreement_Number__c { get; set; }
        public DateTime? PayDate__c { get; set; }
        public double? Instalment__c { get; set; }
        public double? Principle__c { get; set; }
        public double? Interest__c { get; set; }
        public double? Fee__c { get; set; }
        public double? VATonFee__c { get; set; }

        public bool? PayFallenDue__c { get; set; }
    }

    public class TransactionData
    {
        public string Agreement__c { get; set; }
        public string SentinelAgreementNumber__c { get; set; }
        //Not needed as its the external Id for the upsert
        //public Int32 TransactionId__c { get; set; }
        public DateTime TransactionDate__c { get; set; }
        public double Amount__c { get; set; }    //TransNetPayment
        public double InstalmentDue__c { get; set; }
        public double StandingOrder__c { get; set; }
        public double BankTransfer__c { get; set; }
        public double DirectDebit__c { get; set; }
        public double CardPayment__c { get; set; }
        public double Cheque__c { get; set; }
        public double Adjustment__c { get; set; }
        public double SettlementPayment__c { get; set; }
        public double ArrearsAdjustment__c { get; set; }
        public double WriteOff__c { get; set; }
        public double TransactionReversal__c { get; set; }
        public double ReturnedCheque__c { get; set; }
        public double Refund__c { get; set; }
        public double SettlementPaymentReversal__c { get; set; }
        public double ReturnedDD_RTP__c { get; set; }
        public double ReturnedDD_IC__c { get; set; }
        public double ReturnedDD_NI__c { get; set; }
        public double ReturnedDD_NA__c { get; set; }
        public double ReturnedDD_ANYD__c { get; set; }
        public double ReturnedDD_PD__c { get; set; }
        public double ReturnedDD_AC__c { get; set; }
        public double RebateOfInterest__c { get; set; }
        public double PenaltyInterest__c { get; set; }
        public double EarlySettlementFee__c { get; set; }
        public double LatePaymentFee__c { get; set; }
        public double ExcessMileageCharge__c { get; set; }
        public double FairWearAndTearCharge__c { get; set; }
        public double OtherFeesVATable__c { get; set; }
        public double OtherFeesNoVAT__c { get; set; }
        public double RescheduleFee__c { get; set; }
        public double RebateOfInterestReversal__c { get; set; }
        public double Balance__c { get; set; }
    }

    public class AgreementNumberAndIdData
    {
        public string approval_agreementNumber__c { get; set; }
        public string Id { get; set; }
    }


    public class UserNumberAndIdData
    {
        public string jbrPersonNumber__c { get; set; }
        public string Id { get; set; }
    }

    //Address

    //public class Address {

    //    public string Street { get; set; }
    //    public string PostalCode { get; set; }

    //}

    //Introducer //Dealer

    public class AccountData {

        public string Id { get; set; }
        public string sentinalCompanyId__c { get; set; }
        public string Name { get; set; }
        public string BillingStreet { get; set; }
        public string BillingPostalCode { get; set; }
        public string Phone { get; set; }
        public string company_contact_altPhone__c { get; set; }
        public string company_contact_EmailAddress__c { get; set; }
        public string company_contact_name__c { get; set; }
        public string introducerType__c { get; set; }
        public string dealerType__c { get; set; }

        public DateTime? lastUpdatedFromSentinel__c { get; set; }

    }

    
    //CustomerCompany

    public class CustomerCompanyData
    {

        public string sentinalCompanyId__c { get; set; }
        public string Name { get; set; }
        public string BillingStreet { get; set; }
        public string BillingCity { get; set; }
        public string BillingState { get; set; }
        public string BillingPostalCode { get; set; }
        public string BillingCountry { get; set; }

        public string Phone { get; set; }
        public string company_contact_altPhone__c { get; set; }
        public string company_contact_EmailAddress__c { get; set; }
        public string company_contact_name__c { get; set; }
        public string company_registration_VATNumber__c { get; set; }

        public DateTime? lastUpdatedFromSentinel__c { get; set; }

    }

    //Customer

    public class CustomerData
    {

        public string panther_id__c { get; set; }

        public string Title { get; set; }
        public string FirstName { get; set; }
        //public string MiddleName { get; set; }
        public string LastName { get; set; }

        public string Name { get; set; }

        public DateTime? Birthdate { get; set; }

        public string MailingStreet { get; set; }
        public string MailingCity { get; set; }
        public string MailingState { get; set; }
        public string MailingPostalCode { get; set; }
        public string MailingCountry { get; set; }

        public string Phone {get;set;}
        public string HomePhone { get; set; }
        public string OtherPhone { get; set; }

        public string MobilePhone { get; set; }
        public string mobileNumber__c { get; set; }

        public string Email { get; set; }

        public bool? DoNotCall { get; set; }
        public bool? HasOptedOutOfEmail { get; set; }
        //public bool? HasOptedOutOfPost { get; set; }
        //public bool? HasOptedOutOfSMS { get; set; }

        public DateTime? lastUpdatedFromSentinel__c { get; set; }
    }

    //Customer

    public class TriggerControlData
    {
        public string Id { get; set; }
        public bool ShouldTriggersRun__c { get; set; }
    }



    public class VehicleData
    {
        public class c_proposal__r
        {
            public class c_primary_agreement__r {
                public string approval_agreementNumber__c { get; set; }
                public DateTime? payoutDate__c { get; set; }
            }

            public c_primary_agreement__r primary_agreement__r;
        }

        public string id { get; set; }
        public DateTime? insuranceDateLastChecked__c { get; set; }
        public string insuranceClaimsContact__c { get; set; }
        public string insuranceInsurer__c { get; set; }
        public string insurancePolicyNumber__c { get; set; }
        public string registrationPlate__c { get; set; }

        public c_proposal__r proposal__r { get; set; }
        public string vehicle__c { get; set; }

        public DateTime? lastUpdatedFromSentinel__c { get; set; }
    }

    public class SageData // SageData__c
    {
        public double? sage_TrialBalanceNL1160__c { get; set; }
        public double? sage_TrialBalanceNL1170__c { get; set; }
    }

    public class PistonheadsTransactionData // PistonheadsTransaction__c
    {
        public DateTime? pistonheads_TransactionDate__c { get; set; }
        public double? pistonheads_TransactionAmountInvoiced__c { get; set; }
    }

    public class EthosVolumeCommissionData //AgreementEthosVolumeTransaction__c
    {
        public class c_agreement__r
        {
            public double? sentinalProposal_Id__c { get; set; }
        }

        public c_agreement__r agreement__r { get; set; }
        public DateTime? ethosVolume_TransactionDate__c { get; set; }
        public double? ethosVolume_TransactionAmountInvoiced__c { get; set; }
    }

    public class FacebookCommissionData //AgreementFacebookTransaction__c
    {
        public class c_agreement__r
        {
            public double? sentinalProposal_Id__c { get; set; }
        }

        public c_agreement__r agreement__r { get; set; }
        public DateTime? facebook_TransactionDate__c { get; set; }
        public double? facebook_TransactionAmountInvoiced__c { get; set; }
    }


    public class McLarenCommissionData //AgreementMcLarenCommissionTransaction__c
    {
        public class c_agreement__r
        {
            public double? sentinalProposal_Id__c { get; set; }
        }

        public c_agreement__r agreement__r { get; set; }
        public DateTime? mcLarenCommission_CommissionTransactionD__c { get; set; }
        public double? mcLarenCommission_CommissionAmountInvoic__c { get; set; }
    }

    public class McLarenVolumeBonusCommissionData //AgreementMcLarenVolumeBonusCommissionTra__c
    {
        public class c_agreement__r
        {
            public double? sentinalProposal_Id__c { get; set; }
        }

        public c_agreement__r agreement__r { get; set; }
        public DateTime? mcLarenVolumeBonus_CommissionTransacDate__c { get; set; }
        public double? mcLarenVolumeBonus_CommissionAmountInv__c { get; set; }
    }

    public class TrackerData // AgreementTrackerTransaction__c
    {
        public class c_agreement__r
        {
            public double? sentinalProposal_Id__c { get; set; }
        }

        public c_agreement__r agreement__r { get; set; }

        public DateTime? TrackerStartDate__c { get; set; }
        public double? tracker_TransactionAmountInvoiced__c { get; set; }
        public double? tracker_TransactionAmountInvoicedVAT__c { get; set; }
        public double? trackerSubscriptions_TranAmountPaid__c { get; set; }
        public DateTime? tracker_TransactionDate__c { get; set; }
    }

    public class TrackerSubscriptionsData //AgreementTrackerSubscriptionsTransaction__c
    {
        public class c_agreement__r
        {
            public double? sentinalProposal_Id__c { get; set; }
        }

        public c_agreement__r agreement__r { get; set; }

        public DateTime? TrackerStartDate__c { get; set; }
        public double? trackerSubscriptions_TranAmountInv__c { get; set; }
        public double? trackerSubscriptions_TransAmountInvVAT__c { get; set; }
        public double? trackerSubscriptions_TranAmountPaid__c { get; set; }
        public DateTime? trackerSubscriptions_TransactionDate__c { get; set; }
        
    }
}
