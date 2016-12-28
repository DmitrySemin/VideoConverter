using System;
using Common.EndpointMappers;
using Common.Messages;
using Common.Models.Settings;
using Common.Services;
using Common.Services.Interfaces;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace Common.Models.Extensions
{
    public static class NodeExtentions
    {
        /// <summary>
        /// Регистрация среднего слоя
        /// </summary>
        public static void UserNode(this IApplicationBuilder app)
        {
            var endpointMapper = app.ApplicationServices.GetRequiredService<IEndpoints>();
            endpointMapper.MapEndpoints(app);
        }

        /// <summary>
        /// Инициализация ноды и добавление всех ее зависимостей
        /// </summary>
        public static Node AddNode(this IServiceCollection services)
        {
            var statusService = new StatusService<StatusMessage>();
			services.AddSingleton<IStatusService<StatusMessage>>(statusService);
			services.AddTransient<IApiService, ApiService>();
			services.AddTransient<IEndpoints, NodeEndpoints>();
			var serviceProvider = services.BuildServiceProvider();
			var env = serviceProvider.GetRequiredService<IHostingEnvironment>();

			var builder = new ConfigurationBuilder()
				.SetBasePath(env.ContentRootPath)
				.AddJsonFile("appsettings.json", optional: true, reloadOnChange: true);

			var configuration = builder.Build();
			NodeSettings nodeSettings = new NodeSettings()
			{
				FfMpegPath = configuration["NodeSettings:FfMpegPath"],
				TempDirectory = configuration["NodeSettings:TempDirectory"],
				ClusterUrl = configuration["NodeSettings:ClusterUrl"],
				NodeUrl = new Uri(configuration["NodeSettings:NodeUrl"]).Authority,
				ActorsCount = configuration.GetValue<int>("NodeSettings:ActorsCount")
			};
			
			var node = new Node(statusService, nodeSettings, serviceProvider);
            services.AddSingleton<Node>(node);
			
            var lifetime = serviceProvider.GetRequiredService<IApplicationLifetime>();
            lifetime.ApplicationStarted.Register(node.Started);
            lifetime.ApplicationStopped.Register(node.Stopped);
            return node;
        }
    }
}
