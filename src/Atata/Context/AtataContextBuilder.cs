﻿using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Edge;
using OpenQA.Selenium.Firefox;
using OpenQA.Selenium.IE;
using OpenQA.Selenium.Remote;
using OpenQA.Selenium.Safari;

namespace Atata
{
    /// <summary>
    /// Represents the builder of <see cref="AtataContext"/>.
    /// </summary>
    public class AtataContextBuilder
    {
        public AtataContextBuilder(AtataBuildingContext buildingContext) =>
            BuildingContext = buildingContext.CheckNotNull(nameof(buildingContext));

        /// <summary>
        /// Gets the building context.
        /// </summary>
        public AtataBuildingContext BuildingContext { get; internal set; }

        /// <summary>
        /// Gets the builder of context attributes,
        /// which provides the functionality to add extra attributes to different metadata levels:
        /// global, assembly, component and property.
        /// </summary>
        public AttributesAtataContextBuilder Attributes => new AttributesAtataContextBuilder(BuildingContext);

        /// <summary>
        /// Gets the builder of event subscriptions,
        /// which provides the methods to subscribe to Atata and custom events.
        /// </summary>
        public EventSubscriptionsAtataContextBuilder EventSubscriptions => new EventSubscriptionsAtataContextBuilder(BuildingContext);

        /// <summary>
        /// Gets the builder of log consumers,
        /// which provides the methods to add log consumers.
        /// </summary>
        public LogConsumersAtataContextBuilder LogConsumers => new LogConsumersAtataContextBuilder(BuildingContext);

        /// <summary>
        /// Gets the builder of screenshot consumers,
        /// which provides the methods to add screenshot consumers.
        /// </summary>
        public ScreenshotConsumersAtataContextBuilder ScreenshotConsumers => new ScreenshotConsumersAtataContextBuilder(BuildingContext);

        /// <summary>
        /// Returns an existing or creates a new builder for <typeparamref name="TDriverBuilder"/> by the specified alias.
        /// </summary>
        /// <typeparam name="TDriverBuilder">The type of the driver builder.</typeparam>
        /// <param name="alias">The driver alias.</param>
        /// <param name="driverBuilderCreator">The function that creates a driver builder.</param>
        /// <returns>The <typeparamref name="TDriverBuilder"/> instance.</returns>
        public TDriverBuilder ConfigureDriver<TDriverBuilder>(string alias, Func<TDriverBuilder> driverBuilderCreator)
            where TDriverBuilder : AtataContextBuilder, IDriverFactory
        {
            alias.CheckNotNullOrWhitespace(nameof(alias));
            driverBuilderCreator.CheckNotNull(nameof(driverBuilderCreator));

            var driverFactory = BuildingContext.GetDriverFactory(alias);

            if (driverFactory is null)
            {
                driverFactory = driverBuilderCreator.Invoke();
                BuildingContext.DriverFactories.Add(driverFactory);
            }
            else if (!(driverFactory is TDriverBuilder))
            {
                throw new ArgumentException(
                    $@"Existing driver with ""{alias}"" alias has other factory type in {nameof(AtataContextBuilder)}.
Expected: {typeof(TDriverBuilder).FullName}
Actual: {driverFactory.GetType().FullName}",
                    nameof(alias));
            }

            return (TDriverBuilder)driverFactory;
        }

        /// <summary>
        /// Use the driver builder.
        /// </summary>
        /// <typeparam name="TDriverBuilder">The type of the driver builder.</typeparam>
        /// <param name="driverBuilder">The driver builder.</param>
        /// <returns>The <typeparamref name="TDriverBuilder"/> instance.</returns>
        public TDriverBuilder UseDriver<TDriverBuilder>(TDriverBuilder driverBuilder)
            where TDriverBuilder : AtataContextBuilder, IDriverFactory
        {
            driverBuilder.CheckNotNull(nameof(driverBuilder));

            BuildingContext.DriverFactories.Add(driverBuilder);
            BuildingContext.DriverFactoryToUse = driverBuilder;

            return driverBuilder;
        }

        /// <summary>
        /// Sets the alias of the driver to use.
        /// </summary>
        /// <param name="alias">The alias of the driver.</param>
        /// <returns>The <see cref="AtataContextBuilder"/> instance.</returns>
        public AtataContextBuilder UseDriver(string alias)
        {
            alias.CheckNotNullOrWhitespace(nameof(alias));

            IDriverFactory driverFactory = BuildingContext.GetDriverFactory(alias);

            if (driverFactory != null)
                BuildingContext.DriverFactoryToUse = driverFactory;
            else if (UsePredefinedDriver(alias) == null)
                throw new ArgumentException($"No driver with \"{alias}\" alias defined.", nameof(alias));

            return this;
        }

        /// <summary>
        /// Use specified driver instance.
        /// </summary>
        /// <param name="driver">The driver to use.</param>
        /// <returns>The <see cref="CustomDriverAtataContextBuilder"/> instance.</returns>
        public CustomDriverAtataContextBuilder UseDriver(IWebDriver driver)
        {
            driver.CheckNotNull(nameof(driver));

            return UseDriver(() => driver);
        }

        /// <summary>
        /// Use custom driver factory method.
        /// </summary>
        /// <param name="driverFactory">The driver factory method.</param>
        /// <returns>The <see cref="CustomDriverAtataContextBuilder"/> instance.</returns>
        public CustomDriverAtataContextBuilder UseDriver(Func<IWebDriver> driverFactory)
        {
            driverFactory.CheckNotNull(nameof(driverFactory));

            return UseDriver(new CustomDriverAtataContextBuilder(BuildingContext, driverFactory));
        }

        private IDriverFactory UsePredefinedDriver(string alias)
        {
            switch (alias.ToLowerInvariant())
            {
                case DriverAliases.Chrome:
                    return UseChrome();
                case DriverAliases.Firefox:
                    return UseFirefox();
                case DriverAliases.InternetExplorer:
                    return UseInternetExplorer();
                case DriverAliases.Safari:
                    return UseSafari();
                case DriverAliases.Edge:
                    return UseEdge();
                default:
                    return null;
            }
        }

        /// <summary>
        /// Sets the driver initialization stage.
        /// The default value is <see cref="AtataContextDriverInitializationStage.Build"/>.
        /// </summary>
        /// <param name="stage">The stage.</param>
        /// <returns>The <see cref="AtataContextBuilder"/> instance.</returns>
        public AtataContextBuilder UseDriverInitializationStage(AtataContextDriverInitializationStage stage)
        {
            BuildingContext.DriverInitializationStage = stage;
            return this;
        }

        /// <summary>
        /// Creates and returns a new builder for <see cref="ChromeDriver"/>
        /// with default <see cref="DriverAliases.Chrome"/> alias.
        /// Sets this builder as a one to use for a driver creation.
        /// </summary>
        /// <returns>The <see cref="ChromeAtataContextBuilder"/> instance.</returns>
        public ChromeAtataContextBuilder UseChrome() =>
            UseDriver(new ChromeAtataContextBuilder(BuildingContext));

