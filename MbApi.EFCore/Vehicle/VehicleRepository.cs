using SSApi.Dal.Vehicle;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SSApi.EFCore.Vehicle
{
    public class VehicleRepository : RepositoryBase, IVehicleRepository
    {
        private DataContext _context;

        public VehicleRepository(DataContext context)
        {
            _context = context;
        }
        
        public async Task<IList<VehicleDto>> FetchAsync()
        {
            using (var ctx = _context)
            {
                var results = new List<VehicleDto>();

                results.Add(new VehicleDto { Id = 1, Description = "Aston Martin" });
                results.Add(new VehicleDto { Id = 2, Description = "Mini" });
                results.Add(new VehicleDto { Id = 3, Description = "Tesla" });

                return results;
            }
                
        }
    }
}
