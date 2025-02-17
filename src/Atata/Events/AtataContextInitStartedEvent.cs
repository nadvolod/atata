﻿namespace Atata
{
    /// <summary>
    /// Represents an event that occurs when <see cref="AtataContext"/> is started to initialize.
    /// </summary>
    public class AtataContextInitStartedEvent
    {
        public AtataContextInitStartedEvent(AtataContext context) =>
            Context = context;

        /// <summary>
        /// Gets the context.
        /// </summary>
        public AtataContext Context { get; }
    }
}
