using SSApi.EFCore.DataModels;
using Microsoft.EntityFrameworkCore;

namespace SSApi.EFCore
{
    public interface IDataContext
    {
        DbSet<VehicleDataModel> Vehicles { get; set; }
    }
}