/*---------------------------------------------------------------------------------------------
 *  Copyright (c) Dolittle. All rights reserved.
 *  Licensed under the MIT License. See LICENSE in the project root for license information.
 *--------------------------------------------------------------------------------------------*/
using System;
using Dolittle.Protobuf;
using Newtonsoft.Json;

namespace Dolittle.TimeSeries.MQTTBridge
{
    /// <summary>
    /// Represents a <see cref="JsonConverter"/> for dealing with <see cref="Google.Protobuf.WellKnownTypes.Timestamp"/> during serialization to and from JSON
    /// </summary>
    public class ProtobufTimestampConverter : JsonConverter
    {
        /// <inheritdoc/>
        public override bool CanConvert(Type objectType)
        {
            return objectType == typeof(Google.Protobuf.WellKnownTypes.Timestamp);
        }

        /// <inheritdoc/>
        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            var dateTimeOffset = DateTimeOffset.Parse(reader.Value.ToString());
            return Google.Protobuf.WellKnownTypes.Timestamp.FromDateTimeOffset(dateTimeOffset);
        }

        /// <inheritdoc/>
        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            var valueAsTimestamp = value as Google.Protobuf.WellKnownTypes.Timestamp;
            writer.WriteValue(valueAsTimestamp.ToDateTimeOffset().ToString());
        }
    }    
}