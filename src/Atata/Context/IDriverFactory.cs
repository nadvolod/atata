﻿using OpenQA.Selenium;

namespace Atata
{
    /// <summary>
    /// Represents the driver factory.
    /// </summary>
    public interface IDriverFactory
    {
        /// <summary>
        /// Gets the alias.
        /// </summary>
        string Alias { get; }

        /// <summary>
        /// Creates the driver instance.
        /// </summary>
        /// <returns>The created <see cref="IWebDriver"/> instance.</returns>
        IWebDriver Create();
    }
}
