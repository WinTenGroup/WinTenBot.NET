using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Net;
using Serilog;
using Zizi.Bot.IO;

namespace Zizi.Bot.Common
{
    public static class UrlUtil
    {
        public static Uri GenerateUrlQrApi(this string data)
        {
            var baseUrl = "https://api.qrserver.com";
            return new Uri($"{baseUrl}/v1/create-qr-code/?size=300x300&margin=10&data={Flurl.Url.Encode(data)}");
        }

        public static void SaveUrlTo(this string remoteFileUrl, string localFileName)
        {
            var webClient = new WebClient();

            Log.Information("Saving {RemoteFileUrl} to {LocalFileName}", remoteFileUrl, localFileName);
            webClient.DownloadFile(remoteFileUrl, localFileName);
            webClient.Dispose();
        }

        public static string SaveToCache(this string remoteFileUrl, string localFileName)
        {
            var webClient = new WebClient();

            var cachePath = Path.Combine("Storage", "Caches");
            var localPath = Path.Combine(cachePath, localFileName).SanitizeSlash().EnsureDirectory();

            Log.Information("Saving {RemoteFileUrl} to {LocalPath}", remoteFileUrl, localPath);
            webClient.DownloadFile(remoteFileUrl, localPath);
            webClient.Dispose();

            return localPath;
        }

        public static Flurl.Url ParseUrl(this string urlPath)
        {
            var url = new Flurl.Url(urlPath);

            return url;
        }

        public static bool IsValidUrl(this string urlPath)
        {
            return Flurl.Url.IsValid(urlPath);
        }

        public static bool IsUrlRedirected(this string url)
        {
            var req = (HttpWebRequest) HttpWebRequest.Create(url);
            req.Method = "HEAD";
            req.AllowAutoRedirect = false;
            var resp = (HttpWebResponse) req.GetResponse();

            return resp.StatusCode == HttpStatusCode.Redirect ||
                   resp.StatusCode == HttpStatusCode.MovedPermanently ||
                   resp.StatusCode == HttpStatusCode.RedirectKeepVerb ||
                   resp.StatusCode == HttpStatusCode.RedirectMethod;
        }

        public static string GetRedirectedUrl(this string url)
        {
            if (string.IsNullOrWhiteSpace(url))
                return url;

            Log.Information("Trying get redirected url: {0}", url);
            int maxRedirCount = 8; // prevent infinite loops
            string newUrl = url;
            do
            {
                HttpWebRequest webRequest = null;
                HttpWebResponse webResponse = null;
                try
                {
                    webRequest = (HttpWebRequest) HttpWebRequest.Create(url);
                    webRequest.Method = "HEAD";
                    webRequest.AllowAutoRedirect = true;
                    webResponse = (HttpWebResponse) webRequest.GetResponse();
                    var statusCode = (int) webResponse.StatusCode;
                    Log.Debug("Response: {0}", webResponse.ToJson(true));

                    if (statusCode >= 300 && statusCode <= 399)
                    {
                        Log.Debug("Getting redirected from Headers");

                        // newUrl = webResponse.Headers["Location"];
                        if (newUrl == null)
                            return url;

                        if (newUrl.IndexOf("://", StringComparison.Ordinal) == -1)
                        {
                            // Doesn't have a URL Schema, meaning it's a relative or absolute URL
                            Uri u = new Uri(new Uri(url), newUrl);
                            newUrl = u.ToString();
                        }

                        Log.Debug("Finish redirect.");
                    }

                    newUrl = webResponse.ResponseUri.AbsoluteUri;

                    // switch (resp.StatusCode)
                    // {
                    //     case HttpStatusCode.OK:
                    //         return newUrl;
                    //     case HttpStatusCode.Redirect:
                    //     case HttpStatusCode.MovedPermanently:
                    //     case HttpStatusCode.RedirectKeepVerb:
                    //     case HttpStatusCode.RedirectMethod:
                    //         Log.Debug("Getting redirected from Headers");
                    //
                    //         newUrl = resp.Headers["Location"];
                    //         if (newUrl == null)
                    //             return url;
                    //
                    //         if (newUrl.IndexOf("://", System.StringComparison.Ordinal) == -1)
                    //         {
                    //             // Doesn't have a URL Schema, meaning it's a relative or absolute URL
                    //             Uri u = new Uri(new Uri(url), newUrl);
                    //             newUrl = u.ToString();
                    //         }
                    //
                    //         Log.Debug("Finish get.");
                    //         break;
                    //     default:
                    //         return newUrl;
                    // }

                    url = newUrl;
                }
                catch (WebException ex)
                {
                    // Return the last known good URL
                    Log.Error(ex.Demystify(), "Error WebException when try get redirectedUrl");
                    return newUrl;
                }
                catch (Exception ex)
                {
                    Log.Error(ex.Demystify(), "Error when try get redirectedUrl");
                    return null;
                }
                finally
                {
                    if (webResponse != null)
                        webResponse.Close();
                }
            } while (maxRedirCount-- > 0);

            Log.Debug("Redirected URL: {0}", url);
            return url;
        }

        public static Uri GetAutoRedirectedUrl(this string url)
        {
            if (!IsNeedRedirect(url)) return new Uri(url);

            var webRequest = (HttpWebRequest) HttpWebRequest.Create(url);
            webRequest.Method = "HEAD";
            webRequest.AllowAutoRedirect = true;
            var webResponse = (HttpWebResponse) webRequest.GetResponse();
            // var statusCode = (int)webResponse.StatusCode;
            Log.Debug("Response: {0}", webResponse.ToJson(true));

            return webResponse.ResponseUri;
        }

        public static bool IsNeedRedirect(string url)
        {
            var uri = new Uri(url);
            Log.Debug("Uri: {0}", uri.Host.ToJson(true));

            var skipRedirect = new List<string>()
            {
                "soft98.ir"
            };

            var needRedirect = false;
            var host = uri.Host;
            foreach (var skip in skipRedirect)
            {
                if (!host.Contains(skip, StringComparison.CurrentCulture))
                {
                    needRedirect = true;
                }
            }

            Log.Debug("Is {0} need redirect {1}", host, needRedirect);
            return needRedirect;
        }

        // public static bool MakeRequest(Uri url)
        // {
        //     using var client = new HttpClient(new HttpClientHandler()
        //     {
        //         AutomaticDecompression = DecompressionMethods.Deflate | DecompressionMethods.GZip
        //     }) { Timeout = TimeSpan.FromSeconds(30) };
        //     var request = new HttpRequestMessage()
        //     {
        //         RequestUri = url,
        //         Method = HttpMethod.Get
        //     };
        //
        //     HttpResponseMessage response = client.SendAsync(request).Result;
        //     var statusCode = (int)response.StatusCode;
        //
        //     // We want to handle redirects ourselves so that we can determine the final redirect Location (via header)
        //     if (statusCode >= 300 && statusCode <= 399)
        //     {
        //         var redirectUri = response.Headers.Location;
        //         if (!redirectUri.IsAbsoluteUri)
        //         {
        //             redirectUri = new Uri(request.RequestUri.GetLeftPart(UriPartial.Authority) + redirectUri);
        //         }
        //         _status.AddStatus($"Redirecting to {redirectUri}");
        //         return MakeRequest(redirectUri);
        //     }
        //     else if (!response.IsSuccessStatusCode)
        //     {
        //         throw new Exception();
        //     }
        //
        //     return true;
        // }
    }
}