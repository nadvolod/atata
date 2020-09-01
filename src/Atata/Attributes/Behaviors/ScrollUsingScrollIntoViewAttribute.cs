﻿namespace Atata
{
    /// <summary>
    /// Represents the behavior for scrolling to control using JavaScript.
    /// Performs <c>element.scrollIntoView(true)</c> function.
    /// </summary>
    public class ScrollUsingScrollIntoViewAttribute : ScrollBehaviorAttribute
    {
        public override void Execute<TOwner>(IControl<TOwner> control)
        {
            control.Script.ScrollIntoView();
        }
    }
}
