using System;
using System.Collections.Generic;
using System.Linq;
using Common.Models;
using Microsoft.AspNetCore.Razor.TagHelpers;
using Microsoft.DotNet.ProjectModel;

namespace Cluster.TagHelpers
{
    public class NodeListTagHelper : TagHelper
    {
        public List<NodeInfo> Elements { get; set; }

        public override void Process(TagHelperContext context, TagHelperOutput output)
        {
            output.TagName = "ul";
            string listContent = "";

            foreach (NodeInfo info in Elements)
            {
	            string errors = info.Errors == null
		            ? string.Empty
		            : info.Errors.Aggregate(string.Empty, (current, error) => current + ("<li>" + error + "</li>"));

	            string nodeInfo = $@"<p>Host: <b>{info.Name}</b></p>
                                    <p>Состояние: <b>{info.Status}</b></p>
                                    <p>Акторов: <b>{info.ActorsCount}</b></p>
                                    <p>Ошибки: "+errors+"</p>";

                listContent += "<li>" + nodeInfo + "</li>";
            }
            output.Content.SetHtmlContent(listContent);
        }
    }
}
