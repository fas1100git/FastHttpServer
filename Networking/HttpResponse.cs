namespace FastHttpServer.Networking
{
    internal class HttpResponse
    {
        public int StatusCode { get; set; } = 0;
        public string StatusText { get; set; } = string.Empty;
        public string Content { get; set; } = "";
        public string ContentType { get; set; } = "";
        public string? ContentLength { get; set; }
    }
}
