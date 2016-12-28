using Common.EndpointMappers;
using Common.Messages;
using Common.Models.Settings;
using Common.Services;
using Common.Services.Interfaces;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Common.Models.Extensions
{
    public static class ClusterExtensions
    {
        /// <summary>
        /// Регистрация среднего  слоя
        /// </summary>
        public static void UseCluster(this IApplicationBuilder app)
        {
            var endpointMapper = app.ApplicationServices.GetRequiredService<IEndpoints>();
            endpointMapper.MapEndpoints(app);
        }

        /// <summary>
        ///Инициализируем кластер и добавляем зависимости
        /// </summary>
        public static Cluster AddCluster(this IServiceCollection services)
		{
			var statusService = new StatusService<StatusMessage>();
			services.AddSingleton<IStatusService<StatusMessage>>(statusService);
			services.AddTransient<IApiService, ApiService>();
			services.AddTransient<IEndpoints, ClusterEndpoints>();
			var serviceProvider = services.BuildServiceProvider();
			var env = serviceProvider.GetRequiredService<IHostingEnvironment>();

			var builder = new ConfigurationBuilder()
				.SetBasePath(env.ContentRootPath)
				.AddJsonFile("appsettings.json", optional: true, reloadOnChange: true);

			var configuration = builder.Build();
			ClusterSettings settings = new ClusterSettings()
			{
				InputPath = configuration["ClusterSettings:InputPath"],
				OutputPath = configuration["ClusterSettings:OutputPath"]
			};

			var cluster = new Cluster(settings, serviceProvider);
			services.AddSingleton<Cluster>(cluster);
			
			var lifetime = serviceProvider.GetRequiredService<IApplicationLifetime>();
			lifetime.ApplicationStarted.Register(cluster.Started);
			return cluster;
		}
    }
}
