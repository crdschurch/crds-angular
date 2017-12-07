using System.Collections.Generic;
using System.Linq;
using AutoMapper;
using Crossroads.Web.Common.Configuration;
using Crossroads.Web.Common.MinistryPlatform;
using Crossroads.Web.Common.Security;
using MinistryPlatform.Translation.Models;
using MinistryPlatform.Translation.Repositories.Interfaces;

namespace MinistryPlatform.Translation.Repositories
{
    public class ProgramRepository : BaseRepository, IProgramRepository
    {
        private readonly IMinistryPlatformService _ministryPlatformService;
        private readonly IMinistryPlatformRestRepository _ministryPlatformRest;
        private readonly int _onlineGivingProgramsPageViewId;
        private readonly int _programsPageId;

        public ProgramRepository(IMinistryPlatformService ministryPlatformService,
                                 IAuthenticationRepository authenticationService,
                                 IConfigurationWrapper configurationWrapper,
                                 IMinistryPlatformRestRepository ministryPlatformRest)
            : base(authenticationService, configurationWrapper)
        {
            _ministryPlatformService = ministryPlatformService;
            _ministryPlatformRest = ministryPlatformRest;
            _onlineGivingProgramsPageViewId = configurationWrapper.GetConfigIntValue("OnlineGivingProgramsPageViewId");
            _programsPageId = configurationWrapper.GetConfigIntValue("Programs");
        }

        public List<MpProgram> GetAllPrograms()
        {
            var token = ApiLogin();
            var records = _ministryPlatformService.GetPageViewRecords("AllProgramsList", token);
            var programs = new List<MpProgram>();
            if (records == null || records.Count == 0)
            {
                return programs;
            }
            programs.AddRange(records.Select(Mapper.Map<MpProgram>));

            return programs;
        }

        public List<MpProgram> GetOnlineGivingPrograms(int? programType)
        {
            var searchString = programType == null ? null : string.Format(",,,{0}", programType);
            var programs =
                WithApiLogin(
                    apiToken => (_ministryPlatformService.GetPageViewRecords(_onlineGivingProgramsPageViewId, apiToken, searchString)));

            var programList = new List<MpProgram>();
            if (programs == null || programs.Count == 0)
            {
                return programList;
            }
            programList.AddRange(programs.Select(Mapper.Map<MpProgram>));

            return programList;
        }

        public List<MpProgram> GetProgramsForEventTool()
        {
            const string columns = "Program_ID,Program_Name,Communication_ID,Program_Type_ID,Allow_Recurring_Giving";
            const string filter = "Show_On_Event_Tool=1 AND (End_Date IS NULL OR End_Date > GETDATE())";
            const string orderBy = "Program_Name";

            var token = ApiLogin();
            var records = _ministryPlatformRest.UsingAuthenticationToken(token)
                .SearchTable<Dictionary<string, object>>("Programs", filter, columns, orderBy);

            return records.Select(Mapper.Map<MpProgram>).ToList();
        }

        public MpProgram GetProgramById(int programId)
        {
            return (WithApiLogin(token => (Mapper.Map<MpProgram>(_ministryPlatformService.GetRecordDict(_programsPageId, programId, token)))));
        }
    }
}