using System.Collections.Generic;
using MinistryPlatform.Translation.Models;

namespace MinistryPlatform.Translation.Repositories.Interfaces
{
    public interface IProgramRepository
    {
        MpProgram GetProgramById(int programId);
    }
}