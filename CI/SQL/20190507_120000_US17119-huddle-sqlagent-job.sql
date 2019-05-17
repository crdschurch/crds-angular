USE msdb ;  
GO  

DECLARE @JOBNAME nvarchar(100) = 'Huddle Stored Proc';   
DECLARE @STEPNAME nvarchar(100) = 'execute the stored proc';
DECLARE @COMMAND  nvarchar(100) = ' exec [dbo].[crds_Huddle_Participant_Status_Refresh] ';
DECLARE @SCHEDULENAME nvarchar(100) = 'Daily_2AM';
    
IF(NOT EXISTS(SELECT job_id, [name] FROM msdb.dbo.sysjobs where name=@JOBNAME))
BEGIN
	EXEC dbo.sp_add_job  
		@job_name = @JOBNAME ;   

	EXEC sp_add_jobstep  
		@job_name = @JOBNAME,  
		@step_name =@STEPNAME,  
		@subsystem = N'TSQL',  
		@command = @COMMAND,   
		@retry_attempts = 2,  
		@retry_interval = 5 ;  

	EXEC msdb.dbo.sp_add_jobschedule 
		@job_name = @JOBNAME, -- Job name
		@name = @SCHEDULENAME,  -- Schedule name
		@freq_type = 4, -- Daily
		@freq_interval = 1, 
		@active_start_time = 020000 -- 2:00 AM
END

GO

