using Common.Models.Responses;
using Microsoft.AspNetCore.Razor.TagHelpers;

namespace Cluster.TagHelpers
{
    public class ProgressInfoTagHelper : TagHelper
    {
        public ProgressInfoResponse Info { get; set; }

        public override void Process(TagHelperContext context, TagHelperOutput output)
        {
            output.TagName = "div";

            string progressInfo = $@"<p>Всего: <b>{Info.All}MB</b></p>
                                    <p>Осталось: <b>{Info.Remain}MB</b></p>
                                    <p>Осталось времени: <b>{Info.RemainTime}мин</b></p>
                                    <p>Скорость: <b>{Info.Speed}MB/c</b></p>
									<p>Ноды:</p>";

            output.Content.SetHtmlContent(progressInfo);
        }
    }
}
