using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FastHttpServer.Networking
{
    internal class HttpRequest
    {
        public string Method { get; set; } = "GET";
        public string Path { get; set; } = "/";
        public Dictionary<string, string> Headers { get; set; } = [];
        public string Body { get; set; } = "";
    }
}
