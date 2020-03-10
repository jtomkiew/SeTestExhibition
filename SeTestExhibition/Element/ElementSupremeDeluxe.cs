using OpenQA.Selenium;
using OpenQA.Selenium.Internal;
using System;
using System.Collections.ObjectModel;
using System.Drawing;

namespace SeTestExhibition.Element
{
    /// <summary>
    ///     Check ElementSupreme descriptions first - only differences will be mentioned here.
    ///     IWebElement wrapper and cache.
    ///     This version relies fully on IsStale check on every Element getter to clear cache.
    ///     Can (and does) implement IWrapsElement (meaning IS geckodriver compatible).
    ///     In theory should be slightly slower than ElementSupreme.
    ///     "Supreme" in the name is just for kicks.
    ///     "Deluxe" in the name is for some extra kicks.
    /// </summary>
    public class ElementSupremeDeluxe : IWebElement, IWrapsElement
    {
        private readonly ISearchContext _parent;
        private readonly By _elementLocator;

        private readonly Func<ISearchContext, IWebElement> _elementGetter;

        private IWebElement _element;

        private ElementSupremeDeluxe(ISearchContext parent)
        {
            _parent = parent ?? throw new ArgumentNullException(nameof(parent));
        }

        public ElementSupremeDeluxe(ISearchContext parent, By elementLocator) : this(parent)
        {
            _elementLocator = elementLocator ?? throw new ArgumentNullException(nameof(elementLocator));
        }

        public ElementSupremeDeluxe(ISearchContext parent, Func<ISearchContext, IWebElement> elementGetter) : this(parent)
        {
            _elementGetter = elementGetter ?? throw new ArgumentNullException(nameof(elementGetter));
        }

        private IWebElement Element
        {
            get
            {
                if (_element == null || IsStale) // <= always checks for stale
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
        ///     This checks if current element reference is stale or not by invoking one of its properties getters.
        ///     It is not very important which function/getter is invoked but should be quick and easy on webdriver and browser.
        /// </summary>
        private bool IsStale
        {
            get
            {
                if (_element == null)
                    return false;

                try
                {
                    var _ = _element.Displayed;
                    return false;
                }
                catch (StaleElementReferenceException)
                {
                    return true;
                }
            }
        }

        private void SafeAction(Action<IWebElement> action)
        {
            try
            {
                action(Element);
            }
            catch (StaleElementReferenceException)
            {
                action(Element);
            }
        }

        private T SafeFunc<T>(Func<IWebElement, T> func)
        {
            try
            {
                return func(Element);
            }
            catch (StaleElementReferenceException)
            {
                return func(Element);
            }
        }

        public string TagName => SafeFunc(e => e.TagName);

        public string Text => SafeFunc(e => e.Text);

        public bool Enabled => SafeFunc(e => e.Enabled);

        public bool Selected => SafeFunc(e => e.Selected);

        public Point Location => SafeFunc(e => e.Location);

        public Size Size => SafeFunc(e => e.Size);

        public bool Displayed => SafeFunc(e => e.Displayed);

        public IWebElement WrappedElement => Element;

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
