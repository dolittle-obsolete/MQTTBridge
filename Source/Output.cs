/*---------------------------------------------------------------------------------------------
 *  Copyright (c) Dolittle. All rights reserved.
 *  Licensed under the MIT License. See LICENSE in the project root for license information.
 *--------------------------------------------------------------------------------------------*/
using System.Threading.Tasks;
using Dolittle.Logging;
using Google.Protobuf.WellKnownTypes;
using Grpc.Core;
using v = Dolittle.TimeSeries.DataTypes.Protobuf.Value;
using m = Dolittle.TimeSeries.DataTypes.Protobuf.Measurement;
using static Dolittle.TimeSeries.Runtime.DataPoints.Grpc.Server.OutputStream;
using System;
using Dolittle.Protobuf;
using Newtonsoft.Json;

namespace Dolittle.TimeSeries.MQTTBridge
{
    public class Output
    {
        readonly ILogger _logger;

        public Output(ILogger logger)
        {
            _logger = logger;
        }

        public void Start()
        {
            Task.Run(async() =>
            {
                var channel = new Channel("localhost:50052", ChannelCredentials.Insecure);
                var client = new OutputStreamClient(channel);

                _logger.Information("Connecting to output stream");

                var stream = client.Open(new Empty());
                while (await stream.ResponseStream.MoveNext())
                {
                    var dataPoint = stream.ResponseStream.Current;
                    _logger.Information($"DataPoint received {JsonConvert.SerializeObject(dataPoint)}");

                    try
                    {
                        switch (dataPoint.Value.ValueCase)
                        {
                            case v.ValueOneofCase.MeasurementValue:
                                {
                                    object value = "NaN";

                                    switch (dataPoint.Value.MeasurementValue.ValueCase)
                                    {
                                        case m.ValueOneofCase.FloatValue:
                                            value = dataPoint.Value.MeasurementValue.FloatValue;
                                            break;
                                        case m.ValueOneofCase.DoubleValue:
                                            value = dataPoint.Value.MeasurementValue.DoubleValue;
                                            break;
                                        case m.ValueOneofCase.Int32Value:
                                            value = dataPoint.Value.MeasurementValue.Int32Value;
                                            break;
                                        case m.ValueOneofCase.Int64Value:
                                            value = dataPoint.Value.MeasurementValue.Int64Value;
                                            break;
                                    }

                                    _logger.Information($"DataPoint for timeseries '{dataPoint.TimeSeries.ToGuid()}' with value '{value}' generated @ '{dataPoint.Timestamp.ToDateTimeOffset()}'");

                                }
                                break;
                            case v.ValueOneofCase.Vector2Value: 
                            {
                                _logger.Information($"DataPoint for timeseries '{dataPoint.TimeSeries.ToGuid()}' with value '{dataPoint.Value.Vector2Value.X.FloatValue}, {dataPoint.Value.Vector2Value.Y.FloatValue}' generated @ '{dataPoint.Timestamp.ToDateTimeOffset()}'");
                            } break;
                            case v.ValueOneofCase.Vector3Value: 
                            {
                                _logger.Information($"DataPoint for timeseries '{dataPoint.TimeSeries.ToGuid()}' with value '{dataPoint.Value.Vector3Value.X.FloatValue}, {dataPoint.Value.Vector3Value.Y.FloatValue}, {dataPoint.Value.Vector3Value.Z.FloatValue}' generated @ '{dataPoint.Timestamp.ToDateTimeOffset()}'");
                            } break;
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.Error(ex, "Error interpreting datapoint");

                    }
                }
            });
        }
    }
}