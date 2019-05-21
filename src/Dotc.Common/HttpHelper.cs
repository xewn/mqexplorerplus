using System;
using System.Collections.Specialized;
using System.IO;
using System.IO.Compression;
using System.Net;
using System.Text;

namespace Dotc.Common
{
    public class HttpHelper
    {
        #region HttpRequestGet
        public string HttpRequestGet(string url, out WebHeaderCollection responseWebHeaderCollection)
        {
            return HttpRequest(url, WebRequestMethods.Http.Get, "", out responseWebHeaderCollection);
        }
        public string HttpRequestPost(string url, string data, out WebHeaderCollection responseWebHeaderCollection)
        {
            return HttpRequest(url, WebRequestMethods.Http.Post, data, out responseWebHeaderCollection);
        }
        public string HttpRequestPost(string url, string data, WebHeaderCollection httpRequestHeaders, out WebHeaderCollection responseWebHeaderCollection)
        {
            return HttpRequest(url, WebRequestMethods.Http.Post, data,httpRequestHeaders, out responseWebHeaderCollection);
        }
        private string HttpRequest(string url, string method, string data,out WebHeaderCollection responseWebHeaderCollection)
        {
            return HttpRequest(url, method, data, null, out responseWebHeaderCollection);
        }
        private string HttpRequest(string url, string method, string data, WebHeaderCollection httpRequestHeaders, out WebHeaderCollection responseWebHeaderCollection)
        {
            var request = (HttpWebRequest)HttpWebRequest.Create(url);
            request.Method = method;

            if (method == WebRequestMethods.Http.Post)
            {
                byte[] buffer = Encoding.UTF8.GetBytes(data);
                request.ContentType = "text/json";
                SetHeaderValue(request.Headers, "User-Agent", "Fiddler");
                SetHeaderValue(request.Headers, "Accept", "text/json");
                SetHeaderValue(request.Headers, "Accept-Language", "utf-8");
                request.ContentLength = buffer.Length;
                if (null != httpRequestHeaders)
                {
                    //request.Headers.Clear();
                    request.Headers.Add(httpRequestHeaders.ToString()); 
                }
                var streamRequest = request.GetRequestStream();
                streamRequest.Write(buffer, 0, buffer.Length);
                streamRequest.Close();
            }
            Stream streamResponse=null;
            StreamReader reader=null;
            string result=string.Empty;
            responseWebHeaderCollection = null;
            try
            {
                var response = request.GetResponse();
                streamResponse = response.GetResponseStream();
                if (streamResponse == null) return "";
                reader = new StreamReader(streamResponse, Encoding.Default);
                result = reader.ReadToEnd();
            }
            catch (WebException webException)
            {
                if (((HttpWebResponse)(webException.Response)).StatusCode == HttpStatusCode.Forbidden)
                {
                    responseWebHeaderCollection = (webException.Response).Headers;
                }
            }
            catch (Exception exception)
            {
                result = "网络发生错误：" + exception;
            }
            finally
            {
                if (null != streamResponse)
                {
                    streamResponse.Close();
                }
                if (null != reader)
                {
                    reader.Close();
                }
            }
            return result;
        }
        #endregion
        public void SetHeaderValue(WebHeaderCollection header, string name, string value)
        {
            var property = typeof(WebHeaderCollection).GetProperty("InnerCollection",
                System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);
            if (property != null)
            {
                var collection = property.GetValue(header, null) as NameValueCollection;
                collection[name] = value;
            }
        }

        public string httpPorsRequest(string url,string postDataStr)
        {
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
            request.CookieContainer = new CookieContainer();
            CookieContainer cookie = request.CookieContainer;//如果用不到Cookie，删去即可
            //以下是发送的http头，随便加，其中referer挺重要的，有些网站会根据这个来反盗链
            request.Referer = "http://localhost";
            request.Accept = "Accept:text/json,application/xhtml+xml,application/xml;q=0.9,*/*;q=0.8";
            request.Headers["Accept-Language"] = "zh-CN,zh;q=0.";
            request.Headers["Accept-Charset"] = "GBK,utf-8;q=0.7,*;q=0.3";
            request.UserAgent = "User-Agent:Mozilla/5.0 (Windows NT 5.1) AppleWebKit/535.1 (KHTML, like Gecko) Chrome/14.0.835.202 Safari/535.1";
            request.KeepAlive = true;
            //上面的http头看情况而定，但是下面俩必须加
            request.ContentType = "application/x-www-form-urlencoded";
            request.Method = "POST";

            Encoding encoding = Encoding.UTF8;//根据网站的编码自定义
            byte[] postData = encoding.GetBytes(postDataStr);//postDataStr即为发送的数据，格式还是和上次说的一样
            request.ContentLength = postData.Length;
            Stream requestStream = request.GetRequestStream();
            requestStream.Write(postData, 0, postData.Length);

            HttpWebResponse response = (HttpWebResponse)request.GetResponse();
            Stream responseStream = response.GetResponseStream();
            //如果http头中接受gzip的话，这里就要判断是否为有压缩，有的话，直接解压缩即可
            if (response.Headers["Content-Encoding"] != null && response.Headers["Content-Encoding"].ToLower().Contains("gzip"))
            {
                responseStream = new GZipStream(responseStream, CompressionMode.Decompress);
            }

            StreamReader streamReader = new StreamReader(responseStream, encoding);
            string retString = streamReader.ReadToEnd();

            streamReader.Close();
            responseStream.Close();

            return retString;

        }


        public string WebClientGet(string url)
        {
            var client = new WebClient();
            client.Headers.Add("user-agent", "Mozilla/4.0 (compatible; MSIE 6.0; Windows NT 5.2; .NET CLR 1.0.3705;)");
            var stream = client.OpenRead(url);
            if (stream == null) return "";
            var reader = new StreamReader(stream, Encoding.Default);
            var result = reader.ReadToEnd();
            stream.Close();
            reader.Close();
            return result;
        }
    }
}
