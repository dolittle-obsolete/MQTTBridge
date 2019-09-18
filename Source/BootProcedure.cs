/*---------------------------------------------------------------------------------------------
 *  Copyright (c) Dolittle. All rights reserved.
 *  Licensed under the MIT License. See LICENSE in the project root for license information.
 *--------------------------------------------------------------------------------------------*/
using System;
using System.Threading.Tasks;
using Dolittle.Booting;

namespace Dolittle.TimeSeries.MQTTBridge
{

    public class BootProcedure : ICanPerformBootProcedure
    {
        readonly Input _input;
        readonly Output _output;

        public BootProcedure(Input input, Output output)
        {
            _input = input;
            _output = output;
        }

        public bool CanPerform() => true;

        public void Perform()
        {
            _output.Start();
            
            Task.Run(() =>
            {
                Console.WriteLine("\n\n");
                Console.WriteLine("To start streaming data into input stream, press 'I'");
                Console.WriteLine("\n\n");

                for (;;)
                {
                    var key = Console.ReadKey();
                    if (key.Key == ConsoleKey.I) _input.Start();
                }
            });
        }
    }
}