/*---------------------------------------------------------------------------------------------
 *  Copyright (c) Dolittle. All rights reserved.
 *  Licensed under the MIT License. See LICENSE in the project root for license information.
 *--------------------------------------------------------------------------------------------*/
using Dolittle.Booting;

namespace Dolittle.TimeSeries.MQTTBridge
{
    /// <summary>
    /// Represents the <see cref="ICanPerformBootProcedure">boot procedure</see> for setting everything up
    /// </summary>
    public class BootProcedure : ICanPerformBootProcedure
    {
        readonly Input _input;
        readonly Output _output;

        /// <summary>
        /// Initializes a new instance of <see cref="BootProcedure"/>
        /// </summary>
        /// <param name="input">The <see cref="Input"/> system</param>
        /// <param name="output">The <see cref="Output"/> system</param>
        public BootProcedure(Input input, Output output)
        {
            _input = input;
            _output = output;
        }

        /// <inheritdoc/>
        public bool CanPerform() => true;

        /// <inheritdoc/>
        public void Perform()
        {
            _input.Start();
            _output.Start();
        }
    }
}