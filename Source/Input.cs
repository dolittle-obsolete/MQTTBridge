/*---------------------------------------------------------------------------------------------
 *  Copyright (c) Dolittle. All rights reserved.
 *  Licensed under the MIT License. See LICENSE in the project root for license information.
 *--------------------------------------------------------------------------------------------*/
using System;
using System.Timers;
using Dolittle.Logging;
using Grpc.Core;
using Dolittle.TimeSeries.Runtime.DataPoints.Grpc;
using static Dolittle.TimeSeries.Runtime.DataPoints.Grpc.Server.InputStream;
using Dolittle.TimeSeries.DataTypes.Protobuf;
using Google.Protobuf.WellKnownTypes;
using Dolittle.Protobuf;

namespace Dolittle.TimeSeries.MQTTBridge
{
    public class Input
    {
        readonly ILogger _logger;

        public Input(ILogger logger)
        {
            _logger = logger;
        }
        
        public void Start()
        {
            var channel = new Channel("localhost:50052", ChannelCredentials.Insecure);
            var client = new InputStreamClient(channel);

            var random = new Random();

            var stream = client.Open();
            var timer = new Timer(1000);
            timer.Enabled = true;
            timer.Elapsed += async(s, e) =>
            {
                _logger.Information("Send datapoint");

                var dataPoint = new DataPoint
                {
                    TimeSeries = Guid.NewGuid().ToProtobuf(),
                    Timestamp = Timestamp.FromDateTimeOffset(DateTimeOffset.UtcNow),

                    Value = new Dolittle.TimeSeries.DataTypes.Protobuf.Value
                    {
                        MeasurementValue = new Measurement
                        {
                            FloatValue = (float)random.NextDouble(),
                            FloatError = (float)random.NextDouble()
                        }
                    }
                };

                await stream.RequestStream.WriteAsync(dataPoint);

            };
            timer.Start();

        }

    }
}