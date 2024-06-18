using BackGroundService;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Support.UI;
using OpenQA.Selenium;
using System;
using System.Data.SqlClient;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Linq;
using OpenQA.Selenium.Interactions;

public class TimedHostedService : IHostedService, IDisposable
{
    private Timer _timer;
    private Timer _configCheckTimer;
    private TimeSpan _startTime;
    private TimeSpan _configCheckInterval;/* = TimeSpan.FromSeconds(100);*/ // Check for config changes every 5 minutes
    private readonly CancellationTokenSource _stoppingCts = new CancellationTokenSource();
    private readonly ILogger<TimedHostedService> _logger;


    public TimedHostedService(ILogger<TimedHostedService> logger)
    {
        _logger = logger;
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Timed Hosted Service is starting.");
        ScheduleLog("Timed Hosted Service is starting.");
        try
        {
            await LoadConfiguration();
            ScheduleNextExecution();
            _configCheckTimer = new Timer(CheckForConfigurationChange, null, _configCheckInterval, _configCheckInterval);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred while starting the Timed Hosted Service.");
        }
    }

    public async Task StopAsync(CancellationToken cancellationToken)
    {

        ScheduleLog("Timed Hosted Service is stopping");
        _logger.LogInformation("Timed Hosted Service is stopping.");
        _timer?.Change(Timeout.Infinite, 0);
        _configCheckTimer?.Change(Timeout.Infinite, 0);

        _stoppingCts.Dispose();
        Environment.Exit(0);
        // Perform any other cleanup here if necessary
        ScheduleLog("Timed Hosted Service is stopped");
        await Task.CompletedTask;
    }

    private async Task LoadConfiguration()
    {
        try
        {
            _startTime = await GetStartTimeFromDatabaseAsync();
            _logger.LogInformation($"Loaded start time from database: {_startTime}");

            _configCheckInterval = await GetConfigCheckIntervalFromDatabaseAsync();
            _logger.LogInformation($"Loaded config check interval from database: {_configCheckInterval}");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred while loading the configuration.");
        }
    }

    private void ScheduleNextExecution()
    {
        try
        {
            var now = DateTime.Now;
            var nextStartTime = now.Date + _startTime;

            if (nextStartTime <= now)
            {
                nextStartTime = nextStartTime.AddDays(1);
            }

            var initialDelay = nextStartTime - now;
            _logger.LogInformation($"Scheduling next execution in {initialDelay.TotalSeconds} seconds.");
            _timer = new Timer(DoWork, null, initialDelay, TimeSpan.FromHours(12)); // Set periodic interval to 12 hours
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred while scheduling the next execution.");
        }
    }

