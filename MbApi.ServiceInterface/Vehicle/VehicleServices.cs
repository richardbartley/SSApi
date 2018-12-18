using SSApi.Dal.Vehicle;
using MbCoreApp.ServiceModel.Vehicle;
using ServiceStack;
using System.Threading.Tasks;

namespace SSApi.ServiceInterface.Vehicle
{
    public class VehicleServices : Service
    {
        private IVehicleRepository _vehicleRepo;

        public VehicleServices(IVehicleRepository vehicleRepository)
        {
            _vehicleRepo = vehicleRepository;
        }

        [Authenticate]
        public async Task<GetVehiclesResponse> Any(GetVehicles request)
        {
            return new GetVehiclesResponse
            {
                Results = await _vehicleRepo.FetchAsync()
            };
        }
    }
}
