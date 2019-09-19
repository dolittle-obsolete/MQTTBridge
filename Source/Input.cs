/*---------------------------------------------------------------------------------------------
 *  Copyright (c) Dolittle. All rights reserved.
 *  Licensed under the MIT License. See LICENSE in the project root for license information.
 *--------------------------------------------------------------------------------------------*/
using System;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Dolittle.Logging;
using Dolittle.Serialization.Json;
using protobuf = Dolittle.TimeSeries.DataTypes.Protobuf;
using Grpc.Core;
using MQTTnet;
using MQTTnet.Client;
using MQTTnet.Client.Options;
using static Dolittle.TimeSeries.Runtime.DataPoints.Grpc.Server.InputStream;
using Dolittle.Protobuf;
using Dolittle.TimeSeries.DataTypes;
using Newtonsoft.Json;

namespace Dolittle.TimeSeries.MQTTBridge
{
    /// <summary>
    /// 
    /// </summary>
    public class Input
    {
        readonly Channel _channel;
        readonly ILogger _logger;
        readonly Configuration _configuration;
        private readonly ISerializer _serializer;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="channel"></param>
        /// <param name="configuration"></param>
        /// <param name="serializer"></param>
        /// <param name="logger"></param>
        public Input(
            Channel channel,
            Configuration configuration,
            ISerializer serializer,
            ILogger logger)
        {
            _logger = logger;
            _channel = channel;
            _configuration = configuration;
            _serializer = serializer;
        }

        /// <summary>
        /// 
        /// </summary>
        public void Start()
        {
            Task.Run(async() =>
            {
                var streamClient = new InputStreamClient(_channel);
                var inputStream = streamClient.Open();

                var optionsBuilder = new MqttClientOptionsBuilder()
                    .WithClientId(_configuration.Connection.ClientId)
                    .WithTcpServer(_configuration.Connection.Host, _configuration.Connection.Port);

                if (_configuration.Connection.UseTls) optionsBuilder = optionsBuilder.WithTls();

                var options = optionsBuilder.Build();

                _logger.Information($"Creating MQTT Client");
                var factory = new MqttFactory();

                var mqttClient = factory.CreateMqttClient();
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
                    await mqttClient.SubscribeAsync(new TopicFilterBuilder().WithTopic("#").Build());

                });
                _logger.Information($"Connect to MQTT broker");
                await mqttClient.ConnectAsync(options, CancellationToken.None);

                mqttClient.UseApplicationMessageReceivedHandler(async e =>
                {

                    try
                    { 
                        protobuf.DataPoint dataPoint;

                        if (((char) e.ApplicationMessage.Payload[0]) == '{')
                        {
                            var json = Encoding.UTF8.GetString(e.ApplicationMessage.Payload);
                            _logger.Information($"Handle message '{json}'");

                            dynamic dynamicDP = JsonConvert.DeserializeObject<dynamic>(json);
                            dataPoint = new protobuf.DataPoint();
                            var timeSeriesId = (TimeSeriesId)Guid.Parse(dynamicDP.timeSeries.ToString());
                            dataPoint.TimeSeries = timeSeriesId.ToProtobuf();
                            dataPoint.Value = new protobuf.Value
                            {
                                MeasurementValue = new protobuf.Measurement
                                {
                                    FloatValue = dynamicDP.value.value,
                                    FloatError = dynamicDP.value.error
                                }
                            };
                            dataPoint.Timestamp = Google.Protobuf.WellKnownTypes.Timestamp.FromDateTimeOffset(DateTimeOffset.Parse(dynamicDP.timestamp.ToString()));
                        }
                        else
                        {
                            dataPoint = new protobuf.DataPoint();
                        }

                        await inputStream.RequestStream.WriteAsync(dataPoint);
                    }
                    catch (Exception ex)
                    {
                        _logger.Error(ex, $"Error handling received message - topic '{e.ApplicationMessage.Topic}' from client '{e.ClientId}'");
                    }
                });
            });
        }
    }
}