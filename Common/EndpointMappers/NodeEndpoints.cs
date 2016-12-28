using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using Microsoft.AspNetCore.Http;
using Common.Messages;
using Common.Models.Extensions;
using Common.Services.Interfaces;

namespace Common.EndpointMappers
{
	/// <summary>
	/// Конечные точки ноды - интерфейс взаимодействия с кластером
	/// </summary>
	public class NodeEndpoints : IEndpoints
    {
        private IStatusService<StatusMessage> _workerStatus;
        private string _workerRoute = "api/worker";

        public NodeEndpoints(IStatusService<StatusMessage> workerStatus)
        {
            _workerStatus = workerStatus;
        }
        
        public void MapEndpoints(IApplicationBuilder app)
        {
            var routeBuilder = new RouteBuilder(app);
			
			// Прослушиваем запросы на получение задания из очереди. 
			// сюда же можно добавить управление нодами - например обновление 
			var processFileUrl = $"{_workerRoute}/processfile";
           // var getMessageUrl = $"{_workerRoute}/{{message}}";
			
            routeBuilder.MapPost(processFileUrl, ProcessFile);
           // routeBuilder.MapGet(getMessageUrl, GetMessage);

            var routes = routeBuilder.Build();
            app.UseRouter(routes);
        }
		
		/// <summary>
		/// Обработчик запроса получения таска на обработку
		/// </summary>
        public RequestDelegate ProcessFile
        {
            get
            {
                RequestDelegate requestDelegate = context =>
                {
                    var fileInfo = context.Request.ReadAsAsync<ProcessFileInfo>().Result;
                    var workerMessage = new StatusMessage()
                    {
                        Source = "cluster",
                        PayLoadType = typeof(ProcessFileInfo),
                        PayLoad = fileInfo
                    };
                    _workerStatus.AddStatus(workerMessage);
                    return context.Response.WriteAsync("OK");
                };
                return requestDelegate;
            }
        }
        
        public RequestDelegate GetMessage
        {
            get
            {
                RequestDelegate requestDelegate = context =>
                {
                    var message = (string)context.GetRouteValue("message");
                    var workerMessage = new StatusMessage();
                    workerMessage.Message = $"Receive message, {message}!";
                    _workerStatus.AddStatus(workerMessage);
                    return context.Response.WriteAsync($"Receive message, {message}!");
                };
                return requestDelegate;
            }
        }
    }
}