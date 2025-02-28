﻿using System;
using System.IO;
using OpenQA.Selenium;

namespace Atata
{
    public abstract class FileScreenshotConsumerBase : IScreenshotConsumer
    {
        /// <summary>
        /// Gets or sets the image format.
        /// The default format is <see cref="OpenQA.Selenium.ScreenshotImageFormat.Png"/>.
        /// </summary>
        public ScreenshotImageFormat ImageFormat { get; set; } = ScreenshotImageFormat.Png;

        /// <summary>
        /// Takes the specified screenshot.
        /// </summary>
        /// <param name="screenshotInfo">The screenshot information.</param>
        public void Take(ScreenshotInfo screenshotInfo)
        {
            string filePath = BuildFilePath(screenshotInfo);
            filePath = filePath.SanitizeForPath();
            filePath += ImageFormat.GetExtension();

            if (!Path.IsPathRooted(filePath))
                filePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, filePath);

            string directoryPath = Path.GetDirectoryName(filePath);

            if (!Directory.Exists(directoryPath))
                Directory.CreateDirectory(directoryPath);

            screenshotInfo.Screenshot.SaveAsFile(filePath, ImageFormat);

            AtataContext.Current.Log.Info($"Screenshot saved to file \"{filePath}\"");
            AtataContext.Current.EventBus.Publish(new ScreenshotFileSavedEvent(screenshotInfo, filePath));
        }

        /// <summary>
        /// Builds the path of the file without the extension.
        /// </summary>
        /// <param name="screenshotInfo">The screenshot information.</param>
        /// <returns>The file path without the extension.</returns>
        protected abstract string BuildFilePath(ScreenshotInfo screenshotInfo);
    }
}
