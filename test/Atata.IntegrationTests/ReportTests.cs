﻿namespace Atata.IntegrationTests;

public class ReportTests : UITestFixture
{
    [Test]
    public void BulkLog()
    {
        var errorException = new InvalidOperationException("error");
        var fatalException = new InvalidOperationException("fatal");

        Go.To<OrdinaryPage>()
            .Report.Trace("tracemessage")
            .Report.Debug("debugmessage")
            .Report.Info("infomessage")
            .Report.Warn("{0}message", "warn")
            .Report.Error(errorException)
            .Report.Error("errormessage", errorException)
            .Report.Error("errormessage", "stacktrace")
            .Report.Fatal(fatalException)
            .Report.Fatal("fatalmessage", fatalException);

        VerifyLastLogEntries(
            (LogLevel.Trace, "tracemessage", null),
            (LogLevel.Debug, "debugmessage", null),
            (LogLevel.Info, "infomessage", null),
            (LogLevel.Warn, "warnmessage", null),
            (LogLevel.Error, null, errorException),
            (LogLevel.Error, "errormessage", errorException),
            (LogLevel.Error, $"errormessage{Environment.NewLine}stacktrace", null),
            (LogLevel.Fatal, null, fatalException),
            (LogLevel.Fatal, "fatalmessage", fatalException));
    }

    [Test]
    public void EdgeCaseMessages()
    {
        Go.To<OrdinaryPage>()
            .Report.Info("{")
            .Report.Info("}")
            .Report.Info(null);

        VerifyLastLogEntries(
            (LogLevel.Info, "{", null),
            (LogLevel.Info, "}", null),
            (LogLevel.Info, null, null));
    }

    [Test]
    public void Section()
    {
        Go.To<OrdinaryPage>()
            .Report.Start("section1")
            .Report.Start("section2", LogLevel.Trace)
            .Report.Debug("debugmessage")
            .Report.EndSection()
            .Report.EndSection();

        var logEntries = GetLastLogEntries(LogLevel.Trace, 5);

        logEntries[0].Level.Should().Be(LogLevel.Info);
        logEntries[0].Message.Should().Be("> section1");
        logEntries[1].Level.Should().Be(LogLevel.Trace);
        logEntries[1].Message.Should().Be("> section2");
        logEntries[2].Level.Should().Be(LogLevel.Debug);
        logEntries[2].Message.Should().Be("debugmessage");
        logEntries[3].Level.Should().Be(LogLevel.Trace);
        logEntries[3].Message.Should().StartWith("< section2");
        logEntries[4].Level.Should().Be(LogLevel.Info);
        logEntries[4].Message.Should().StartWith("< section1");
    }

    [Test]
    public void Screenshot_WithoutScreenshotConsumer()
    {
        Go.To<OrdinaryPage>()
            .Report.Screenshot()
            .Report.Screenshot("sometitle");

        VerifyLastLogMessagesContain(
            minLogLevel: LogLevel.Trace,
            "Go to");
    }

    [Test]
    public void Screenshot()
    {
        MockScreenshotConsumer screenshotConsumer = new();

        ((LogManager)AtataContext.Current.Log).Use(screenshotConsumer);

        Go.To<OrdinaryPage>()
            .Report.Screenshot()
            .Report.Screenshot("sometitle");

        VerifyLastLogMessages(
            minLogLevel: LogLevel.Trace,
            "Take screenshot #01",
            "Take screenshot #02 - sometitle");

        screenshotConsumer.Items.Should().HaveCount(2);
        screenshotConsumer.Items[0].Number.Should().Be(1);
        screenshotConsumer.Items[0].Title.Should().BeNull();
        screenshotConsumer.Items[1].Number.Should().Be(2);
        screenshotConsumer.Items[1].Title.Should().Be("sometitle");
    }

    private class MockScreenshotConsumer : IScreenshotConsumer
    {
        public List<ScreenshotInfo> Items { get; } = new();

        public void Take(ScreenshotInfo screenshotInfo) =>
            Items.Add(screenshotInfo);
    }
}
