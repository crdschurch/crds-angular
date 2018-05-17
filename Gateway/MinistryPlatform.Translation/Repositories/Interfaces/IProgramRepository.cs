using System.Collections.Generic;
using MinistryPlatform.Translation.Models;

namespace MinistryPlatform.Translation.Repositories.Interfaces
{
    public interface IProgramRepository
    {
        List<MpProgram> GetOnlineGivingPrograms(int? programType);
        List<MpProgram> GetProgramsForEventTool();
        MpProgram GetProgramById(int programId);
        List<MpProgram> GetAllPrograms();
        MpProgram GetProgramByProductId(int productId);
    }
}