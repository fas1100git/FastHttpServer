using FastHttpServer.Controllers;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace FastHttpServer.Networking
{
    internal class HttpServer
    {
        const int HTTP_PORT = 8080;
        const int HTTP_HEADER_SIZE = 4096;

        public async Task StartAsync()
        {
            IPEndPoint ipEndPoint = new IPEndPoint(IPAddress.Any, HTTP_PORT);
            Socket socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            socket.Bind(ipEndPoint);
            socket.Listen();

            Console.WriteLine($"Server is running on port: {HTTP_PORT}");

            do
            {
                Socket clientSocket = await socket.AcceptAsync();

                Console.WriteLine($"Connect client: {clientSocket.RemoteEndPoint}");

                _ = Task.Run(() => HandleClientAsync(clientSocket));

            } while (true);
        }

        private static async Task HandleClientAsync(Socket clientSocket)
        {
            try
            {
                HttpRequest request = Parse(clientSocket);

                HttpResponse response = HttpController.Route(request);

                await SendHttpResponseAsync(clientSocket, response);
            }
            catch (Exception e)
            {
                Console.WriteLine($"Client fatal error {clientSocket.RemoteEndPoint}: {e.Message}");
            }
            finally
            {
                clientSocket.Shutdown(SocketShutdown.Both);
                clientSocket.Close();
            }
        }


        private static HttpRequest Parse(Socket clientSocket)
        {
            HttpRequest request = new HttpRequest();
            byte[] buffer = new byte[HTTP_HEADER_SIZE];
            List<byte> receivedData = [];
            int headerEndIndex = -1;
            int bytesRead = 0;

            // header
            while (headerEndIndex == -1)
            {
                bytesRead = clientSocket.Receive(buffer);
                if (bytesRead == 0)
                    throw new Exception("Connection closed before receiving complete headers");

                receivedData.AddRange(buffer[..bytesRead]);
                byte[] dataArray = [.. receivedData];


                for (var i = 0; i < dataArray.Length - 3; i++)
                {
                    // find \r\n\r\n
                    if (dataArray[i] == 0x0D && dataArray[i + 1] == 0x0A &&
                        dataArray[i + 2] == 0x0D && dataArray[i + 3] == 0x0A)
                    {
                        headerEndIndex = i;
                        break;
                    }
                }

                if (receivedData.Count > HTTP_HEADER_SIZE)
                    throw new Exception("Headers too large");
            }

            byte[] headerBytes = [.. receivedData.GetRange(0, headerEndIndex + 4)];
            ParseHeaders(request, Encoding.UTF8.GetString(headerBytes));

            List<byte> bodyData = [];
            byte[] initialBodyData = [.. receivedData.GetRange(headerEndIndex + 4, receivedData.Count - (headerEndIndex + 4))];
            bodyData.AddRange(initialBodyData);

            // body
            if (request.Headers.TryGetValue("Content-Length", out string? value))
            {
                var contentLength = int.Parse(value);
                var totalReceived = bodyData.Count;

                while (totalReceived < contentLength)
                {
                    var bytesToRead = Math.Min(buffer.Length, contentLength - totalReceived);
                    bytesRead = clientSocket.Receive(buffer, bytesToRead, SocketFlags.None);

                    if (bytesRead == 0)
                        break;

                    bodyData.AddRange(buffer[..bytesRead]);
                    totalReceived += bytesRead;
                }

                if (bodyData.Count > 0)
                {
                    request.Body = Encoding.UTF8.GetString([.. bodyData]);
                }
            }

            return request;
        }

        private static void ParseHeaders(HttpRequest request, string headersText)
        {
            var lines = headersText.Split(["\r\n"], StringSplitOptions.RemoveEmptyEntries);

            if (lines.Length == 0)
                throw new Exception("Empty request");

            var firstLineParts = lines[0].Split(' ');
            if (firstLineParts.Length >= 2)
            {
                request.Method = firstLineParts[0];
                request.Path = firstLineParts[1];
            }

            for (var i = 1; i < lines.Length; i++)
            {
                var line = lines[i];
                var separatorIndex = line.IndexOf(':');
                if (separatorIndex > 0)
                {
                    var key = line[..separatorIndex].Trim();
                    var value = line[(separatorIndex + 1)..].Trim();
                    request.Headers[key] = value;
                }
            }
        }

        private static async Task SendHttpResponseAsync(Socket clientSocket, HttpResponse response)
        {
            string httpResponse = $"HTTP/1.1 {response.StatusCode} {response.StatusText}\r\n" +
                                 $"Content-Type: {response.ContentType}\r\n" +
                                 $"Content-Length: {Encoding.UTF8.GetByteCount(response.Content)}\r\n" +
                                 "Connection: close\r\n" +
                                 "\r\n" +
                                 response.Content;

            byte[] responseBytes = Encoding.UTF8.GetBytes(httpResponse);

            await clientSocket.SendAsync(new ArraySegment<byte>(responseBytes), SocketFlags.None);
        }
    }
}
