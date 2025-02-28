﻿namespace Atata
{
    /// <summary>
    /// Represents the <c>&lt;h6&gt;</c> heading tag.
    /// Default search finds the first occurring <c>&lt;h6&gt;</c> element.
    /// </summary>
    /// <typeparam name="TOwner">The type of the owner page object.</typeparam>
    [ControlDefinition("h6", ComponentTypeName = "<h6> heading", IgnoreNameEndings = "Header,Heading")]
    public class H6<TOwner> : Content<string, TOwner>
        where TOwner : PageObject<TOwner>
    {
    }
}
