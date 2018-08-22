using System;
using System.IO;
using System.Net;
using System.Text;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Net.Http.Headers;
using System.Net.Http;

namespace ICS.XFramework
{
    /// <summary>
    /// WEB助手类
    /// </summary>
    public partial class WebHelper
    {
        #region 网络

        /// <summary>
        /// HttpWebRequest 用POST方法访问指定URI
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="uri">请求发送到的 URI。</param>
        /// <param name="content">POST内容</param>
        /// <param name="headers">请求的头部信息</param>
        /// <returns></returns>
        public static T HttpPost<T>(string uri, string content, IDictionary<string, string> headers = null, string contentType = "application/json", int? timeout = null)
        {
            //application/x-www-form-urlencoded
#if net45
            if (uri.StartsWith("https", StringComparison.OrdinalIgnoreCase)) ServicePointManager.SecurityProtocol = SecurityProtocolType.Ssl3 | SecurityProtocolType.Tls | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls12;
#endif
#if net40
            if (uri.StartsWith("https", StringComparison.OrdinalIgnoreCase)) ServicePointManager.SecurityProtocol = SecurityProtocolType.Ssl3 | SecurityProtocolType.Tls;
#endif

            HttpWebRequest request = WebRequest.Create(uri) as HttpWebRequest;
            if (timeout != null) request.Timeout = timeout.Value;
            request.Method = "POST";
            request.ContentType = contentType;
            request.ContentLength = 0;
            if (headers != null) foreach (var kv in headers) request.Headers.Add(kv.Key, kv.Value);

            if (!string.IsNullOrEmpty(content))
            {
                byte[] sndBytes = Encoding.UTF8.GetBytes(content);
                request.ContentLength = sndBytes.Length;

                Stream rs = request.GetRequestStream();
                rs.Write(sndBytes, 0, sndBytes.Length);
                rs.Close();
            }


            StreamReader reader = null;
            Stream stream = null;
            try
            {
                HttpWebResponse response = request.GetResponse() as HttpWebResponse;
                stream = response.GetResponseStream();
                reader = new StreamReader(stream);

                string line = reader.ReadToEnd();
                T value = SerializeHelper.DeserializeFromJson<T>(line);
                return value;
            }
            catch (WebException we)
            {
                WebHelper.ThrowWebException(we);
                throw;
            }
            finally
            {
                if (stream != null) stream.Close();
                if (reader != null) reader.Close();
            }
        }

        /// <summary>
        /// HttpWebRequest 用GET方法访问指定URI
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="uri">请求发送到的 URI。</param>
        /// <param name="headers">请求的头部信息</param>
        /// <returns></returns>
        public static T HttpGet<T>(string uri, IDictionary<string, string> headers = null)
        {
#if net45
            if (uri.StartsWith("https", StringComparison.OrdinalIgnoreCase)) ServicePointManager.SecurityProtocol = SecurityProtocolType.Ssl3 | SecurityProtocolType.Tls | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls12;
#endif
#if net40
            if (uri.StartsWith("https", StringComparison.OrdinalIgnoreCase)) ServicePointManager.SecurityProtocol = SecurityProtocolType.Ssl3 | SecurityProtocolType.Tls;
#endif

            HttpWebRequest request = WebRequest.Create(uri) as HttpWebRequest;
            request.Method = "GET";
            request.ContentType = "text/json";
            if (headers != null) foreach (var kv in headers) request.Headers.Add(kv.Key, kv.Value);


            StreamReader reader = null;
            Stream stream = null;
            try
            {
                HttpWebResponse response = request.GetResponse() as HttpWebResponse;
                stream = response.GetResponseStream();
                reader = new StreamReader(stream);

                string line = reader.ReadToEnd();
                T value = SerializeHelper.DeserializeFromJson<T>(line);
                return value;
            }
            catch (WebException we)
            {
                WebHelper.ThrowWebException(we);
                throw;
            }
            finally
            {
                if (stream != null) stream.Close();
                if (reader != null) reader.Close();
            }
        }

