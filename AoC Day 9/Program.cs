using System.Collections.Generic;
using System.Security.Cryptography.X509Certificates;

try
{
    var tilesFile = Path.Combine(AppContext.BaseDirectory, "Files", "Tiles.txt");
    using StreamReader tilesReader = new StreamReader(tilesFile);
    var tilesResult = tilesReader.ReadToEnd().Split(new[] { "\r\n", "\n" }, StringSplitOptions.RemoveEmptyEntries);

  //  Console.WriteLine($"Largest tile area:  {getTileArea(tilesResult)}");

    Console.WriteLine($"Largest red/green area:  {getRedGreenArea(tilesResult)}");
}
catch (Exception ex)
{
    Console.WriteLine($"An error occurred: {ex.Message}");
}

List<(int x, int y)> getTiles(string[] tileCooridnates)
{
    List<(int x, int y)> tiles = new List<(int x, int y)>();
    foreach (var t in tileCooridnates)
    {
        var tile = t.Split(",");
        if (tile == null) continue;
        tiles.Add((int.Parse(tile[0]), int.Parse(tile[1])));
    }

    return tiles;
}
long getTileArea(string[] tileCooridnates)
{
    long area = 0;

    var tiles = getTiles(tileCooridnates);
    //loop through the list and find the largest area
    for (int i = 0; i < tiles.Count; i++)
    {
        for (int j = 0; j < tiles.Count; j++)
        {
            if (tiles[i].x == tiles[j].x || tiles[i].y == tiles[j].y) continue;

            long dx = Math.Abs(tiles[i].x - tiles[j].x);
            long dy = Math.Abs(tiles[i].y - tiles[j].y);

            // +1 because we're counting tiles, not gaps
            long tileArea = (dx + 1) * (dy + 1);

            if (tileArea > area) area = tileArea;

        }
        
    }
    return area;
}
//not working:  If you can fix, please do so.
long getRedGreenArea(string[] tileCoordinates)
{
    long maxArea = 0;

    var redTiles = getTiles(tileCoordinates);
    int n = redTiles.Count;
    if (n < 2) return 0;

    // 1. Build red+green boundary (loop)
    var goodTiles = new HashSet<(int x, int y)>(redTiles);

    for (int i = 0; i < n; i++)
    {
        var start = redTiles[i];
        var end = redTiles[(i + 1) % n]; // wrap to first

        if (start.x == end.x)
        {
            // vertical segment
            int minY = Math.Min(start.y, end.y);
            int maxY = Math.Max(start.y, end.y);
            for (int y = minY; y <= maxY; y++)
                goodTiles.Add((start.x, y));
        }
        else if (start.y == end.y)
        {
            // horizontal segment
            int minX = Math.Min(start.x, end.x);
            int maxX = Math.Max(start.x, end.x);
            for (int x = minX; x <= maxX; x++)
                goodTiles.Add((x, start.y));
        }
        else
        {
            // Per problem text, this shouldn't happen
            // throw new InvalidOperationException("Non-axis-aligned neighbor.");
        }
    }

    // 2. Fill interior of the loop: all tiles inside the polygon are also green
    // First, compute bounding box of all red tiles
    int minPolyX = redTiles[0].x;
    int maxPolyX = redTiles[0].x;
    int minPolyY = redTiles[0].y;
    int maxPolyY = redTiles[0].y;

    foreach (var t in redTiles)
    {
        if (t.x < minPolyX) minPolyX = t.x;
        if (t.x > maxPolyX) maxPolyX = t.x;
        if (t.y < minPolyY) minPolyY = t.y;
        if (t.y > maxPolyY) maxPolyY = t.y;
    }

    // Helper: point-in-polygon (ray casting), using tile centers
    bool IsInsidePolygon((int x, int y) tile)
    {
        double px = tile.x + 0.5;
        double py = tile.y + 0.5;

        bool inside = false;
        int count = redTiles.Count;

        for (int i = 0, j = count - 1; i < count; j = i++)
        {
            double xi = redTiles[i].x;
            double yi = redTiles[i].y;
            double xj = redTiles[j].x;
            double yj = redTiles[j].y;

            // Check if edge (j -> i) crosses horizontal ray to the right of (px, py)
            bool intersect = ((yi > py) != (yj > py)) &&
                             (px < (xj - xi) * (py - yi) / (yj - yi) + xi);

            if (intersect)
                inside = !inside;
        }

        return inside;
    }

    // Mark all interior tiles as green
    for (int x = minPolyX; x <= maxPolyX; x++)
    {
        for (int y = minPolyY; y <= maxPolyY; y++)
        {
            var tile = (x, y);

            // Skip if already marked as boundary (red or boundary green)
            if (goodTiles.Contains(tile)) continue;

            if (IsInsidePolygon(tile))
            {
                goodTiles.Add(tile);
            }
        }
    }

    // 3. Try every pair of red tiles as opposite corners
    for (int i = 0; i < n; i++)
    {
        var a = redTiles[i];

        for (int j = i + 1; j < n; j++)
        {
            var b = redTiles[j];

            // Must form a proper rectangle with non-zero area
            if (a.x == b.x || a.y == b.y)
                continue;

            int minX = Math.Min(a.x, b.x);
            int maxX = Math.Max(a.x, b.x);
            int minY = Math.Min(a.y, b.y);
            int maxY = Math.Max(a.y, b.y);

            bool valid = true;

            // Check that EVERY tile in the rectangle is red or green
            for (int x = minX; x <= maxX && valid; x++)
            {
                for (int y = minY; y <= maxY; y++)
                {
                    if (!goodTiles.Contains((x, y)))
                    {
                        valid = false;
                        break;
                    }
                }
            }

            if (!valid) continue;

            long width = maxX - minX + 1;
            long height = maxY - minY + 1;
            long area = width * height;

            if (area > maxArea)
                maxArea = area;
        }
    }

    return maxArea;
}

