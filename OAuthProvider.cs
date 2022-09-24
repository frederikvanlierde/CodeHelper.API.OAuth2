using System.Net.Http.Headers;
using CodeHelper.Core.PlaceHolder;
using CodeHelper.Core.Extensions;
using System.Net.Http;
using System.Threading.Tasks;
using System.Collections.Generic;
using System;
using System.Text.Json;

namespace CodeHelper.Core.OAuth2
{
    public class OAuthProvider
    {
        #region Properties
        protected readonly HttpClient _httpClient = new();
        public string Endpoint { get; set; } = "";
        [Placeholder("{SCOPE}")]        public string Scope { get; set; } = "";        
        [Placeholder("{CLIENTID}")]     public string ClientId { get; set; } = "";
        [Placeholder("{CLIENTSECRET}")] public string ClientSecret { get; set; } = "";
        [Placeholder("{STATE}")] public string State { get; set; } = "";
        [Placeholder("{RESPONSETYPE}")] public string ResponseType { get; set; } = "";
        [Placeholder("{REDIRECTURL}")] public string RedirectUri { get; set; } = "";
        
        public string OAuthCode { get; set; } = "";
        public string AccessToken { get; set; } = "";
        public string ApiKey { get; set; } ="";
        public string GrantType { get; set; } = "";
        public List<(string, string)> ExtraRequestHeaders = new();
        #endregion

        #region Constructors
        public OAuthProvider() { }
        #endregion

        #region Public Methods
        protected virtual void SetAuthorizationHeader()
        {
            foreach (var _extra in ExtraRequestHeaders)
            {
                if (_httpClient.DefaultRequestHeaders.Contains(_extra.Item1))
                    _httpClient.DefaultRequestHeaders.Remove(_extra.Item1);
                _httpClient.DefaultRequestHeaders.Add(_extra.Item1, _extra.Item2);
            }
            if (_httpClient.DefaultRequestHeaders.Contains("Authorization"))
                _httpClient.DefaultRequestHeaders.Remove("Authorization");
            _httpClient.DefaultRequestHeaders.Add("Authorization", "Bearer " + this.AccessToken);
        }
        public async Task<string> GetJson(string apiURL)
        {
            SetAuthorizationHeader();
            var _x = await _httpClient.GetStringAsync(apiURL);
            return _x;
        }

        public async Task<T> GetJson<T>(string apiURL) where T : new()
        {
            SetAuthorizationHeader();
            return (JsonSerializer.Deserialize<T>(await _httpClient.GetStringAsync(apiURL.CleanUrl())) ?? new T());
        }
        public async Task<string> PostJson(string apiURL, HttpContent data)
        {
            SetAuthorizationHeader();
            var _task = await _httpClient.PostAsync(apiURL.CleanUrl(), data);
            string _result = await _task.Content.ReadAsStringAsync();
            return _result;
        }
        public async Task<T> PostJson<T>(string apiURL, HttpContent data) where T : new()
        {
            SetAuthorizationHeader();
            var _task = await _httpClient.PostAsync(apiURL.CleanUrl(), data);
            string _result = await _task.Content.ReadAsStringAsync();
            return (JsonSerializer.Deserialize<T>(_result) ?? new T());
        }

        public async Task DeleteRequest(string apiURL)
        {
            SetAuthorizationHeader();
            var _task = await _httpClient.DeleteAsync(apiURL.CleanUrl());            
        }

        public virtual string GetOAuthTokenUrl()
        {
            return this.Endpoint.Replace(this);
        }
        public virtual async Task GetAccessToken()
        {
            AccessTokenResponse token = new();
            try
            {
                _httpClient.DefaultRequestHeaders.Accept.Clear();
                _httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/x-www-form-urlencoded"));

                if(!string.IsNullOrEmpty(ApiKey))
                    _httpClient.DefaultRequestHeaders.Add("apikey", ApiKey);

                _httpClient.DefaultRequestHeaders.Add("Authorization", "Basic " + Convert.ToBase64String(System.Text.ASCIIEncoding.ASCII.GetBytes($"{this.ClientId}:{this.ClientSecret}")));
               
                var content = new FormUrlEncodedContent(new[]
                {
                    new KeyValuePair<string, string>("grant_type", this.GrantType),
                    new KeyValuePair<string, string>("code", this.OAuthCode),
                    new KeyValuePair<string, string>("redirect_uri", this.RedirectUri)
                });
                
                HttpResponseMessage tokenResponse = _httpClient.PostAsync(Endpoint, content).Result;
                string _result = await tokenResponse.Content.ReadAsStringAsync();
                token = (JsonSerializer.Deserialize<AccessTokenResponse>(_result) ?? new AccessTokenResponse());
            }
            catch
            {
                throw;
            }

            this.AccessToken = token.AccessToken;
        }        
        #endregion

        private void AddHeader(string name, string value)
        {
            if (_httpClient.DefaultRequestHeaders.Contains(name))
                _httpClient.DefaultRequestHeaders.Remove(name);
            _httpClient.DefaultRequestHeaders.Add(name, value);
        }
    }
}