        /// <summary>
        /// Creates and returns a new builder for <see cref="FirefoxDriver"/>
        /// with default <see cref="DriverAliases.Firefox"/> alias.
        /// Sets this builder as a one to use for a driver creation.
        /// </summary>
        /// <returns>The <see cref="FirefoxAtataContextBuilder"/> instance.</returns>
        public FirefoxAtataContextBuilder UseFirefox() =>
            UseDriver(new FirefoxAtataContextBuilder(BuildingContext));

        /// <summary>
        /// Creates and returns a new builder for <see cref="InternetExplorerDriver"/>
        /// with default <see cref="DriverAliases.InternetExplorer"/> alias.
        /// Sets this builder as a one to use for a driver creation.
        /// </summary>
        /// <returns>The <see cref="InternetExplorerAtataContextBuilder"/> instance.</returns>
        public InternetExplorerAtataContextBuilder UseInternetExplorer() =>
            UseDriver(new InternetExplorerAtataContextBuilder(BuildingContext));

        /// <summary>
        /// Creates and returns a new builder for <see cref="EdgeDriver"/>
        /// with default <see cref="DriverAliases.Edge"/> alias.
        /// Sets this builder as a one to use for a driver creation.
        /// </summary>
        /// <returns>The <see cref="EdgeAtataContextBuilder"/> instance.</returns>
        public EdgeAtataContextBuilder UseEdge() =>
            UseDriver(new EdgeAtataContextBuilder(BuildingContext));

        /// <summary>
        /// Creates and returns a new builder for <see cref="SafariDriver"/>
        /// with default <see cref="DriverAliases.Safari"/> alias.
        /// Sets this builder as a one to use for a driver creation.
        /// </summary>
        /// <returns>The <see cref="SafariAtataContextBuilder"/> instance.</returns>
        public SafariAtataContextBuilder UseSafari() =>
            UseDriver(new SafariAtataContextBuilder(BuildingContext));

        /// <summary>
        /// Creates and returns a new builder for <see cref="RemoteWebDriver"/>
        /// with default <see cref="DriverAliases.Remote"/> alias.
        /// Sets this builder as a one to use for a driver creation.
        /// </summary>
        /// <returns>The <see cref="RemoteDriverAtataContextBuilder"/> instance.</returns>
        public RemoteDriverAtataContextBuilder UseRemoteDriver() =>
            UseDriver(new RemoteDriverAtataContextBuilder(BuildingContext));

        /// <summary>
        /// Returns an existing or creates a new builder for <see cref="ChromeDriver"/> by the specified alias.
        /// </summary>
        /// <param name="alias">
        /// The driver alias.
        /// The default value is <see cref="DriverAliases.Chrome"/>.
        /// </param>
        /// <returns>The <see cref="ChromeAtataContextBuilder"/> instance.</returns>
        public ChromeAtataContextBuilder ConfigureChrome(string alias = DriverAliases.Chrome) =>
            ConfigureDriver(
                alias,
                () => new ChromeAtataContextBuilder(BuildingContext).WithAlias(alias));

        /// <summary>
        /// Returns an existing or creates a new builder for <see cref="FirefoxDriver"/> by the specified alias.
        /// </summary>
        /// <param name="alias">
        /// The driver alias.
        /// The default value is <see cref="DriverAliases.Firefox"/>.
        /// </param>
        /// <returns>The <see cref="FirefoxAtataContextBuilder"/> instance.</returns>
        public FirefoxAtataContextBuilder ConfigureFirefox(string alias = DriverAliases.Firefox) =>
            ConfigureDriver(
                alias,
                () => new FirefoxAtataContextBuilder(BuildingContext).WithAlias(alias));

        /// <summary>
        /// Returns an existing or creates a new builder for <see cref="InternetExplorerDriver"/> by the specified alias.
        /// </summary>
        /// <param name="alias">
        /// The driver alias.
        /// The default value is <see cref="DriverAliases.InternetExplorer"/>.
        /// </param>
        /// <returns>The <see cref="InternetExplorerAtataContextBuilder"/> instance.</returns>
        public InternetExplorerAtataContextBuilder ConfigureInternetExplorer(string alias = DriverAliases.InternetExplorer) =>
            ConfigureDriver(
                alias,
                () => new InternetExplorerAtataContextBuilder(BuildingContext).WithAlias(alias));

        /// <summary>
        /// Returns an existing or creates a new builder for <see cref="EdgeDriver"/> by the specified alias.
        /// </summary>
        /// <param name="alias">
        /// The driver alias.
        /// The default value is <see cref="DriverAliases.Edge"/>.
        /// </param>
        /// <returns>The <see cref="EdgeAtataContextBuilder"/> instance.</returns>
        public EdgeAtataContextBuilder ConfigureEdge(string alias = DriverAliases.Edge) =>
            ConfigureDriver(
                alias,
                () => new EdgeAtataContextBuilder(BuildingContext).WithAlias(alias));

        /// <summary>
        /// Returns an existing or creates a new builder for <see cref="SafariDriver"/> by the specified alias.
        /// </summary>
        /// <param name="alias">
        /// The driver alias.
        /// The default value is <see cref="DriverAliases.Safari"/>.
        /// </param>
        /// <returns>The <see cref="SafariAtataContextBuilder"/> instance.</returns>
        public SafariAtataContextBuilder ConfigureSafari(string alias = DriverAliases.Safari) =>
            ConfigureDriver(
                alias,
                () => new SafariAtataContextBuilder(BuildingContext).WithAlias(alias));

        /// <summary>
        /// Returns an existing or creates a new builder for <see cref="RemoteWebDriver"/> by the specified alias.
        /// </summary>
        /// <param name="alias">
        /// The driver alias.
        /// The default value is <see cref="DriverAliases.Remote"/>.
        /// </param>
        /// <returns>The <see cref="RemoteDriverAtataContextBuilder"/> instance.</returns>
        public RemoteDriverAtataContextBuilder ConfigureRemoteDriver(string alias = DriverAliases.Remote) =>
            ConfigureDriver(
                alias,
                () => new RemoteDriverAtataContextBuilder(BuildingContext).WithAlias(alias));

        [Obsolete("Use LogConsumers.Add(...) instead.")] // Obsolete since v2.0.0.
        public LogConsumerAtataContextBuilder<TLogConsumer> AddLogConsumer<TLogConsumer>()
            where TLogConsumer : ILogConsumer, new()
            =>
            LogConsumers.Add<TLogConsumer>();

        [Obsolete("Use LogConsumers.Add(...) instead.")] // Obsolete since v2.0.0.
        public LogConsumerAtataContextBuilder<TLogConsumer> AddLogConsumer<TLogConsumer>(TLogConsumer consumer)
            where TLogConsumer : ILogConsumer
            =>
            LogConsumers.Add(consumer);

        [Obsolete("Use LogConsumers.Add(...) instead.")] // Obsolete since v2.0.0.
        public LogConsumerAtataContextBuilder<ILogConsumer> AddLogConsumer(string typeNameOrAlias) =>
            LogConsumers.Add(typeNameOrAlias);

