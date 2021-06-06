﻿using System;

namespace Atata
{
    /// <summary>
    /// Represents the configuration of <see cref="ILogConsumer"/>.
    /// </summary>
    // TODO: Rename LogConsumerInfo to LogConsumerConfiguration.
    public class LogConsumerInfo : ICloneable
    {
        public LogConsumerInfo(
            ILogConsumer consumer,
            LogLevel minLevel = LogLevel.Trace,
            bool logSectionFinish = true)
        {
            Consumer = consumer;
            LogSectionFinish = logSectionFinish;
            MinLevel = minLevel;
        }

        /// <summary>
        /// Gets the log consumer.
        /// </summary>
        public ILogConsumer Consumer { get; private set; }

        /// <summary>
        /// Gets the minimum log level.
        /// The default value is <see cref="LogLevel.Trace"/>.
        /// </summary>
        public LogLevel MinLevel { get; internal set; }

        /// <summary>
        /// Gets a value indicating whether to log section finish.
        /// The default value is <see langword="true"/>.
        /// </summary>
        public bool LogSectionFinish { get; internal set; }

        /// <summary>
        /// Gets or sets the message nesting level indent.
        /// The default value is <c>"- "</c>.
        /// </summary>
        public string MessageNestingLevelIndent { get; set; } = "- ";

        /// <summary>
        /// Gets or sets the message start section prefix.
        /// The default value is <c>"&gt; "</c>.
        /// </summary>
        public string MessageStartSectionPrefix { get; set; } = "> ";

        /// <summary>
        /// Gets or sets the message end section prefix.
        /// The default value is <c>"&lt; "</c>.
        /// </summary>
        public string MessageEndSectionPrefix { get; set; } = "< ";

        /// <summary>
        /// Creates a copy of the current instance.
        /// </summary>
        /// <returns>The copied <see cref="LogConsumerInfo"/> instance.</returns>
        public LogConsumerInfo Clone()
        {
            LogConsumerInfo clone = (LogConsumerInfo)MemberwiseClone();

            if (Consumer is ICloneable cloneableConsumer)
                clone.Consumer = (ILogConsumer)cloneableConsumer.Clone();

            return clone;
        }

        object ICloneable.Clone() =>
            Clone();
    }
}
