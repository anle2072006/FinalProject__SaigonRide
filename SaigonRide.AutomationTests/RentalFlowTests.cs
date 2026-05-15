using NUnit.Framework;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Support.UI;
using System;
using System.Threading;

namespace SaigonRide.AutomationTests
{
    public class RentalCheckoutFlowTests
    {
        private IWebDriver driver;
        private readonly string baseUrl = "https://localhost:7004";

        [SetUp]
        public void Setup()
        {
            ChromeOptions options = new ChromeOptions();
            options.BinaryLocation = @"C:\Program Files\BraveSoftware\Brave-Browser\Application\brave.exe";
            options.AddArgument("--incognito");
            driver = new ChromeDriver(options);
            driver.Manage().Window.Maximize();


        }

        // ─────────────────────────────────────────────
        //  TEST 1: Full flow PayPal
        // ─────────────────────────────────────────────

        [Test]
        public void Test_RentalFlow_PayPal_WithCoupon()
        {
            PerformLogin();
            PerformRentalAndDropoff(10000);
            HandlePaymentFlow("PayPal");

            ShowStep("Step: Done - verifying back on app");
            Assert.That(driver.Url.Contains(baseUrl), Is.True, "PayPal flow failed!");
        }

        // ─────────────────────────────────────────────
        //  TEST 2: Full flow VNPay
        // ─────────────────────────────────────────────

        [Test]
        public void Test_RentalFlow_VNPay_NCB()
        {
            PerformLogin();
            PerformRentalAndDropoff(10000);
            HandlePaymentFlow("VNPay");

            ShowStep("Step: Done - verifying back on app");
            Assert.That(driver.Url.Contains(baseUrl), Is.True, "VNPay flow failed!");
        }

        // ─────────────────────────────────────────────
        //  TEST 3: Cả 2 cycle liên tiếp (giữ lại test gốc)
        // ─────────────────────────────────────────────

        [Test]
        public void Test_MegaFlow_Cycle1_PayPal_Cycle2_VNPay()
        {
            PerformLogin();
            PerformRentalAndDropoff(10000);
            HandlePaymentFlow("PayPal");

            PerformLogin();
            PerformRentalAndDropoff(10000);
            HandlePaymentFlow("VNPay");
        }

        // ─────────────────────────────────────────────
        //  SHARED STEPS
        // ─────────────────────────────────────────────

        private void PerformLogin()
        {
            ShowStep("Step: Navigating to Login");
            driver.Navigate().GoToUrl(baseUrl + "/Identity/Account/Login");
            Thread.Sleep(2000);
            ShowStep("Step: Entering credentials");
            driver.FindElement(By.Id("Input_Username")).SendKeys("admin");
            driver.FindElement(By.Id("Input_Password")).SendKeys("12345");
            driver.FindElement(By.XPath("//button[@type='submit']")).Click();
            ShowStep("Step: Logging in...");
            Thread.Sleep(3000);
        }

        private void PerformRentalAndDropoff(int waitTime)
        {
            ShowStep("Step: Opening Stations page");
            driver.Navigate().GoToUrl(baseUrl + "/Home/Stations");
            Thread.Sleep(3000);

            try
            {
                var activeTripBtn = driver.FindElement(By.XPath("//a[contains(text(), 'Go to In-Use')]"));
                if (activeTripBtn.Displayed)
                {
                    ShowStep("Step: Active trip found, resuming");
                    activeTripBtn.Click();
                    Thread.Sleep(3000);
                    goto StartDropOff;
                }
            }
            catch (Exception) { }

            ShowStep("Step: Selecting station");
            var stationBtn = driver.FindElement(By.XPath("//a[contains(@href, 'IndexVehicles') and contains(text(), 'Choose')]"));
            ((IJavaScriptExecutor)driver).ExecuteScript("arguments[0].click();", stationBtn);
            Thread.Sleep(2000);

            ShowStep("Step: Booking vehicle");
            var bookBtn = driver.FindElement(By.CssSelector(".btn-book"));
            ((IJavaScriptExecutor)driver).ExecuteScript("arguments[0].click();", bookBtn);
            Thread.Sleep(1500);
            driver.FindElement(By.XPath("//button[contains(text(), 'Go to In Use')]")).Click();
            Thread.Sleep(3000);

        StartDropOff:
            ShowStep("Step: Waiting before drop-off...");
            Thread.Sleep(waitTime);
            ShowStep("Step: Selecting Drop-off station");
            driver.FindElement(By.XPath("//button[contains(text(), 'Select Drop-off')]")).Click();
            Thread.Sleep(1500);

            var stationCard = driver.FindElement(By.CssSelector(".station-card"));
            ((IJavaScriptExecutor)driver).ExecuteScript("arguments[0].click();", stationCard);
            Thread.Sleep(1000);
            ShowStep("Step: Confirming drop-off destination");
            driver.FindElement(By.Id("btnSaveDestination")).Click();
            Thread.Sleep(2000);
        }