        [Obsolete("Use LogConsumers.AddTrace() instead.")] // Obsolete since v2.0.0.
        public LogConsumerAtataContextBuilder<TraceLogConsumer> AddTraceLogging() =>
            LogConsumers.AddTrace();

        [Obsolete("Use LogConsumers.AddDebug() instead.")] // Obsolete since v2.0.0.
        public LogConsumerAtataContextBuilder<DebugLogConsumer> AddDebugLogging() =>
            LogConsumers.AddDebug();

        [Obsolete("Use LogConsumers.AddConsole() instead.")] // Obsolete since v2.0.0.
        public LogConsumerAtataContextBuilder<ConsoleLogConsumer> AddConsoleLogging() =>
            LogConsumers.AddConsole();

        [Obsolete("Use LogConsumers.AddNUnitTestContext() instead.")] // Obsolete since v2.0.0.
        public LogConsumerAtataContextBuilder<NUnitTestContextLogConsumer> AddNUnitTestContextLogging() =>
            LogConsumers.AddNUnitTestContext();

        [Obsolete("Use LogConsumers.AddNLog(...) instead.")] // Obsolete since v2.0.0.
        public LogConsumerAtataContextBuilder<NLogConsumer> AddNLogLogging(string loggerName = null) =>
            LogConsumers.AddNLog(loggerName);

        [Obsolete("Use LogConsumers.AddNLogFile() instead.")] // Obsolete since v2.0.0.
        public LogConsumerAtataContextBuilder<NLogFileConsumer> AddNLogFileLogging() =>
            LogConsumers.AddNLogFile();

        [Obsolete("Use LogConsumers.AddLog4Net(...) instead.")] // Obsolete since v2.0.0.
        public LogConsumerAtataContextBuilder<Log4NetConsumer> AddLog4NetLogging(string loggerName = null) =>
            LogConsumers.AddLog4Net(loggerName);

        [Obsolete("Use LogConsumers.AddLog4Net(...) instead.")] // Obsolete since v2.0.0.
        public LogConsumerAtataContextBuilder<Log4NetConsumer> AddLog4NetLogging(string repositoryName, string loggerName = null) =>
            LogConsumers.AddLog4Net(repositoryName, loggerName);

        [Obsolete("Use LogConsumers.AddLog4Net(...) instead.")] // Obsolete since v2.0.0.
        public LogConsumerAtataContextBuilder<Log4NetConsumer> AddLog4NetLogging(Assembly repositoryAssembly, string loggerName = null) =>
            LogConsumers.AddLog4Net(repositoryAssembly, loggerName);

        /// <summary>
        /// Adds the variable.
        /// </summary>
        /// <param name="key">The variable key.</param>
        /// <param name="value">The variable value.</param>
        /// <returns>The <see cref="AtataContextBuilder"/> instance.</returns>
        public AtataContextBuilder AddVariable(string key, object value)
        {
            key.CheckNotNullOrWhitespace(nameof(key));

            BuildingContext.Variables[key] = value;

            return this;
        }

        /// <summary>
        /// Adds the variables.
        /// </summary>
        /// <param name="variables">The variables to add.</param>
        /// <returns>The <see cref="AtataContextBuilder"/> instance.</returns>
        public AtataContextBuilder AddVariables(IDictionary<string, object> variables)
        {
            variables.CheckNotNull(nameof(variables));

            foreach (var variable in variables)
                BuildingContext.Variables[variable.Key] = variable.Value;

            return this;
        }

        /// <summary>
        /// Adds the secret string to mask in log.
        /// </summary>
        /// <param name="value">The secret string value.</param>
        /// <param name="mask">The mask, which should replace the secret string.</param>
        /// <returns>The <see cref="AtataContextBuilder"/> instance.</returns>
        public AtataContextBuilder AddSecretStringToMaskInLog(string value, string mask = "{*****}")
        {
            value.CheckNotNullOrWhitespace(nameof(value));
            mask.CheckNotNullOrWhitespace(nameof(mask));

            BuildingContext.SecretStringsToMaskInLog.Add(
                new SecretStringToMask(value, mask));

            return this;
        }

        [Obsolete("Use ScreenshotConsumers.Add<TScreenshotConsumer>() instead.")] // Obsolete since v2.0.0.
        public ScreenshotConsumerAtataContextBuilder<TScreenshotConsumer> AddScreenshotConsumer<TScreenshotConsumer>()
            where TScreenshotConsumer : IScreenshotConsumer, new()
            =>
            ScreenshotConsumers.Add<TScreenshotConsumer>();

        [Obsolete("Use ScreenshotConsumers.Add(...) instead.")] // Obsolete since v2.0.0.
        public ScreenshotConsumerAtataContextBuilder<TScreenshotConsumer> AddScreenshotConsumer<TScreenshotConsumer>(TScreenshotConsumer consumer)
            where TScreenshotConsumer : IScreenshotConsumer
            =>
            ScreenshotConsumers.Add(consumer);

        [Obsolete("Use ScreenshotConsumers.Add(...) instead.")] // Obsolete since v2.0.0.
        public ScreenshotConsumerAtataContextBuilder<IScreenshotConsumer> AddScreenshotConsumer(string typeNameOrAlias) =>
            ScreenshotConsumers.Add(typeNameOrAlias);

        [Obsolete("Use ScreenshotConsumers.AddFile() instead.")] // Obsolete since v2.0.0.
        public ScreenshotConsumerAtataContextBuilder<FileScreenshotConsumer> AddScreenshotFileSaving() =>
            ScreenshotConsumers.AddFile();

        /// <summary>
        /// Sets the factory method of the test name.
        /// </summary>
        /// <param name="testNameFactory">The factory method of the test name.</param>
        /// <returns>The <see cref="AtataContextBuilder"/> instance.</returns>
        public AtataContextBuilder UseTestName(Func<string> testNameFactory)
        {
            testNameFactory.CheckNotNull(nameof(testNameFactory));

            BuildingContext.TestNameFactory = testNameFactory;
            return this;
        }

        /// <summary>
        /// Sets the name of the test.
        /// </summary>
        /// <param name="testName">The name of the test.</param>
        /// <returns>The <see cref="AtataContextBuilder"/> instance.</returns>
        public AtataContextBuilder UseTestName(string testName)
        {
            BuildingContext.TestNameFactory = () => testName;
            return this;
        }

        /// <summary>
        /// Sets the factory method of the test suite name.
        /// </summary>
        /// <param name="testSuiteNameFactory">The factory method of the test suite name.</param>
        /// <returns>The <see cref="AtataContextBuilder"/> instance.</returns>
        public AtataContextBuilder UseTestSuiteName(Func<string> testSuiteNameFactory)
        {
            testSuiteNameFactory.CheckNotNull(nameof(testSuiteNameFactory));

            BuildingContext.TestSuiteNameFactory = testSuiteNameFactory;
            return this;
        }

