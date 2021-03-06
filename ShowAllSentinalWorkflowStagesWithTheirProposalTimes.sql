--SELECT [StatusId], [Description]
--FROM [JBR_Proposals].[dbo].[ProposalStatusCode]

DECLARE @holidays [dbo].[holidaysTableType];
INSERT INTO @holidays select * from [dbo].[GetPublicHolidaysAndHolidayExceptions] ();

DECLARE @numberOfSecondsInADay int = 28800;

WITH PivotData AS
(
--select ProposalId, [2] from 
--( 
	--Select ProposalId,StatusId, Sum(WorkingTime) TotalWorkingTime, StageDescription from (
	select ProposalId, StatusId,
	       dbo.WorkTime(StartDateTime,EndDateTime,'1900-01-01 09:00:00','1900-01-01 17:00:00',0) -
		   ((Select Count(*) from (select date from @holidays h where h.Date >= StartDateTime and h.Date <= EndDateTime group by h.date) uh) * @numberOfSecondsInADay) WorkingTime,
		   StageDescription 
		  
	from 

	( SELECT ph.ProposalId, StatusId,
	 (SELECT Description FROM [JBR_Proposals].[dbo].[ProposalStatusCode] psc where psc.StatusId = ph.StatusId) StageDescription, 
	         DateStamp StartDateTime, 
			 ISNULL((select top 1 DateStamp from [JBR_Proposals].[dbo].[ProposalHistory] iph 
			 where iph.ProposalId = ph.ProposalId and iph.ProposalHistoryId > ph.ProposalHistoryId 
			 and iph.statusId in (SELECT StatusId FROM [JBR_Proposals].[dbo].[ProposalStatusCode])
			 order by ProposalHistoryId),DateAdd(s,100,DateStamp)) EndDateTime

			  FROM [JBR_Proposals].[dbo].[ProposalHistory] ph inner join [JBR_Proposals].[dbo].[Proposal] pr on ph.proposalId = pr.proposalId
	          where ph.statusId in (SELECT StatusId FROM [JBR_Proposals].[dbo].[ProposalStatusCode])) stagesInner
	--and ph.ProposalId = 27
) --src
SELECT ProposalId, (Select [ProposalCreateDate] from [JBR_Proposals].[dbo].[Proposal] ip where ip.ProposalId = p.ProposalId) CreatedDateTime
, SUM(ISNULL([Active Proposals],0)) [Active Proposals]
, SUM(ISNULL([New Proposals],0)) [New Proposals]
, SUM(ISNULL([Referred for Information],0)) [Referred for Information]
, SUM(ISNULL([DealTrak Leads],0)) [DealTrak Leads]
, SUM(ISNULL([Auto Decisioning],0)) [Auto Decisioning]
, SUM(ISNULL([Auto Accept],0)) [Auto Accept]
, SUM(ISNULL([Refer for Manual Underwriting],0)) [Refer for Manual Underwriting]
, SUM(ISNULL([Rules Error],0)) [Rules Error]
, SUM(ISNULL([Underwriting],0)) [Underwriting]
, SUM(ISNULL([Credit Manager],0)) [Credit Manager]
, SUM(ISNULL([Credit Committee],0)) [Credit Committee]
, SUM(ISNULL([Funder],0)) [Funder]
, SUM(ISNULL([Decisioned],0)) [Decisioned]
, SUM(ISNULL([Accepted],0)) [Accepted]
, SUM(ISNULL([Accept on Revised Terms],0)) [Accept on Revised Terms]
, SUM(ISNULL([Referred],0)) [Referred]
, SUM(ISNULL([Declined],0)) [Declined]
, SUM(ISNULL([New Vehicle Orders],0)) [New Vehicle Orders]
, SUM(ISNULL([Document Production],0)) [Document Production]
, SUM(ISNULL([Tracker Instruction],0)) [Tracker Instruction]
, SUM(ISNULL([Awaiting Signed Documents],0)) [Awaiting Signed Documents]
, SUM(ISNULL([7 Days],0)) [7 Days]
, SUM(ISNULL([14 Days],0)) [14 Days]
, SUM(ISNULL([30 Days],0)) [30 Days]
, SUM(ISNULL([60 Days],0)) [60 Days]
, SUM(ISNULL([90 Days],0)) [90 Days]
, SUM(ISNULL([Contract Checks],0)) [Contract Checks]
, SUM(ISNULL([Pre Payout Checks],0)) [Pre Payout Checks]
, SUM(ISNULL([Awaiting Payout],0)) [Awaiting Payout]
, SUM(ISNULL([Payout Complete],0)) [Payout Complete]
, SUM(ISNULL([Declined Proposals],0)) [Declined Proposals]
, SUM(ISNULL([Customer Not Taken Up],0)) [Customer Not Taken Up]
, SUM(ISNULL([Lost to Competitor],0)) [Lost to Competitor]
, SUM(ISNULL([Gone Away Pre Credit],0)) [Gone Away Pre Credit]
, SUM(ISNULL([Credit Manager Decline],0)) [Credit Manager Decline]
, SUM(ISNULL([Credit Committee Decline],0)) [Credit Committee Decline]
, SUM(ISNULL([Funder Decline],0)) [Funder Decline]
, SUM(ISNULL([Miscellaneous Decline],0)) [Miscellaneous Decline]
, SUM(ISNULL([Live Contracts],0)) [Live Contracts]
, SUM(ISNULL([Quoted Proposals],0)) [Quoted Proposals]
FROM PivotData
PIVOT(SUM(WorkingTime) FOR StageDescription IN([Active Proposals]
,[New Proposals]
,[Referred for Information]
,[DealTrak Leads]
,[Auto Decisioning]
,[Auto Accept]
,[Refer for Manual Underwriting]
,[Rules Error]
,[Underwriting]
,[Credit Manager]
,[Credit Committee]
,[Funder]
,[Decisioned]
,[Accepted]
,[Accept on Revised Terms]
,[Referred]
,[Declined]
,[New Vehicle Orders]
,[Document Production]
,[Tracker Instruction]
,[Awaiting Signed Documents]
,[7 Days]
,[14 Days]
,[30 Days]
,[60 Days]
,[90 Days]
,[Contract Checks]
,[Pre Payout Checks]
,[Awaiting Payout]
,[Payout Complete]
,[Declined Proposals]
,[Customer Not Taken Up]
,[Lost to Competitor]
,[Gone Away Pre Credit]
,[Credit Manager Decline]
,[Credit Committee Decline]
,[Funder Decline]
,[Miscellaneous Decline]
,[Live Contracts]
,[Quoted Proposals])) AS P
group by ProposalId;