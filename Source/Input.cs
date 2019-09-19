/*---------------------------------------------------------------------------------------------
 *  Copyright (c) Dolittle. All rights reserved.
 *  Licensed under the MIT License. See LICENSE in the project root for license information.
 *--------------------------------------------------------------------------------------------*/
using System;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Dolittle.Logging;
using protobuf = Dolittle.TimeSeries.DataTypes.Protobuf;
using Grpc.Core;
using MQTTnet;
using MQTTnet.Client;
using MQTTnet.Client.Options;
using static Dolittle.TimeSeries.Runtime.DataPoints.Grpc.Server.InputStream;
using Newtonsoft.Json;

namespace Dolittle.TimeSeries.MQTTBridge
{
    /// <summary>
    /// Represents the handler for input from MQTT
    /// </summary>
    public class Input
    {
        readonly ILogger _logger;
        readonly Configuration _configuration;

        /// <summary>
        /// Initializes a new instance of <see cref="Input"/>
        /// </summary>
        /// <param name="configuration"><see cref="Configuration"/> to use</param>
        /// <param name="logger"><see cref="ILogger"/> for logging</param>
        public Input(
            Configuration configuration,
            ILogger logger)
        {
            _logger = logger;
            _configuration = configuration;
        }

        /// <summary>
        /// Start the input - consuming and processing messages from MQTT
        /// </summary>
        public void Start()
        {
            Task.Run(async() =>
            {
                var channel = new Channel(_configuration.RuntimeEndpoint, ChannelCredentials.Insecure);
                var streamClient = new InputStreamClient(channel);
                var inputStream = streamClient.Open();

                var optionsBuilder = new MqttClientOptionsBuilder()
                    .WithClientId(_configuration.Connection.ClientId)
                    .WithTcpServer(_configuration.Connection.Host, _configuration.Connection.Port);

                if (_configuration.Connection.UseTls) optionsBuilder = optionsBuilder.WithTls();

                var options = optionsBuilder.Build();

                _logger.Information($"Creating MQTT Client");
                var factory = new MqttFactory();

                var mqttClient = factory.CreateMqttClient();
                await HandleMQTTConnection(options, mqttClient);

                mqttClient.UseApplicationMessageReceivedHandler(async e => await ProcessMessage(e, inputStream));
            });
        }

        async Task ProcessMessage(MqttApplicationMessageReceivedEventArgs e, AsyncDuplexStreamingCall<protobuf.DataPoint, Runtime.DataPoints.Grpc.Server.TimeSerie> inputStream)
        {
            try
            {
                protobuf.DataPoint dataPoint;
                _logger.Information($"Message received on '{e.ApplicationMessage.Topic}' from client '{e.ClientId}' ");

                if (((char)e.ApplicationMessage.Payload[0]) == '{')
                {
                    var json = Encoding.UTF8.GetString(e.ApplicationMessage.Payload);
                    _logger.Information($"Handle as JSON : '{json}'");
                    dataPoint = JsonConvert.DeserializeObject<protobuf.DataPoint>(json,
                        new ProtobufGuidConverter(),
                        new ProtobufTimestampConverter()
                    );
                }
                else
                {
                    dataPoint = protobuf.DataPoint.Parser.ParseFrom(e.ApplicationMessage.Payload);
                }

                await inputStream.RequestStream.WriteAsync(dataPoint);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, $"Error handling received message - topic '{e.ApplicationMessage.Topic}' from client '{e.ClientId}'");
            }
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