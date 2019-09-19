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
        /// <param name="mQTT"><see cref="MQTTSettings"/></param>
        public Configuration(MQTTSettings mQTT)
        {
            MQTT = mQTT;
        }


        /// <summary>
        /// Gets the <see cref="MQTTSettings"/>
        /// </summary>
        public MQTTSettings MQTT {  get; }

    }
}