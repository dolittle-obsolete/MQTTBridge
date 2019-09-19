/*---------------------------------------------------------------------------------------------
 *  Copyright (c) Dolittle. All rights reserved.
 *  Licensed under the MIT License. See LICENSE in the project root for license information.
 *--------------------------------------------------------------------------------------------*/
using Dolittle.Configuration;

namespace Dolittle.TimeSeries.MQTTBridge
{
    /// <summary>
    /// Represents the configuration for the MQTT Bridge
    /// </summary>
    [Name("configuration")]
    public class Configuration : IConfigurationObject
    {
        /// <summary>
        /// Initializes a new instance of <see cref="Configuration"/>
        /// </summary>
        /// <param name="connection"><see cref="ConnectionConfiguration"/></param>
        public Configuration(ConnectionConfiguration connection)
        {
            Connection = connection;
        }


        /// <summary>
        /// Gets the <see cref="ConnectionConfiguration"/>
        /// </summary>
        public ConnectionConfiguration Connection {  get; }

    }
}