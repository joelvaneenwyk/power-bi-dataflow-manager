using Microsoft.DataFlow.Services.Common;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;

namespace Microsoft.DataFlow.Services
{
    public interface IAuthService
    {
        Task<string> GetBearerToken();
    }

    public class AuthService : IAuthService
    {
        public AuthService(IConfiguration configuration)
        {
            _config = configuration;

        }

        private readonly IConfiguration _config;

        private string token { get; set; }

        public async Task<string> GetBearerToken()
        {
            if (!string.IsNullOrEmpty(token))
                return token;

            var dictBToken = new Dictionary<string, string>()
            {
                    { "grant_type", "password" },
                    { "username", _config["SvcUser"]},
                    { "password", _config["Password"]},
                    { "client_id", _config["ClientId"]},
                    { "client_secret", _config["ClientSecret"]},
                    { "resource", _config["Resource"]}
            };

            using var reqMessage = new HttpRequestMessage()
            {
                Method = HttpMethod.Post,
                RequestUri = new Uri("https://login.microsoftonline.com/common/oauth2/token"),
                Content = new FormUrlEncodedContent(dictBToken),

            };

            return JObject.Parse(await Util.GetRESTContent(reqMessage))["access_token"].ToString();
        }

    }
}
