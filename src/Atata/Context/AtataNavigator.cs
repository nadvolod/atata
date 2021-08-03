﻿using System;
using System.Linq;

namespace Atata
{
    /// <summary>
    /// Represents the navigation functionality between pages and windows.
    /// </summary>
    public class AtataNavigator
    {
        private readonly AtataContext _context;

        /// <summary>
        /// Initializes a new instance of the <see cref="AtataNavigator"/> class.
        /// </summary>
        /// <param name="context">The context.</param>
        public AtataNavigator(AtataContext context)
        {
            _context = context.CheckNotNull(nameof(context));
        }

        /// <summary>
        /// Navigates to the specified page object.
        /// </summary>
        /// <typeparam name="T">The type of the page object.</typeparam>
        /// <param name="pageObject">
        /// The page object.
        /// If set to <see langword="null"/> creates an instance using the default constructor.</param>
        /// <param name="url">The URL.</param>
        /// <param name="navigate">If set to <see langword="true"/> executes page object navigation functionality.</param>
        /// <param name="temporarily">If set to <see langword="true"/> navigates temporarily preserving current page object state.</param>
        /// <returns>The page object.</returns>
        public T To<T>(T pageObject = null, string url = null, bool navigate = true, bool temporarily = false)
            where T : PageObject<T>
        {
            return To(pageObject, new GoOptions { Url = url, Navigate = string.IsNullOrWhiteSpace(url) && navigate, Temporarily = temporarily });
        }

        /// <summary>
        /// Navigates to the window with the specified page object by name.
        /// </summary>
        /// <typeparam name="T">The type of the page object.</typeparam>
        /// <param name="pageObject">
        /// The page object.
        /// If set to <see langword="null"/> creates an instance using the default constructor.</param>
        /// <param name="windowName">Name of the browser window.</param>
        /// <param name="temporarily">If set to <see langword="true"/> navigates temporarily preserving current page object state.</param>
        /// <returns>The page object.</returns>
        public T ToWindow<T>(T pageObject, string windowName, bool temporarily = false)
            where T : PageObject<T>
        {
            SetContextAsCurrent();
            _context.Log.Info($"Switch to \"{windowName}\" window");

            return To(pageObject, new GoOptions { Navigate = false, WindowName = windowName, Temporarily = temporarily });
        }

        /// <summary>
        /// Navigates to the window by name.
        /// </summary>
        /// <typeparam name="T">The type of the page object.</typeparam>
        /// <param name="windowName">Name of the browser window.</param>
        /// <param name="temporarily">If set to <see langword="true"/> navigates temporarily preserving current page object state.</param>
        /// <returns>The page object.</returns>
        public T ToWindow<T>(string windowName, bool temporarily = false)
            where T : PageObject<T>
        {
            SetContextAsCurrent();
            _context.Log.Info($"Switch to \"{windowName}\" window");

            return To<T>(null, new GoOptions { Navigate = false, WindowName = windowName, Temporarily = temporarily });
        }

        /// <summary>
        /// Navigates to the next window with the specified page object.
        /// </summary>
        /// <typeparam name="T">The type of the page object.</typeparam>
        /// <param name="pageObject">
        /// The page object.
        /// If set to <see langword="null"/> creates an instance using the default constructor.</param>
        /// <param name="temporarily">If set to <see langword="true"/> navigates temporarily preserving current page object state.</param>
        /// <returns>The page object.</returns>
        public T ToNextWindow<T>(T pageObject = null, bool temporarily = false)
            where T : PageObject<T>
        {
            SetContextAsCurrent();
            _context.Log.Info("Switch to next window");

            string currentWindowHandle = _context.Driver.CurrentWindowHandle;

            string nextWindowHandle = _context.Driver.WindowHandles.
                SkipWhile(x => x != currentWindowHandle).
                ElementAt(1);

            return To(pageObject, new GoOptions { Navigate = false, WindowName = nextWindowHandle, Temporarily = temporarily });
        }

        /// <summary>
        /// Navigates to the previous window with the specified page object.
        /// </summary>
        /// <typeparam name="T">The type of the page object.</typeparam>
        /// <param name="pageObject">
        /// The page object.
        /// If set to <see langword="null"/> creates an instance using the default constructor.</param>
        /// <param name="temporarily">If set to <see langword="true"/> navigates temporarily preserving current page object state.</param>
        /// <returns>The page object.</returns>
        public T ToPreviousWindow<T>(T pageObject = null, bool temporarily = false)
            where T : PageObject<T>
        {
            SetContextAsCurrent();
            _context.Log.Info("Switch to previous window");

            string currentWindowHandle = _context.Driver.CurrentWindowHandle;

            string previousWindowHandle = _context.Driver.WindowHandles
                .Reverse()
                .SkipWhile(x => x != currentWindowHandle)
                .ElementAt(1);

            return To(pageObject, new GoOptions { Navigate = false, WindowName = previousWindowHandle, Temporarily = temporarily });
        }

        private T To<T>(T pageObject, GoOptions options)
            where T : PageObject<T>
        {
            SetContextAsCurrent();

            if (_context.PageObject is null)
            {
                pageObject = pageObject ?? ActivatorEx.CreateInstance<T>();
                _context.PageObject = pageObject;

                if (!string.IsNullOrWhiteSpace(options.Url))
                    ToUrl(options.Url);

                pageObject.NavigateOnInit = options.Navigate;
                pageObject.Init();
                return pageObject;
            }
            else
            {
                IPageObject currentPageObject = (IPageObject)_context.PageObject;
                T newPageObject = currentPageObject.GoTo(pageObject, options);
                _context.PageObject = newPageObject;
                return newPageObject;
            }
        }

        /// <summary>
        /// Navigates to the specified URL.
        /// </summary>
        /// <param name="url">The URL.</param>
        public void ToUrl(string url)
        {
            SetContextAsCurrent();

            if (!UriUtils.TryCreateAbsoluteUrl(url, out Uri absoluteUrl))
            {
                if (!_context.IsNavigated && _context.BaseUrl is null)
                {
                    if (string.IsNullOrWhiteSpace(url))
                        throw new InvalidOperationException("Cannot navigate to empty or null URL. AtataContext.Current.BaseUrl can be set with base URL.");
                    else
                        throw new InvalidOperationException($"Cannot navigate to relative URL \"{url}\". AtataContext.Current.BaseUrl can be set with base URL.");
                }

                if (_context.BaseUrl is null)
                {
                    Uri currentUri = new Uri(_context.Driver.Url, UriKind.Absolute);

                    string domainPart = currentUri.GetComponents(UriComponents.SchemeAndServer | UriComponents.UserInfo, UriFormat.Unescaped);
                    Uri domainUri = new Uri(domainPart);

                    absoluteUrl = new Uri(domainUri, url);
                }
                else
                {
                    absoluteUrl = UriUtils.Concat(_context.BaseUrl, url);
                }
            }

            Navigate(absoluteUrl);
        }

        private void Navigate(Uri uri)
        {
            _context.Log.Info($"Go to URL \"{uri}\"");
            _context.Driver.Navigate().GoToUrl(uri);
            _context.IsNavigated = true;
        }

        private void SetContextAsCurrent()
        {
            AtataContext.Current = _context;
        }
    }
}
