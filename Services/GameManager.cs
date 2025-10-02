using FastHttpServer.Models;

namespace FastHttpServer.Services
{
    internal class GameManager
    {
        public static List<Point> FindShortestKnightPath(Point from, Point to)
        {
            // Поиск в ширину - для поиска кратчайшего пути.
            var queue = new Queue<(int x, int y, List<Point> path)>();
            var visited = new bool[8, 8];

            var moves = new (int dx, int dy)[]
            {
                (2, 1), (2, -1), (-2, 1), (-2, -1),
                (1, 2), (1, -2), (-1, 2), (-1, -2)
            };

            queue.Enqueue((from.X, from.Y, new List<Point> { from }));

            visited[from.X, from.Y] = true;

            while (queue.Count > 0)
            {
                var current = queue.Dequeue();

                if (current.x == to.X && current.y == to.Y)
                {
                    return current.path;
                }

                foreach (var (dx, dy) in moves)
                {
                    int newX = current.x + dx;
                    int newY = current.y + dy;

                    if (newX >= 0 && newX < 8 && newY >= 0 && newY < 8 && !visited[newX, newY])
                    {
                        visited[newX, newY] = true;
                        var newPos = new Point(newX, newY);
                        var newPath = new List<Point>(current.path) { newPos };
                        queue.Enqueue((newX, newY, newPath));
                    }
                }
            }

            return [from];
        }
    }
}
