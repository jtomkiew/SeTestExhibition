using OpenQA.Selenium;
using System;
using System.Collections.ObjectModel;
using System.Drawing;

namespace SeTestExhibition.Element
{
    /// <summary>
    ///     IWebElement wrapper and cache.
    ///     This version relies fully on SafeAction and SafeFunc to clear cache when StaleElementReferenceException is thrown.
    ///     Does not implement IWrapsElement (meaning is not geckodriver compatible) because of above constraint.
    ///     In theory should be slightly faster than ElementSupremeDeluxe.
    ///     "Supreme" in the name is just for kicks.
    /// </summary>
    public class ElementSupreme : IWebElement
    {
        private readonly ISearchContext _parent;
        private readonly By _elementLocator;

        private readonly Func<ISearchContext, IWebElement> _elementGetter;

        private IWebElement _element;

        private ElementSupreme(ISearchContext parent)
        {
            _parent = parent ?? throw new ArgumentNullException(nameof(parent));
        }

        /// <summary>
        ///     The "nice" and clear constructor - finds element under parent using element locator (lazy).
        /// </summary>
        /// <param name="parent">This can be anything but in reality should be either IWeDriver or another ElementSupreme instance.</param>
        /// <param name="elementLocator"></param>
        public ElementSupreme(ISearchContext parent, By elementLocator) : this(parent)
        {
            _elementLocator = elementLocator ?? throw new ArgumentNullException(nameof(elementLocator));
        }

        /// <summary>
        ///     The "more custom and less nice" constructor - finds element under parent using element getter function (lazy).
        /// </summary>
        /// <param name="parent"></param>
        /// <param name="elementGetter"></param>
        public ElementSupreme(ISearchContext parent, Func<ISearchContext, IWebElement> elementGetter) : this(parent)
        {
            _elementGetter = elementGetter ?? throw new ArgumentNullException(nameof(elementGetter));
        }

        /// <summary>
        ///     This always returns current element instance (stale or not) - evaluates element if null.
        /// </summary>
        private IWebElement Element
        {
            get
            {
                if (_element == null)
                {
                    if (_elementGetter != null)
                        _element = _elementGetter(_parent);
                    else
                        _element = _parent.FindElement(_elementLocator);
                }

                return _element;
            }
        }

        /// <summary>
        ///     Protects against StaleElementReferenceException on action invocation. Once.
        /// </summary>
        /// <param name="action"></param>
        private void SafeAction(Action<IWebElement> action)
        {
            try
            {
                action(Element);
            }
            catch (StaleElementReferenceException)
            {
                _element = null;
                action(Element);
            }
        }

        /// <summary>
        ///     Protects against StaleElementReferenceException on function invocation. Once.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="func"></param>
        /// <returns></returns>
        private TResult SafeFunc<TResult>(Func<IWebElement, TResult> func)
        {
            try
            {
                return func(Element);
            }
            catch (StaleElementReferenceException)
            {
                _element = null;
                return func(Element);
            }
        }

        // Below are all required implementations for the IWebElement interface using above SafeAction/Func.

        public string TagName => SafeFunc(e => e.TagName);

        public string Text => SafeFunc(e => e.Text);

        public bool Enabled => SafeFunc(e => e.Enabled);

        public bool Selected => SafeFunc(e => e.Selected);

        public Point Location => SafeFunc(e => e.Location);

        public Size Size => SafeFunc(e => e.Size);

        public bool Displayed => SafeFunc(e => e.Displayed);

        public void Clear()
        {
            SafeAction(e => e.Clear());
        }

        public void Click()
        {
            SafeAction(e => e.Click());
        }

        public IWebElement FindElement(By by)
        {
            return SafeFunc(e => e.FindElement(by));
        }

        public ReadOnlyCollection<IWebElement> FindElements(By by)
        {
            return SafeFunc(e => e.FindElements(by));
        }

        public string GetAttribute(string attributeName)
        {
            return SafeFunc(e => e.GetAttribute(attributeName));
        }

        public string GetCssValue(string propertyName)
        {
            return SafeFunc(e => e.GetCssValue(propertyName));
        }

        public string GetProperty(string propertyName)
        {
            return SafeFunc(e => e.GetProperty(propertyName));
        }

        public void SendKeys(string text)
        {
            SafeAction(e => e.SendKeys(text));
        }

        public void Submit()
        {
            SafeAction(e => e.Submit());
        }
    }
}
