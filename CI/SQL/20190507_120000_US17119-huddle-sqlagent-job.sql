USE msdb ;  
GO  

DECLARE @JOBNAME nvarchar(30) = 'Huddle Stored Proc';   
    
EXEC dbo.sp_add_job  
    @job_name = @JOBNAME ;   


EXEC sp_add_jobstep  
    @job_name = @JOBNAME,  
    @step_name = N'execute the stored proc',  
    @subsystem = N'TSQL',  
    @command = N' exec [dbo].[crds_Huddle_Participant_Status_Refresh] ',   
    @retry_attempts = 2,  
    @retry_interval = 5 ;  


EXEC msdb.dbo.sp_add_jobschedule 
	@job_name = @JOBNAME, -- Job name
	@name = N'Daily_2AM',  -- Schedule name
	@freq_type = 4, -- Daily
	@freq_interval = 4, 
	@active_start_time = 020000 -- 2:00 AM
GO

