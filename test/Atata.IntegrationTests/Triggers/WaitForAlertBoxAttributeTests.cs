﻿namespace Atata.IntegrationTests.Triggers;

public class WaitForAlertBoxAttributeTests : UITestFixture
{
    protected override bool ReuseDriver => false;

    [Test]
    public void Execute()
    {
        Go.To<MessageBoxPage>()
            .AlertWithDelayButton.Click();

        AtataContext.Current.Driver.SwitchTo().Alert().Text.Should().Be("Alert with delay!!!");
    }

    [Test]
    public void Execute_WithTimeout()
    {
        var sut = Go.To<MessageBoxPage>().NoneButton;
        sut.Metadata.Push(new WaitForAlertBoxAttribute { Timeout = 1 });

        Assert.Throws<TimeoutException>(() =>
            sut.Click());
    }
}
