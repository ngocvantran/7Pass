using System;
using Newtonsoft.Json;

namespace KeePass.Sources.DropBox.Api.Serialize
{
    internal class JsonSerializer : SerializerBase
    {
        public override string ContentType
        {
            get { return "application/json"; }
        }

        public JsonSerializer() {}

        public JsonSerializer(JsonSerializerSettings settings)
            : base(settings) {}

        public override T Deserialize<T>(string content)
        {
            return DeserializeJson<T>(content);
        }

        public override object Deserialize(string content, Type type)
        {
            return DeserializeJson(content, type);
        }

        public override string Serialize(object instance, Type type)
        {
            return SerializeJson(instance, type);
        }
    }
}