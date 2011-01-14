using System;
using System.IO;
using Hammock.Serialization;
using Newtonsoft.Json;

namespace KeePass.Sources.DropBox.Api.Serialize
{
    internal abstract class SerializerBase : Utf8Serializer, ISerializer, IDeserializer
    {
        private readonly Newtonsoft.Json.JsonSerializer _serializer;
        public abstract string ContentType { get; }

        protected SerializerBase()
            : this(new JsonSerializerSettings
            {
                MissingMemberHandling = MissingMemberHandling.Ignore,
                NullValueHandling = NullValueHandling.Ignore,
                DefaultValueHandling = DefaultValueHandling.Include,
                ContractResolver = new JsonConventionResolver()
            }) {}

        protected SerializerBase(JsonSerializerSettings settings)
        {
            _serializer = new Newtonsoft.Json.JsonSerializer
            {
                ConstructorHandling = settings.ConstructorHandling,
                ContractResolver = settings.ContractResolver,
                ObjectCreationHandling = settings.ObjectCreationHandling,
                MissingMemberHandling = settings.MissingMemberHandling,
                DefaultValueHandling = settings.DefaultValueHandling,
                NullValueHandling = settings.NullValueHandling
            };

            foreach (var converter in settings.Converters)
            {
                _serializer.Converters.Add(converter);
            }
        }

        public abstract T Deserialize<T>(string content);

        public abstract object Deserialize(string content, Type type);

        public virtual object DeserializeJson(string content, Type type)
        {
            using (var stringReader = new StringReader(content))
            {
                using (var jsonTextReader = new JsonTextReader(stringReader))
                {
                    return _serializer.Deserialize(jsonTextReader, type);
                }
            }
        }

        public virtual T DeserializeJson<T>(string content)
        {
            using (var stringReader = new StringReader(content))
            {
                using (var jsonTextReader = new JsonTextReader(stringReader))
                {
                    return _serializer.Deserialize<T>(jsonTextReader);
                }
            }
        }

        public abstract string Serialize(object instance, Type type);

        public virtual string SerializeJson(object instance, Type type)
        {
            using (var stringWriter = new StringWriter())
            {
                using (var jsonTextWriter = new JsonTextWriter(stringWriter))
                {
                    jsonTextWriter.Formatting = Formatting.Indented;
                    jsonTextWriter.QuoteChar = '"';

                    _serializer.Serialize(jsonTextWriter, instance);

                    var result = stringWriter.ToString();
                    return result;
                }
            }
        }
    }
}