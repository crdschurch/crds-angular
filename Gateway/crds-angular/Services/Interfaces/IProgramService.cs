using System.Collections.Generic;
using crds_angular.Models.Crossroads;

namespace crds_angular.Services.Interfaces
{
    public interface IProgramService
    {
        List<ProgramDTO> GetOnlineGivingPrograms(int? programType = null);
        List<ProgramDTO> GetProgramsForEventTool();
        ProgramDTO GetProgramById(int programId);
        List<ProgramDTO> GetAllProgramsForReal();
    }
}