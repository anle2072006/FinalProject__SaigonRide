using NUnit.Framework;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using System.Threading;

namespace SaigonRide.AutomationTests
{
    public class AuthTests
    {
        private IWebDriver driver;
        private readonly string baseUrl = "https://localhost:7004";

        [SetUp]
        public void Setup()
        {
            ChromeOptions options = new ChromeOptions();
            options.BinaryLocation = @"C:\Program Files\BraveSoftware\Brave-Browser\Application\brave.exe";
            driver = new ChromeDriver(options);
            driver.Manage().Window.Maximize();

            // Kiểm tra server có chạy không trước khi test
            try
            {
                driver.Navigate().GoToUrl(baseUrl);
                Thread.Sleep(2000);
            }
            catch
            {
                Assert.Fail("Không kết nối được tới server. Hãy chắc chắn ứng dụng đang chạy tại " + baseUrl);
            }
        }

        // ─────────────────────────────────────────────
        //  REGISTER
        // ─────────────────────────────────────────────

        [Test]
        public void Test_UserSignUp_And_AutoLogin_Success()
        {
            ShowStep("Step: Navigating to Register page");
            driver.Navigate().GoToUrl(baseUrl + "/Identity/Account/Register");
            Thread.Sleep(2000);

            string randomSuffix = System.DateTime.Now.Ticks.ToString();

            ShowStep("Step: Filling registration form");
            driver.FindElement(By.Id("Input_Username")).SendKeys("AutoUser_" + randomSuffix);
            driver.FindElement(By.Id("Input_FirstName")).SendKeys("Auto");
            driver.FindElement(By.Id("Input_LastName")).SendKeys("Test");
            driver.FindElement(By.Id("docInput")).SendKeys("079204012345");
            driver.FindElement(By.Id("Input_Email")).SendKeys($"tester_{randomSuffix}@ex.com");
            driver.FindElement(By.Id("Input_Password")).SendKeys("Test@123456");

            ShowStep("Step: Submitting registration");
            driver.FindElement(By.XPath("//button[@type='submit']")).Click();
            Thread.Sleep(3000);

            ShowStep("Step: Verifying redirect to homepage");
            Assert.That(
                driver.Url == baseUrl + "/" || driver.Url == baseUrl,
                Is.True,
                "Registration failed, cannot access the homepage."
            );
        }

        // ─────────────────────────────────────────────
        //  LOGIN
        // ─────────────────────────────────────────────

        [Test]
        public void Test_UserLogin_Success()
        {
            ShowStep("Step: Navigating to Login page");
            driver.Navigate().GoToUrl(baseUrl + "/Identity/Account/Login");
            Thread.Sleep(3000);

            ShowStep("Step: Entering valid credentials");
            driver.FindElement(By.Id("Input_Username")).SendKeys("admin");
            driver.FindElement(By.Id("Input_Password")).SendKeys("12345");

            ShowStep("Step: Submitting login");
            driver.FindElement(By.XPath("//button[@type='submit']")).Click();
            Thread.Sleep(3000);

            ShowStep("Step: Verifying login success");
            bool isNotOnLoginPage = !driver.Url.Contains("Login");
            Assert.That(
                isNotOnLoginPage,
                Is.True,
                $"Đăng nhập thất bại! Bot vẫn đang bị kẹt ở: {driver.Url}"
            );
        }

        [Test]
        public void Test_UserLogin_WithWrongPassword_ShowsError()
        {
            ShowStep("Step: Navigating to Login page");
            driver.Navigate().GoToUrl(baseUrl + "/Identity/Account/Login");
            Thread.Sleep(3000);

            ShowStep("Step: Entering wrong password");
            driver.FindElement(By.Id("Input_Username")).SendKeys("admin");
            driver.FindElement(By.Id("Input_Password")).SendKeys("mat_khau_sai_tum_lum");

            ShowStep("Step: Submitting login");
            driver.FindElement(By.XPath("//button[@type='submit']")).Click();
            Thread.Sleep(2000);

            ShowStep("Step: Verifying error shown");
            Assert.That(
                driver.Url.Contains("/Identity/Account/Login"),
                Is.True,
                "Security vulnerability: System bypassed with wrong password!"
            );
        }

        [Test]
        public void Test_UserLogin_WithEmptyFields_ShowsError()
        {
            ShowStep("Step: Navigating to Login page");
            driver.Navigate().GoToUrl(baseUrl + "/Identity/Account/Login");
            Thread.Sleep(2000);

            ShowStep("Step: Submitting empty form");
            driver.FindElement(By.XPath("//button[@type='submit']")).Click();
            Thread.Sleep(1500);

            ShowStep("Step: Verifying still on login page");
            Assert.That(
                driver.Url.Contains("/Identity/Account/Login"),
                Is.True,
                "System should not allow login with empty credentials!"
            );
        }

        // ─────────────────────────────────────────────
        //  LOGOUT
        // ─────────────────────────────────────────────

        [Test]
        public void Test_UserLogout_Success()
        {
            ShowStep("Step: Logging in first");
            driver.Navigate().GoToUrl(baseUrl + "/Identity/Account/Login");
            Thread.Sleep(2000);
            driver.FindElement(By.Id("Input_Username")).SendKeys("admin");
            driver.FindElement(By.Id("Input_Password")).SendKeys("12345");
            driver.FindElement(By.XPath("//button[@type='submit']")).Click();
            Thread.Sleep(3000);

            ShowStep("Step: Navigating to Settings to logout");
            driver.Navigate().GoToUrl(baseUrl + "/Setting/IndexSetting");
            Thread.Sleep(2000);

            ShowStep("Step: Clicking logout button");
            driver.FindElement(By.CssSelector(".btn-logout-red")).Click();
            Thread.Sleep(3000);

            ShowStep("Step: Verifying redirected after logout");
            Assert.That(
                driver.Url.Contains("/Identity/Account/Login") || driver.Url == baseUrl + "/" || driver.Url == baseUrl,
                Is.True,
                $"Logout failed! Still at: {driver.Url}"
            );
        }

        // ─────────────────────────────────────────────
        //  HELPER
        // ─────────────────────────────────────────────

        private void ShowStep(string message)
        {
            try
            {
                ((IJavaScriptExecutor)driver).ExecuteScript($@"
                    var el = document.getElementById('__step_overlay__');
                    if (!el) {{
                        el = document.createElement('div');
                        el.id = '__step_overlay__';
                        el.style.cssText = 'position:fixed;top:16px;right:16px;z-index:99999;background:rgba(0,0,0,0.75);color:#fff;padding:10px 18px;border-radius:8px;font-size:15px;font-family:sans-serif;max-width:320px;word-wrap:break-word;pointer-events:none;';
                        document.body.appendChild(el);
                    }}
                    el.textContent = '{message.Replace("'", "\\'")}';
                ");
            }
            catch { }
        }

        [TearDown]
        public void Teardown()
        {
            driver?.Quit();
            driver?.Dispose();
        }
    }
}
