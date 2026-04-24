using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Support.UI;
using NUnit.Framework;
using System;

[TestFixture]
public class WizzairTestClass
{
    [Test]
    public void WizzairTests()
    {
        IWebDriver driver = new ChromeDriver();
        driver.Navigate().GoToUrl("https://www.wizzair.com/en-gb");

        WebDriverWait wait = new WebDriverWait(driver, TimeSpan.FromSeconds(20));

        // --- COOKIE POPUP (FIXED) ---
        try
        {
            var acceptBtn = wait.Until(d => d.FindElement(
                By.XPath("//button[contains(., 'Accept all')]")
            ));

            // JS click, mert néha overlay miatt nem kattintható
            ((IJavaScriptExecutor)driver).ExecuteScript("arguments[0].click();", acceptBtn);
        }
        catch (Exception)
        {
            // ha nincs popup → nem gond
        }

        // --- FROM (Budapest) ---
        var fromInput = wait.Until(d => d.FindElement(By.XPath("//input[contains(@placeholder,'Origin')]")));
        fromInput.Click();
        fromInput.SendKeys("Budapest");

        var fromOption = wait.Until(d => d.FindElement(By.XPath("//div[contains(text(),'Budapest')]")));
        fromOption.Click();

        // --- TO (Bucharest) ---
        var toInput = wait.Until(d => d.FindElement(By.XPath("//input[contains(@placeholder,'Destination')]")));
        toInput.Click();
        toInput.SendKeys("Bucharest");

        var toOption = wait.Until(d => d.FindElement(By.XPath("//div[contains(text(),'Bucharest')]")));
        toOption.Click();

        // --- SEARCH ---
        var searchButton = wait.Until(d => d.FindElement(By.XPath("//button[@type='submit']")));
        searchButton.Click();

        // --- WAIT RESULTS ---
        wait.Until(d => d.FindElements(By.CssSelector("[data-testid='flight-card']")).Count > 0);

        var flights = driver.FindElements(By.CssSelector("[data-testid='flight-card']"));

        Assert.That(flights.Count >= 2, "Nincs legalább 2 járat Budapest és Bucharest között!");

        driver.Quit();
    }
}