        /// <summary>
        /// Sets the name of the test suite (fixture/class).
        /// </summary>
        /// <param name="testSuiteName">The name of the test suite.</param>
        /// <returns>The <see cref="AtataContextBuilder"/> instance.</returns>
        public AtataContextBuilder UseTestSuiteName(string testSuiteName)
        {
            BuildingContext.TestSuiteNameFactory = () => testSuiteName;
            return this;
        }

        /// <summary>
        /// Sets the factory method of the test suite (fixture/class) type.
        /// </summary>
        /// <param name="testSuiteTypeFactory">The factory method of the test suite type.</param>
        /// <returns>The <see cref="AtataContextBuilder"/> instance.</returns>
        public AtataContextBuilder UseTestSuiteType(Func<Type> testSuiteTypeFactory)
        {
            testSuiteTypeFactory.CheckNotNull(nameof(testSuiteTypeFactory));

            BuildingContext.TestSuiteTypeFactory = testSuiteTypeFactory;
            return this;
        }

        /// <summary>
        /// Sets the type of the test suite (fixture/class).
        /// </summary>
        /// <param name="testSuiteType">The type of the test suite.</param>
        /// <returns>The <see cref="AtataContextBuilder"/> instance.</returns>
        public AtataContextBuilder UseTestSuiteType(Type testSuiteType)
        {
            testSuiteType.CheckNotNull(nameof(testSuiteType));

            BuildingContext.TestSuiteTypeFactory = () => testSuiteType;
            return this;
        }

        /// <summary>
        /// Sets the UTC time zone.
        /// </summary>
        /// <returns>The <see cref="AtataContextBuilder"/> instance.</returns>
        public AtataContextBuilder UseUtcTimeZone() =>
            UseTimeZone(TimeZoneInfo.Utc);

        /// <summary>
        /// Sets the time zone by identifier, which corresponds to the <see cref="TimeZoneInfo.Id"/> property.
        /// </summary>
        /// <param name="timeZoneId">The time zone identifier.</param>
        /// <returns>The <see cref="AtataContextBuilder"/> instance.</returns>
        public AtataContextBuilder UseTimeZone(string timeZoneId)
        {
            timeZoneId.CheckNotNullOrWhitespace(nameof(timeZoneId));
            TimeZoneInfo timeZone = TimeZoneInfo.FindSystemTimeZoneById(timeZoneId);

            return UseTimeZone(timeZone);
        }

        /// <summary>
        /// Sets the time zone.
        /// </summary>
        /// <param name="timeZone">The time zone.</param>
        /// <returns>The <see cref="AtataContextBuilder"/> instance.</returns>
        public AtataContextBuilder UseTimeZone(TimeZoneInfo timeZone)
        {
            timeZone.CheckNotNull(nameof(timeZone));

            BuildingContext.TimeZone = timeZone;
            return this;
        }

        /// <summary>
        /// Sets the base URL.
        /// </summary>
        /// <param name="baseUrl">The base URL.</param>
        /// <returns>The <see cref="AtataContextBuilder"/> instance.</returns>
        public AtataContextBuilder UseBaseUrl(string baseUrl)
        {
            if (baseUrl != null && !Uri.IsWellFormedUriString(baseUrl, UriKind.Absolute))
                throw new ArgumentException($"Invalid URL format \"{baseUrl}\".", nameof(baseUrl));

            BuildingContext.BaseUrl = baseUrl;
            return this;
        }

        /// <summary>
        /// Sets the base retry timeout.
        /// The default value is <c>5</c> seconds.
        /// </summary>
        /// <param name="timeout">The retry timeout.</param>
        /// <returns>The <see cref="AtataContextBuilder"/> instance.</returns>
        public AtataContextBuilder UseBaseRetryTimeout(TimeSpan timeout)
        {
            BuildingContext.BaseRetryTimeout = timeout;
            return this;
        }

        /// <summary>
        /// Sets the base retry interval.
        /// The default value is <c>500</c> milliseconds.
        /// </summary>
        /// <param name="interval">The retry interval.</param>
        /// <returns>The <see cref="AtataContextBuilder"/> instance.</returns>
        public AtataContextBuilder UseBaseRetryInterval(TimeSpan interval)
        {
            BuildingContext.BaseRetryInterval = interval;
            return this;
        }

        /// <summary>
        /// Sets the element find timeout.
        /// The default value is taken from <see cref="AtataBuildingContext.BaseRetryTimeout"/>, which is equal to <c>5</c> seconds by default.
        /// </summary>
        /// <param name="timeout">The retry timeout.</param>
        /// <returns>The <see cref="AtataContextBuilder"/> instance.</returns>
        public AtataContextBuilder UseElementFindTimeout(TimeSpan timeout)
        {
            BuildingContext.ElementFindTimeout = timeout;
            return this;
        }

        /// <summary>
        /// Sets the element find retry interval.
        /// The default value is taken from <see cref="AtataBuildingContext.BaseRetryInterval"/>, which is equal to <c>500</c> milliseconds by default.
        /// </summary>
        /// <param name="interval">The retry interval.</param>
        /// <returns>The <see cref="AtataContextBuilder"/> instance.</returns>
        public AtataContextBuilder UseElementFindRetryInterval(TimeSpan interval)
        {
            BuildingContext.ElementFindRetryInterval = interval;
            return this;
        }

        /// <summary>
        /// Sets the waiting timeout.
        /// The default value is taken from <see cref="AtataBuildingContext.BaseRetryTimeout"/>, which is equal to <c>5</c> seconds by default.
        /// </summary>
        /// <param name="timeout">The retry timeout.</param>
        /// <returns>The <see cref="AtataContextBuilder"/> instance.</returns>
        public AtataContextBuilder UseWaitingTimeout(TimeSpan timeout)
        {
            BuildingContext.WaitingTimeout = timeout;
            return this;
        }

        /// <summary>
        /// Sets the waiting retry interval.
        /// The default value is taken from <see cref="AtataBuildingContext.BaseRetryInterval"/>, which is equal to <c>500</c> milliseconds by default.
        /// </summary>
        /// <param name="interval">The retry interval.</param>
        /// <returns>The <see cref="AtataContextBuilder"/> instance.</returns>
        public AtataContextBuilder UseWaitingRetryInterval(TimeSpan interval)
        {
            BuildingContext.WaitingRetryInterval = interval;
            return this;
        }

        /// <summary>
        /// Sets the verification timeout.
        /// The default value is taken from <see cref="AtataBuildingContext.BaseRetryTimeout"/>, which is equal to <c>5</c> seconds by default.
        /// </summary>
        /// <param name="timeout">The retry timeout.</param>
        /// <returns>The <see cref="AtataContextBuilder"/> instance.</returns>
        public AtataContextBuilder UseVerificationTimeout(TimeSpan timeout)
        {
            BuildingContext.VerificationTimeout = timeout;
            return this;
        }

        /// <summary>
        /// Sets the verification retry interval.
        /// The default value is taken from <see cref="AtataBuildingContext.BaseRetryInterval"/>, which is equal to <c>500</c> milliseconds by default.
        /// </summary>
        /// <param name="interval">The retry interval.</param>
        /// <returns>The <see cref="AtataContextBuilder"/> instance.</returns>
        public AtataContextBuilder UseVerificationRetryInterval(TimeSpan interval)
        {
            BuildingContext.VerificationRetryInterval = interval;
            return this;
        }

