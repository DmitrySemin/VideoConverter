using System.Collections.Generic;
using System.Net.Http;

namespace Common.Services.Interfaces
{
    public interface IApiService
    {
		/// <summary>
		/// Отправка и парсинг GET запроса
		/// </summary>
		T GetRequest<T>(string url, Dictionary<string, string> kvp = null);
		/// <summary>
		/// Отправка и парсинг PUT или POST запроса
		/// </summary>
		V PutOrPostRequest<T, V>(string url, T obj, HttpMethod verb = null, bool isFormPost = false);
    }
}
