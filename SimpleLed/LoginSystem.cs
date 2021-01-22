using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Timers;
using Newtonsoft.Json;

namespace SimpleLed
{
    public static class APICore
    {
        public static string BaseUrl = "http://api.simpleled.net/";
    }
    public class LoginSystem
    {
        public bool Authenticated { get; set; }
        public string AccessToken { get; set; }
        public string UserName { get; set; }
        public Guid? UserId { get; set; }
        public Guid? ConnectionRequestId { get; internal set; }
        public bool Online { get; set; }
        public Guid? ConnectionRequest { get; internal set; }
        private Action onSuccessCallBack;
        private string MakeSafe(string input)
        {
            string result = "";
            for (int i = 0; i < input.Length; i++)
            {
                if ("1234567890qwertyuioplkjhgfdsazxcvbnm ".Contains(input.Substring(i, 1).ToLower()))
                {
                    result += input.Substring(i, 1);
                }
            }

            return result;
        }
        private class DataModels
        {
            internal class AppConnectionRequest
            {
                public Guid Id { get; set; } = Guid.NewGuid();
                public Guid RequestId { get; set; }
                //      public string RequestIdAsString { get => RequestId.ToString(); set => RequestId = Guid.Parse(value); }
                public DateTime Requested { get; set; }
                public string MachineName { get; set; }
                public string OS { get; set; }
                public Guid? ConnectedUser { get; set; }
            }

            internal class AuthReturn
            {
                public Guid UserId { get; set; }
                public string Name { get; set; }
                public string AuthKey { get; set; }
            }
        }

        private Guid CreateAppConnectionRequest(DataModels.AppConnectionRequest appConnection)
        {
            string dbg = JsonConvert.SerializeObject(appConnection);
            return HTTP_POST<DataModels.AppConnectionRequest, Guid>(APICore.BaseUrl + "Auth/AppConnectRequest", appConnection);
        }

        public string Login(Action onSuccess)
        {
            loginCheckTimer?.Stop();
            loginCheckTimer = new Timer();
            loginCheckTimer.Elapsed += (sender, args) => {
                Update();
                if ((DateTime.Now - loginCheckTimerStart).TotalMinutes > 2)
                {
                    loginCheckTimer.Stop();
                }
            };
            loginCheckTimer.Interval = 5000; // in miliseconds
            loginCheckTimer.AutoReset = true;
            loginCheckTimer.Start();

            loginCheckTimerStart = DateTime.Now;

            onSuccessCallBack = onSuccess;


            DataModels.AppConnectionRequest ap = new DataModels.AppConnectionRequest
            {
                RequestId = Guid.NewGuid(),
                MachineName = MakeSafe(Environment.MachineName),
                OS = MakeSafe(Environment.OSVersion.VersionString),
            };

            var cnxRequestId = CreateAppConnectionRequest(ap);
            ConnectionRequest = cnxRequestId;
            ConnectionRequestId = ap.RequestId;
            string url = $"http://www.simpleled.net/appconnection/{ConnectionRequestId.ToString()}";
            return url;
        }

        private Timer loginCheckTimer;
        private DateTime loginCheckTimerStart = DateTime.Now;

        private DateTime lastConnectionAttemptMade = DateTime.MinValue;
        bool doingConnectionTest = false;
        public void Update()
        {
            if (!doingConnectionTest)
            {
                if (ConnectionRequest != null && ConnectionRequestId != null)
                {
                    if ((DateTime.Now - lastConnectionAttemptMade).TotalSeconds > 2)
                    {
                        doingConnectionTest = true;
                        string url = APICore.BaseUrl + $"Auth/AppLogIn?requestId={ConnectionRequestId}&connectionId={ConnectionRequest}&dummy={Guid.NewGuid().ToString().Replace("{", "").Replace("}", "").Replace("-", "")}";

                        Debug.WriteLine(url);

                        try
                        {
                            string result = Get(url);
                            //loginWindow.Close();

                            DataModels.AuthReturn ar = JsonConvert.DeserializeObject<DataModels.AuthReturn>(result);

                            AccessToken = ar.AuthKey;
                            Authenticate(() =>
                            {
                                Debug.WriteLine("all done!");
                                Online = true;
                                onSuccessCallBack?.Invoke();
                            });

                            Debug.WriteLine("woot!");
                            loginCheckTimer.Stop();
                            ConnectionRequest = null;
                        }
                        catch (Exception e)
                        {
                            Debug.WriteLine(e.Message);
                        }
                        lastConnectionAttemptMade = DateTime.Now;
                        doingConnectionTest = false;
                    }
                }
            }
        }


