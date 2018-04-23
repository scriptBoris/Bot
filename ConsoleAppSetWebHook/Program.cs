using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Script.Serialization;

namespace ConsoleAppSetWebHook
{
    class Program
    {
        static void Main(string[] args)
        {
            using (var ngok = new Ngrok(@"C:\Users\Nimaev\Desktop\ngrok.exe"))
            {
                var tokenNgrok = "2LmcYNVDFEL73cTut8diq_5hMBSDuyJFSS8vnXEvNsQ";
                ngok.Auth(tokenNgrok);
                ngok.StartTunel(8000);

                var adres = ngok.GetAdressNgrok();
                // ngok.OpenWebStatus(); 
                var token = "47945e1c86e7d0d9-583cae4899b374c1-23198a1f8ffe96ff";

                var urlWebHook = adres + "/ViberBotService/WebHook/";
                Task.Run(() => MainAsync(token, urlWebHook));
                Console.WriteLine("Run");
                Console.ReadLine();
            }

            Console.ReadLine();
        }

        static async Task MainAsync(string token, string urlWebHook)
        {

            using (var client = new HttpClient())
            {
                client.BaseAddress = new Uri("https://chatapi.viber.com");
                var del = new
                {
                    url = ""
                };
                var json = new JavaScriptSerializer().Serialize(del);
                var stringContent = new StringContent(json, Encoding.UTF8, "application/json");
                stringContent.Headers.Add("X-Viber-Auth-Token", token);
                var result = await client.PostAsync("/pa/set_webhook", stringContent);
                string resultContent = await result.Content.ReadAsStringAsync();
                Console.WriteLine(resultContent);


                var set = new
                {
                    url = urlWebHook,
                    event_types = new string[] { "delivered", "seen", "failed", "subscribed", "unsubscribed", "conversation_started" }
                };

                json = new JavaScriptSerializer().Serialize(set);
                stringContent = new StringContent(json, Encoding.UTF8, "application/json");
                stringContent.Headers.Add("X-Viber-Auth-Token", token);
                result = await client.PostAsync("/pa/set_webhook", stringContent);
                resultContent = await result.Content.ReadAsStringAsync();
                Console.WriteLine(resultContent);


            }
        }
    }
}