        /// <summary>
        /// Sets the default control visibility.
        /// The default value is <see cref="Visibility.Any"/>.
        /// </summary>
        /// <param name="visibility">The visibility.</param>
        /// <returns>The <see cref="AtataContextBuilder"/> instance.</returns>
        public AtataContextBuilder UseDefaultControlVisibility(Visibility visibility)
        {
            BuildingContext.DefaultControlVisibility = visibility;
            return this;
        }

        /// <summary>
        /// Sets the culture.
        /// The default value is <see cref="CultureInfo.CurrentCulture"/>.
        /// </summary>
        /// <param name="culture">The culture.</param>
        /// <returns>The <see cref="AtataContextBuilder"/> instance.</returns>
        public AtataContextBuilder UseCulture(CultureInfo culture)
        {
            BuildingContext.Culture = culture;
            return this;
        }

        /// <summary>
        /// Sets the culture by the name.
        /// The default value is <see cref="CultureInfo.CurrentCulture"/>.
        /// </summary>
        /// <param name="cultureName">The name of the culture.</param>
        /// <returns>The <see cref="AtataContextBuilder"/> instance.</returns>
        public AtataContextBuilder UseCulture(string cultureName) =>
            UseCulture(CultureInfo.GetCultureInfo(cultureName));

        /// <summary>
        /// Sets the type of the assertion exception.
        /// The default value is a type of <see cref="AssertionException"/>.
        /// </summary>
        /// <typeparam name="TException">The type of the exception.</typeparam>
        /// <returns>The <see cref="AtataContextBuilder"/> instance.</returns>
        public AtataContextBuilder UseAssertionExceptionType<TException>()
            where TException : Exception
            =>
            UseAssertionExceptionType(typeof(TException));

        /// <summary>
        /// Sets the type of the assertion exception.
        /// The default value is a type of <see cref="AssertionException"/>.
        /// </summary>
        /// <param name="exceptionType">The type of the exception.</param>
        /// <returns>The <see cref="AtataContextBuilder"/> instance.</returns>
        public AtataContextBuilder UseAssertionExceptionType(Type exceptionType)
        {
            exceptionType.CheckIs<Exception>(nameof(exceptionType));

            BuildingContext.AssertionExceptionType = exceptionType;
            return this;
        }

        /// <summary>
        /// Sets the type of aggregate assertion exception.
        /// The default value is a type of <see cref="AggregateAssertionException"/>.
        /// The exception type should have public constructor with <c>IEnumerable&lt;AssertionResult&gt;</c> argument.
        /// </summary>
        /// <typeparam name="TException">The type of the exception.</typeparam>
        /// <returns>The <see cref="AtataContextBuilder"/> instance.</returns>
        public AtataContextBuilder UseAggregateAssertionExceptionType<TException>()
            where TException : Exception
            =>
            UseAggregateAssertionExceptionType(typeof(TException));

        /// <summary>
        /// Sets the type of aggregate assertion exception.
        /// The default value is a type of <see cref="AggregateAssertionException"/>.
        /// The exception type should have public constructor with <c>IEnumerable&lt;AssertionResult&gt;</c> argument.
        /// </summary>
        /// <param name="exceptionType">The type of the exception.</param>
        /// <returns>The <see cref="AtataContextBuilder"/> instance.</returns>
        public AtataContextBuilder UseAggregateAssertionExceptionType(Type exceptionType)
        {
            exceptionType.CheckIs<Exception>(nameof(exceptionType));

            BuildingContext.AggregateAssertionExceptionType = exceptionType;
            return this;
        }

        /// <summary>
        /// Sets the default assembly name pattern that is used to filter assemblies to find types in them.
        /// Modifies the <see cref="AtataBuildingContext.DefaultAssemblyNamePatternToFindTypes"/> property value of <see cref="BuildingContext"/>.
        /// </summary>
        /// <param name="pattern">The pattern.</param>
        /// <returns>The <see cref="AtataContextBuilder"/> instance.</returns>
        public AtataContextBuilder UseDefaultAssemblyNamePatternToFindTypes(string pattern)
        {
            pattern.CheckNotNullOrWhitespace(nameof(pattern));

            BuildingContext.DefaultAssemblyNamePatternToFindTypes = pattern;
            return this;
        }

        /// <summary>
        /// Sets the assembly name pattern that is used to filter assemblies to find component types in them.
        /// Modifies the <see cref="AtataBuildingContext.AssemblyNamePatternToFindComponentTypes"/> property value of <see cref="BuildingContext"/>.
        /// </summary>
        /// <param name="pattern">The pattern.</param>
        /// <returns>The <see cref="AtataContextBuilder"/> instance.</returns>
        public AtataContextBuilder UseAssemblyNamePatternToFindComponentTypes(string pattern)
        {
            pattern.CheckNotNullOrWhitespace(nameof(pattern));

            BuildingContext.AssemblyNamePatternToFindComponentTypes = pattern;
            return this;
        }

        /// <summary>
        /// Sets the assembly name pattern that is used to filter assemblies to find attribute types in them.
        /// Modifies the <see cref="AtataBuildingContext.AssemblyNamePatternToFindAttributeTypes"/> property value of <see cref="BuildingContext"/>.
        /// </summary>
        /// <param name="pattern">The pattern.</param>
        /// <returns>The <see cref="AtataContextBuilder"/> instance.</returns>
        public AtataContextBuilder UseAssemblyNamePatternToFindAttributeTypes(string pattern)
        {
            pattern.CheckNotNullOrWhitespace(nameof(pattern));

            BuildingContext.AssemblyNamePatternToFindAttributeTypes = pattern;
            return this;
        }

        /// <summary>
        /// Sets the assembly name pattern that is used to filter assemblies to find event types in them.
        /// Modifies the <see cref="AtataBuildingContext.AssemblyNamePatternToFindEventTypes"/> property value of <see cref="BuildingContext"/>.
        /// </summary>
        /// <param name="pattern">The pattern.</param>
        /// <returns>The <see cref="AtataContextBuilder"/> instance.</returns>
        public AtataContextBuilder UseAssemblyNamePatternToFindEventTypes(string pattern)
        {
            pattern.CheckNotNullOrWhitespace(nameof(pattern));

            BuildingContext.AssemblyNamePatternToFindEventTypes = pattern;
            return this;
        }

        /// <summary>
        /// Sets the assembly name pattern that is used to filter assemblies to find event handler types in them.
        /// Modifies the <see cref="AtataBuildingContext.AssemblyNamePatternToFindEventHandlerTypes"/> property value of <see cref="BuildingContext"/>.
        /// </summary>
        /// <param name="pattern">The pattern.</param>
        /// <returns>The <see cref="AtataContextBuilder"/> instance.</returns>
        public AtataContextBuilder UseAssemblyNamePatternToFindEventHandlerTypes(string pattern)
        {
            pattern.CheckNotNullOrWhitespace(nameof(pattern));

            BuildingContext.AssemblyNamePatternToFindEventHandlerTypes = pattern;
            return this;
        }

