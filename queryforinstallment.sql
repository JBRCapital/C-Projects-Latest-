/****** Script for SelectTopNRows command from SSMS  ******/
SELECT [tt].[TransTypeDescription],*
  FROM [JBR_S3DB01].[dbo].[TransactionTable] t
  Inner join [JBR_S3DB01].[dbo].[TransactionTypeTable] tt on t.TransTypeID = tt.TransTypeID
  where [TransAgreementNumber] = 'A00274'
   and t.TransReversalID = 0 and t.TransTypeID in (12,13,7)
  Order By TransDate


  SELECT [tt].[TransTypeDescription],*
  FROM [JBR_S3DB01].[dbo].[TransactionTable] t
  Inner join [JBR_S3DB01].[dbo].[TransactionTypeTable] tt on t.TransTypeID = tt.TransTypeID
  where t.TransTypeID = 6
  Order By TransDate

SELECT *, SUM(Amount) over(order by TransDate rows unbounded preceding) Balance FROM 
	(SELECT TransCounter, TransAgreementNumber, TransDate, TransNetPayment Amount, 
	IIF ( t.TransTypeID = 99,  TransNetPayment, 0 ) InstalmentDue, 
	IIF ( t.TransTypeID = 6,   TransNetPayment, 0 ) StandingOrder,
	IIF ( t.TransTypeID = 7,   TransNetPayment, 0 ) BankTransfer,
	IIF ( t.TransTypeID = 12,  TransNetPayment, 0 ) DirectDebit,
	IIF ( t.TransTypeID = 13,  TransNetPayment, 0 ) CardPayment,
	IIF ( t.TransTypeID = 18,  TransNetPayment, 0 ) Cheque,
	IIF ( t.TransTypeID = 20,  TransNetPayment, 0 ) Adjustment,
	IIF ( t.TransTypeID = 25,  TransNetPayment, 0 ) SettlementPayment,
	IIF ( t.TransTypeID = 29,  TransNetPayment, 0 ) ArrearsAdjustment,
	IIF ( t.TransTypeID = 40,  TransNetPayment, 0 ) WriteOff,
	IIF ( t.TransTypeID = 4,   TransNetPayment, 0 ) TransactionReversal,
	IIF ( t.TransTypeID = 19,  TransNetPayment, 0 ) ReturnedCheque,
	IIF ( t.TransTypeID = 50,  TransNetPayment, 0 ) Refund,
	IIF ( t.TransTypeID = 51,  TransNetPayment, 0 ) SettlementPaymentReversal,
	IIF ( t.TransTypeID = 150, TransNetPayment, 0 ) ReturnedDD_RTP,
	IIF ( t.TransTypeID = 151, TransNetPayment, 0 ) ReturnedDD_IC,
	IIF ( t.TransTypeID = 152, TransNetPayment, 0 ) ReturnedDD_NI,
	IIF ( t.TransTypeID = 153, TransNetPayment, 0 ) ReturnedDD_NA,
	IIF ( t.TransTypeID = 154, TransNetPayment, 0 ) ReturnedDD_ANYD,
	IIF ( t.TransTypeID = 155, TransNetPayment, 0 ) ReturnedDD_PD,
	IIF ( t.TransTypeID = 156, TransNetPayment, 0 ) ReturnedDD_AC,
	IIF ( t.TransTypeID = 1,   TransNetPayment, 0 ) RebateOfInterest,
	IIF ( t.TransTypeID = 21,  TransNetPayment, 0 ) PenaltyInterest,
	IIF ( t.TransTypeID = 30,  TransNetPayment, 0 ) EarlySettlementFee,
	IIF ( t.TransTypeID = 31,  TransNetPayment, 0 ) LatePaymentFee,
	IIF ( t.TransTypeID = 32,  TransNetPayment, 0 ) ExcessMileageCharge,
	IIF ( t.TransTypeID = 33,  TransNetPayment, 0 ) FairWearAndTearCharge,
	IIF ( t.TransTypeID = 34,  TransNetPayment, 0 ) OtherFeesVATable,
	IIF ( t.TransTypeID = 35,  TransNetPayment, 0 ) OtherFeesNoVAT,
	IIF ( t.TransTypeID = 36,  TransNetPayment, 0 ) RescheduleFee,
	IIF ( t.TransTypeID = 52,  TransNetPayment, 0 ) RebateOfInterestReversal
	FROM [JBR_S3DB01].[dbo].[TransactionTable] t
	WHERE TransAgreementNumber = 'A00274'
	) InnerQuery


SELECT [tt].[TransTypeDescription], COUNT(*)
FROM [JBR_S3DB01].[dbo].[TransactionTable] t
INNER JOIN [JBR_S3DB01].[dbo].[TransactionTypeTable] tt on t.TransTypeID = tt.TransTypeID
GROUP BY [tt].[TransTypeDescription]
Order By COUNT(*)

select * from [JBR_S3DB01].[dbo].[TransactionTable]
select * from [JBR_S3DB01].[dbo].[TransactionTypeTable] order by TransTypeType, TransTypeID 
select * from [JBR_S3DB01].[dbo].[TransactionTypeCodesTable]
select * from [JBR_S3DB01].[dbo].[TransAgTypeMethodTable]
select * from [JBR_S3DB01].[dbo].[TransactionTotalsTable]

