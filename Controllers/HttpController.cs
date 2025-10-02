using FastHttpServer.Models;
using FastHttpServer.Networking;
using FastHttpServer.Services;
using System.Text.Json;

namespace FastHttpServer.Controllers
{
    internal class HttpController
    {
        static readonly string ROOT_DIRECTORY = "./www";

        public static HttpResponse Route(HttpRequest request)
        {
            try
            {
                if (request.Method == "GET")
                {
                    string filePath = request.Path;

                    if (filePath == "/")
                        filePath = "/index.html";

                    filePath = filePath.Replace('/', Path.DirectorySeparatorChar)
                        .Replace('\\', Path.DirectorySeparatorChar)
                        .TrimStart(Path.DirectorySeparatorChar)
                        .Replace("..", string.Empty);

                    string fullPath = Path.Combine(ROOT_DIRECTORY, filePath);

                    if (!File.Exists(fullPath))
                        return CreateNotFoundHttpResponse(request.Path);

                    return DownloadFileHttpResponse(fullPath);
                }

                if (request.Method == "POST" && (request.Path == "/" || request.Path == "/get-path"))
                {
                    var pathRequest = JsonSerializer.Deserialize<PathRequest>(request.Body);

                    if (pathRequest is null)
                        return CreateErrorHttpResponse();

                    if (pathRequest.From.Length != 2 || pathRequest.To.Length != 2)
                        return CreateErrorPointHttpResponse();

                    Point from = GetPointFromPathRequest(pathRequest.From);
                    Point to = GetPointFromPathRequest(pathRequest.To);

                    if (CheckingCorrectnessPoint(from))
                        return CreateErrorPointHttpResponse();

                    if (CheckingCorrectnessPoint(to))
                        return CreateErrorPointHttpResponse();

                    List<Point> path = GameManager.FindShortestKnightPath(from, to);

                    List<string> pathResult = GetPathResultFromPoint(path);

                    string content = JsonSerializer.Serialize(pathResult);

                    return new HttpResponse
                    {
                        StatusCode = 200,
                        StatusText = "OK",
                        Content = content,
                        ContentType = "application/json"
                    };
                }

                return CreateNotFoundHttpResponse(request.Path);

            }
            catch (Exception)
            {
                return CreateErrorHttpResponse();
            }
        }


        public static Point GetPointFromPathRequest(string path)
        {
            char file = char.ToLower(path[0]);
            char rank = path[1];

            return new Point(file - 'a', rank - '1');
        }

        public static List<string> GetPathResultFromPoint(List<Point> points)
        {
            return points.Select(value => $"{(char)('a' + value.X)}{(char)('1' + value.Y)}").ToList();
        }

        private static bool CheckingCorrectnessPoint(Point point)
        {
            return point.X < 0 || point.X > 8 || point.Y < 0 || point.Y > 8;
        }


        private static HttpResponse DownloadFileHttpResponse(string filePath)
        {
            var fileInfo = new FileInfo(filePath);

            return new HttpResponse
            {
                StatusCode = 200,
                StatusText = "OK",
                ContentType = GetContentType(filePath),
                Content = File.ReadAllText(filePath),
                ContentLength = fileInfo.Length.ToString()
            };
        }

        private static string GetContentType(string filePath)
        {
            var extension = Path.GetExtension(filePath).ToLowerInvariant();

            return extension switch
            {
                ".txt" => "text/plain",
                ".html" => "text/html",
                ".jpg" or ".jpeg" => "image/jpeg",
                ".png" => "image/png",
                ".pdf" => "application/pdf",
                ".zip" => "application/zip",
                ".json" => "application/json",
                ".xml" => "application/xml",
                _ => "application/octet-stream"
            };
        }


        private static HttpResponse CreateErrorHttpResponse()
        {
            return new HttpResponse
            {
                StatusCode = 400,
                StatusText = "ERROR",
                Content = "{\"message\": \"Use method with JSON body for path finding\"}",
                ContentType = "application/json"
            };
        }

        private static HttpResponse CreateErrorPointHttpResponse()
        {
            return new HttpResponse
            {
                StatusCode = 422,
                StatusText = "UNPROCESSABLE ENTITY",
                Content = "{\"message\": \"incorrect coordinates\"}",
                ContentType = "application/json"
            };
        }

        private static HttpResponse CreateNotFoundHttpResponse(string path)
        {
            return new HttpResponse
            {
                StatusCode = 404,
                StatusText = "UNPROCESSABLE ENTITY",
                Content = "{\"message\": \"The requested URL " + path + " was not found on this server.\"}",
                ContentType = "application/json"
            };
        }
    }
}
