using crds_angular.Models.Crossroads;

namespace crds_angular.Services.Interfaces
{
    public interface IDonorStatementService
    {
        DonorStatementDTO GetDonorStatement(int contactId);
        void SaveDonorStatement(DonorStatementDTO donorStatement);
    }
}