    private async void CheckForConfigurationChange(object state)
    {
        try
        {
            var newStartTime = await GetStartTimeFromDatabaseAsync();
            if (newStartTime != _startTime)
            {
                _logger.LogInformation($"Start time has changed to {newStartTime}. Rescheduling...");
                _startTime = newStartTime;
                ScheduleNextExecution();
            }

            var newConfigCheckInterval = await GetConfigCheckIntervalFromDatabaseAsync();
            if (newConfigCheckInterval != _configCheckInterval)
            {
                _logger.LogInformation($"Config check interval has changed to {newConfigCheckInterval}. Updating...");
                _configCheckInterval = newConfigCheckInterval;
                _configCheckTimer.Change(_configCheckInterval, _configCheckInterval);
            }

            var shouldStop = await ShouldStopServiceAsync();
            if (shouldStop)
            {
                _logger.LogInformation("Stop condition met. Stopping service.");
                await StopAsync(_stoppingCts.Token);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred while checking for configuration changes.");
        }
    }

    public async void DoWork(object state)
    {      
        #region main
        //Helper helper = new Helper();
        //var tickets = helper.GetTickets();

        //var chromeOptions = new ChromeOptions();
        //string url = "https://www.goindigo.in/edit-booking.html";
        //chromeOptions.AddExcludedArguments(new List<string> { "enable-automation" });
        //chromeOptions.AddAdditionalOption("useAutomationExtension", false);

        ////chromeOptions.AddArguments("headless");
        //chromeOptions.AddArgument("--kiosk-printing");
        //chromeOptions.AddArgument("--disable-blink-features");
        //chromeOptions.AddArgument("--disable-blink-features=AutomationControlled");

        //var driver = new ChromeDriver(chromeOptions);
        //WebDriverWait wait = new WebDriverWait(driver, TimeSpan.FromMinutes(60)); // Adjust the timeout as needed

        //driver.Navigate().GoToUrl(url);
        //ScheduleLog("Do work is started");
        //foreach (var item in tickets)
        //{
        //    if (!string.IsNullOrEmpty(item.pnr) && !string.IsNullOrEmpty(item.lastname))
        //    {
        //        try
        //        {
        //            var pnrIp = driver.FindElement(By.Name("pnr-booking-ref"));
        //            if (pnrIp != null)
        //            {
        //                pnrIp.SendKeys(item.pnr);
        //            }

        //            var lastNameIp = driver.FindElement(By.Name("email-last-name"));
        //            if (lastNameIp != null)
        //            {
        //                lastNameIp.SendKeys(item.lastname);
        //            }

        //            Thread.Sleep(15000);

        //            IJavaScriptExecutor js = (IJavaScriptExecutor)driver;

        //            var submitBtn = wait.Until(SeleniumExtras.WaitHelpers.ExpectedConditions.ElementIsVisible(By.ClassName("form-button-item")));

        //            //var submitBtn = driver.FindElements(By.ClassName("form-button-item"));
        //            Thread.Sleep(5000);

        //            if (submitBtn != null)
        //            {
        //                submitBtn.Click();
        //            }

        //            Thread.Sleep(20000);

        //            IWebElement table = wait.Until(SeleniumExtras.WaitHelpers.ExpectedConditions.ElementIsVisible(By.ClassName("skyplus-table")));

        //            ((IJavaScriptExecutor)driver).ExecuteScript("arguments[0].scrollIntoView({behavior: 'smooth', block: 'center'});", table);

        //            var pnrCancelBtn = wait.Until(SeleniumExtras.WaitHelpers.ExpectedConditions.ElementIsVisible(By.ClassName("passenger-no-show-container__link")));

        //            if (pnrCancelBtn != null)
        //            {
        //                Thread.Sleep(10000);
        //                pnrCancelBtn.Click();
        //                Thread.Sleep(15000);

        //                IWebElement liElement = wait.Until(SeleniumExtras.WaitHelpers.ExpectedConditions.ElementIsVisible(By.CssSelector("li.desc")));

        //                if (liElement != null)
        //                {
        //                    #region Screen Shot                        
        //                    //Screenshot screenshot = ((ITakesScreenshot)driver).GetScreenshot();
        //                    //string screenshotFilePath = Path.Combine(Directory.GetCurrentDirectory(), "screenshot.png");
        //                    //screenshot.SaveAsFile(screenshotFilePath);
        //                    #endregion

        //                    // Retrieve the text content
        //                    string text = liElement.Text;
        //                    // Define the regular expression pattern to match the number
        //                    string pattern = @"INR\s(.*?)\sis";
        //                    // Use Regex.Match to find the match in the input string
        //                    Match match = Regex.Match(text, pattern);
        //                    string refundamount = "";

        //                    if (match.Success)
        //                    {
        //                        // Extract the captured group (the number)
        //                        refundamount = match.Groups[1].Value;
        //                    }

        //                    #region Update refund

        //                    if (refundamount != "")
        //                    {
        //                        var pnrLst = helper.CheckPnr(item.pnr);
        //                        if (pnrLst.Count() < 2)
        //                        {
        //                            helper.UpdateRefund(refundamount, pnrLst[0].entry_number);
        //                        }
        //                        else
        //                        {
        //                            var refundsplitamount = Math.Round((decimal)(Convert.ToDecimal(refundamount)) / pnrLst.Count(), 2);
        //                            foreach (var pnrs in pnrLst)
        //                            {
        //                                helper.UpdateRefund(refundsplitamount.ToString(), pnrs.entry_number);
        //                            }
        //                        }
        //                        driver.Navigate().GoToUrl(url);
        //                        continue;
        //                    }
        //                    else
        //                    {
        //                        driver.Navigate().GoToUrl(url);
        //                        continue;
        //                    }
        //                    #endregion
        //                }
        //                else
        //                {
        //                    driver.Navigate().GoToUrl(url);
        //                    continue;
        //                }
        //            }
        //            else
        //            {
        //                helper.UpdateStatus(item.pnr, item.lastname);
        //            }

        //        }
        //        catch (Exception ex)
        //        {
        //            driver.Dispose();
        //            helper.LogError(ex, item.pnr, item.lastname);
        //            helper.SendExceptionEmail(ex);
        //            break;
        //        }
        //    }
        //    else
        //    {
        //        helper.UpdateStatus(item.pnr, item.lastname);
        //    }
        //}
        //driver.Dispose();
        //helper.SendSuccessEmail();
        //ScheduleLog("Do work is ended");
        //Environment.Exit(0);
        #endregion
    }

    private async Task<TimeSpan> GetStartTimeFromDatabaseAsync()
    {
        try
        {
            // Your logic to retrieve the start time from the database
            string startTimeString = await RetrieveStartTimeFromDatabase();
            _logger.LogInformation($"Retrieved start time from database: {startTimeString}");
            return TimeSpan.Parse(startTimeString);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred while retrieving the start time from the database.");
            throw;
        }
    }

    private async Task<bool> ShouldStopServiceAsync()
    {
        try
        {
            // Your logic to check if the service should stop
            bool shouldStop = await RetrieveShouldStopFromDatabase();
            _logger.LogInformation($"Retrieved stop condition from database: {shouldStop}");
            return shouldStop;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred while retrieving the stop condition from the database.");
            throw;
        }
    }

    private async Task<TimeSpan> GetConfigCheckIntervalFromDatabaseAsync()
    {
        try
        {
            // Your logic to retrieve the interval from the database
            string intervalString = await RetrieveIntervalFromDatabase();
            _logger.LogInformation($"Retrieved interval from database: {intervalString}");
            return TimeSpan.FromSeconds(int.Parse(intervalString));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred while retrieving the config check interval from the database.");
            throw;
        }
    }

    public void Dispose()
    {
        _timer?.Dispose();
        _configCheckTimer?.Dispose();
    }

    private async Task<string> RetrieveStartTimeFromDatabase()
    {
        string startTime = "";
        string cs = "Server=11.11.11.208,9099;Database=Audit;user id=SabreEdoc;password=SabreEdoc@123;MultipleActiveResultSets=true;";
        SqlConnection con = new SqlConnection(cs);
        string filecheckdate = "select top 1 StartTime from ScheduleExeTime";
        SqlCommand cmd = new SqlCommand(filecheckdate, con);
        con.Open();
        SqlDataReader dr = cmd.ExecuteReader();
        bool isStop = false;
        while (dr.Read())
        {
            startTime = dr["StartTime"].ToString();
        }
        con.Close();

        await Task.Delay(100); // Simulating async database call delay.
        return startTime; // Return the actual stop condition from the database.


    }

    private async Task<bool> RetrieveShouldStopFromDatabase()
    {
        string cs = "Server=11.11.11.208,9099;Database=Audit;user id=SabreEdoc;password=SabreEdoc@123;MultipleActiveResultSets=true;";
        SqlConnection con = new SqlConnection(cs);
        string filecheckdate = "select top 1 EndTime  from ScheduleExeTime";
        SqlCommand cmd = new SqlCommand(filecheckdate, con);
        con.Open();
        SqlDataReader dr = cmd.ExecuteReader();
        bool isStop = false;
        while (dr.Read())
        {
            isStop = Convert.ToBoolean(dr["EndTime"]);
        }
        con.Close();

        await Task.Delay(100); // Simulating async database call delay.
        return isStop; // Return the actual stop condition from the database.
    }


    private async Task<string> RetrieveIntervalFromDatabase()
    {
        string intervalTime = "";
        string cs = "Server=11.11.11.208,9099;Database=Audit;user id=SabreEdoc;password=SabreEdoc@123;MultipleActiveResultSets=true;";
        SqlConnection con = new SqlConnection(cs);
        string filecheckdate = "select top 1 IntervalTime from ScheduleExeTime";
        SqlCommand cmd = new SqlCommand(filecheckdate, con);
        con.Open();
        SqlDataReader dr = cmd.ExecuteReader();

        while (dr.Read())
        {
            intervalTime = dr["IntervalTime"].ToString();
        }
        con.Close();

        await Task.Delay(100);
        return intervalTime;
    }

    public void LogInfo(string log)
    {
        string cs = "Server=11.11.11.208,9099;Database=Audit;user id=SabreEdoc;password=SabreEdoc@123;MultipleActiveResultSets=true;";
        SqlConnection con = new SqlConnection(cs);
        string filecheckdate = "Insert into OpenTickets_Log(LogInfo) Values(@log)";
        SqlCommand cmd = new SqlCommand(filecheckdate, con);
        cmd.Parameters.AddWithValue("@log", log);
        con.Open();
        cmd.ExecuteReader().Close();
        con.Close();
    }

    public void ScheduleLog(string description)
    {
        string cs = "Server=11.11.11.208,9099;Database=Audit;user id=SabreEdoc;password=SabreEdoc@123;MultipleActiveResultSets=true;";
        SqlConnection con = new SqlConnection(cs);
        string query = "Insert into OpenTicket_ScheduleLogs(LogTime,IntervalTime,Description) Values(@LogTime,@IntervalTime,@Description)";
        SqlCommand cmd = new SqlCommand(query, con);
        cmd.Parameters.AddWithValue("@LogTime", DateTime.Now);
        cmd.Parameters.AddWithValue("@IntervalTime", _configCheckInterval.ToString());
        cmd.Parameters.AddWithValue("@Description", description);
        con.Open();
        cmd.ExecuteReader().Close();
        con.Close();
    }

    public Task ManualStartAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Manual start of Timed Hosted Service.");
        return StartAsync(stoppingToken);
    }

    public Task ManualStopAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Manual stop of Timed Hosted Service.");
        return StopAsync(stoppingToken);
    }
}
