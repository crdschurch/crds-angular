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
        private readonly MPServices.IProgramRepository _programRepository;

        public ProgramService(MPServices.IProgramRepository programRepository)
        {
            _programRepository = programRepository;
        }

        public List<ProgramDTO> GetAllProgramsForReal()
        {
            var programs = _programRepository.GetAllPrograms();
            return programs == null ? (null) : (Enumerable.OrderBy(programs.Select(Mapper.Map<ProgramDTO>), x => x.Name).ToList());
        }

        public List<ProgramDTO> GetOnlineGivingPrograms(int? programType = null)
        {
            var programs = _programRepository.GetOnlineGivingPrograms(programType);
            return programs == null ? (null) : (Enumerable.ToList(programs.Select(Mapper.Map<ProgramDTO>)));
        }

        public List<ProgramDTO> GetProgramsForEventTool()
        {
            var programs = _programRepository.GetProgramsForEventTool();
            return programs == null ? (null) : (Enumerable.ToList(programs.Select(Mapper.Map<ProgramDTO>)));
        }

        public ProgramDTO GetProgramById(int programId)
        {
            var program = _programRepository.GetProgramById(programId);
            return program == null ? (null) : Mapper.Map<ProgramDTO>(program);
        }
    }
}