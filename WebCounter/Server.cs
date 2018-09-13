using System;
using System.Linq;
using System.Collections.Generic;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using System.Text;
using System.Reflection;

namespace WebCounter
{
    // https://codehosting.net/blog/BlogEngine/post/Simple-C-Web-Server
    public class Server
    {
        readonly HttpListener listener;
        readonly Dictionary<string, MethodInfo> callbackMap;

        public Server(Type type)
        {
            listener = new HttpListener();
            callbackMap = new Dictionary<string, MethodInfo>();
            var methods = type
                .GetMethods(BindingFlags.Public | BindingFlags.Static)
                .Where(mi => mi.GetCustomAttributes<Mapping>().Count() > 0)
                .Select(mi => new { attr = mi.GetCustomAttribute<Mapping>(), method = mi });

            foreach (var m in methods)
            {
                callbackMap.Add(m.attr.Map, m.method);
                listener.Prefixes.Add($"http://localhost/{m.attr.Map}/");
            }
            listener.Start();
        }

        public void Run()
        {
            Task.Run(() =>
            {
                while (listener.IsListening)
                {
                    // 메모리 엄청 먹음!!! ㅠㅠ
                    // Task.Run(() => 
                    ThreadPool.QueueUserWorkItem((ctx) =>
                    {
                        var context = ctx as HttpListenerContext;
                        try
                        {
                            var name = context.Request.Url.Segments[1].Replace("/", "");
                            var urlparams = context.Request.Url.Segments.Skip(2).Select(s => s.Replace("/", ""));
                            if (!callbackMap.ContainsKey(name))
                                return;
                            var method = callbackMap[name];
                            var typedparams = method.GetParameters()
                                .Zip(urlparams, (p, s) => Convert.ChangeType(s, p.ParameterType));

                            var res = method.Invoke(null, typedparams.ToArray()).ToString();
                            byte[] buf = Encoding.UTF8.GetBytes(res);
                            context.Response.ContentLength64 = buf.Length;
                            context.Response.OutputStream.Write(buf, 0, buf.Length);
                        }
                        catch { }
                        finally
                        {
                            context.Response.OutputStream.Close();
                        }
                    }, listener.GetContext());
                }
            });
        }

        public void Stop()
        {
            listener.Stop();
            listener.Close();
        }
    }
}
