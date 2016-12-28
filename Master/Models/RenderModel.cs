using System.Collections.Generic;

namespace Cluster.Models
{
	/// <summary>
	/// Модель для рендера графа на фронте
	/// </summary>
	public class RenderModel
	{
		/// <summary>
		/// Список вершин
		/// </summary>
		public List<GraphNode> Nodes { get; set; }
		/// <summary>
		/// Список ребер
		/// </summary>
		public List<GraphLink> Links { get; set; }
	}
}
