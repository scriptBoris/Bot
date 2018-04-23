using Newtonsoft.Json;
using System;
using System.Net;
using System.Net.Http;
using System.Runtime.Serialization;
// Обратите внимание, данную библиотеку нужно будет подключить 
using System.ServiceModel;
using System.ServiceModel.Web;
using System.Text;
using System.Web.Script.Serialization;

namespace ConsoleAppWcf
{
    class Program
    {
        static void Main(string[] args)
        {
            var port = 8000;
            var url = $"http://localhost:{port}/ViberBotService";
            // Инициализируем службу, указываем адрес, по которому она будет доступна 
            using (var host = new WebServiceHost(typeof(ViberBotService), new Uri(url)))
            {
                // Добавляем конечную точку службы с заданным интерфейсом, привязкой (создаём новую) и адресом конечной точки 
                host.AddServiceEndpoint(typeof(IViberBotService), GetBinding(), "");
                // Запускаем службу 
                host.Open();

                Console.WriteLine("Start service " + url);
                Console.WriteLine("Press key to exit...");
                Console.ReadLine();

                // Закрываем службу 
                host.Close();
            }
        }
        private static WebHttpBinding GetBinding()
        {
            var b = new WebHttpBinding
            {
                // WriteEncoding = System.Text.Encoding.UTF8
                // TODO одределить  допустимый размер сообщений
                //MaxBufferSize =
                //MaxBufferSize =
            };
            return b;

        }
    }

    [ServiceContract]
    public interface IViberBotService
    {
        [OperationContract]
        [WebInvoke(Method = "POST", UriTemplate = "/WebHook/",
            ResponseFormat = WebMessageFormat.Json)]
        void WebHook(S msg);

        [WebInvoke(Method = "GET", UriTemplate = "/Hello/", ResponseFormat = WebMessageFormat.Json)]
        string Hello();
    }


    public class ViberBotService : IViberBotService
    {
        public void WebHook(S msg)
        {
            // Чтение HTTP заголовка
            //IncomingWebRequestContext request = WebOperationContext.Current.IncomingRequest;
            //WebHeaderCollection headers = request.Headers;
            //Console.WriteLine("-------------------------------------------------------");
            //Console.WriteLine(request.Method + " " + request.UriTemplateMatch.RequestUri.AbsolutePath);
            //foreach (string headerName in headers.AllKeys)
            //{
            //    Console.WriteLine(headerName + ": " + headers[headerName]);
            //}
            //Console.WriteLine("-------------------------------------------------------");

            // Запись сообщения 
            var time = DateTime.Now.ToShortTimeString();
            Console.WriteLine(time);
            var json = new JavaScriptSerializer().Serialize(msg); ;
            Console.WriteLine(json);
            Console.WriteLine();

            if (msg.Message != null)
            {
                ReplyMessage(msg.Message.text, msg.User.Id);

               // Keybord(msg.User.Id);
            }

        }

        private static async System.Threading.Tasks.Task Keybord(string id)
        {
            Console.WriteLine("test keybord");
            var msg = new Message
            {
                receiver = id,
                type = MessageTypes.text.ToString(),
                tracking_data = "tracking_data",
                min_api_version = 3,
                text = "test",
                keyboard = new Keybord
                {
                    Type = "keyboard",
                    Buttons = new Button[]
                {
                    new Button
                    {
                        columns = 1,
                        rows =1,
                        text= "<br><font color=\"#494E67\"><b>ASIAN</b></font>",
                        textSize= "large",
                        textHAlign= "center",
                        textVAlign= "middle",
                        actionType= "reply",
                        actionBody= "ASIAN",
                        bgColor= "#f7bb3f",
                        image= "https://www.benchmarkemail.com/images/screen_shots/imageurl01.gif",
                        bgMediaType= "gif",
                        bgMedia= "https://www.benchmarkemail.com/images/screen_shots/imageurl01.gif"
                    }
                }
                }
            };

            var json = new JavaScriptSerializer().Serialize(msg);
            Console.WriteLine("**************");

            Console.WriteLine(json);

            var stringContent = new StringContent(json, Encoding.UTF8, "application/json");
            var token = "47945e1c86e7d0d9-583cae4899b374c1-23198a1f8ffe96ff";
            stringContent.Headers.Add("X-Viber-Auth-Token", token);

            var client = new HttpClient
            {
                BaseAddress = new Uri("https://chatapi.viber.com")
            };
            var result = await client.PostAsync("/pa/send_message", stringContent);
            var resultContent = await result.Content.ReadAsStringAsync();
            Console.WriteLine(resultContent);
        }

