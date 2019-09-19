/*---------------------------------------------------------------------------------------------
 *  Copyright (c) Dolittle. All rights reserved.
 *  Licensed under the MIT License. See LICENSE in the project root for license information.
 *--------------------------------------------------------------------------------------------*/
using Dolittle.Concepts;

namespace Dolittle.TimeSeries.MQTTBridge
{
    /// <summary>
    /// Represents the concept of a topic prefix string
    /// </summary>
    public class TopicPrefix
    {
        /// <summary>
        /// Initializes a new instance of <see cref="TopicPrefix"/>
        /// </summary>
        /// <param name="value"><see cref="string"/> value</param>
        public TopicPrefix(string value)
        {
            if( value.EndsWith("/") ) value = value.Substring(0, value.Length-1);
            Value = value;
        }

        /// <summary>
        /// Gets the value for the <see cref="TopicPrefix"/>
        /// </summary>
        public string Value { get; }

        /// <summary>
        /// Implicitly convert from <see cref="string"/> to <see cref="TopicPrefix"/>
        /// </summary>
        /// <param name="value"><see cref="string"/> to convert from</param>
        public static implicit operator TopicPrefix(string value) => new TopicPrefix(value);

        /// <summary>
        /// Implicitly convert from <see cref="TopicPrefix"/> to <see cref="string"/>
        /// </summary>
        /// <param name="value"><see cref="TopicPrefix"/> to convert from</param>
        public static implicit operator string(TopicPrefix value) => value.Value;

        /// <summary>
        /// Get the wildcard combined with the prefix
        /// </summary>
        public string Wildcard => string.IsNullOrEmpty(Value)?"#":$"{Value}/#";
        
    }
}