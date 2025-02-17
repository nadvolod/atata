﻿using System;
using OpenQA.Selenium;

namespace Atata
{
    public class CustomDriverAtataContextBuilder : DriverAtataContextBuilder<CustomDriverAtataContextBuilder>
    {
        private readonly Func<IWebDriver> _driverFactory;

        public CustomDriverAtataContextBuilder(AtataBuildingContext buildingContext, Func<IWebDriver> driverFactory)
            : base(buildingContext) =>
            _driverFactory = driverFactory.CheckNotNull(nameof(driverFactory));

        protected override IWebDriver CreateDriver() =>
            _driverFactory.Invoke();
    }
}
