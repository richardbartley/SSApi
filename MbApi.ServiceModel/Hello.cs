using ServiceStack;

namespace SSApi.ServiceModel
{
    [Route("/hello")]
    [Route("/hello/{Name}")]
    public class Hello : IReturn<HelloResponse>
    {
        public string Name { get; set; }
    }

    public class HelloResponse : BaseResponse
    {
        public string Result { get; set; }
    }
}
