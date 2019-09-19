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
        /// <param name="runtimeEndpoint">The endpoint for the runtime to connect to</param>
        /// <param name="inputTopicPrefix">The input topic prefix to be used</param>
        /// <param name="outputTopicPrefix">The output topic prefix to be used</param>
        /// <param name="outputAdditionalJSON">Whether or not to output additional JSON to specific topic - <see cref="OutputAdditionalJSON"/></param>

        public Configuration(
            ConnectionConfiguration connection,
            string runtimeEndpoint,
            string inputTopicPrefix,
            string outputTopicPrefix,
            bool outputAdditionalJSON)
        {
            Connection = connection;
            RuntimeEndpoint = runtimeEndpoint;
            InputTopicPrefix = inputTopicPrefix;
            OutputTopicPrefix = outputTopicPrefix;
            OutputAdditionalJSON = outputAdditionalJSON;
        }


        /// <summary>
        /// Gets the <see cref="ConnectionConfiguration"/>
        /// </summary>
        public ConnectionConfiguration Connection {  get; }

        /// <summary>
        /// Gets the endpoint in the form of host:port for the runtime to connect to for input and output streams
        /// </summary>
        public string RuntimeEndpoint { get; }

        /// <summary>
        /// Gets the prefix used for filtering - the last segment of the topic is assumed
        /// to be a TimeSeriesId
        /// </summary>
        public TopicPrefix InputTopicPrefix { get; }

        /// <summary>
        /// Gets the prefix used when outputting datapoints - the TimeSeriesId is appended to
        /// the topic as a last segment in the topic path
        /// </summary>
        public TopicPrefix OutputTopicPrefix { get; }

        /// <summary>
        /// Gets whether or not to output an additional JSON message - can be useful during debugging
        /// </summary>
        /// <remarks>
        /// The JSON messages will be outputted to named topic for the purpose.
        /// The topic path will be "OutputTopicPrefix/JSON/{TimeSeriesId}"
        /// </remarks>
        public bool OutputAdditionalJSON { get; }
    }
}