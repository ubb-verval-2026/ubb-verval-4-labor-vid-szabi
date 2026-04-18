using FluentAssertions;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Firefox;
using OpenQA.Selenium.Support.UI;
using SeleniumExtras.WaitHelpers;
using System;
using System.Diagnostics;
using System.Reflection;
using System.Text;

namespace DatesAndStuff.Web.Tests;

[TestFixture]
public class PersonPageTests
{
    private IWebDriver driver;
    private StringBuilder verificationErrors;
    private const string BaseURL = "http://localhost:5091";
    private bool acceptNextAlert = true;

    private Process? _blazorProcess;

    [OneTimeSetUp]
    public void StartBlazorServer()
    {
        var webProjectPath = Path.GetFullPath(Path.Combine(
            Assembly.GetExecutingAssembly().Location,
            "../../../../../../src/DatesAndStuff.Web/DatesAndStuff.Web.csproj"
            ));

        var webProjFolderPath = Path.GetDirectoryName(webProjectPath);

        var startInfo = new ProcessStartInfo
        {
            FileName = "dotnet",
            //Arguments = $"run --project \"{webProjectPath}\"",
            Arguments = "dotnet run --no-build",
            WorkingDirectory = webProjFolderPath,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false
        };

        _blazorProcess = Process.Start(startInfo);

        // Wait for the app to become available
        var client = new HttpClient();
        var timeout = TimeSpan.FromSeconds(30);
        var start = DateTime.Now;

        while (DateTime.Now - start < timeout)
        {
            try
            {
                var result = client.GetAsync(BaseURL).Result;
                if (result.IsSuccessStatusCode)
                {
                    break;
                }
            }
            catch (Exception e)
            {
                Thread.Sleep(1000);
            }
        }
    }

    [OneTimeTearDown]
    public void StopBlazorServer()
    {
        if (_blazorProcess != null && !_blazorProcess.HasExited)
        {
            _blazorProcess.Kill(true);
            _blazorProcess.Dispose();
        }
    }

    [SetUp]
    public void SetupTest()
    {
        driver = new ChromeDriver();
        verificationErrors = new StringBuilder();
    }

    [TearDown]
    public void TeardownTest()
    {
        try
        {
            driver.Quit();
            driver.Dispose();
        }
        catch (Exception)
        {
            // Ignore errors if unable to close the browser
        }
        Assert.That(verificationErrors.ToString(), Is.EqualTo(""));
    }

    [TestCase(10)]
    [TestCase(25)]
    [TestCase(0)]
    [TestCase(-5)]
    [TestCase(-10)]
    public void Person_SalaryIncrease_ShouldIncrease(double percentage)
    {
        // Arrange
        driver.Navigate().GoToUrl(BaseURL);
        driver.FindElement(By.XPath("//*[@data-test='PersonPageNavigation']")).Click();

        var wait = new WebDriverWait(driver, TimeSpan.FromSeconds(5));

        var salaryLabel = wait.Until(ExpectedConditions.ElementExists(By.XPath("//*[@data-test='DisplayedSalary']")));
        var initialSalary = double.Parse(salaryLabel.Text);

        var input = wait.Until(ExpectedConditions.ElementExists(By.XPath("//*[@data-test='SalaryIncreasePercentageInput']")));
        input.Clear();
        input.SendKeys(percentage.ToString());

        // Act
        var submitButton = wait.Until(ExpectedConditions.ElementExists(By.XPath("//*[@data-test='SalaryIncreaseSubmitButton']")));
        submitButton.Click();

        Thread.Sleep(500);

        var expectedSalary = initialSalary + (initialSalary * (percentage / 100.0));

        // Assert
        var updatedSalaryLabel = wait.Until(ExpectedConditions.ElementExists(By.XPath("//*[@data-test='DisplayedSalary']")));
        var salaryAfterSubmission = double.Parse(updatedSalaryLabel.Text);
        salaryAfterSubmission.Should().BeApproximately(expectedSalary, 0.001);
    }

    [Test]
    public void SalaryIncrease_WhenValueIsInvalid_ShouldShowValidationErrors()
    {
        driver.Navigate().GoToUrl("https://localhost:7222/");
        driver.FindElement(By.LinkText("Person")).Click();
        driver.FindElement(By.Name("formModel.SalaryIncreasePercentage")).Clear();
        driver.FindElement(By.Name("formModel.SalaryIncreasePercentage")).SendKeys("-15");
        driver.FindElement(By.Name("formModel.SalaryIncreasePercentage")).SendKeys(Keys.Enter);
        try
        {
            Assert.That(driver.FindElement(By.XPath("(.//*[normalize-space(text()) and normalize-space(.)='About'])[1]/following::li[1]")).Text,
                Is.EqualTo("The specified percentag should be between -10 and infinity."));
        }
        catch (AssertionException e)
        {
            verificationErrors.Append(e.Message);
        }
        try
        {
            Assert.That(driver.FindElement(By.XPath("(.//*[normalize-space(text()) and normalize-space(.)='*'])[2]/following::div[2]")).Text,
                Is.EqualTo("The specified percentag should be between -10 and infinity."));
        }
        catch (AssertionException e)
        {
            verificationErrors.Append(e.Message);
        }
    }
    private bool IsElementPresent(By by)
    {
        try
        {
            driver.FindElement(by);
            return true;
        }
        catch (NoSuchElementException)
        {
            return false;
        }
    }

    private bool IsAlertPresent()
    {
        try
        {
            driver.SwitchTo().Alert();
            return true;
        }
        catch (NoAlertPresentException)
        {
            return false;
        }
    }

    private string CloseAlertAndGetItsText()
    {
        try
        {
            IAlert alert = driver.SwitchTo().Alert();
            string alertText = alert.Text;
            if (acceptNextAlert)
            {
                alert.Accept();
            }
            else
            {
                alert.Dismiss();
            }
            return alertText;
        }
        finally
        {
            acceptNextAlert = true;
        }
    }
}