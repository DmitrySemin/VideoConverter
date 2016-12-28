using System;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using Microsoft.AspNetCore.Http;
using Common.Models;
using Common.Messages;
using Common.Models.Extensions;
using Common.Utilities;

namespace Common.EndpointMappers
{
	/// <summary>
	///  Конечные точки кластера - интерфейс взаимодействия с нодами
	/// </summary>
	public class ClusterEndpoints : IEndpoints
	{
		private Cluster _cluster;
		private const string _workerRoute = "api/worker";

		public ClusterEndpoints(Cluster cluster)
		{
			_cluster = cluster;
		}

		public void MapEndpoints(IApplicationBuilder app)
		{
			var routeBuilder = new RouteBuilder(app);

			// Прослушиваем события старта и стопа нод/тасков и т.д.
			var startedUrl = $"{_workerRoute}/started";
			var leftUrl = $"{_workerRoute}/left/{{ip}}";
			var readyUrl = $"{_workerRoute}/ready/{{ip}}";
			var taskCompleteUrl = $"{_workerRoute}/taskComplete";
			var errorUrl = $"{_workerRoute}/error";

			routeBuilder.MapPost(startedUrl, NodeStarted);
			routeBuilder.MapGet(readyUrl, NodeReady);
			routeBuilder.MapGet(leftUrl, NodeLeft);
			routeBuilder.MapPost(taskCompleteUrl, TaskComplete);
			routeBuilder.MapPost(errorUrl, NodeError);

			var routes = routeBuilder.Build();
			app.UseRouter(routes);
		}

		/// <summary>
		///  Обработчик запроса с информацией о стартовавшей ноде
		/// </summary>
		public RequestDelegate NodeStarted
		{
			get
			{
				RequestDelegate requestDelegate = context =>
				{
					var nodeStarted = context.Request.ReadAsAsync<NodeStarted>().Result;
					_cluster.Send(nodeStarted);
					return context.Response.WriteAsync("OK");
				};
				return requestDelegate;
			}
		}
		/// <summary>
		/// Делегат запроса хардбита ноды
		/// </summary>
		public RequestDelegate NodeReady
		{
			get
			{
				RequestDelegate requestDelegate = context =>
				{
					var ip = (string)context.GetRouteValue("ip");
					var nodeReady = new NodeReady() { Host = ip };
					_cluster.Send(nodeReady);
					return context.Response.WriteAsync("OK");
				};
				return requestDelegate;
			}
		}
		/// <summary>
		///  Обработчик запроса отключающейся ноды
		/// </summary>
		public RequestDelegate NodeLeft
		{
			get
			{
				RequestDelegate requestDelegate = context =>
				{
					var ip = (string)context.GetRouteValue("ip");
					var nodeLeft = new NodeLeft() { Host = ip };
					_cluster.Send(nodeLeft);
					return context.Response.WriteAsync("OK");
				};
				return requestDelegate;
			}
		}
		/// <summary>
		/// Обработчик запроса выполненого таска
		/// </summary>
		public RequestDelegate TaskComplete
		{
			get
			{
				RequestDelegate requestDelegate = context =>
				{
					var taskComplete = context.Request.ReadAsAsync<TaskComplete>().Result;
					_cluster.Send(taskComplete);
					return context.Response.WriteAsync("OK");
				};
				return requestDelegate;
			}
		}
		/// <summary>
		/// Обработчик запроса обработки ошибки ноды
		/// </summary>
		public RequestDelegate NodeError
		{
			get
			{
				RequestDelegate requestDelegate = context =>
				{
					var nodeError = context.Request.ReadAsAsync<NodeError>().Result;
					_cluster.Send(nodeError);
					return context.Response.WriteAsync("OK");
				};
				return requestDelegate;
			}
		}
	}
}