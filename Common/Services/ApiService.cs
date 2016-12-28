using Microsoft.AspNetCore.WebUtilities;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Dynamic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using Common.Services.Interfaces;

namespace Common.Services
{
	/// <summary>
	/// 
	/// </summary>
    public class ApiService : IApiService
    {
        private TimeSpan _timeout;

        private static string _jsonMediaType = "application/json";
        private static string _formMediaType = "application/x-www-form-urlencoded";

        public ApiService(int timeout = 20000)
        {
            _timeout = TimeSpan.FromMilliseconds(timeout);
        }

		/// <summary>
		/// Отправка и парсинг GET запроса
		/// </summary>
		/// <typeparam name="T">Ожидаемый тип</typeparam>
		/// <param name="url">Урл</param>
		/// <param name="parameters">Параметры</param>
		public T GetRequest<T>(string url, Dictionary<string, string> parameters = null)
		{
			var requestUri = parameters == null ?
				new Uri(url) :
				new Uri(string.Format("{0}?{1}",
					url,
					string.Join("&",
						parameters.Keys
						.Where(key => !string.IsNullOrWhiteSpace(parameters[key]))
						.Select(key => string.Format("{0}={1}", WebUtility.HtmlEncode(key), WebUtility.HtmlEncode(parameters[key]))))
					)
				);

			var requestMessage = new HttpRequestMessage()
            {
                RequestUri = requestUri,
                Method = HttpMethod.Get
            };

            return SendRequest<T>(requestMessage);
        }

		/// <summary>
		/// Отправка и парсинг PUT или POST запроса
		/// </summary>
		/// <typeparam name="T">Отправляемый тип</typeparam>
		/// <typeparam name="V">Ожидаемый тип</typeparam>
		/// <param name="url">Урл</param>
		/// <param name="obj">объект для отправки</param>
		/// <param name="verb">глагол HTTP</param>
		/// <param name="isFormPost">Отправлять как JSON или формой</param>
		public V PutOrPostRequest<T, V>(string url, T obj, HttpMethod verb = null, bool isFormPost = false)
        {
            var requestContent = string.Empty;
            verb = verb == HttpMethod.Put ? HttpMethod.Put : HttpMethod.Post;
            var requestMessage = new HttpRequestMessage()
            {
                RequestUri = new Uri(url),
                Method = verb
            };

            var json = JsonConvert.SerializeObject(obj);

            if (isFormPost)
            {
                var jObj = (JObject)JsonConvert.DeserializeObject(json);
                var queryParams = String.Join("&",
                                jObj.Children().Cast<JProperty>()
                                .Select(jp => jp.Name + "=" + WebUtility.HtmlEncode(jp.Value.ToString())));
                requestMessage.Content = new StringContent(queryParams, Encoding.ASCII, _formMediaType);
            }
            else
            {
                requestMessage.Content = new StringContent(json, Encoding.UTF8, _jsonMediaType);
            }

            requestMessage.Headers.Clear();
            requestMessage.Content.Headers.ContentType = isFormPost ?
                new MediaTypeHeaderValue(_formMediaType) :
                new MediaTypeHeaderValue(_jsonMediaType);

            return SendRequest<V>(requestMessage);
        }
		

        /// <summary>
        /// Парсинг JSON
        /// </summary>
        /// <typeparam name="T">Тип</typeparam>
        /// <param name="json">строка для парсинга</param>
        private T Deserialize<T>(string json)
        {
            T retValue;
            var type = typeof(T);

            if (type == typeof(Decimal) || type == typeof(string))
            {
                json = System.Net.WebUtility.HtmlDecode(json);
                retValue = (T)Convert.ChangeType(json, typeof(T));
            }
            else
            {
                if (type == typeof(ExpandoObject))
                {
                    var converter = new ExpandoObjectConverter();
                    var obj = JsonConvert.DeserializeObject<T>(json, converter);
                    return obj;
                }
                retValue = JsonConvert.DeserializeObject<T>(json);
            }

            return retValue;
        }

		/// <summary>
		/// Отправка запроса
		/// </summary>
		/// <typeparam name="T">Тип результата</typeparam>
		/// <param name="requestMessage">Сообщение</param>
        private T SendRequest<T>(HttpRequestMessage requestMessage)
        {
            HttpResponseMessage response = null;
            var json = string.Empty;
            Stopwatch sw = new Stopwatch();
            sw.Start();

            try
            {
                using (var client = new HttpClient() { Timeout = _timeout })
                {
                    client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue(_jsonMediaType));

                    response = client.SendAsync(requestMessage).Result;
                    response.EnsureSuccessStatusCode();
                    Task<Stream> streamTask = response.Content.ReadAsStreamAsync();
                    Stream stream = streamTask.Result;
                    var sr = new StreamReader(stream);
                    json = sr.ReadToEnd();
                    var retValue = Deserialize<T>(json);
                    return retValue;
                }
            }
            catch (AggregateException ex)
            {
                if (ex.InnerException is TaskCanceledException)
                {
                     throw ex.InnerException;
                }
                else
                {
                    throw ex;
                }
            }
            catch (Exception ex)
            {
                
                throw ex;
            }
        }
    }
}
