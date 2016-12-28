using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Cluster.Models
{
	/// <summary>
	/// Модель для рендеринга ребра графа
	/// </summary>
	public class GraphLink
	{
		public string Source { get; set; }
		public string Target { get; set; }
		public int Value { get; set; }
	}
}
