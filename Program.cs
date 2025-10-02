using FastHttpServer.Networking;

namespace FastHttpServer
{
    internal class Program
    {
        static void Main(string[] args)
        {
            new HttpServer().StartAsync().Wait();
        }
    }
}