        /// <summary>
        /// Sets the path to the Artifacts directory.
        /// </summary>
        /// <param name="directoryPath">The directory path.</param>
        /// <returns>The <see cref="AtataContextBuilder"/> instance.</returns>
        public AtataContextBuilder UseArtifactsPath(string directoryPath)
        {
            directoryPath.CheckNotNullOrWhitespace(nameof(directoryPath));

            return UseArtifactsPath(_ => directoryPath);
        }

        /// <summary>
        /// Sets the builder of the path to the Artifacts directory.
        /// </summary>
        /// <param name="directoryPathBuilder">The directory path builder.</param>
        /// <returns>The <see cref="AtataContextBuilder"/> instance.</returns>
        public AtataContextBuilder UseArtifactsPath(Func<AtataContext, string> directoryPathBuilder)
        {
            directoryPathBuilder.CheckNotNull(nameof(directoryPathBuilder));

            BuildingContext.ArtifactsPathBuilder = directoryPathBuilder;
            return this;
        }

        /// <summary>
        /// Sets the default Artifacts path with optionally including <c>"{build-start:yyyyMMddTHHmmss}"</c> folder in the path.
        /// </summary>
        /// <param name="include">Whether to include the <c>"{build-start:yyyyMMddTHHmmss}"</c> folder in the path.</param>
        /// <returns>The <see cref="AtataContextBuilder"/> instance.</returns>
        public AtataContextBuilder UseDefaultArtifactsPathIncludingBuildStart(bool include) =>
            UseArtifactsPath(include
                ? AtataBuildingContext.DefaultArtifactsPath
                : AtataBuildingContext.DefaultArtifactsPathWithoutBuildStartFolder);

        /// <summary>
        /// Defines that the name of the test should be taken from the NUnit test.
        /// </summary>
        /// <returns>The <see cref="AtataContextBuilder"/> instance.</returns>
        public AtataContextBuilder UseNUnitTestName() =>
            UseTestName(NUnitAdapter.GetCurrentTestName);

        /// <summary>
        /// Defines that the name of the test suite should be taken from the NUnit test fixture.
        /// </summary>
        /// <returns>The <see cref="AtataContextBuilder"/> instance.</returns>
        public AtataContextBuilder UseNUnitTestSuiteName() =>
            UseTestSuiteName(NUnitAdapter.GetCurrentTestFixtureName);

        /// <summary>
        /// Defines that the type of the test suite should be taken from the NUnit test fixture.
        /// </summary>
        /// <returns>The <see cref="AtataContextBuilder"/> instance.</returns>
        public AtataContextBuilder UseNUnitTestSuiteType() =>
            UseTestSuiteType(NUnitAdapter.GetCurrentTestFixtureType);

        /// <summary>
        /// Sets <see cref="NUnitAggregateAssertionStrategy"/> as the aggregate assertion strategy.
        /// The <see cref="NUnitAggregateAssertionStrategy"/> uses NUnit's <c>Assert.Multiple</c> method for aggregate assertion.
        /// </summary>
        /// <returns>The <see cref="AtataContextBuilder"/> instance.</returns>
        public AtataContextBuilder UseNUnitAggregateAssertionStrategy() =>
            UseAggregateAssertionStrategy(new NUnitAggregateAssertionStrategy());

        /// <summary>
        /// Sets the aggregate assertion strategy.
        /// </summary>
        /// <typeparam name="TAggregateAssertionStrategy">The type of the aggregate assertion strategy.</typeparam>
        /// <returns>The <see cref="AtataContextBuilder"/> instance.</returns>
        public AtataContextBuilder UseAggregateAssertionStrategy<TAggregateAssertionStrategy>()
            where TAggregateAssertionStrategy : IAggregateAssertionStrategy, new()
        {
            IAggregateAssertionStrategy strategy = Activator.CreateInstance<TAggregateAssertionStrategy>();

            return UseAggregateAssertionStrategy(strategy);
        }

        /// <summary>
        /// Sets the aggregate assertion strategy.
        /// </summary>
        /// <param name="strategy">The aggregate assertion strategy.</param>
        /// <returns>The <see cref="AtataContextBuilder"/> instance.</returns>
        public AtataContextBuilder UseAggregateAssertionStrategy(IAggregateAssertionStrategy strategy)
        {
            BuildingContext.AggregateAssertionStrategy = strategy;

            return this;
        }

        /// <summary>
        /// Sets <see cref="NUnitWarningReportStrategy"/> as the strategy for warning assertion reporting.
        /// </summary>
        /// <returns>The <see cref="AtataContextBuilder"/> instance.</returns>
        public AtataContextBuilder UseNUnitWarningReportStrategy() =>
            UseWarningReportStrategy(new NUnitWarningReportStrategy());

        /// <summary>
        /// Sets the strategy for warning assertion reporting.
        /// </summary>
        /// <param name="strategy">The warning report strategy.</param>
        /// <returns>The <see cref="AtataContextBuilder"/> instance.</returns>
        public AtataContextBuilder UseWarningReportStrategy(IWarningReportStrategy strategy)
        {
            BuildingContext.WarningReportStrategy = strategy;

            return this;
        }

        /// <summary>
        /// Defines that an error occurred during the NUnit test execution should be added to the log during the cleanup.
        /// </summary>
        /// <returns>The <see cref="AtataContextBuilder"/> instance.</returns>
        public AtataContextBuilder LogNUnitError() =>
            EventSubscriptions.Add(new LogNUnitErrorOnCleanUpEventHandler());

        /// <summary>
        /// Defines that an error occurred during the NUnit test execution should be captured by a screenshot during the cleanup.
        /// </summary>
        /// <param name="title">The screenshot title.</param>
        /// <returns>The <see cref="AtataContextBuilder"/> instance.</returns>
        public AtataContextBuilder TakeScreenshotOnNUnitError(string title = "Failed") =>
            EventSubscriptions.Add(new TakeScreenshotOnNUnitErrorOnCleanUpEventHandler(title));

        /// <summary>
        /// Defines that on <see cref="AtataContext"/> clean-up the files stored in Artifacts directory
        /// should be added to NUnit <c>TestContext</c>.
        /// </summary>
        /// <returns>The <see cref="AtataContextBuilder"/> instance.</returns>
        public AtataContextBuilder OnCleanUpAddArtifactsToNUnitTestContext() =>
            EventSubscriptions.Add(new AddArtifactsToNUnitTestContextOnCleanUpEventHandler());

        /// <summary>
        /// Defines that on <see cref="AtataContext" /> clean-up the files stored in the directory
        /// specified by <paramref name="directoryPath"/> should be added to NUnit <c>TestContext</c>.
        /// Directory path supports template variables.
        /// </summary>
        /// <param name="directoryPath">The directory path.</param>
        /// <returns>The <see cref="AtataContextBuilder" /> instance.</returns>
        public AtataContextBuilder OnCleanUpAddDirectoryFilesToNUnitTestContext(string directoryPath)
        {
            directoryPath.CheckNotNull(nameof(directoryPath));
            return OnCleanUpAddDirectoryFilesToNUnitTestContext(_ => directoryPath);
        }

