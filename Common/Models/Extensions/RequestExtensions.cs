using System;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Internal;
using Newtonsoft.Json;

namespace Common.Models.Extensions
{
	/// <summary>
	/// Экстеншн для парсинга сообщений от ноды/кластера
	/// </summary>
    public static class MapperExtentions
    {
        public static async Task<string> ReadAsString(this HttpRequest request)
        {
            var initialBody = request.Body; 

            request.EnableRewind();
            var buffer = new byte[Convert.ToInt32(request.ContentLength)];
            await request.Body.ReadAsync(buffer, 0, buffer.Length);
            var body = Encoding.UTF8.GetString(buffer);
            request.Body = initialBody; 
            return body;
        }

        public static async Task<T> ReadAsAsync<T>(this HttpRequest request)
        {
            var json = await request.ReadAsString();
            T retValue = JsonConvert.DeserializeObject<T>(json);
            return retValue;
        }
    }
}