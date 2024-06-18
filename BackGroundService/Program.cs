using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BackGroundService
{

    //public class Program
    //{
    //    public static async Task Main(string[] args)
    //    {
    //        var host = CreateHostBuilder(args).Build();

    //        using (var scope = host.Services.CreateScope())
    //        {
    //            var services = scope.ServiceProvider;

    //            try
    //            {
    //                // Ensure the hosted service is started
    //                var hostedService = services.GetRequiredService<IHostedService>();
    //                await hostedService.StartAsync(default);
    //            }
    //            catch (Exception ex)
    //            {
    //                var logger = services.GetRequiredService<ILogger<Program>>();
    //                logger.LogError(ex, "An error occurred while starting the application.");
    //                Environment.Exit(1);
    //            }
    //        }

    //        // Start the host
    //        await host.RunAsync();

    //        // Ensure the console application exits after stopping
    //        Environment.Exit(0);
    //    }

    //    public static IHostBuilder CreateHostBuilder(string[] args) =>
    //        Host.CreateDefaultBuilder(args)
    //            .ConfigureServices((hostContext, services) =>
    //            {
    //                services.AddHostedService<TimedHostedService>();
    //                services.AddLogging(configure => configure.AddConsole());
    //            });
    //}






    class Program
    {
        public static void Main(string[] args)
        {
            CreateHostBuilder(args).Build().Run();            
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
         Host.CreateDefaultBuilder(args)
             .ConfigureServices((hostContext, services) =>
             {
                 services.AddSingleton<TimedHostedService>(); // Register as a singleton
             })
             .ConfigureLogging(logging =>
             {
                 logging.ClearProviders();
                 logging.AddConsole();
             });
    }
}
