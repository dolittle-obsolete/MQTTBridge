/*---------------------------------------------------------------------------------------------
 *  Copyright (c) Dolittle. All rights reserved.
 *  Licensed under the MIT License. See LICENSE in the project root for license information.
 *--------------------------------------------------------------------------------------------*/
using System.Threading.Tasks;
using Dolittle.Logging;
using Google.Protobuf.WellKnownTypes;
using Grpc.Core;
using static Dolittle.TimeSeries.Runtime.DataPoints.Grpc.Server.OutputStream;
using Newtonsoft.Json;

namespace Dolittle.TimeSeries.MQTTBridge
{
    /// <summary>
    /// 
    /// </summary>
    public class Output
    {
        readonly Channel _channel;
        readonly ILogger _logger;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="channel"></param>
        /// <param name="logger"></param>
        public Output(
            Channel channel,
            ILogger logger)
        {
            _logger = logger;
            _channel = channel;
        }

        /// <summary>
        /// 
        /// </summary>
        public void Start()
        {
            Task.Run(async() =>
            {
                var client = new OutputStreamClient(_channel);

                _logger.Information("Connecting to output stream");

                var stream = client.Open(new Empty());
                while (await stream.ResponseStream.MoveNext())
                {
                    var dataPoint = stream.ResponseStream.Current;
                    _logger.Information($"DataPoint received {JsonConvert.SerializeObject(dataPoint)}");
                }
            });
        }
    }
}