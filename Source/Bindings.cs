/*---------------------------------------------------------------------------------------------
 *  Copyright (c) Dolittle. All rights reserved.
 *  Licensed under the MIT License. See LICENSE in the project root for license information.
 *--------------------------------------------------------------------------------------------*/
using Dolittle.DependencyInversion;
using Grpc.Core;

namespace Dolittle.TimeSeries.MQTTBridge
{
    /// <summary>
    /// Represents all the custom bindings
    /// </summary>
    public class Bindings : ICanProvideBindings
    {
        /// <inheritdoc/>
        public void Provide(IBindingProviderBuilder builder)
        {
            var channel = new Channel("localhost:50052", ChannelCredentials.Insecure);
            builder.Bind<Channel>().To(channel);
        }
    }
}