        /// <inheritdoc cref="OnCleanUpAddDirectoryFilesToNUnitTestContext(Func{AtataContext, string})"/>
        public AtataContextBuilder OnCleanUpAddDirectoryFilesToNUnitTestContext(Func<string> directoryPathBuilder)
        {
            directoryPathBuilder.CheckNotNull(nameof(directoryPathBuilder));
            return OnCleanUpAddDirectoryFilesToNUnitTestContext(_ => directoryPathBuilder.Invoke());
        }

        /// <summary>
        /// Defines that on <see cref="AtataContext" /> clean-up the files stored in the directory
        /// specified by <paramref name="directoryPathBuilder"/> should be added to NUnit <c>TestContext</c>.
        /// Directory path supports template variables.
        /// </summary>
        /// <param name="directoryPathBuilder">The directory path builder.</param>
        /// <returns>The <see cref="AtataContextBuilder" /> instance.</returns>
        public AtataContextBuilder OnCleanUpAddDirectoryFilesToNUnitTestContext(Func<AtataContext, string> directoryPathBuilder)
        {
            directoryPathBuilder.CheckNotNull(nameof(directoryPathBuilder));
            return EventSubscriptions.Add(
                new AddDirectoryFilesToNUnitTestContextOnCleanUpEventHandler(directoryPathBuilder));
        }

        /// <summary>
        /// Sets the type of <c>NUnit.Framework.AssertionException</c> as the assertion exception type.
        /// The default value is a type of <see cref="AssertionException"/>.
        /// </summary>
        /// <returns>The <see cref="AtataContextBuilder"/> instance.</returns>
        public AtataContextBuilder UseNUnitAssertionExceptionType() =>
            UseAssertionExceptionType(NUnitAdapter.AssertionExceptionType);

        /// <summary>
        /// Enables all NUnit features for Atata.
        /// Executes the following methods:
        /// <list type="bullet">
        /// <item><see cref="UseNUnitTestName"/>,</item>
        /// <item><see cref="UseNUnitTestSuiteName"/>,</item>
        /// <item><see cref="UseNUnitTestSuiteType"/>,</item>
        /// <item><see cref="UseNUnitAssertionExceptionType"/>,</item>
        /// <item><see cref="UseNUnitAggregateAssertionStrategy"/>,</item>
        /// <item><see cref="UseNUnitWarningReportStrategy"/>,</item>
        /// <item><see cref="LogConsumersAtataContextBuilder.AddNUnitTestContext"/>,</item>
        /// <item><see cref="LogNUnitError"/>,</item>
        /// <item><see cref="TakeScreenshotOnNUnitError(string)"/>,</item>
        /// <item><see cref="OnCleanUpAddArtifactsToNUnitTestContext"/>.</item>
        /// </list>
        /// </summary>
        /// <returns>The <see cref="AtataContextBuilder"/> instance.</returns>
        public AtataContextBuilder UseAllNUnitFeatures() =>
            UseNUnitTestName()
                .UseNUnitTestSuiteName()
                .UseNUnitTestSuiteType()
                .UseNUnitAssertionExceptionType()
                .UseNUnitAggregateAssertionStrategy()
                .UseNUnitWarningReportStrategy()
                .LogConsumers.AddNUnitTestContext()
                .LogNUnitError()
                .TakeScreenshotOnNUnitError()
                .OnCleanUpAddArtifactsToNUnitTestContext();

        private DirectorySubject CreateArtifactsDirectorySubject(AtataContext context)
        {
            string pathTemplate = BuildingContext.ArtifactsPathBuilder.Invoke(context);

            string path = context.FillTemplateString(pathTemplate);

            return new DirectorySubject(path, "Artifacts");
        }

        /// <summary>
        /// Clears the <see cref="BuildingContext"/>.
        /// </summary>
        /// <returns>The <see cref="AtataContextBuilder"/> instance.</returns>
        public AtataContextBuilder Clear()
        {
            BuildingContext = new AtataBuildingContext();
            return this;
        }

        /// <summary>
        /// Builds the <see cref="AtataContext" /> instance and sets it to <see cref="AtataContext.Current" /> property.
        /// </summary>
        /// <returns>The created <see cref="AtataContext"/> instance.</returns>
        public AtataContext Build()
        {
            ValidateBuildingContextBeforeBuild();

            AtataContext context = new AtataContext();
            LogManager logManager = CreateLogManager(context);

            IObjectConverter objectConverter = new ObjectConverter
            {
                AssemblyNamePatternToFindTypes = BuildingContext.DefaultAssemblyNamePatternToFindTypes
            };

            IObjectMapper objectMapper = new ObjectMapper(objectConverter);
            IObjectCreator objectCreator = new ObjectCreator(objectConverter, objectMapper);

            context.TestName = BuildingContext.TestNameFactory?.Invoke();
            context.TestSuiteName = BuildingContext.TestSuiteNameFactory?.Invoke();
            context.TestSuiteType = BuildingContext.TestSuiteTypeFactory?.Invoke();
            context.TimeZone = BuildingContext.TimeZone;
            context.BaseUrl = BuildingContext.BaseUrl;
            context.Log = logManager;
            context.Attributes = BuildingContext.Attributes.Clone();
            context.BaseRetryTimeout = BuildingContext.BaseRetryTimeout;
            context.BaseRetryInterval = BuildingContext.BaseRetryInterval;
            context.ElementFindTimeout = BuildingContext.ElementFindTimeout;
            context.ElementFindRetryInterval = BuildingContext.ElementFindRetryInterval;
            context.WaitingTimeout = BuildingContext.WaitingTimeout;
            context.WaitingRetryInterval = BuildingContext.WaitingRetryInterval;
            context.VerificationTimeout = BuildingContext.VerificationTimeout;
            context.VerificationRetryInterval = BuildingContext.VerificationRetryInterval;
            context.DefaultControlVisibility = BuildingContext.DefaultControlVisibility;
            context.Culture = BuildingContext.Culture ?? CultureInfo.CurrentCulture;
            context.AssertionExceptionType = BuildingContext.AssertionExceptionType;
            context.AggregateAssertionExceptionType = BuildingContext.AggregateAssertionExceptionType;
            context.AggregateAssertionStrategy = BuildingContext.AggregateAssertionStrategy ?? new AtataAggregateAssertionStrategy();
            context.WarningReportStrategy = BuildingContext.WarningReportStrategy ?? new AtataWarningReportStrategy();
            context.ObjectConverter = objectConverter;
            context.ObjectMapper = objectMapper;
            context.ObjectCreator = objectCreator;
            context.EventBus = new EventBus(context, BuildingContext.EventSubscriptions);

            if (context.TestSuiteName is null && context.TestSuiteType != null)
                context.TestSuiteName = context.TestSuiteType.Name;

            context.DriverFactory = BuildingContext.DriverFactoryToUse
                ?? BuildingContext.DriverFactories.LastOrDefault();
            context.DriverAlias = context.DriverFactory?.Alias;
            context.DriverInitializationStage = BuildingContext.DriverInitializationStage;

            context.InitDateTimeProperties();
            context.InitMainVariables();
            context.InitCustomVariables(BuildingContext.Variables);
            context.Artifacts = CreateArtifactsDirectorySubject(context);
            context.InitArtifactsVariable();

            AtataContext.Current = context;

            context.EventBus.Publish(new AtataContextPreInitEvent(context));

            context.LogTestStart();

            context.Log.ExecuteSection(
                new LogSection("Set up AtataContext", LogLevel.Trace),
                () => SetUp(context));

            context.PureExecutionStopwatch.Start();

            return context;
        }

