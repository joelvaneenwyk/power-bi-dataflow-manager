using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace Microsoft.DataFlow.Services.Common
{
    public static class Util
    {

        public static HttpRequestMessage GetRequestMessage(HttpMethod method, string requestURI, Dictionary<string,string> formContent)
        {
            return new HttpRequestMessage() {
                Method = method,
                RequestUri = new Uri(requestURI),
                Content = new FormUrlEncodedContent(formContent)
            };
        }

        public static HttpRequestMessage GetRequestMessage(HttpMethod method, string requestURI)
        {
            return new HttpRequestMessage() {
                Method = method,
                RequestUri = new Uri(requestURI)
            };
        }

        public static async Task<string> GetRESTContent(this HttpClient thisClient, HttpRequestMessage requestMessage)
        {
            using var resp = await thisClient.SendAsync(requestMessage);
            return await resp.Content.ReadAsStringAsync();
        }

        public static async Task<string> GetRESTContent(HttpRequestMessage requestMessage)
        {
            using var newClient = new HttpClient();
            return await newClient.GetRESTContent(requestMessage);
        }

        public static HttpResponseMessage RESTResponse<T>(T model)
        {

            HttpStatusCode httpStatusCode;

            if (model is Exception)
                httpStatusCode = HttpStatusCode.InternalServerError;
            else
                httpStatusCode = HttpStatusCode.OK;

            return GenerateRequestMessage(model, httpStatusCode);
        }

        private static HttpResponseMessage GenerateRequestMessage<T>(T model, HttpStatusCode statusCode)
        {
            return new HttpResponseMessage(statusCode) { Content = GenerateJSONContent(model) };
        }

        public static StringContent GenerateJSONContent<T>(T model)
        {
            return new StringContent(JsonConvert.SerializeObject(model),
                                                 Encoding.UTF8,
                                                 "application/json");
        }

    }
}