using Salesforce.Common;
using Salesforce.Common.Models.Json;
using Salesforce.Common.Models.Xml;
using Salesforce.Force;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Web;

namespace SalesforceDataLibrary
{
    public class SalesforceClient
    {
        private static SalesforceSettings _settings;

        private static StringBuilder _emailLog;
        private static StringBuilder _emailErrorLog;

        private static SalesforceClient salesforceClient;
        public static SalesforceClient GetSalesforceConnection(
            SalesforceSettings salesforceSettings, StringBuilder emailLog = null, StringBuilder emailErrorLog = null)//;, System.Text)//.StringBuilder emailTransactionLog, System.Text.StringBuilder emailTransactionLog)
        {
            _emailLog = emailLog;
            _emailErrorLog = emailErrorLog;
            
            if (salesforceClient != null) return salesforceClient;

            Init(salesforceSettings, _emailLog, _emailErrorLog);
            salesforceClient = new SalesforceClient();
            return salesforceClient;
        }

        public static Stopwatch TimeWatcherFull = new Stopwatch();
        public static Stopwatch TimeWatcher = new Stopwatch();
        public static Stopwatch TimeWatcherInternal = new Stopwatch();

        private static CustomSObjectList<CustomSObject> batch;
        //private static StringBuilder sb;
        //private static StringBuilder sberror;

        private int batchNumber;

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

        public class DataToDeleteById
        {
            public string Id { get; set; }
        }

        public static void Init(SalesforceSettings settings, StringBuilder emailLog, StringBuilder emailErrorLog)
        {
            _settings = settings;
            _emailLog = emailLog;
            _emailErrorLog = emailErrorLog;
        }

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

        private static ForceClient _client;
        private static ForceClient client
        {
            get
            {
                if (_client == null)
                {
                    RefreshAccessToken();
                    _client = new ForceClient(_instanceUrl, _accessToken, _apiVersion);
                }
                return _client;
            }
        }
        
