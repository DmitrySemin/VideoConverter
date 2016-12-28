using Cluster.Models;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;

using Common.Models;
using Common.Models.Responses;
using NLog;

namespace Cluster.Controllers
{
    public class HomeController : Controller
    {
	    private const string CLUSTER_NAME = "Cluster";
        private readonly Common.Models.Cluster _cluster;
		private readonly Logger _logger = LogManager.GetCurrentClassLogger();

		public HomeController(Common.Models.Cluster cluster)
        {
            _cluster = cluster;
        }

        public IActionResult Index()
        {
			ProgressInfoResponse masterState = new ProgressInfoResponse()
            {
                All = _cluster.AllBytes / 1048576, //считаем в мегабайтах,
				Remain = _cluster.Remain / 1048576, //считаем в мегабайтах,
				RemainTime = _cluster.RemainTime / 60, //считаем в минутах,
				Speed = _cluster.Speed / 1048576, //считаем в мегабайтах/сек,
				Nodes = _cluster.AllNodes.Values.ToList()
            };

            return View(masterState);
        }

		/// <summary>
		/// Отрисовка графа нод
		/// </summary>
		[HttpGet]
		public JsonResult Render()
		{
			try
			{
				var response = new RenderModel()
				{
					Nodes = new List<GraphNode>()
					{
						new GraphNode
						{
							Id = CLUSTER_NAME,
							Group = 0
						}
					},
					Links = new List<GraphLink>()
				};

				if (_cluster.AllNodes != null)
				{
					int i = 1;

					foreach (var node in _cluster.AllNodes.Values)
					{
						response.Nodes.Add( new GraphNode()
						{
							Id = node.Name,
							Group = i
						});

						response.Links.Add(new GraphLink()
						{
							Source = CLUSTER_NAME,
							Target = node.Name,
							Value = 9
						});

						for (var k = 0; k < node.ActorsCount; k++)
						{
							var clusterNodeName = string.Format("Actor #{0} of node {1}", k, node.Name);
							response.Nodes.Add(new GraphNode()
							{
								Id = clusterNodeName,
								Group = i
							});

							response.Links.Add(new GraphLink()
							{
								Source = node.Name,
								Target = clusterNodeName,
								Value = 1
							});
						}

						i++;
					}
				}

				return Json(response);
			}
			catch (Exception e)
			{
				_logger.Error(e);
				return Json(null);
			}
		}
	}
}