        private void HandlePaymentFlow(string method)
        {
            ShowStep("Step: Opening Payment page");
            driver.FindElement(By.Id("btnPayment")).Click();
            Thread.Sleep(2000);

            IJavaScriptExecutor js = (IJavaScriptExecutor)driver;

            if (method == "PayPal")
            {
                ShowStep("Step: Applying Coupon");
                var couponSelectElement = driver.FindElement(By.Id("couponSelect"));
                var selectCoupon = new SelectElement(couponSelectElement);
                if (selectCoupon.Options.Count > 1)
                {
                    selectCoupon.SelectByIndex(1);
                    driver.FindElement(By.Id("btnApplyCoupon")).Click();
                    Thread.Sleep(2000);
                }

                ShowStep("Step: Selecting PayPal");
                driver.FindElement(By.Id("pay_paypal")).Click();
                ShowStep("Step: Redirect to PayPal");
                driver.FindElement(By.XPath("//button[contains(text(), 'Payment Confirmation')]")).Click();
                Thread.Sleep(8000);

                ShowStep("Step: PayPal Login - entering email");
                driver.FindElement(By.Id("email")).SendKeys("sb-eillr51084274@personal.example.com");
                driver.FindElement(By.Id("btnNext")).Click();
                Thread.Sleep(3000);
                ShowStep("Step: PayPal Login - entering password");
                driver.FindElement(By.Id("password")).SendKeys("Eg!hC5$9");
                driver.FindElement(By.Id("btnLogin")).Click();
                Thread.Sleep(6000);
                ShowStep("Step: PayPal - Confirming payment");
                js.ExecuteScript("document.querySelector('button#payment-submit-btn')?.click();");
                js.ExecuteScript("document.evaluate(\"//button[contains(text(), 'Continue')]\", document, null, XPathResult.FIRST_ORDERED_NODE_TYPE, null).singleNodeValue?.click();");
            }
            else
            {
                ShowStep("Step: Selecting VNPay");
                driver.FindElement(By.Id("pay_vnpay")).Click();
                ShowStep("Step: Redirect to VNPay");
                driver.FindElement(By.XPath("//button[contains(text(), 'Payment Confirmation')]")).Click();
                Thread.Sleep(12000);

                ShowStep("Step: Selecting The noi dia");
                string[] selectors = { "Thẻ nội địa", "Local card", "ATM" };
                foreach (var text in selectors)
                {
                    try
                    {
                        var el = driver.FindElement(By.XPath($"//*[contains(text(), '{text}')]"));
                        js.ExecuteScript("arguments[0].click();", el);
                        break;
                    }
                    catch { }
                }
                Thread.Sleep(5000);

                ShowStep("Step: Selecting NCB bank");
                var ncbWait = new WebDriverWait(driver, TimeSpan.FromSeconds(15));
                bool ncbClicked = false;

                string[] ncbXpaths = new[]
                {
                    "//button[@id='NCB']",
                    "//button[@name='paymethod' and @id='NCB']",
                    "//button[contains(@class,'list-bank-item') and @id='NCB']",
                    "//*[contains(@style,'ncb.svg') or contains(@style,'NCB.svg')]",
                    "//button[.//div[contains(@style,'ncb')]]"
                };

                foreach (var xpath in ncbXpaths)
                {
                    try
                    {
                        var ncbEl = ncbWait.Until(d =>
                        {
                            try { var el = d.FindElement(By.XPath(xpath)); return el.Displayed ? el : null; }
                            catch { return null; }
                        });
                        if (ncbEl != null)
                        {
                            js.ExecuteScript("arguments[0].scrollIntoView({block:'center'});", ncbEl);
                            Thread.Sleep(500);
                            js.ExecuteScript("arguments[0].click();", ncbEl);
                            ncbClicked = true;
                            break;
                        }
                    }
                    catch { }
                }

                if (!ncbClicked)
                {
                    js.ExecuteScript(@"
                        var all = document.querySelectorAll('img, label, li, div, a, button');
                        for (var i = 0; i < all.length; i++) {
                            var el = all[i];
                            var hint = (el.alt || el.textContent || el.id || el.getAttribute('src') || '').toLowerCase();
                            if (hint.includes('ncb')) { el.click(); break; }
                        }
                    ");
                }

                Thread.Sleep(4000);

                ShowStep("Step: Filling card details");
                var formWait = new WebDriverWait(driver, TimeSpan.FromSeconds(15));
                formWait.Until(d =>
                {
                    try { var e = d.FindElement(By.Id("card_number_mask")); return e.Displayed ? e : null; }
                    catch { return null; }
                });

                js.ExecuteScript(@"
                    var nativeInputSetter = Object.getOwnPropertyDescriptor(window.HTMLInputElement.prototype, 'value').set;

                    var cardNum = document.getElementById('card_number_mask');
                    nativeInputSetter.call(cardNum, '9704198526191432198');
                    cardNum.dispatchEvent(new Event('input', {bubbles:true}));
                    cardNum.dispatchEvent(new Event('change', {bubbles:true}));

                    var holder = document.getElementById('cardHolder');
                    nativeInputSetter.call(holder, 'NGUYEN VAN A');
                    holder.dispatchEvent(new Event('input', {bubbles:true}));
                    holder.dispatchEvent(new Event('change', {bubbles:true}));

                    var date = document.getElementById('cardDate');
                    nativeInputSetter.call(date, '07/15');
                    date.dispatchEvent(new Event('input', {bubbles:true}));
                    date.dispatchEvent(new Event('change', {bubbles:true}));
                ");
                Thread.Sleep(1000);

                ShowStep("Step: Clicking Continue");
                var continueBtn = formWait.Until(d =>
                {
                    try { var e = d.FindElement(By.Id("btnContinue")); return e.Displayed ? e : null; }
                    catch { return null; }
                });
                js.ExecuteScript("arguments[0].click();", continueBtn);
                Thread.Sleep(3000);

                // Popup Dieu khoan hien SAU khi click Tiep tuc
                try
                {
                    var agreeWait = new WebDriverWait(driver, TimeSpan.FromSeconds(8));
                    var agreeBtn = agreeWait.Until(d =>
                    {
                        try { var e = d.FindElement(By.CssSelector("a#btnAgree")); return e.Displayed ? e : null; }
                        catch { return null; }
                    });
                    ShowStep("Step: Accepting Terms & Conditions");
                    js.ExecuteScript("arguments[0].click();", agreeBtn);
                    Thread.Sleep(3000);
                }
                catch { }

                ShowStep("Step: Entering OTP");
                var otpWait = new WebDriverWait(driver, TimeSpan.FromSeconds(15));
                var otpEl = otpWait.Until(d =>
                {
                    try { var e = d.FindElement(By.Id("otpvalue")); return e.Displayed ? e : null; }
                    catch { return null; }
                });
                otpEl.Clear();
                otpEl.SendKeys("123456");

                ShowStep("Step: Confirming payment");
                var confirmBtn = otpWait.Until(d =>
                {
                    try { var e = d.FindElement(By.Id("btnConfirm")); return e.Displayed ? e : null; }
                    catch { return null; }
                });
                js.ExecuteScript("arguments[0].click();", confirmBtn);
            }

            ShowStep("Step: Payment complete - Logging out");
            Thread.Sleep(6000);
            driver.Navigate().GoToUrl(baseUrl + "/Setting/IndexSetting");
            Thread.Sleep(2000);
            driver.FindElement(By.CssSelector(".btn-logout-red")).Click();
            Thread.Sleep(3000);
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
        public void TearDown()
        {
            if (driver != null)
            {
                driver.Quit();
                driver.Dispose();
            }
        }
    }
}
