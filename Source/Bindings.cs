/*---------------------------------------------------------------------------------------------
 *  Copyright (c) Dolittle. All rights reserved.
 *  Licensed under the MIT License. See LICENSE in the project root for license information.
 *--------------------------------------------------------------------------------------------*/
using System;
using System.Threading;
using System.Threading.Tasks;
using Dolittle.DependencyInversion;
using Dolittle.Logging;
using MQTTnet;
using MQTTnet.Client;
using MQTTnet.Client.Options;

namespace Dolittle.TimeSeries.MQTTBridge
{
    /// <summary>
    /// Provide custom bindings
    /// </summary>
    public class Bindings : ICanProvideBindings
    {
        readonly GetContainer _getContainer;
        readonly ILogger _logger;

        /// <summary>
        /// Initializes a new instance of <see cref="Bindings"/>
        /// </summary>
        /// <param name="getContainer">The <see cref="GetContainer"/> for getting the <see cref="IContainer"/></param>
        /// <param name="logger"><see cref="ILogger"/> for logging</param>
        public Bindings(GetContainer getContainer, ILogger logger)
        {
            _getContainer = getContainer;
            _logger = logger;
        }

        /// <inheritdoc/>
        public void Provide(IBindingProviderBuilder builder)
        {
            builder.Bind<IMqttClient>().To(ProvideMQTTClient);
        }


        IMqttClient ProvideMQTTClient() 
        {
            var configuration = _getContainer().Get<Configuration>();

            _logger.Information($"Connecting to MQTT broker '{configuration.Connection.Host}:{configuration.Connection.Port}'");

            var optionsBuilder = new MqttClientOptionsBuilder()
                .WithClientId(configuration.Connection.ClientId)
                .WithTcpServer(configuration.Connection.Host, configuration.Connection.Port);

            if (configuration.Connection.UseTls) optionsBuilder = optionsBuilder.WithTls();

            var options = optionsBuilder.Build();

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
            _logger.Information($"Connect to MQTT broker");
            mqttClient.ConnectAsync(options, CancellationToken.None);

            return mqttClient;
        }
    }
}