        private static async Task<T> ExecWithRetry<T>(Func<Task<T>> op)
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
                async () => await client.QueryAsync<T>(soql).ConfigureAwait(false)
            ).ConfigureAwait(false);
        }

        public async Task<QueryResult<T>> QueryContinuationAsync<T>(string soql)
        {
            return await ExecWithRetry<QueryResult<T>>(
                async () => await client.QueryContinuationAsync<T>(soql).ConfigureAwait(false)
            ).ConfigureAwait(false);
        }

        public static BatchResultList BulkUpsertExternalAsync(string objectName, string externalFieldName, List<CustomSObjectList<CustomSObject>> batchList)
        {
            return client.RunJobAndPollAsync(objectName, externalFieldName, BulkConstants.OperationType.Upsert, batchList).GetAwaiter().GetResult().First();;
        }

        public List<T> GetRecordsFromSalesforce<T>(string objective,string query)
        {
            LogHelper.Logger.WriteOutput(string.Concat("Querying for: ", objective), _emailLog);
            
            List<T> result = new List<T>();
            //try
            //{
            var queryString = string.Concat(query);

            QueryResult<T> records = null;

            Task.Run(async () =>
            {
                records = await client.QueryAsync<T>(queryString);
                var finished = false;
                do
                {
                    finished = records.Done;

                    // add any records
                    if (records.Records.Any())
                        result.AddRange(records.Records);

                    // if more
                    if (!finished)
                    {
                        // get next batch
                        records = await client.QueryContinuationAsync<T>(records.NextRecordsUrl);
                    }
                } while (!finished);
            }).Wait(Timeout.InfiniteTimeSpan);

            return result;
        }


        public void SanatizeObjects(CustomSObjectList<CustomSObject> CustomSObjectList)
        {
            foreach (var o in CustomSObjectList)
                foreach (KeyValuePair<string, object> p in o.Cast<KeyValuePair<string, object>>().Where(p => p.Value is string).ToList())
                    o[p.Key] = (object)HttpUtility.HtmlEncode(p.Value);
        }

        public void BulkUpsertFromSQL(string name,string objectName, string externalFieldName, bool async,
         Func<SqlDataReader> GetSQLReader, Func<SqlDataReader, CustomSObject> parser, Func<SqlDataReader, string> Logger = null,
         int batchSize = 200)
        {
            TimeWatcherFull.Restart();

            LogHelper.Logger.WriteOutput(string.Concat(Environment.NewLine, "Started Sync of: ", name, " - " , objectName), _emailLog);

            TimeWatcherInternal.Restart();

            LogHelper.Logger.WriteOutput(string.Concat("Started SQL Query for Data to Sync with Salesforce"), _emailLog);
            var sqlReader = GetSQLReader();
            LogHelper.Logger.WriteOutput(string.Concat("Ended SQL Query for Data to Sync with Salesforce: ", TimeWatcherInternal.ElapsedMilliseconds, "ms"), _emailLog);

            batch = new CustomSObjectList<CustomSObject>();
            //sb = new StringBuilder();
            //sberror = new StringBuilder();

            batchNumber = 1;

            //var taskList = new List<List<Task>>();
            //var subTaskList = new List<Task>();
            //taskList.Add(subTaskList);

            int allRecordsCounter = 0;
            //int validRecordsCounter = 0;
            
            //var batchSize = 200;

            TimeWatcherInternal.Restart();
            //Stopwatch sw = new Stopwatch();

            while (sqlReader.Read())
            {
                var recordToUpsert = parser(sqlReader);
                if (recordToUpsert == null) continue;
                
                 allRecordsCounter++;

                if (Logger != null && !string.IsNullOrEmpty(Logger(sqlReader)))
                    LogHelper.Logger.WriteOutput(Logger(sqlReader), _emailErrorLog);

                if (!async) {
                    UpsertAsync(objectName, externalFieldName, recordToUpsert, out string error);
                    if (error != null)
                    {
                        LogHelper.Logger.WriteOutput(error, _emailErrorLog);
                        //LogHelper.Logger.WriteOutput(Newtonsoft.Json.JsonConvert.SerializeObject(recordToUpsert), _emailErrorLog);
                    }
                    continue;
                }

                if (batch.Count >= batchSize) { ExecuteSavingBatch(objectName, externalFieldName, async,  _emailLog, _emailErrorLog); }

                batch.Add(recordToUpsert); 
            }

            //the last batch
            if (batch.Any()) { ExecuteSavingBatch(objectName, externalFieldName, async, _emailLog, _emailErrorLog); }

            sqlReader.Close();
            batch = null;

            LogHelper.Logger.WriteOutput($"Full Batch of ({objectName}) Ended - Execution Time: {TimeWatcherFull.ElapsedMilliseconds} ms.", _emailLog);

            //_emailLog.Append(_emailLog);
            //_emailErrorLog.Append(_emailLog);
        }

        private void ExecuteSavingBatch(string objectName, string externalFieldName, bool async, StringBuilder _emailLog = null, StringBuilder _emailErrorLog = null)
        {
            LogHelper.Logger.WriteOutput(string.Concat("Batch Creation: ", batchNumber, " ", TimeWatcherInternal.ElapsedMilliseconds, "ms"), _emailLog);
            
            TimeWatcherInternal.Restart();
            var result = UpdateBatch(objectName, externalFieldName, batchNumber, async);
            batch = new CustomSObjectList<CustomSObject>();

            if (result != null)
            {
                result.Items.ToList().ForEach(res =>
                {
                    if (res.Created)
                    {
                        LogHelper.Logger.WriteOutput(string.Concat(Environment.NewLine, "*New Record*: ", objectName, " -  id: ", res.Id), _emailLog);
                    }

                    if (!res.Success)
                    {
                        LogHelper.Logger.WriteOutput(string.Concat(Environment.NewLine, "Error Upserting: ", objectName, " - id: ", res.Id, " - Error: ",
                            $"CODE: {res.Errors.StatusCode}, fields: ({string.Join(", ", res.Errors.Fields)}) => {res.Errors.Message}"), _emailErrorLog);
                    ///////LogHelper.Logger.WriteOutput(batch.Output.ToString(), _emailErrorLog);
                }
                });
            }
            

            result = null;

            batchNumber++;

            //subTaskList.Add(UpdateBatch(objectName, externalFieldName, batch, batchNumber, async));

            
            TimeWatcherInternal.Restart();
        }


        //CustomSObjectList<CustomSObject> Batch,
        private BatchResultList UpdateBatch(string objectName, string externalFieldName,  int batchNumber, bool async,
            StringBuilder _emailLog = null, StringBuilder _emailErrorLog = null)
        {
            LogHelper.Logger.WriteOutput(String.Concat("Start Saving to Salesforce - subBatch (",objectName,") - ",batchNumber), _emailLog);

            TimeWatcher.Restart();

            SanatizeObjects(batch);
            var result = BulkUpsertExternalAsync(objectName, externalFieldName, new List<CustomSObjectList<CustomSObject>> { batch });
            
            //var result = BulkUpsertExternal(objectName, externalFieldName, Batch, async).Result;
            TimeWatcher.Stop();

            LogHelper.Logger.WriteOutput(
                String.Concat("End Saving to Salesforce - subBatch (",objectName, ") - Execution Time: ",TimeWatcher.ElapsedMilliseconds," ms. for ",batch.Count), _emailLog);

            batch = null;

            return result;
        }



        public SuccessResponse UpdateAsync<T>(string objectName, string id, T obj)
        {
            Task<SuccessResponse> successResponse = null;

            Task.Run(() =>
            {
                LogHelper.Logger.WriteOutput(string.Concat("Updating/serting: ", objectName, " - ", id), _emailLog);
                successResponse = UpdateExternalAsync(objectName, id, obj);
            }).Wait(Timeout.InfiniteTimeSpan);
         
            return successResponse.Result;
        }

        private async Task<SuccessResponse> UpdateExternalAsync<T>(string objectName, string id, T obj)
        {
            return await ExecWithRetry<SuccessResponse>(
                async () => await client.UpdateAsync(objectName, id, obj).ConfigureAwait(false)
            ).ConfigureAwait(false);
        }

        public SuccessResponse UpsertAsync(string objectName, string externalFieldName, CustomSObject obj, out string error)
        {
            Task<SuccessResponse> successResponse = null;
            var id = obj[externalFieldName].ToString();
            error = null;

            try
            {
                Task.Run(() =>
                {
                    LogHelper.Logger.WriteOutput(string.Concat(Environment.NewLine,"Upserting: ", objectName, " - ", id), _emailLog);
                    obj.Remove(externalFieldName);
                    successResponse = UpsertExternalAsync(objectName, externalFieldName, id, obj);
                }).Wait(Timeout.InfiniteTimeSpan);

                return successResponse.Result;
            }
            catch (Exception ex)
            {
                error = string.Concat(Environment.NewLine, "Error Upserting: ", objectName, " - external id: ", id, " - Error: ", ex.Message, 
                      (ex.InnerException != null) ?  string.Concat(" - ",ex.InnerException.Message)
                      : string.Empty);
            }
            return null;
        }

        private async Task<SuccessResponse> UpsertExternalAsync<T>(string objectName, string externalFieldName, string externalId, T record)
        {
            return await ExecWithRetry(
                async () => await client.UpsertExternalAsync(objectName, externalFieldName, externalId, record).ConfigureAwait(false)
            ).ConfigureAwait(false);
        }

        public void DeleteByBatchId(string objectName, string batchFieldName, 
                                    int updateBatchNumber, StringBuilder _emailLog = null, StringBuilder _emailErrorLog = null)
        {
            TimeWatcher.Restart();

            LogHelper.Logger.WriteOutput(
                $"Started - Fetching records for Deleting all {objectName} that does not match the update Batch Number - {updateBatchNumber}",
                _emailLog);

            var idBatch = new CustomSObjectList<CustomSObject>();
            idBatch.AddRange(GetRecordsFromSalesforce<DataToDeleteById>("DataToDeleteById",
              string.Concat("SELECT Id FROM ", objectName, " Where ", batchFieldName, " <> " + updateBatchNumber)).Select(
                result => new CustomSObject { { "Id", result.Id } }));

            TimeWatcher.Stop();

            LogHelper.Logger.WriteOutput(
                String.Concat("End - Fetching records for Deleting all " , objectName, " that does not match the update Batch Number - ", 
                updateBatchNumber, " - Execution Time: {TimeWatcher.ElapsedMilliseconds} ms."), 
                _emailLog
                );

            BulkDeleteExternalAsync(objectName, SplitList(idBatch), idBatch.Count, _emailLog, _emailErrorLog);
        }

        public static void BulkDeleteExternalAsync(string objectName, List<CustomSObjectList<CustomSObject>> batchList, int totalBatchcount,
            StringBuilder _emailLog = null, StringBuilder _emailErrorLog = null)
        {
            if (batchList == null) return;

            LogHelper.Logger.WriteOutput($"Deleting {totalBatchcount} records of {objectName}", _emailLog);

            TimeWatcher.Restart();

            Task.Run(async () =>
            {
                return await ExecWithRetry<List<BatchResultList>>(
                async () => await client.RunJobAndPollAsync(objectName,
                                                            BulkConstants.OperationType.Delete,
                                                            batchList).ConfigureAwait(false));
            }).Wait(Timeout.InfiniteTimeSpan);

            TimeWatcher.Stop();

            LogHelper.Logger.WriteOutput($"Deleted {totalBatchcount} records of {objectName} - Execution Time: {TimeWatcher.ElapsedMilliseconds} ms.", _emailLog);
            //_emailLog.AppendLine($"Deleted {totalBatchcount} records of {objectName} - Execution Time: {TimeWatcher.ElapsedMilliseconds} ms.");
            //Console.WriteLine($"Deleting {totalBatchcount} records of {objectName} - Execution Time: {TimeWatcher.ElapsedMilliseconds} ms.");
        }

        public static List<CustomSObjectList<CustomSObject>> SplitList(CustomSObjectList<CustomSObject> listIn, int size = 10000)
        {
            if (listIn.Count == 0) return null;

            CustomSObjectList<CustomSObject> innerList = null;
            int remainder = 0;

            var list = new List<CustomSObjectList<CustomSObject>>();
            for (int i = 0; i < listIn.Count; i++)
            {
                Math.DivRem(i, size, out remainder);
                if (remainder == 0)
                {
                    if (i != 0) { list.Add(innerList); }
                    innerList = new CustomSObjectList<CustomSObject>();
                }

                innerList.Add(listIn[i]);
            }

            list.Add(innerList);

            return list;
        }

        //        public Task<List<BatchResultList>> BulkUpsertExternal(string objectName, string externalFieldName, CustomSObjectList<CustomSObject> batchList, bool async)
        //        {
        //            SanatizeObjects(batchList);
        //return BulkUpsertExternalAsync(objectName, externalFieldName, new List<CustomSObjectList<CustomSObject>> { batchList });
        //            //Task<List<BatchResultList>> result = null;

        //            //if (async)
        //            //{
        //                //result =

        //                //Console.WriteLine(result.Result.FirstOrDefault().ToString());
        //                //return result;
        //            //}
        //            //else
        //            //{

        //            //    Task.Run(async () =>
        //            //        {
        //            //            client.UpdateAsync(objectName, id, obj);
        //            //            //anchorWebServices = await salesforceClient.QueryAsync<AccountData>(queryString);
        //            //        }).Wait(Timeout.InfiniteTimeSpan);


        //            //    Task.Run(async () =>
        //            //    return await ExecWithRetry<SuccessResponse>(
        //            //           async () => await client.UpdateAsync(objectName, id, obj).ConfigureAwait(false)
        //            //    ).ConfigureAwait(false);
        //            //}).Wait(Timeout.InfiniteTimeSpan);
        //            //
        //            //         {
        //            //             await BulkUpsertExternalAsync(objectName, externalFieldName, new List<CustomSObjectList<CustomSObject>> { batchList });
        //            //         }).Wait(Timeout.InfiniteTimeSpan);

        //       // }
        //            //return null;
        //        }


        //public static Exception[] Exceptions WhenAllEx(List<Task> tasks)
        //{
        //    Task.WhenAll(tasks).ContinueWith(_ => // return a continuation of WhenAll
        //    {
        //        //var results = tasks
        //        //    .Where(t => t.Status == TaskStatus.RanToCompletion)
        //        //    .Select(t => t..Result)
        //        //    .ToArray();
        //        var aggregateExceptions = tasks
        //            .Where(t => t.IsFaulted)
        //            .Select(t => t.Exception) // The Exception is of type AggregateException
        //            .ToArray();
        //        var exceptions = new AggregateException(aggregateExceptions).Flatten()
        //            .InnerExceptions.ToArray(); // Trick to flatten the hierarchy of AggregateExceptions
        //        return exceptions;
        //    }, TaskContinuationOptions.ExecuteSynchronously);
        //}

        //public static async Task<SuccessResponse> CreateAsync<T>(string objectName, T obj)
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

        //public static async Task<QueryResult<T>> QueryAllAsync<T>(string soql)
        //{
        //    return await ExecWithRetry<QueryResult<T>>(
        //        async () => await client.QueryAllAsync<T>(soql).ConfigureAwait(false)
        //    ).ConfigureAwait(false);
        //}


        //public static async Task<SuccessResponse> UpsertExternalAsync<T>(string objectName, string externalFieldName, string externalId, T record)
        //{
        //    return await ExecWithRetry<SuccessResponse>(
        //        async () => await client.UpsertExternalAsync(objectName, externalFieldName, externalId, record).ConfigureAwait(false)
        //    ).ConfigureAwait(false);
        //}
    }
}
