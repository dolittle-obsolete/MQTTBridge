/*---------------------------------------------------------------------------------------------
 *  Copyright (c) Dolittle. All rights reserved.
 *  Licensed under the MIT License. See LICENSE in the project root for license information.
 *--------------------------------------------------------------------------------------------*/
using System.Threading.Tasks;
using Dolittle.Logging;
using Google.Protobuf.WellKnownTypes;
using Grpc.Core;
using static Dolittle.TimeSeries.Runtime.DataPoints.Grpc.Server.OutputStream;
using System.Text;
using MQTTnet;
using MQTTnet.Client;
using Newtonsoft.Json;
using Dolittle.Protobuf;
using Google.Protobuf;
using System.IO;

namespace Dolittle.TimeSeries.MQTTBridge
{
    /// <summary>
    /// Represents the handler for output to MQTT
    /// </summary>
    public class Output
    {
        readonly ILogger _logger;
        readonly Configuration _configuration;
        private readonly IMqttClient _mqttClient;

        /// <summary>
        /// Initializes a new instance of <see cref="Output"/>
        /// </summary>
        /// <param name="configuration"><see cref="Configuration"/> to use</param>
        /// <param name="mqttClient"><see cref="IMqttClient"/> to use</param>
        /// <param name="logger"><see cref="ILogger"/> for logging</param>
        public Output(
            Configuration configuration,
            IMqttClient mqttClient,
            ILogger logger)
        {
            _logger = logger;
            _configuration = configuration;
            _mqttClient = mqttClient;
        }

        /// <summary>
        /// Start the output - consume output stream from runtime and send to MQTT
        /// </summary>
        public void Start()
        {
            Task.Run(async() =>
            {
                var channel = new Channel(_configuration.RuntimeEndpoint, ChannelCredentials.Insecure);
                var client = new OutputStreamClient(channel);
                var stream = client.Open(new Empty());

                while (await stream.ResponseStream.MoveNext())
                {
                    var dataPoint = stream.ResponseStream.Current;
                    var timeSeriesId = dataPoint.TimeSeries.ToGuid();
                    var memoryStream = new MemoryStream();
                    var outputStream = new CodedOutputStream(memoryStream);
                    dataPoint.WriteTo(outputStream);

                    var topic = $"{_configuration.OutputTopicPrefix}/{timeSeriesId}";
                        await _mqttClient.PublishAsync(new MqttApplicationMessage 
                        {
                            Topic = topic,
                            Payload = memoryStream.GetBuffer()
                        });


                    if( _configuration.OutputAdditionalJSON )
                    {
                        var JSONtopic = $"{_configuration.OutputTopicPrefix}/JSON/{timeSeriesId}";
                        var dataPointAsJSON = JsonConvert.SerializeObject(dataPoint);
                        await _mqttClient.PublishAsync(new MqttApplicationMessage 
                        {
                            Topic = JSONtopic,
                            Payload = Encoding.UTF8.GetBytes(dataPointAsJSON)
                        });
                    }
                }
            });
        }
    }
}