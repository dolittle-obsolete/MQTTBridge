/*---------------------------------------------------------------------------------------------
 *  Copyright (c) Dolittle. All rights reserved.
 *  Licensed under the MIT License. See LICENSE in the project root for license information.
 *--------------------------------------------------------------------------------------------*/
using System.Threading.Tasks;
using Dolittle.Logging;
using Google.Protobuf.WellKnownTypes;
using Grpc.Core;
using static Dolittle.TimeSeries.Runtime.DataPoints.Grpc.Server.OutputStream;
using System;
using System.Threading;
using System.Text;
using MQTTnet;
using MQTTnet.Client;
using MQTTnet.Client.Options;
using Newtonsoft.Json;
using Dolittle.Protobuf;
using Dolittle.TimeSeries.DataTypes.Protobuf;
using Google.Protobuf;
using System.IO;

namespace Dolittle.TimeSeries.MQTTBridge
{
    /// <summary>
    /// 
    /// </summary>
    public class Output
    {
        readonly ILogger _logger;
        readonly Configuration _configuration;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="configuration"><see cref="Configuration"/> to use</param>
        /// <param name="logger"><see cref="ILogger"/> for logging</param>
        public Output(
            Configuration configuration,
            ILogger logger)
        {
            _logger = logger;
            _configuration = configuration;
        }

        /// <summary>
        /// 
        /// </summary>
        public void Start()
        {
            Task.Run(async() =>
            {
                var channel = new Channel(_configuration.RuntimeEndpoint, ChannelCredentials.Insecure);
                var client = new OutputStreamClient(channel);
                var stream = client.Open(new Empty());

                var optionsBuilder = new MqttClientOptionsBuilder()
                    .WithClientId(_configuration.Connection.ClientId)
                    .WithTcpServer(_configuration.Connection.Host, _configuration.Connection.Port);

                if (_configuration.Connection.UseTls) optionsBuilder = optionsBuilder.WithTls();

                var options = optionsBuilder.Build();

                _logger.Information($"Creating MQTT Client");
                var factory = new MqttFactory();

                var mqttClient = factory.CreateMqttClient();
                await HandleMQTTConnection(options, mqttClient);

                while (await stream.ResponseStream.MoveNext())
                {
                    var dataPoint = stream.ResponseStream.Current;
                    var timeSeriesId = dataPoint.TimeSeries.ToGuid();
                    var memoryStream = new MemoryStream();
                    var outputStream = new CodedOutputStream(memoryStream);
                    dataPoint.WriteTo(outputStream);

                    var topic = $"{_configuration.OutputTopicPrefix}/{timeSeriesId}";
                        await mqttClient.PublishAsync(new MqttApplicationMessage 
                        {
                            Topic = topic,
                            Payload = memoryStream.GetBuffer()
                        });


                    if( _configuration.OutputAdditionalJSON )
                    {
                        var JSONtopic = $"{_configuration.OutputTopicPrefix}/JSON/{timeSeriesId}";
                        var dataPointAsJSON = JsonConvert.SerializeObject(dataPoint);
                        await mqttClient.PublishAsync(new MqttApplicationMessage 
                        {
                            Topic = JSONtopic,
                            Payload = Encoding.UTF8.GetBytes(dataPointAsJSON)
                        });
                    }
                }
            });
        }

        async Task HandleMQTTConnection(IMqttClientOptions options, IMqttClient mqttClient)
        {
            mqttClient.UseDisconnectedHandler(async e =>
            {
                await Task.Delay(TimeSpan.FromSeconds(5));
                try
                {
                    await mqttClient.ConnectAsync(options, CancellationToken.None);
                }
                catch (Exception ex)
                {
                    _logger.Error(ex, "Couldn't reconnect MQTT client");
                }
            });
            mqttClient.UseConnectedHandler(async e =>
            {
                _logger.Information($"Connected to MQTT broker");
                var wildcard = _configuration.InputTopicPrefix.Wildcard;
                _logger.Information($"Subscribe to '{wildcard}'");
                await mqttClient.SubscribeAsync(new TopicFilterBuilder().WithTopic(wildcard).Build());
            });
            _logger.Information($"Connect to MQTT broker");
            await mqttClient.ConnectAsync(options, CancellationToken.None);
        }
    }
}