        private LogManager CreateLogManager(AtataContext context)
        {
            LogManager logManager = new LogManager(
                new AtataContextLogEventInfoFactory(context));

            logManager.AddSecretStringsToMask(BuildingContext.SecretStringsToMaskInLog);

            foreach (var logConsumerItem in BuildingContext.LogConsumerConfigurations)
                logManager.Use(logConsumerItem);

            foreach (var screenshotConsumer in BuildingContext.ScreenshotConsumers)
                logManager.Use(screenshotConsumer);

            return logManager;
        }

        private void SetUp(AtataContext context)
        {
            context.EventBus.Publish(new AtataContextInitStartedEvent(context));

            if (context.BaseUrl != null)
                context.Log.Trace($"Set: BaseUrl={context.BaseUrl}");

            LogRetrySettings(context);

            if (BuildingContext.Culture != null)
                ApplyCulture(context, BuildingContext.Culture);

            context.Log.Trace($"Set: Artifacts={context.Artifacts.FullName.Value}");

            if (context.DriverInitializationStage == AtataContextDriverInitializationStage.Build)
                context.InitDriver();

            if (context.DriverInitializationStage != AtataContextDriverInitializationStage.None)
            {
                string driverTypeName = context.DriverInitializationStage == AtataContextDriverInitializationStage.OnDemand
                    ? "{on demand}"
                    : context.Driver.GetType().Name;

                context.Log.Trace($"Set: Driver={driverTypeName}{context.DriverFactory?.Alias?.ToFormattedString(" (alias={0})")}");
            }

            context.EventBus.Publish(new AtataContextInitCompletedEvent(context));
        }

        private static void LogRetrySettings(AtataContext context)
        {
            string messageFormat = "Set: {0}Timeout={1}; {0}RetryInterval={2}";

            context.Log.Trace(messageFormat, "ElementFind", context.ElementFindTimeout.ToShortIntervalString(), context.ElementFindRetryInterval.ToShortIntervalString());
            context.Log.Trace(messageFormat, "Waiting", context.WaitingTimeout.ToShortIntervalString(), context.WaitingRetryInterval.ToShortIntervalString());
            context.Log.Trace(messageFormat, "Verification", context.VerificationTimeout.ToShortIntervalString(), context.VerificationRetryInterval.ToShortIntervalString());
        }

        private static void ApplyCulture(AtataContext context, CultureInfo culture)
        {
            Thread.CurrentThread.CurrentCulture = Thread.CurrentThread.CurrentUICulture = culture;

            if (AtataContext.ModeOfCurrent == AtataContextModeOfCurrent.Static)
                CultureInfo.DefaultThreadCurrentCulture = CultureInfo.DefaultThreadCurrentUICulture = culture;

            context.Log.Trace($"Set: Culture={culture.Name}");
        }

        private void ValidateBuildingContextBeforeBuild()
        {
            if (BuildingContext.DriverInitializationStage == AtataContextDriverInitializationStage.Build
                && BuildingContext.DriverFactoryToUse == null
                && BuildingContext.DriverFactories.Count == 0)
            {
                throw new InvalidOperationException(
                    $"Cannot build {nameof(AtataContext)} as no driver is specified. " +
                    $"Use one of \"Use*\" methods to specify the driver to use, e.g.:AtataContext.Configure().UseChrome().Build();");
            }
        }

        protected internal IObjectMapper CreateObjectMapper()
        {
            IObjectConverter objectConverter = new ObjectConverter
            {
                AssemblyNamePatternToFindTypes = BuildingContext.DefaultAssemblyNamePatternToFindTypes
            };

            return new ObjectMapper(objectConverter);
        }

        /// <summary>
        /// <para>
        /// Sets up driver with auto version detection for the local browser to use.
        /// Gets the name of the local browser to use from <see cref="AtataBuildingContext.LocalBrowserToUseName"/> property.
        /// Then invokes <c>Atata.WebDriverSetup.DriverSetup.AutoSetUpSafely(...)</c> static method
        /// from <c>Atata.WebDriverSetup</c> package.
        /// </para>
        /// <para>
        /// In order to use this method,
        /// ensure that <c>Atata.WebDriverSetup</c> package is installed.
        /// </para>
        /// </summary>
        public void AutoSetUpDriverToUse()
        {
            if (BuildingContext.UsesLocalBrowser)
                InvokeAutoSetUpSafelyMethodOfDriverSetup(new[] { BuildingContext.LocalBrowserToUseName });
        }

        /// <inheritdoc cref="AutoSetUpDriverToUse"/>
        /// <returns>The task object representing the asynchronous operation.</returns>
        public async Task AutoSetUpDriverToUseAsync() =>
            await Task.Run(AutoSetUpDriverToUse);

        /// <summary>
        /// <para>
        /// Sets up drivers with auto version detection for the local configured browsers.
        /// Gets the names of configured local browsers from <see cref="AtataBuildingContext.ConfiguredLocalBrowserNames"/> property.
        /// Then invokes <c>Atata.WebDriverSetup.DriverSetup.AutoSetUpSafely(...)</c> static method
        /// from <c>Atata.WebDriverSetup</c> package.
        /// </para>
        /// <para>
        /// In order to use this method,
        /// ensure that <c>Atata.WebDriverSetup</c> package is installed.
        /// </para>
        /// </summary>
        public void AutoSetUpConfiguredDrivers() =>
            InvokeAutoSetUpSafelyMethodOfDriverSetup(BuildingContext.ConfiguredLocalBrowserNames);

        /// <inheritdoc cref="AutoSetUpConfiguredDrivers"/>
        /// <returns>The task object representing the asynchronous operation.</returns>
        public async Task AutoSetUpConfiguredDriversAsync() =>
            await Task.Run(AutoSetUpConfiguredDrivers);

        private static void InvokeAutoSetUpSafelyMethodOfDriverSetup(IEnumerable<string> browserNames)
        {
            Type driverSetupType = Type.GetType("Atata.WebDriverSetup.DriverSetup,Atata.WebDriverSetup", true);

            var setUpMethod = driverSetupType.GetMethodWithThrowOnError(
                "AutoSetUpSafely",
                BindingFlags.Public | BindingFlags.Static);

            setUpMethod.InvokeStaticAsLambda(browserNames);
        }
    }
}
