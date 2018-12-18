using ServiceStack;
using System;
using System.Collections.Generic;
using System.Text;

namespace SSApi.ServiceModel
{
    public abstract class BaseResponse
    {
        public ResponseStatus ResponseStatus { get; set; } // inject servicestack structured errors automatically
    }
}