        public void Authenticate(Action onComplete)
        {
            try
            {
                string url = APICore.BaseUrl + "Auth?authToken=" + System.Uri.EscapeDataString(AccessToken);
                string result = Get(url);

                DataModels.AuthReturn r = JsonConvert.DeserializeObject<DataModels.AuthReturn>(result);

                UserName = r.Name;
                UserId = r.UserId;
                Authenticated = true;

                //SetOffLineToken("access.token", new OfflineAuthToken
                //{
                //    AuthReturn = r
                //});

                Online = true;
                onComplete();
            }
            catch (InvalidTokenException e)
            {
                Online = false;
                // OfflineAuthToken auth = GetOfflineToken<OfflineAuthToken>("access.token");

                //if (auth != null)
                //{
                //    UserName = auth.AuthReturn.Name;
                //    UserId = auth.AuthReturn.UserId;
                //    Authenticated = true;

                //    onComplete();
                //}
                //else
                //{
                Debug.WriteLine(e.Message);
                Authenticated = false;
                UserName = "";
                UserId = null;
                AccessToken = "";
                //      }
            }
        }



        public string Get(string uri)
        {
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(uri);
            request.AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate;

            using (HttpWebResponse response = (HttpWebResponse)request.GetResponse())
            using (Stream stream = response.GetResponseStream())
            using (StreamReader reader = new StreamReader(stream))
            {
                return reader.ReadToEnd();
            }
        }

        public string Get(string uri, string authToken)
        {
            Debug.WriteLine(uri);
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(uri);
            request.AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate;
            request.Headers.Add("authToken", authToken);

            try
            {
                using (HttpWebResponse response = (HttpWebResponse)request.GetResponse())
                {

                    using (Stream stream = response.GetResponseStream())
                    {
                        using (StreamReader reader = new StreamReader(stream))
                        {
                            return reader.ReadToEnd();
                        }
                    }
                }
            }
            catch (WebException ex)
            {
                using (var stream = ex.Response.GetResponseStream())
                using (var reader = new StreamReader(stream))
                {
                    string errorTxt = (reader.ReadToEnd());

                    if (errorTxt.StartsWith("System.Exception: Token Not Found"))
                    {
                        throw new InvalidTokenException("Token Not Valid", ex);
                    }
                }

                throw;
            }
            catch (Exception e)
            {
                Debug.WriteLine(e.InnerException);

                throw;
            }
        }



        public class JsonResultModel
        {
            public string ErrorMessage { get; set; }
            public bool IsSuccess { get; set; }
            public string Results { get; set; }
        }
        // HTTP_PUT Function
        private static ReturnType HTTP_PUT<DataType, ReturnType>(string Url, DataType tData, string authToken = "")
        {
            return HttpPush<DataType, ReturnType>(Url, tData, "PUT", authToken);
        }

        private static ReturnType HTTP_POST<DataType, ReturnType>(string Url, DataType tData, string authToken = "")
        {
            return HttpPush<DataType, ReturnType>(Url, tData, "POST", authToken);
        }

        private static ReturnType HttpPush<DataType, ReturnType>(string Url, DataType tData, string httpverb, string authToken = "")
        {
            string Data = JsonConvert.SerializeObject(tData);
            JsonResultModel model = new JsonResultModel();
            string Out = String.Empty;
            string Error = String.Empty;
            WebRequest req = WebRequest.Create(Url);

            try
            {
                req.Method = httpverb;
                req.Timeout = 100000;
                req.ContentType = "application/json";
                byte[] sentData = Encoding.UTF8.GetBytes(Data);
                req.ContentLength = sentData.Length;

                if (!string.IsNullOrWhiteSpace(authToken))
                {
                    req.Headers.Add("authToken", authToken);
                }

                using (Stream sendStream = req.GetRequestStream())
                {
                    sendStream.Write(sentData, 0, sentData.Length);
                    sendStream.Close();
                }

                WebResponse res = req.GetResponse();
                Stream ReceiveStream = res.GetResponseStream();
                using (StreamReader sr = new
                StreamReader(ReceiveStream, Encoding.UTF8))
                {
                    Out = sr.ReadToEnd();
                }
            }
            catch (ArgumentException ex)
            {
                Error = string.Format("HTTP_ERROR :: The second HttpWebRequest object has raised an Argument Exception as 'Connection' Property is set to 'Close' :: {0}", ex.Message);
            }
            catch (WebException ex)
            {
                Error = ($"{ex.Status}:: WebException raised! :: {ex.Message}");
            }

            catch (Exception ex)
            {
                Error = string.Format("HTTP_ERROR :: Exception raised! :: {0}", ex.Message);
            }

            model.Results = Out;
            model.ErrorMessage = Error;

            if (!string.IsNullOrWhiteSpace(Error))
            {

                throw new Exception(model.ErrorMessage);
            }

            if (string.IsNullOrWhiteSpace(Out))
            {
                return default;
            }

            if (string.IsNullOrWhiteSpace(Error))
            {
                return (ReturnType)JsonConvert.DeserializeObject(model.Results, typeof(ReturnType));
            }

            throw new Exception(model.ErrorMessage);
        }
    }
}