        private static async System.Threading.Tasks.Task ReplyMessage(string message, string id)
        {
            var msg = new Message
            {
                receiver = id,
                type = MessageTypes.text.ToString(),
                tracking_data = "tracking_data",
                min_api_version = 1,
                text = message
            };

            var json = new JavaScriptSerializer().Serialize(msg);
            Console.WriteLine("**************");
            Console.WriteLine(json);
            var stringContent = new StringContent(json, Encoding.UTF8, "application/json");
            var token = "47945e1c86e7d0d9-583cae4899b374c1-23198a1f8ffe96ff";
            stringContent.Headers.Add("X-Viber-Auth-Token", token);

            var client = new HttpClient
            {
                BaseAddress = new Uri("https://chatapi.viber.com")
            };
            var result = await client.PostAsync("/pa/send_message", stringContent);
            var resultContent = await result.Content.ReadAsStringAsync();
            Console.WriteLine(resultContent);
        }

        public string Hello() => "Hello";
    }

    [DataContract]
    public class S
    {
        [DataMember(Name = "event")]
        public string Event { get; set; }
        [DataMember(Name = "timestamp")]
        public long Timestamp { get; set; }
        [DataMember(Name = "message_token")]
        public long Message_token { get; set; }
        [DataMember(Name = "user_id")]
        public string user_id;
        [DataMember(Name = "sender")]
        public Sender User { get; set; }
        [DataMember(Name = "message")]
        public Message Message { get; set; }
    }
    [DataContract]
    public class Sender
    {
        [DataMember(Name = "id")]
        public string Id { get; set; }
        [DataMember(Name = "name")]
        public string Name { get; set; }
    }

    [DataContract]
    public class Message
    {
        [DataMember(Name = "receiver")]
        public string receiver { get; set; }
        [DataMember(Name = "text")]
        public string text { get; set; }
        [DataMember(Name = "type")]
        public string type { get; set; } //= MessageTypes.text;
        [DataMember(Name = "tracking_data")]
        public string tracking_data { get; set; } //= "tracking_data";
        [DataMember(Name = "min_api_version")]
        public int min_api_version { get; set; } = 1;
        [DataMember(Name = "keyboard")]
        public Keybord keyboard { get; set; }
    }
    [DataContract]

    public enum MessageTypes
    {
        [EnumMember]
        text,
        [EnumMember]
        picture,
        [EnumMember]
        video,
        [EnumMember]
        file,
        [EnumMember]
        sticker,
        [EnumMember]
        contact,
        [EnumMember]
        url,
        [EnumMember]
        location
    }
    [DataContract]

    public partial class Keybord
    {
        [DataMember]
        public string Type { get; set; }
        [DataMember]
        public Button[] Buttons { get; set; }
    }
    [DataContract]

    public partial class Button
    {
        [DataMember]
        public string bgMediaType;
        [DataMember]
        public string bgMedia;

        [DataMember]
        public long columns { get; set; }
        [DataMember]
        public long rows { get; set; }
        [DataMember]
        public string text { get; set; }
        [DataMember]
        public string textSize { get; set; }
        [DataMember]
        public string textHAlign { get; set; }
        [DataMember]
        public string textVAlign { get; set; }
        [DataMember]
        public string actionType { get; set; }
        [DataMember]
        public string actionBody { get; set; }
        [DataMember]
        public string bgColor { get; set; }
        [DataMember]
        public string image { get; set; }
    }


}