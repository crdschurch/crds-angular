using Newtonsoft.Json;
using RestSharp.Serializers;

namespace Crossroads.Utilities.Serializers
{
    public class RestsharpJsonNetSerializer : ISerializer
    {
        public RestsharpJsonNetSerializer()
        {
            ContentType = "application/json";
        }

        public string Serialize(object obj)
        {
            return JsonConvert.SerializeObject(obj, Formatting.None);
        }
        
        public string ContentType { get; set; }
    }
}