using Salesforce.Common;
using Salesforce.Common.Models.Json;
using Salesforce.Force;
using System;
using System.Threading.Tasks;

namespace UpdateSalesforceData
{
    public class SalesforceHttpClient
    {
        public class SalesforceSettings
        {
            public string ConsumerKey { get; set; }
            public string ConsumerSecret { get; set; }
            public string Username { get; set; }
            public string Password { get; set; }
            public string SecurityToken { get; set; }
            public string Domain { get; set; }
        }


        #region Static

        private static volatile string _accessToken = null;
        private static volatile string _instanceUrl;
        private static volatile string _apiVersion;

        private static volatile bool _accessTokenValid = false;

        private static object _syncRoot = new object();

        private static SalesforceSettings _settings;

        //public static void Init(SalesforceSettings settings)
        //{
        //    _settings = settings;
        //}

        public static void RefreshAccessToken()
        {
            if (!_accessTokenValid)
            {
                lock (_syncRoot)
                {
                    if (!_accessTokenValid)
                    {
                        var token = Authenticate().Result;
                        _instanceUrl = token.InstanceUrl;
                        _apiVersion = "v32.0";// token.ApiVersion;
                        _accessToken = token.AccessToken;

                        _accessTokenValid = true;
                    }
                }
            }
        }

        private static void InvalidateToken()
        {
            _accessTokenValid = false;
        }

        private static async Task<AuthToken> Authenticate()
        {
            using (var authClient = new AuthenticationClient())
            {
                await authClient.UsernamePasswordAsync(
                    _settings.ConsumerKey,
                    _settings.ConsumerSecret,
                    _settings.Username,
                    _settings.Password + _settings.SecurityToken,
                    _settings.Domain + "/services/oauth2/token"
                ).ConfigureAwait(false);

                return new AuthToken
                {
                    InstanceUrl = authClient.InstanceUrl,
                  //  ApiVersion = authClient.ApiVersion,
                    AccessToken = authClient.AccessToken
                };
            }
        }

        #endregion

        private ForceClient _client;

        //public SalesforceHttpClient()
        //{
        //    RefreshAccessToken();
        //    _client = new ForceClient(_instanceUrl, _accessToken, _apiVersion);
        //}

        private async Task<T> ExecWithRetry<T>(Func<Task<T>> op)
        {
            try
            {
                return await op().ConfigureAwait(false);
            }
            catch (ForceException e)
            {
                if (true) //if (e.Message == "todo - need to only do this if it's an auth error")
                {
                    InvalidateToken();
                    RefreshAccessToken();
                    _client = new ForceClient(_instanceUrl, _accessToken, _apiVersion);
                    return await op().ConfigureAwait(false);
                }
                throw;
            }
        }

        public async Task<QueryResult<T>> QueryAsync<T>(string soql)
        {
            return await ExecWithRetry<QueryResult<T>>(
                async () => await _client.QueryAsync<T>(soql).ConfigureAwait(false)
            ).ConfigureAwait(false);
        }

        
        public async Task<QueryResult<T>> QueryContinuationAsync<T>(string soql)
        {
            return await ExecWithRetry<QueryResult<T>>(
                async () => await _client.QueryContinuationAsync<T>(soql).ConfigureAwait(false)
            ).ConfigureAwait(false);
        }

        //public async Task<QueryResult<T>> QueryAllAsync<T>(string soql)
        //{
        //    return await ExecWithRetry<QueryResult<T>>(
        //        async () => await _client.QueryAllAsync<T>(soql).ConfigureAwait(false)
        //    ).ConfigureAwait(false);
        //}

        //public async Task<SuccessResponse> UpsertExternalAsync<T>(string objectName, string externalFieldName, string externalId, T record)
        //{
        //    return await ExecWithRetry<SuccessResponse>(
        //        async () => await _client.UpsertExternalAsync(objectName,  externalFieldName, externalId, record).ConfigureAwait(false)
        //    ).ConfigureAwait(false);
        //}

        //public async Task<SuccessResponse> CreateAsync<T>(string objectName, T obj)
        //{
        //    return await ExecWithRetry<SuccessResponse>(
        //        async () => await _client.CreateAsync(objectName, obj).ConfigureAwait(false)
        //    ).ConfigureAwait(false);
        //}

        //public async Task<bool> DeleteAsync<T>(string objectName, string recordId)
        //{
        //    return await ExecWithRetry<bool>(
        //        async () => await _client.DeleteAsync(objectName, recordId).ConfigureAwait(false)
        //    ).ConfigureAwait(false);
        //}

        //public async Task<SuccessResponse> UpdateAsync<T>(string objectName, string id, T obj)
        //{
        //    return await ExecWithRetry<SuccessResponse>(
        //        async () => await _client.UpdateAsync(objectName, id, obj).ConfigureAwait(false)
        //    ).ConfigureAwait(false);
        //}


    }
}