SELECT CONVERT(date,PayProDate) PayDate,
	SUM(PayProNetAmount) InstalmentValue,
    SUM(PayProPrincipal) Principle,
	SUM(IIF ( PayProType = 'Pay' , PayProInterest, 0 )) Interest,
	SUM(IIF ( PayProType = 'Fee' , PayProNetAmount, 0 )) Fee,
    SUM(IIF ( PayProType = 'Vfe' , PayProNetAmount, 0 )) VATonFee,
	CAST(MAX(CAST(PayProFallenDue as INT)) AS BIT) PayFallenDue
FROM [JBR_S3DB01].[dbo].[PaymentProfileTable]
WHERE PayProAgreementNumber = @AgreementNumber
Group by PayProDate

  SELECT [tt].[TransTypeDescription],*
  FROM [JBR_S3DB01].[dbo].[TransactionTable] t
  Inner join [JBR_S3DB01].[dbo].[TransactionTypeTable] tt on t.TransTypeID = tt.TransTypeID
  where [TransAgreementNumber] = 'A00274'
  and  t.TransTypeID not in (12,7,13, 99) 
  Order By TransDate

  SELECT sum(TransNetPayment) Payment,sum(TransNetPrincipal) Principal,  sum(TransNetInterest) Interest
  FROM [JBR_S3DB01].[dbo].[TransactionTable] t
  where [TransAgreementNumber] = 'A00383'
 -- and  t.TransTypeID not in (12,7,13, 99) 
  --Order By TransDate

    SELECT [TransCounter]
      ,[TransAgreementNumber]
      ,[TransDate]
      ,[t].[TransTypeID]
	  ,[tt].[TransTypeDescription]
      ,[TransNetPayment]
      ,[TransInsPayment]
      ,[TransVatPayment]
      ,[TransNetPrincipal]
      ,[TransNetInterest]
      ,[TransInsPremium]
      ,[TransInsCharges]
      ,[TransReference]
      ,[TransPayExtra]
      ,[TransBatchNumber]
      ,[TransCreator]
      ,[TransAmendor]
      ,[TransCreateDate]
      ,[TransAmendDate]
      ,[TransCreateDateOnly]
      ,[TransInsTax]
      ,[TransArrearsNetPrincipal]
      ,[TransArrearsNetInterest]
      ,[TransArrearsInsPremium]
      ,[TransArrearsInsCharges]
      ,[TransArrearsInsTax]
      ,[TransArrearsVat]
      ,[TransArrearsNetFee]
      ,[TransArrearsVatFee]
      ,[TransInsVat]
      ,[TransFeesNet]
      ,[TransFeesVat]
      ,[TransPeriodEndDate]
      ,[TransTAX]
      ,[TransArrearsTAX]
      ,[TransCosts]
      ,[TransExtraIntr]
      ,[TransCurrencyCode]
      ,[TransCode]
      ,[TransReportingFlag]
      ,[TransBACSPaymentMade]
      ,[TransReversalID]
      ,[TransBACSRetransmit]
      ,[TransCollectCurrency]
      ,[TransCollectCurrencyAmount]
      ,[TransCollectFXRate]
      ,[TransExported]
      ,[TransPassTimeChecked]
      ,[TransNotPostedToAccLink]
  FROM [JBR_S3DB01].[dbo].[TransactionTable] t
  Inner join [JBR_S3DB01].[dbo].[TransactionTypeTable] tt on t.TransTypeID = tt.TransTypeID
  where [TransAgreementNumber] = 'A00383'
  Order By TransCounter


  --TOP 1000 [TransCounter]
  --    ,[TransAgreementNumber]
  --    ,[TransDate]
  --    ,[t].[TransTypeID]
	 -- ,[tt].[TransTypeDescription]
  --    ,[TransNetPayment]
  --    ,[TransInsPayment]
  --    ,[TransVatPayment]
  --    ,[TransNetPrincipal]
  --    ,[TransNetInterest]
  --    ,[TransInsPremium]
  --    ,[TransInsCharges]
  --    ,[TransReference]
  --    ,[TransPayExtra]
  --    ,[TransBatchNumber]
  --    ,[TransCreator]
  --    ,[TransAmendor]
  --    ,[TransCreateDate]
  --    ,[TransAmendDate]
  --    ,[TransCreateDateOnly]
  --    ,[TransInsTax]
  --    ,[TransArrearsNetPrincipal]
  --    ,[TransArrearsNetInterest]
  --    ,[TransArrearsInsPremium]
  --    ,[TransArrearsInsCharges]
  --    ,[TransArrearsInsTax]
  --    ,[TransArrearsVat]
  --    ,[TransArrearsNetFee]
  --    ,[TransArrearsVatFee]
  --    ,[TransInsVat]
  --    ,[TransFeesNet]
  --    ,[TransFeesVat]
  --    ,[TransPeriodEndDate]
  --    ,[TransTAX]
  --    ,[TransArrearsTAX]
  --    ,[TransCosts]
  --    ,[TransExtraIntr]
  --    ,[TransCurrencyCode]
  --    ,[TransCode]
  --    ,[TransReportingFlag]
  --    ,[TransBACSPaymentMade]
  --    ,[TransReversalID]
  --    ,[TransBACSRetransmit]
  --    ,[TransCollectCurrency]
  --    ,[TransCollectCurrencyAmount]
  --    ,[TransCollectFXRate]
  --    ,[TransExported]
  --    ,[TransPassTimeChecked]
  --    ,[TransNotPostedToAccLink]