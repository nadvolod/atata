﻿using System;
using System.Collections.Generic;

namespace Atata
{
    /// <summary>
    /// Allows to access the component scope element's attribute values.
    /// </summary>
    /// <typeparam name="TOwner">The type of the owner page object.</typeparam>
    public class UIComponentAttributeProvider<TOwner> : UIComponentPart<TOwner>
        where TOwner : PageObject<TOwner>
    {
        private const string AttributeProviderNameFormat = "{0} attribute";

        public ValueProvider<string, TOwner> Id => Get<string>(nameof(Id));

        public ValueProvider<string, TOwner> Name => Get<string>(nameof(Name));

        public ValueProvider<string, TOwner> Value => Get<string>(nameof(Value));

        public ValueProvider<string, TOwner> Title => Get<string>(nameof(Title));

        public ValueProvider<string, TOwner> Href => Get<string>(nameof(Href));

        public ValueProvider<string, TOwner> For => Get<string>(nameof(For));

        public ValueProvider<string, TOwner> Type => Get<string>(nameof(Type));

        public ValueProvider<string, TOwner> Style => Get<string>(nameof(Style));

        public ValueProvider<string, TOwner> Alt => Get<string>(nameof(Alt));

        public ValueProvider<string, TOwner> Placeholder => Get<string>(nameof(Placeholder));

        public ValueProvider<string, TOwner> Target => Get<string>(nameof(Target));

        public ValueProvider<string, TOwner> Pattern => Get<string>(nameof(Pattern));

        public ValueProvider<string, TOwner> Accept => Get<string>(nameof(Accept));

        public ValueProvider<string, TOwner> Src => Get<string>(nameof(Src));

        public ValueProvider<string, TOwner> TextContent => Component.CreateValueProvider(
            AttributeProviderNameFormat.FormatWith("textContent"),
            () => GetValue("textContent")?.Trim());

        public ValueProvider<string, TOwner> InnerHtml => Component.CreateValueProvider(
            AttributeProviderNameFormat.FormatWith("innerHTML"),
            () => GetValue("innerHTML")?.Trim());

        public ValueProvider<bool, TOwner> Disabled => Get<bool>(nameof(Disabled));

        public ValueProvider<bool, TOwner> ReadOnly => Get<bool>(nameof(ReadOnly));

        public ValueProvider<bool, TOwner> Checked => Get<bool>(nameof(Checked));

        public ValueProvider<bool, TOwner> Required => Get<bool>(nameof(Required));

        public ValueProvider<IEnumerable<string>, TOwner> Class => Component.CreateValueProvider<IEnumerable<string>>(
            "class attribute",
            () => GetValue("class").Split(new[] { " " }, StringSplitOptions.RemoveEmptyEntries));

        /// <summary>
        /// Gets the <see cref="ValueProvider{TValue, TOwner}"/> instance for the value of the specified control's scope element attribute.
        /// </summary>
        /// <param name="attributeName">The name of the attribute.</param>
        /// <returns>The <see cref="ValueProvider{TValue, TOwner}"/> instance for the attribute's current value.</returns>
        public ValueProvider<string, TOwner> this[string attributeName] =>
            Get<string>(attributeName);

        /// <summary>
        /// Gets the <see cref="ValueProvider{TValue, TOwner}"/> instance for the value of the specified control's scope element attribute.
        /// </summary>
        /// <typeparam name="TValue">The type of the attribute value.</typeparam>
        /// <param name="attributeName">The name of the attribute.</param>
        /// <returns>The <see cref="ValueProvider{TValue, TOwner}"/> instance for the attribute's current value.</returns>
        public ValueProvider<TValue, TOwner> Get<TValue>(string attributeName)
        {
            attributeName.CheckNotNullOrWhitespace(nameof(attributeName));

            string lowerCaseName = attributeName.ToLowerInvariant();
            return Component.CreateValueProvider(AttributeProviderNameFormat.FormatWith(lowerCaseName), () => GetValue<TValue>(lowerCaseName));
        }

        /// <summary>
        /// Gets the value of the specified control's scope element attribute.
        /// </summary>
        /// <param name="attributeName">The name of the attribute.</param>
        /// <returns>The attribute's current value.
        /// Returns <see langword="null"/> if the value is not set.</returns>
        public string GetValue(string attributeName)
        {
            attributeName.CheckNotNullOrWhitespace(nameof(attributeName));

            return Component.Scope.GetAttribute(attributeName);
        }

        /// <summary>
        /// Gets the value of the specified control's scope element attribute.
        /// </summary>
        /// <typeparam name="TValue">The type of the attribute value.</typeparam>
        /// <param name="attributeName">The name of the attribute.</param>
        /// <returns>The attribute's current value.
        /// Returns <see langword="null"/> if the value is not set.</returns>
        public TValue GetValue<TValue>(string attributeName)
        {
            string valueAsString = GetValue(attributeName);

            if (string.IsNullOrEmpty(valueAsString) && typeof(TValue) == typeof(bool))
                return default;

            return TermResolver.FromString<TValue>(valueAsString);
        }
    }
}
