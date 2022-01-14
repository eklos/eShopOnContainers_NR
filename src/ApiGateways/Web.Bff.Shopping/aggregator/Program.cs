await ﻿BuildWebHost(args).RunAsync();

IWebHost BuildWebHost(string[] args) =>
    WebHost
        .CreateDefaultBuilder(args)
        .ConfigureAppConfiguration(cb =>
        {
            var sources = cb.Sources;
            sources.Insert(3, new Microsoft.Extensions.Configuration.Json.JsonConfigurationSource()
            {
                Optional = true,
                Path = "appsettings.localhost.json",
                ReloadOnChange = false
            });
        })
        .UseStartup<Startup>()
        .UseSerilog((builderContext, config) =>
        {
            config
                .MinimumLevel.Information()
                .Enrich.FromLogContext()
                .WriteTo.Console()
                .WriteTo.NewRelicLogs();
//                .WriteTo.NewRelicLogs(applicationName: "Web.Bff.ShoppingX", licenseKey: "876eeaf1347dc227536fd8dcba0ae242be64fc4f");
        })
        .Build();