using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace SSApi.Dal.Vehicle
{
    public interface IVehicleRepository
    {
        Task<IList<VehicleDto>> FetchAsync();
    }
}
