using RPPP_WebApp;
using RPPP_WebApp.Models;
using NLog.Web;
using NLog;
using FluentValidation;
using FluentValidation.AspNetCore;
using RPPP_WebApp.ModelsValidation;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.EntityFrameworkCore;



//NOTE: Add dependencies/services in StartupExtensions.cs and keep this file as-is

var builder = WebApplication.CreateBuilder(args);
var logger = LogManager.Setup().GetCurrentClassLogger();



try
{
    logger.Debug("init main");
  builder.Host.UseNLog(new NLogAspNetCoreOptions() { RemoveLoggerFactoryFilter = false });
    #region Configure services
    var appSection = builder.Configuration.GetSection("AppSettings");
    builder.Services.Configure<AppSettings>(appSection);

    builder.Services.AddDbContext<ProjektContext>(options => options.UseSqlServer(builder.Configuration.GetConnectionString("RPPP10")));

    builder.Services.AddControllersWithViews();

    builder.Services
            .AddFluentValidationAutoValidation()
            .AddFluentValidationClientsideAdapters()
            .AddValidatorsFromAssemblyContaining<ZadatakValidator>();
    #endregion
    var app = builder.ConfigureServices().ConfigurePipeline();
  app.Run();
}
catch (Exception exception)
{
  // NLog: catch setup errors
  logger.Error(exception, "Stopped program because of exception");
  throw;
}
finally
{
  // Ensure to flush and stop internal timers/threads before application-exit (Avoid segmentation fault on Linux)
  NLog.LogManager.Shutdown();
}

public partial class Program { }