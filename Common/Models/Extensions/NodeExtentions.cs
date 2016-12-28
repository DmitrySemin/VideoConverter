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
        public static Node AddNode(this IServiceCollection services, NodeSettings nodeSettings)
        {
            var statusService = new StatusService<StatusMessage>();
            var workerNode = new Node(statusService, nodeSettings);
            services.AddSingleton<Node>(workerNode);
            services.AddSingleton<IStatusService<StatusMessage>>(statusService);
            services.AddTransient<IApiService, ApiService>();
            services.AddTransient<IEndpoints, NodeEndpoints>();

            var serviceProvider = workerNode.ServiceProvider = services.BuildServiceProvider();
            var env = serviceProvider.GetRequiredService<IHostingEnvironment>();
			
			//TODO !!!
            var builder = new ConfigurationBuilder()
                .SetBasePath(env.ContentRootPath)
                .AddJsonFile(@"Properties/launchSettings.json", optional: false, reloadOnChange: true);
            var launchConfig = builder.Build();
            var ssslPort = launchConfig.GetValue<int>("iisSettings:iisExpress:sslPort");
            var hostUrl = launchConfig.GetValue<string>("iisSettings:iisExpress:applicationUrl");

            workerNode.HostUrl = hostUrl;

            var lifetime = serviceProvider.GetRequiredService<IApplicationLifetime>();
            lifetime.ApplicationStarted.Register(workerNode.Started);
            lifetime.ApplicationStopped.Register(workerNode.Stopped);
            return workerNode;
        }
    }
}
