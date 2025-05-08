using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Support.UI;
using SeleniumExtras.WaitHelpers;
using System;

class Program
{
    static void Main(string[] args)
    {
        // 1) Read credentials & target
        Console.Write("Facebook email: ");
        var email = Console.ReadLine()!;
        Console.Write("Facebook password: ");
        var password = ReadPassword();
        Console.Write("Target profile username or ID: ");
        var profileId = Console.ReadLine()!;
        Console.Write("Message to send: ");
        var messageText = Console.ReadLine()!;

        // 2) Start ChromeDriver
        var options = new ChromeOptions();
        options.AddArgument("--start-maximized");
        using var driver = new ChromeDriver(options);

        var wait = new WebDriverWait(driver, TimeSpan.FromSeconds(15));

        try
        {
            // LOGIN
            driver.Navigate().GoToUrl("https://www.facebook.com/login");
            wait.Until(ExpectedConditions.ElementIsVisible(By.Id("email"))).SendKeys(email);
            driver.FindElement(By.Id("pass")).SendKeys(password);
            driver.FindElement(By.Name("login")).Click();

            // ensure login completed
            wait.Until(d => d.Url.Contains("facebook.com"));

            // NAVIGATE TO PROFILE
            var profileUrl = $"https://www.facebook.com/{profileId}";
            driver.Navigate().GoToUrl(profileUrl);

            // EXTRACT DATA
            string name = wait
                .Until(ExpectedConditions.ElementIsVisible(
                    By.XPath("//h1[contains(@id,'seo_h1_tag')]")))
                .Text;
            Console.WriteLine($"Name: {name}");

            // headline or bio (optional)
            var headlineElem = driver.FindElements(
                By.XPath("//div[contains(@data-pagelet,'ProfileIntroCard')]//span"));
            if (headlineElem.Count > 0)
                Console.WriteLine("Headline: " + headlineElem[0].Text);

            // SEND MESSAGE via Messenger
            var msgUrl = $"https://www.facebook.com/messages/t/{profileId}";
            driver.Navigate().GoToUrl(msgUrl);

            // wait for the messenger textbox
            var textbox = wait
                .Until(ExpectedConditions.ElementIsVisible(
                    By.XPath("//div[@role='textbox']")));
            textbox.Click();
            textbox.SendKeys(messageText);
            textbox.SendKeys(Keys.Enter);

            Console.WriteLine("Message sent!");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error: {ex.Message}");
        }
        finally
        {
            Console.WriteLine("Press any key to quit...");
            Console.ReadKey();
            driver.Quit();
        }
    }

    // helper to read password without echoing
    static string ReadPassword()
    {
        var pwd = string.Empty;
        ConsoleKey key;
        do
        {
            var keyInfo = Console.ReadKey(intercept: true);
            key = keyInfo.Key;

            if (key == ConsoleKey.Backspace && pwd.Length > 0)
            {
                pwd = pwd[0..^1];
                Console.Write("\b \b");
            }
            else if (!char.IsControl(keyInfo.KeyChar))
            {
                pwd += keyInfo.KeyChar;
                Console.Write("*");
            }
        } while (key != ConsoleKey.Enter);
        Console.WriteLine();
        return pwd;
    }
}
