using OpenQA.Selenium;

namespace SeTestExhibition.Element
{
    class Examples
    {
        IWebElement mainElement;
        IWebElement subElement;
        IWebElement finalElement;

        public Examples(IWebDriver driver)
        {
            // ElementSupreme is lazy - all of bellow are valid even if such elements would not be found yet (when page is not loaded) - perfect for constructors:
            mainElement = new ElementSupreme(driver, By.Id("main-element"));
            subElement = new ElementSupreme(mainElement, By.Id("sub-element"));
            finalElement = new ElementSupreme(subElement, By.Id("final-element"));
        }

        public void Example1_Lazyness()
        {
            // Assuming now a page is loaded and above elements selector are available...

            finalElement.Click();

            // Now what will happen:
            // * finalElement will try to invoke Click
            // * finalElement will try to evaluate its element by searching parent (subElement)
            // * subElement will try to invoke FindElement with finalElement selector
            // * subElement will try to evaluate its element by searching parent (mainElement)
            // * mainElement will try to invoke FindElement with subElement selector
            // * mainElement will try to evaluate its element by searching parent (driver)
            // * mainElement element is evaluated
            // * subElement element is evaluated
            // * finalElement element is evaluated
            // * finalElement.Click() is invoked (no exception)
        }

        public void Example2_Cache()
        {
            // Assuming now the page stays the same...

            finalElement.Click();

            // Now what will happen:
            // * finalElement will try to invoke Click
            // * finalElement has already cached element
            // * finalElement.Click() is invoked (no exception)
        }

        public void Example3_StaleResistance_PartialRelaod()
        {
            // Assuming now part of the page is reloaded and elements mainElement and subElement are still available and the same but finalElement is has new reference (would throw as stale)...

            finalElement.Click();

            // Now what will happen:
            // * finalElement will try to invoke Click
            // * finalElement has already cached element
            // * finalElement.Click() is invoked (stale exception is thrown)
            // * finalElement cache is cleared
            // * finalElement will try to evaluate its element by searching parent (subElement)
            // * subElement has already cached element
            // * finalElement element is evaluated
            // * finalElement.Click() is invoked (no exception)
        }

        public void Example4_StaleResistance_FullRelaod()
        {
            // Assuming now page is reloaded and all elements are still available but with new references (all would throw as stale)...

            finalElement.Click();

            // Now what will happen:
            // * finalElement will try to invoke Click
            // * finalElement has already cached element
            // * finalElement.Click() is invoked (stale exception is thrown)
            // * finalElement cache is cleared
            // * finalElement will try to evaluate its element by searching parent (subElement)
            // * subElement will try to invoke FindElement with finalElement selector (stale exception is thrown)
            // * subElement cache is cleared
            // * subElement will try to evaluate its element by searching parent (mainElement)
            // * mainElement will try to invoke FindElement with finalElement selector (stale exception is thrown)
            // * mainElement cache is cleared
            // * mainElement will try to evaluate its element by searching parent (driver)
            // * mainElement element is evaluated
            // * subElement element is evaluated
            // * finalElement element is evaluated
            // * finalElement.Click() is invoked (no exception)
        }
    }
}
