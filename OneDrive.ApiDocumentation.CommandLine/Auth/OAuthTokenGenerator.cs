﻿using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.IO;
using System.Threading.Tasks;

namespace OneDrive.ApiDocumentation.ConsoleApp
{
    public class OAuthTokenGenerator
    {

        private static string GetVariable(string name)
        {
            var value = Environment.GetEnvironmentVariable(name);
            if (string.IsNullOrEmpty(value))
            {
                Console.WriteLine("No value for variable {0}", name);
            }
            return value;
        }


        /// <summary>
        /// Uses environment variables to pull refresh token from service
        /// </summary>
        /// <returns></returns>
        public static async Task<TokenResponse> RedeemRefreshTokenFromEnvironment()
        {
            string clientId = GetVariable("oauth-client-id");
            string clientSecret = GetVariable("oauth-client-secret");
            string tokenService = GetVariable("oauth-token-service");
            string redirectUri = GetVariable("oauth-redirect-uri");
            string refreshToken = GetVariable("oauth-refresh-token");

            try
            {
                return await RedeemRefreshToken(tokenService, refreshToken, clientId, clientSecret, redirectUri);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error redeeming token: " + ex.Message);
                return null;
            }
        }


        public static async Task<TokenResponse> RedeemRefreshToken(string tokenService, string refreshToken, string clientId, string clientSecret, string redirectUri)
        {
            HttpWebRequest request = HttpWebRequest.CreateHttp(tokenService);
            request.ContentType = "application/x-www-form-urlencoded";
            request.Method = "POST";

            var requestStream = await request.GetRequestStreamAsync();
            var writer = new StreamWriter(requestStream);

            var content = string.Format("client_id={0}&redirect_uri={1}&client_secret={2}&refresh_token={3}&grant_type=refresh_token", 
                Uri.EscapeDataString(clientId),
                Uri.EscapeDataString(redirectUri),
                Uri.EscapeDataString(clientSecret),
                Uri.EscapeDataString(refreshToken));

            await writer.WriteAsync(content);
            await writer.FlushAsync();

            var response = await request.GetResponseAsync() as HttpWebResponse;
            if (null != response && response.StatusCode == HttpStatusCode.OK)
            {
                var responseStream = response.GetResponseStream();
                var reader = new StreamReader(responseStream);
                return JsonConvert.DeserializeObject<TokenResponse>(await reader.ReadToEndAsync());
            }
            else
            {
                return null;
            }
        }
    }

    public class TokenResponse
    {
        [JsonProperty("access_token")]
        public string AccessToken { get; set; }
        [JsonProperty("refresh_token")]
        public string RefreshToken { get; set; }
    }
}