        /// <summary>
        /// HttpWebRequest 用GET方法访问指定URI<c>使用完记得调用Stream.Close方法</c>
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="uri">请求发送到的 URI。</param>
        /// <param name="headers">请求的头部信息</param>
        /// <param name="contentType">请求的验证信息</param>
        /// <returns></returns>
        public static Stream HttpGet(string uri, IDictionary<string, string> headers = null, string contentType = "text/json")
        {
#if net45
            if (uri.StartsWith("https", StringComparison.OrdinalIgnoreCase)) ServicePointManager.SecurityProtocol = SecurityProtocolType.Ssl3| SecurityProtocolType.Tls | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls12;
#endif
#if net40
            if (uri.StartsWith("https", StringComparison.OrdinalIgnoreCase)) ServicePointManager.SecurityProtocol = SecurityProtocolType.Ssl3 | SecurityProtocolType.Tls;
#endif

            HttpWebRequest request = WebRequest.Create(uri) as HttpWebRequest;
            request.Method = "GET";
            request.ContentType = contentType;
            if (headers != null) foreach (var kv in headers) request.Headers.Add(kv.Key, kv.Value);

            try
            {
                HttpWebResponse response = request.GetResponse() as HttpWebResponse;
                return response.GetResponseStream();
            }
            catch (WebException we)
            {
                WebHelper.ThrowWebException(we);
                throw;
            }
        }

        /// <summary>
        /// HttpWebRequest 用POST方法访问指定URI<c>使用完记得调用Stream.Close方法</c>
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="uri">请求发送到的 URI。</param>
        /// <param name="content">POST内容</param>
        /// <param name="headers">请求的头部信息</param>
        /// <returns></returns>
        public static Stream HttpPost(string uri, string content, IDictionary<string, string> headers = null, string contentType = "application/json", int? timeout = null, Encoding encoding = null)
        {
            //application/x-www-form-urlencoded

            encoding = encoding ?? Encoding.UTF8;
#if net45
            if (uri.StartsWith("https", StringComparison.OrdinalIgnoreCase)) ServicePointManager.SecurityProtocol = SecurityProtocolType.Ssl3 | SecurityProtocolType.Tls | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls12;
#endif
#if net40
            if (uri.StartsWith("https", StringComparison.OrdinalIgnoreCase)) ServicePointManager.SecurityProtocol = SecurityProtocolType.Ssl3 | SecurityProtocolType.Tls;
#endif

            HttpWebRequest request = WebRequest.Create(uri) as HttpWebRequest;
            if (timeout != null) request.Timeout = timeout.Value;
            request.Method = "POST";
            request.ContentType = contentType;
            request.ContentLength = 0;
            if (headers != null) foreach (var kv in headers) request.Headers.Add(kv.Key, kv.Value);

            if (!string.IsNullOrEmpty(content))
            {
                byte[] sndBytes = encoding.GetBytes(content);
                request.ContentLength = sndBytes.Length;

                Stream rs = request.GetRequestStream();
                rs.Write(sndBytes, 0, sndBytes.Length);
                rs.Close();
            }

            try
            {
                HttpWebResponse response = request.GetResponse() as HttpWebResponse;
                return response.GetResponseStream();
            }
            catch (WebException we)
            {
                WebHelper.ThrowWebException(we);
                throw;
            }
        }

