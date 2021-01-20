using crds_angular.Models.Crossroads;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Linq;

namespace crds_angular.Models.Json
{
    public class AccountInfoSerializer : JsonConverter
    {
        public override bool CanConvert(Type objectType)
        {
            throw new NotImplementedException();
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            JObject jsonObject = JObject.Load(reader);
            var properties = jsonObject.Properties().ToList();
            return new AccountInfo
            {
                EmailNotifications = Convert.ToBoolean(jsonObject["EmailNotifications"]),
                TextNotifications = Convert.ToBoolean(jsonObject["TextNotifications"]),
                PaperlessStatements = Convert.ToBoolean(jsonObject["PaperlessStatements"])    
            };
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            var accountInfo = value as AccountInfo;
            writer.WriteStartObject();
            writer.WritePropertyName("EmailNotifications");
            writer.WriteValue(accountInfo.EmailNotifications.ToString());
            writer.WritePropertyName("TextNotifications");
            writer.WriteValue(accountInfo.TextNotifications.ToString());
            writer.WritePropertyName("PaperlessStatements");
            writer.WriteValue(accountInfo.PaperlessStatements.ToString());
            writer.WriteEndObject();
        }
    }
}