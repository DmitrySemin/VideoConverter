using System;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Common.Models.Extensions;
using Common.Models.Settings;

namespace Node2
{
    public class Startup
    {
        public IServiceProvider Services { get; set; }

        public Startup(IHostingEnvironment env)
        {
            var builder = new ConfigurationBuilder()
                .SetBasePath(env.ContentRootPath)
                .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
                .AddEnvironmentVariables();
            Configuration = builder.Build();
        }

        public IConfigurationRoot Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            // Add framework services.
            services.AddMvc();

            NodeSettings settings = new NodeSettings()
            {
                FfMpegPath = Configuration["NodeSettings:FfMpegPath"],
                TempDirectory = Configuration["NodeSettings:TempDirectory"],
                ClusterUrl = Configuration["NodeSettings:ClusterUrl"],
                ActorsCount = Configuration.GetValue<int>("NodeSettings:ActorsCount")
            };

            services.AddNode(settings);
            Services = services.BuildServiceProvider();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory)
        {
            loggerFactory.AddConsole(Configuration.GetSection("Logging"));
            loggerFactory.AddDebug();

            app.UseMvc();
            app.UserNode();
        }
    }
}
