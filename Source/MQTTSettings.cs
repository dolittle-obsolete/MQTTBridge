/*---------------------------------------------------------------------------------------------
 *  Copyright (c) Dolittle. All rights reserved.
 *  Licensed under the MIT License. See LICENSE in the project root for license information.
 *--------------------------------------------------------------------------------------------*/
using System;
using Dolittle.Configuration;

namespace Dolittle.TimeSeries.MQTTBridge
{
    /// <summary>
    /// Represents the settings for MQTT
    /// </summary>
    public class MQTTSettings : IConfigurationObject
    {
        /// <summary>
        /// Initializes a new instance of <see cref="MQTTSettings"/>
        /// </summary>
        /// <param name="host">Host MQTT broker to connect to</param>
        /// <param name="port">Port of the MQTT broker to connect to</param>
        /// <param name="useTls">Whether or not to use Tls</param>
        /// <param name="clientId">Unique client identifier - can be empty, which would lead to a random id</param>
        public MQTTSettings(
            string host,
            int port,
            bool useTls,
            string clientId)
        {
            Host = host;
            Port = port;
            UseTls = useTls;
            ClientId = !string.IsNullOrEmpty(clientId)?clientId:Guid.NewGuid().ToString();
        }

        /// <summary>
        /// Gets the host name to connect to MQTT broker
        /// </summary>
        public string Host {  get; }

        /// <summary>
        /// Gets the port to use when connecting to MQTT broker
        /// </summary>
        public int Port {  get; }

        /// <summary>
        /// Gets whether or not to use Tls terminated transport
        /// </summary>
        public bool UseTls {  get; }

        /// <summary>
        /// Gets the unique identifier for the client - if left blank, a random Guid is used
        /// </summary>
        public string ClientId {  get; }
    }
}