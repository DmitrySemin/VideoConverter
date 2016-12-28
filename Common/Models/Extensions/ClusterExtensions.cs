using Common.EndpointMappers;
using Common.Messages;
using Common.Models.Settings;
using Common.Services;
using Common.Services.Interfaces;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
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
        public static Cluster AddCluster(this IServiceCollection services, ClusterSettings clusterSettings)
		{
			var statusService = new StatusService<StatusMessage>();
			var cluster = new Cluster(clusterSettings);
			services.AddSingleton<Cluster>(cluster);
			services.AddSingleton<IStatusService<StatusMessage>>(statusService);
			services.AddTransient<IApiService, ApiService>();
			services.AddTransient<IEndpoints, ClusterEndpoints>();

			var serviceProvider = cluster.ServiceProvider = services.BuildServiceProvider();
			var lifetime = serviceProvider.GetRequiredService<IApplicationLifetime>();
			lifetime.ApplicationStarted.Register(cluster.Started);
			return cluster;
		}
    }
}
