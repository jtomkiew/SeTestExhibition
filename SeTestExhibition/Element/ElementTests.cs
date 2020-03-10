using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using OpenQA.Selenium;

namespace SeTestExhibition.Element
{
    [TestClass]
    public class ElementTests
    {
        [TestMethod]
        public void ElementSupreme_IsLazy_WhenNew()
        {
            // Arrange
            var mainElementSelector = By.Id("main-element");
            var subElementSelector = By.Id("sub-element");

            var subElementMock = new Mock<IWebElement>();

            var mainElementMock = new Mock<IWebElement>();
            mainElementMock.Setup(e => e.FindElement(subElementSelector))
                .Returns(subElementMock.Object)
                .Verifiable();

            var driverMock = new Mock<IWebDriver>();
            driverMock.Setup(d => d.FindElement(mainElementSelector))
                .Returns(mainElementMock.Object)
                .Verifiable();

            // Act
            var supremeMainElement = new ElementSupreme(driverMock.Object, mainElementSelector);
            var supremeSubElement = new ElementSupreme(supremeMainElement, subElementSelector);

            // Assert
            driverMock.Verify(d => d.FindElement(mainElementSelector), Times.Never);
            mainElementMock.Verify(e => e.FindElement(subElementSelector), Times.Never);
        }

        [TestMethod]
        public void ElementSupreme_IsCached_OnSecondCall()
        {
            // Arrange
            var mainElementSelector = By.Id("main-element");

            var mainElementMock = new Mock<IWebElement>();
            mainElementMock.Setup(e => e.Click())
                .Verifiable();

            var driverMock = new Mock<IWebDriver>();
            driverMock.Setup(d => d.FindElement(mainElementSelector))
                .Returns(mainElementMock.Object)
                .Verifiable();

            var supremeMainElement = new ElementSupreme(driverMock.Object, mainElementSelector);

            // Act
            supremeMainElement.Click();
            supremeMainElement.Click();

            // Assert
            driverMock.Verify(d => d.FindElement(mainElementSelector), Times.Once);
            mainElementMock.Verify(e => e.Click(), Times.Exactly(2));
        }

        [TestMethod]
        public void ElementSupreme_FindsElementAgain_OnStaleException()
        {
            // Arrange
            var mainElementSelector = By.Id("main-element");

            var mainElementMock = new Mock<IWebElement>();
            mainElementMock.SetupSequence(e => e.Click())
                .Pass() // new element reference, will call FindElement on parent, then Click
                .Throws<StaleElementReferenceException>().Pass(); // element reference stale, will call FindElement on parent, then Click

            var driverMock = new Mock<IWebDriver>();
            driverMock.Setup(d => d.FindElement(mainElementSelector))
                .Returns(mainElementMock.Object)
                .Verifiable();

            var supremeMainElement = new ElementSupreme(driverMock.Object, mainElementSelector);

            // Act
            supremeMainElement.Click();
            supremeMainElement.Click();

            // Assert
            driverMock.Verify(d => d.FindElement(mainElementSelector), Times.Exactly(2));
            mainElementMock.Verify(e => e.Click(), Times.Exactly(3)); // Pass, Throw, Pass
        }
    }
}