        /// <summary>
        /// HttpWebRequest 用POST方法访问指定URI<c>使用完记得调用Stream.Close方法</c>
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="uri">请求发送到的 URI。</param>
        /// <param name="content">POST内容</param>
        /// <param name="headers">请求的头部信息</param>
        /// <returns></returns>
        public static Stream HttpDelete(string uri, string content, IDictionary<string, string> headers = null, string contentType = "application/json", int? timeout = null, Encoding encoding = null)
        {
            //application/x-www-form-urlencoded

            encoding = encoding ?? Encoding.UTF8;
#if net45
            if (uri.StartsWith("https", StringComparison.OrdinalIgnoreCase)) ServicePointManager.SecurityProtocol = SecurityProtocolType.Ssl3 | SecurityProtocolType.Tls | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls12;
#endif
#if net40
            if (uri.StartsWith("https", StringComparison.OrdinalIgnoreCase)) ServicePointManager.SecurityProtocol = SecurityProtocolType.Ssl3 | SecurityProtocolType.Tls;
#endif

            HttpWebRequest request = WebRequest.Create(uri) as HttpWebRequest;
            if (timeout != null) request.Timeout = timeout.Value;
            request.Method = "DELETE";
            request.ContentType = contentType;
            request.ContentLength = 0;
            if (headers != null) foreach (var kv in headers) request.Headers.Add(kv.Key, kv.Value);

            if (!string.IsNullOrEmpty(content))
            {
                byte[] sndBytes = encoding.GetBytes(content);
                request.ContentLength = sndBytes.Length;

                Stream rs = request.GetRequestStream();
                rs.Write(sndBytes, 0, sndBytes.Length);
                rs.Close();
            }

            try
            {
                HttpWebResponse response = request.GetResponse() as HttpWebResponse;
                return response.GetResponseStream();
            }
            catch (WebException we)
            {
                WebHelper.ThrowWebException(we);
                throw;
            }
        }

        /// <summary>
        /// HttpWebRequest 用POST方法访问指定URI<c>使用完记得调用Stream.Close方法</c>
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="uri">请求发送到的 URI。</param>
        /// <param name="content">POST内容</param>
        /// <param name="headers">请求的头部信息</param>
        /// <returns></returns>
        public static HttpWebResponse HttpResponse(string uri, string content, IDictionary<string, string> headers = null, string contentType = "application/json", int? timeout = null, Encoding encoding = null)
        {
            //application/x-www-form-urlencoded

            encoding = encoding ?? Encoding.UTF8;
#if net45
            if (uri.StartsWith("https", StringComparison.OrdinalIgnoreCase)) ServicePointManager.SecurityProtocol = SecurityProtocolType.Ssl3 | SecurityProtocolType.Tls | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls12;
#endif
#if net40
            if (uri.StartsWith("https", StringComparison.OrdinalIgnoreCase)) ServicePointManager.SecurityProtocol = SecurityProtocolType.Ssl3 | SecurityProtocolType.Tls;
#endif

            HttpWebRequest request = WebRequest.Create(uri) as HttpWebRequest;
            if (timeout != null) request.Timeout = timeout.Value;
            request.Method = "POST";
            request.ContentType = contentType;
            request.ContentLength = 0;
            if (headers != null) foreach (var kv in headers) request.Headers.Add(kv.Key, kv.Value);

            if (!string.IsNullOrEmpty(content))
            {
                byte[] sndBytes = encoding.GetBytes(content);
                request.ContentLength = sndBytes.Length;

                Stream rs = request.GetRequestStream();
                rs.Write(sndBytes, 0, sndBytes.Length);
                rs.Close();
            }

            try
            {
                HttpWebResponse response = request.GetResponse() as HttpWebResponse;
                return response;
            }
            catch (WebException we)
            {
                WebHelper.ThrowWebException(we);
                throw;
            }
        }

        /// <summary>
        /// 抛出 webException 里面的信息
        /// </summary>
        public static void ThrowWebException(WebException we)
        {
            if (we == null) return;
            if (we.Response == null) return;

            Stream stream = null;
            StreamReader reader = null;
            try
            {
                stream = we.Response.GetResponseStream();
                reader = new StreamReader(stream);
                string line = reader.ReadToEnd();
                if (!string.IsNullOrEmpty(line)) throw new WebException(line, we);
            }
            finally
            {
                if (reader != null) reader.Close();
                if (stream != null) stream.Dispose();
            }
        }

        /// <summary>
        /// 读取 WebException 里的详细信息
        /// </summary>
        public static string ReadWebException(WebException we)
        {
            if (we == null) return string.Empty;
            if (we.Response == null) return we.Message;

            Stream stream = null;
            StreamReader reader = null;
            try
            {
                stream = we.Response.GetResponseStream();
                reader = new StreamReader(stream);
                string line = reader.ReadToEnd();
                return (!string.IsNullOrEmpty(line)) ? line : we.Message;
            }
            catch
            {
                return we.Message;
            }
            finally
            {
                if (reader != null) reader.Close();
                if (stream != null) stream.Dispose();
            }
        }

