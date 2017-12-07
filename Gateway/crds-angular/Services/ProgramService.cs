using System.Collections.Generic;
using System.Linq;
using AutoMapper;
using crds_angular.Models.Crossroads;
using crds_angular.Services.Interfaces;
using MPServices = MinistryPlatform.Translation.Repositories.Interfaces;

namespace crds_angular.Services
{
    public class ProgramService : IProgramService
    {
        private readonly MPServices.IProgramRepository _programService;

        public ProgramService(MPServices.IProgramRepository programService)
        {
            _programService = programService;
        }

        public List<ProgramDTO> GetAllProgramsForReal()
        {
            var programs = _programService.GetAllPrograms();
            return programs == null ? (null) : (Enumerable.OrderBy(programs.Select(Mapper.Map<ProgramDTO>), x => x.Name).ToList());
        }

        public List<ProgramDTO> GetOnlineGivingPrograms(int? programType = null)
        {
            var programs = _programService.GetOnlineGivingPrograms(programType);
            return programs == null ? (null) : (Enumerable.ToList(programs.Select(Mapper.Map<ProgramDTO>)));
        }

        public List<ProgramDTO> GetProgramsForEventTool()
        {
            var programs = _programService.GetProgramsForEventTool();
            return programs == null ? (null) : (Enumerable.ToList(programs.Select(Mapper.Map<ProgramDTO>)));
        }

        public ProgramDTO GetProgramById(int programId)
        {
            var program = _programService.GetProgramById(programId);
            return program == null ? (null) : Mapper.Map<ProgramDTO>(program);
        }
    }
}