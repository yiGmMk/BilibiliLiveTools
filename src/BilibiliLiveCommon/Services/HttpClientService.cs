﻿using BilibiliLiveCommon.Model.Enums;
using BilibiliLiveCommon.Services.Interface;
using BilibiliLiver.Utils;
using Microsoft.Extensions.Logging;
using System.Net;
using System.Net.Http.Headers;
using System.Reflection;

namespace BilibiliLiveCommon.Services
{
    public class HttpClientService : IHttpClientService
    {
        private readonly ILogger<HttpClientService> _logger;
        private readonly IBilibiliCookieService _bilibiliCookie;

        public HttpClientService(ILogger<HttpClientService> logger
            , IBilibiliCookieService bilibiliCookie)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _bilibiliCookie = bilibiliCookie ?? throw new ArgumentNullException(nameof(bilibiliCookie));
        }

        public async Task<T> Execute<T>(string url, HttpMethod method, object body = null, BodyFormat format = BodyFormat.Json, bool withCookie = true) where T : class
        {
            using (HttpClient httpClient = new HttpClient(new HttpClientHandler()
            {
                ServerCertificateCustomValidationCallback = delegate { return true; },
            })
            {
                Timeout = TimeSpan.FromSeconds(60)
            })
            {

                httpClient.DefaultRequestHeaders.Add("origin", "https://www.bilibili.com");
                httpClient.DefaultRequestHeaders.Add("referer", "https://www.bilibili.com/");
                httpClient.DefaultRequestHeaders.Add("user-agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/100.0.4896.75 Safari/537.36");
                httpClient.DefaultRequestHeaders.Add("accept", "*/*");
                httpClient.DefaultRequestHeaders.Add("cache-control", "no-cache");
                if (withCookie)
                {
                    httpClient.DefaultRequestHeaders.Add("cookie", _bilibiliCookie.Get());
                }

                HttpResponseMessage response = null;
                if (method == HttpMethod.Post)
                {
                    if (body == null)
                    {
                        throw new ArgumentNullException(nameof(body));
                    }
                    switch (format)
                    {
                        case BodyFormat.Json:
                            {
                                string postData = null;
                                if (body.GetType() == typeof(string))
                                {
                                    postData = body.ToString();
                                }
                                else
                                {
                                    postData = JsonUtil.SerializeObject(body);
                                }
                                using (StringContent content = new StringContent(postData))
                                {
                                    content.Headers.ContentType = new MediaTypeHeaderValue("application/json");
                                    httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue(content.Headers.ContentType.MediaType));
                                    response = await httpClient.PostAsync(url, content);
                                }
                            }
                            break;
                        case BodyFormat.Form:
                            {
                                if (body.GetType() == typeof(string))
                                {
                                    throw new Exception("When post body format is form, body can not string.");
                                }
                                Dictionary<string, string> @params = ObjectToMap(body);
                                if (@params.Count == 0)
                                {
                                    throw new ArgumentNullException("Cannot convert body to dictionary data.", nameof(body));
                                }
                                using (MultipartFormDataContent content = new MultipartFormDataContent())
                                {
                                    foreach (var item in @params)
                                    {
                                        content.Add(new StringContent(item.Value), item.Key);
                                    }
                                    httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue(content.Headers.ContentType.MediaType));
                                    response = await httpClient.PostAsync(url, content);
                                }
                            }
                            break;
                        case BodyFormat.Form_UrlEncoded:
                            {
                                if (typeof(bool) == typeof(string))
                                {
                                    throw new Exception("When post body format is form-urlencoded, body can not string.");
                                }
                                Dictionary<string, string> @params = ObjectToMap(body);
                                using (FormUrlEncodedContent content = new FormUrlEncodedContent(@params))
                                {
                                    httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue(content.Headers.ContentType.MediaType));
                                    response = await httpClient.PostAsync(url, content);
                                }
                            }
                            break;
                    }
                }
                else if (method == HttpMethod.Get)
                {
                    response = await httpClient.GetAsync(url);
                }
                else
                {
                    throw new Exception($"Not support http method: {method.ToString()}");
                }
                if (response == null)
                {
                    throw new Exception($"Http request failed, url: {url}, method: {method},  result is null.");
                }
                string resultStr = await TryGetStringResponse(response);
                if (response.StatusCode != HttpStatusCode.OK)
                {
                    throw new Exception($"Http request failed, url: {url}, method: {method}, status code: {response.StatusCode}, result: {(string.IsNullOrEmpty(resultStr) ? "null" : resultStr)}");
                }
                response.Dispose();

                string data = resultStr.Replace("\"data\":[]", "\"data\":null");
                return JsonUtil.DeserializeJsonToObject<T>(data);
            }
        }

        /// <summary>
        /// Object转化为dictionory
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="isIgnoreNull"></param>
        /// <returns></returns>
        private Dictionary<string, string> ObjectToMap(object obj, bool isIgnoreNull = false)
        {
            Dictionary<string, string> map = new Dictionary<string, string>();
            if (obj == null)
            {
                return map;
            }
            Type t = obj.GetType();
            PropertyInfo[] pi = t.GetProperties(BindingFlags.Public | BindingFlags.Instance); // 获取当前type公共属性
            foreach (PropertyInfo p in pi)
            {
                MethodInfo m = p.GetGetMethod();
                if (m != null && m.IsPublic)
                {
                    // 进行判NULL处理 
                    if (m.Invoke(obj, new object[] { }) != null || !isIgnoreNull)
                    {
                        string value = m.Invoke(obj, new object[] { }).ToString();
                        map.Add(p.Name, string.IsNullOrEmpty(value) ? null : value); // 向字典添加元素
                    }
                }
            }
            return map;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="content"></param>
        /// <returns></returns>
        private async Task<string> TryGetStringResponse(HttpResponseMessage content)
        {
            if (content == null)
            {
                return null;
            }
            try
            {
                string resultStr = await content.Content.ReadAsStringAsync();
                return resultStr;
            }
            catch { }
            return null;
        }
    }
}
