using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ConsoleAppSetWebHook
{
    public class Ngrok : IDisposable
    {
        private string _ngrokPath;
        private readonly string _defaultPath = @"C:\ngrok.exe";
        private readonly string _webInterface = "http://localhost:4040/";

        public Ngrok(string path = null)
        {
            if (path == null)
                _ngrokPath = _defaultPath;
            else
                _ngrokPath = path;
            if (!File.Exists(_ngrokPath))
            {
                throw new FileNotFoundException($"По пути '{_ngrokPath}' Ngrok.exe не найден");
            }

        }

        public void Auth(string token)
        {
            if (string.IsNullOrEmpty(token))
                throw new ArgumentNullException(nameof(token));

            var proc = new ProcessStartInfo
            {
                FileName = _ngrokPath,
                CreateNoWindow = true,
                Arguments = token
            };
            Process.Start(proc);

        }

        public void StartTunel(int port, string adress = null)
        {
            if (adress != string.Empty)
                adress += ":";

            var proc = new ProcessStartInfo
            {
                CreateNoWindow = true,
                FileName = _ngrokPath,
                Arguments = $"http {adress}{port}"
            };
            Process.Start(proc);
        }

        public string GetAdressNgrok()
        {
            Thread.Sleep(3000);
            var urlAddress = _webInterface;
            var request = (HttpWebRequest)WebRequest.Create(urlAddress);
            var response = (HttpWebResponse)request.GetResponse();
            string html;

            if (response.StatusCode == HttpStatusCode.OK)
            {
                Stream receiveStream = response.GetResponseStream();
                StreamReader readStream = null;

                if (response.CharacterSet == null)
                {
                    readStream = new StreamReader(receiveStream);
                }
                else
                {
                    readStream = new StreamReader(receiveStream, Encoding.GetEncoding(response.CharacterSet));
                }

                html = readStream.ReadToEnd();

                response.Close();
                readStream.Close();
            }
            else
            {
                throw new AggregateException($"{response.StatusCode}");
            }


            var ngrok = "ngrok.io";
            int startIndex = 0;
            int endIndex;
            do
            {
                startIndex = html.IndexOf("https://", startIndex + 1);
                if (startIndex == -1) break;
                var res = html.Substring(startIndex, 30);
                if (res.Contains(ngrok))
                {
                    endIndex = res.IndexOf(ngrok);
                    var adress = html.Substring(startIndex, endIndex + ngrok.Length);
                    return adress;
                }
            } while (true);

            throw new AggregateException("Адрес не найден");
        }

        public void OpenWebStatus()
        {
            Process.Start(_webInterface + "status");
        }

        public void Dispose()
        {
            Thread.Sleep(2000);
            var processes = Process.GetProcesses();
            foreach (var item in processes)
            {
                if (item.ProcessName.ToLower().Contains("ngrok".ToLower()))
                {
                    item.Kill();
                }
            }
        }

    }
}
