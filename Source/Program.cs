/*---------------------------------------------------------------------------------------------
 *  Copyright (c) Dolittle. All rights reserved.
 *  Licensed under the MIT License. See LICENSE in the project root for license information.
 *--------------------------------------------------------------------------------------------*/
using System.Threading.Tasks;

namespace Dolittle.TimeSeries.MQTTBridge
{
    class Program
    {
        static async Task Main(string[] args)
        {
            await Dolittle.Clients.Bootloader.Start();
        }
    }
}