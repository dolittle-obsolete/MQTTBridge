/*---------------------------------------------------------------------------------------------
 *  Copyright (c) Dolittle. All rights reserved.
 *  Licensed under the MIT License. See LICENSE in the project root for license information.
 *--------------------------------------------------------------------------------------------*/
using System;
using System.Protobuf;
using Dolittle.Protobuf;
using Newtonsoft.Json;

namespace Dolittle.TimeSeries.MQTTBridge
{
    /// <summary>
    /// Represents a <see cref="JsonConverter"/> for dealing with <see cref="guid"/> during serialization to and from JSON
    /// </summary>
    public class ProtobufGuidConverter : JsonConverter
    {
        /// <inheritdoc/>
        public override bool CanConvert(Type objectType)
        {
            return objectType == typeof(guid);
        }

        /// <inheritdoc/>
        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            var guid = Guid.Parse(reader.Value.ToString());
            return guid.ToProtobuf();
        }

        /// <inheritdoc/>
        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            var valueAsGuid = value as guid;
            writer.WriteValue(valueAsGuid.ToGuid().ToString());
        }
    }
}