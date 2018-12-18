using SSApi.Dal.Vehicle;
using SSApi.ServiceModel;
using ServiceStack;
using System.Collections.Generic;

namespace MbCoreApp.ServiceModel.Vehicle
{
    //The response DTO must follow the {Request DTO}Response naming convention and has to be in the same namespace as the Request DTO

    [Route("/vehicles")]
    public class GetVehicles : IReturn<GetVehiclesResponse> { }
    
    public class GetVehiclesResponse : BaseResponse
    {
        public IList<VehicleDto> Results { get; set; }
    }
}