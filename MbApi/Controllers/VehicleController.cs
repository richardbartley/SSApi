using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using SSApi.Dal.Vehicle;
using MbCoreApp.ServiceModel.Vehicle;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace SSApi.Controllers
{
    [Route("[controller]")]
    [Authorize]
    public class VehicleController : ControllerBase
    {
        private IVehicleRepository _vehicleRepo;

        public VehicleController(IVehicleRepository vehicleRepository)
        {
            _vehicleRepo = vehicleRepository;
        }

        [HttpGet]
        public async Task<IActionResult> GetAsync()
        {
            return new JsonResult(await _vehicleRepo.FetchAsync());
        }
    }
}
