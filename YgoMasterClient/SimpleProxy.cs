using System;
using System.IO;
using System.Net;
using System.Threading;

namespace YgoMasterClient
{
    static class SimpleProxy
    {
        public static void Run()
        {
            if (ClientSettings.ProxyPort == 0)
            {
                Console.WriteLine("Can't create proxy server on port " + ClientSettings.ProxyPort);
                return;
            }

            new Thread(delegate ()
            {
                try
                {
                    HttpListener listener = new HttpListener();
                    listener.Prefixes.Add("http://localhost:" + ClientSettings.ProxyPort + "/");
                    listener.Start();
                    while (true)
                    {
                        try
                        {
                            HttpListenerContext context = listener.GetContext();
                            try
                            {
                                Process(context);
                            }
                            catch (Exception e)
                            {
                                Console.WriteLine("[Proxy] error " + e);
                            }
                            finally
                            {
                                context.Response.Close();
                            }
                        }
                        catch
                        {
                        }
                        Thread.Sleep(100);
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine("[ERROR] Failed to start proxy server on port " + ClientSettings.ProxyPort + ". Error: " + e);
                }
            }).Start();
        }

        static void Process(HttpListenerContext context)
        {
            context.Response.StatusCode = (int)HttpStatusCode.BadRequest;

            byte[] requestBuffer = new byte[context.Request.ContentLength64];
            int readBytes = 0;
            int offset = 0;
            while ((readBytes = context.Request.InputStream.Read(requestBuffer, offset, requestBuffer.Length - offset)) > 0)
            {
                offset += readBytes;
            }
            if (offset != requestBuffer.Length)
            {
                Console.WriteLine("[WARNING] Read " + offset + " / " + context.Request.ContentLength64 + " bytes");
                return;
            }

            using (WebClient webClient = new WebClient())
            {
                webClient.Proxy = null;
                foreach (string key in context.Request.Headers.Keys)
                {
                    switch (key.ToLowerInvariant())
                    {
                        case "atoken":
                        case "x-unity-version":
                        case "x_acts":
                            webClient.Headers[key] = context.Request.Headers[key];
                            continue;
                    }
                }
                byte[] responseBuffer = webClient.UploadData("http://" + ClientSettings.BaseIP + ":" + ClientSettings.BasePort + context.Request.RawUrl, requestBuffer);

                context.Response.KeepAlive = false;
                context.Response.Headers[HttpResponseHeader.ContentType] = "application/octet-stream";
                context.Response.ContentLength64 = responseBuffer.Length;
                context.Response.StatusCode = (int)HttpStatusCode.OK;
                context.Response.OutputStream.Write(responseBuffer, 0, responseBuffer.Length);
            }
        }
    }
}