        /// <summary>
        /// HttpClient 用POST方法访问指定URI
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="uri">请求发送到的 URI。</param>
        /// <param name="content">发送到服务器的 HTTP 请求内容。</param>
        /// <param name="headers">请求的头部信息</param>
        /// <param name="authentication">请求的验证信息</param>
        /// <returns></returns>
        public static async Task<T> PostAsync<T>(string uri, string content, IDictionary<string, string> headers = null, AuthenticationHeaderValue authentication = null)
        {
            HttpClient c = null;
            HttpContent httpContent = null;
            HttpResponseMessage msg = null;
            T TEntity = default(T);

            try
            {
                c = new HttpClient();
                httpContent = new StringContent(content);
                if (headers != null) foreach (var kv in headers) c.DefaultRequestHeaders.Add(kv.Key, kv.Value);
                if (authentication != null) c.DefaultRequestHeaders.Authorization = authentication;

                msg = await c.PostAsync(uri, httpContent);
                string json = await msg.Content.ReadAsStringAsync(); // ReadAsAsync
                TEntity = SerializeHelper.DeserializeFromJson<T>(json);
            }
            catch (WebException we)
            {
                WebHelper.ThrowWebException(we);
                throw;
            }
            finally
            {
                if (httpContent != null) httpContent.Dispose();
                if (c != null) c.Dispose();
                if (msg != null) msg.Dispose();

            }

            return TEntity;
        }

        /// <summary>
        /// HttpClient 用GET方法访问指定URI
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="uri">请求发送到的 URI。</param>
        /// <param name="headers">请求的头部信息</param>
        /// <param name="authentication">请求的验证信息</param>
        /// <returns></returns>
        public static async Task<T> GetAsync<T>(string uri, IDictionary<string, string> headers = null, AuthenticationHeaderValue authentication = null)
        {
            HttpClient c = null;
            HttpResponseMessage msg = null;
            T TEntity = default(T);

            try
            {
                c = new HttpClient();
                if (headers != null) foreach (var kv in headers) c.DefaultRequestHeaders.Add(kv.Key, kv.Value);
                if (authentication != null) c.DefaultRequestHeaders.Authorization = authentication;

                msg = await c.GetAsync(uri);
                string json = await msg.Content.ReadAsStringAsync();
                TEntity = SerializeHelper.DeserializeFromJson<T>(json);
            }
            catch (WebException we)
            {
                WebHelper.ThrowWebException(we);
                throw;
            }
            finally
            {
                if (c != null) c.Dispose();
                if (msg != null) msg.Dispose();

            }

            return TEntity;
        }

        /// <summary>
        /// HttpClient 用GET方法访问指定URI
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="uri">请求发送到的 URI。</param>
        /// <param name="token">Basic 验证模式的令牌</param>
        /// <param name="headers">请求的头部信息</param>
        /// <returns></returns>
        public static async Task<T> GetAsync<T>(string uri, string token, IDictionary<string, string> headers = null)
        {
            return await WebHelper.GetAsync<T>(uri, headers, new AuthenticationHeaderValue("Basic", token));
        }

        /// <summary>
        /// HttpClient 用GET方法访问指定URI <c>用完注意调用HttpContent.Dispose方法</c>
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="uri">请求发送到的 URI。</param>
        /// <param name="token">Basic 验证模式的令牌</param>
        /// <param name="headers">请求的头部信息</param>
        /// <returns></returns>
        public static async Task<HttpContent> GetAsync(string uri, string token, IDictionary<string, string> headers = null)
        {
            HttpClient c = null;

            try
            {
                c = new HttpClient();
                if (headers != null) foreach (var kv in headers) c.DefaultRequestHeaders.Add(kv.Key, kv.Value);
                if (!string.IsNullOrEmpty(token)) c.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", token);


                var r = await c.GetAsync(uri);
                return r.Content;
            }
            catch (WebException we)
            {
                WebHelper.ThrowWebException(we);
                throw;
            }
            finally
            {
                if (c != null) c.Dispose();
            }
        }

        #endregion
    }
}
