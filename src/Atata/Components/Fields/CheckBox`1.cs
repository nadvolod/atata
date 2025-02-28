﻿namespace Atata
{
    /// <summary>
    /// Represents the checkbox control (<c>&lt;input type="checkbox"&gt;</c>).
    /// Default search is performed by the label.
    /// </summary>
    /// <typeparam name="TOwner">The type of the owner page object.</typeparam>
    [ControlDefinition("input[@type='checkbox']", ComponentTypeName = "checkbox", IgnoreNameEndings = "Checkbox,CheckBox,Option")]
    [FindByLabel]
    public class CheckBox<TOwner> : EditableField<bool, TOwner>, ICheckable<TOwner>
        where TOwner : PageObject<TOwner>
    {
        /// <summary>
        /// Gets the <see cref="ValueProvider{TValue, TOwner}" /> of the checked state value.
        /// </summary>
        public ValueProvider<bool, TOwner> IsChecked =>
            CreateValueProvider("checked state", () => Value);

        /// <summary>
        /// Gets the assertion verification provider that has a set of verification extension methods.
        /// </summary>
        public new FieldVerificationProvider<bool, CheckBox<TOwner>, TOwner> Should => new FieldVerificationProvider<bool, CheckBox<TOwner>, TOwner>(this);

        /// <summary>
        /// Gets the expectation verification provider that has a set of verification extension methods.
        /// </summary>
        public new FieldVerificationProvider<bool, CheckBox<TOwner>, TOwner> ExpectTo => Should.Using<ExpectationVerificationStrategy>();

        /// <summary>
        /// Gets the waiting verification provider that has a set of verification extension methods.
        /// Uses <see cref="AtataContext.WaitingTimeout"/> and <see cref="AtataContext.WaitingRetryInterval"/> of <see cref="AtataContext.Current"/> for timeout and retry interval.
        /// </summary>
        public new FieldVerificationProvider<bool, CheckBox<TOwner>, TOwner> WaitTo => Should.Using<WaitingVerificationStrategy>();

        protected override bool GetValue() =>
            Scope.Selected;

        protected override void SetValue(bool value) =>
            Context.UIComponentAccessChainScopeCache.ExecuteWithin(() =>
            {
                if (GetValue() != value)
                    OnClick();
            });

        /// <summary>
        /// Checks the control.
        /// Also executes <see cref="TriggerEvents.BeforeSet" /> and <see cref="TriggerEvents.AfterSet" /> triggers.
        /// </summary>
        /// <returns>The owner page object.</returns>
        public TOwner Check() =>
            Set(true);

        /// <summary>
        /// Unchecks the control.
        /// Also executes <see cref="TriggerEvents.BeforeSet" /> and <see cref="TriggerEvents.AfterSet" /> triggers.
        /// </summary>
        /// <returns>The owner page object.</returns>
        public TOwner Uncheck() =>
            Set(false);
    }
}
