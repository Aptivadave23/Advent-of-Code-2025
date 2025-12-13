using System.Data;
using System.Text.RegularExpressions;
using System.Linq;

try
{
    var inputFile = Path.Combine(AppContext.BaseDirectory, "Files", "Input.txt");
    using StreamReader inputReader = new StreamReader(inputFile);
    var inputResult = inputReader.ReadToEnd().Split(new[] { "\r\n", "\n" }, StringSplitOptions.RemoveEmptyEntries);
    var (presents, grids) = parseInput(inputResult);
    var shapes = ParsePresents(presents);
    var regions = parseRegions(grids, shapes.Count);

    Console.WriteLine($"Question 1 answer: {Solve(shapes, regions)}");

}
catch (Exception ex)
{
    Console.WriteLine($"Following exception thrown:  {ex.Message}");
}

(string[] presents, string[] grids) parseInput(string[] inputLines)
{
    //loop through inputLines to separate presents and grids
   

    int splitIndex = Array.FindIndex(inputLines, line => Regex.IsMatch(line, @"^\d+x\d+:\s*"));
    if (splitIndex == -1) throw new InvalidOperationException("Invalid file format");

    string[] presents = inputLines.Take(splitIndex).ToArray();
    string[] grids = inputLines.Skip(splitIndex).ToArray();
    return (presents, grids);
}

List<PresentShape> ParsePresents(string[] presentLines)
{
    var shapes = new List<PresentShape>();
    int i = 0;

    while (i < presentLines.Length)
    {
        var line = presentLines[i];
        
        if (string.IsNullOrWhiteSpace(line))
        {
            i++;
            continue;
        }

        if (!line.EndsWith(":"))
            throw new InvalidOperationException($"Expected shape header like '0:' but got '{line}'");

        var idPart = line.Substring(0, line.Length - 1).Trim();//drop the colon
        if (!int.TryParse(idPart, out int id))
            throw new InvalidOperationException($"Could not parse shape id from: '{line}'");

        i++;

        var rowStrings = new List<string>();

        while (i < presentLines.Length)
        {
            var raw = presentLines[i];
            var trimmed = raw.Trim();

            if (string.IsNullOrWhiteSpace(trimmed))
            {
                i++;
                break;
            }
            //stop before consuming if this is the next header like "1:" or "2:"
            if (trimmed.EndsWith(":") && int.TryParse(trimmed[..^1], out _)) break;

            rowStrings.Add(trimmed);
            i++;
        }

        if (rowStrings.Count == 0) throw new InvalidOperationException($"Shape {id} has no pattern rows.");

        int height = rowStrings.Count;
        int width = rowStrings[0].Length;
        var cells = new char[height, width];


        for (int r = 0; r < height; r++)
        {
            if (rowStrings[r].Length != width)
                throw new InvalidOperationException($"Shape {id} has inconsistent row widths");

            for (int c = 0; c < width; c++)
            {
                cells[r, c] = rowStrings[r][c];
            }
        }

        shapes.Add(new PresentShape()
        {
            ID = id,
            Cells = cells
        });
    }

    return shapes;
}

List<Region> parseRegions(string[] gridLines, int shapeCount)
{
    var regions = new List<Region>();
    foreach (var line in gridLines)
    {
        if(string.IsNullOrWhiteSpace(line)) continue;


        var parts = line.Split(':', 2, StringSplitOptions.TrimEntries);
        if (parts.Length != 2) throw new InvalidOperationException($"Bad region line:  '{line}");

        var sizeParts = parts[0].Split('x', StringSplitOptions.TrimEntries);
        if (sizeParts.Length != 2) throw new InvalidOperationException($"Bad size in region line:  '{line}'");

        int width = int.Parse(sizeParts[0]);
        int height = int.Parse(sizeParts[1]);

        var countStrings = parts[1].Split(' ', StringSplitOptions.RemoveEmptyEntries);
        if (countStrings.Length != shapeCount)
            throw new InvalidOperationException(
                $"Region '{line}' has {countStrings.Length} counts but {shapeCount} shapes.");

        int[] counts = Array.ConvertAll(countStrings, int.Parse);

        regions.Add(new Region
        {
            Width = width,
            Height = height,
            Counts = counts
        });
    }
    return regions;
}

void BuildOrientationsForAllShapes(List<PresentShape> shapes)
{
    foreach (var shape in shapes)
    {
        var original = GetFilledCells(shape.Cells);
        var variants = new List<List<(int r, int c)>>();
        
        //4 rotations of original
        var v0 = original;
        var v1 = Rotate90(v0);
        var v2 = Rotate90(v1);
        var v3 = Rotate90(v2);

        //flip horizontally
        var f0 = FlipHorizontal(v0);
        var f1 = Rotate90(f0);
        var f2 = Rotate90(f1);
        var f3 = Rotate90(f2);

        variants.Add(v0);
        variants.Add(v1);
        variants.Add(v2);
        variants.Add(v3);
        variants.Add(f0);
        variants.Add(f1);
        variants.Add(f2);
        variants.Add(f3);

        //normalize and dedupe
        var seen = new HashSet<string>();
        foreach (var v in variants)
        {
            var norm = Normalize(v);
            var key = MakeKey(norm);

            if (seen.Add(key)) shape.Orientations.Add(norm);
        }
    }
}

List<Placement>[] BuildPlacementsForRegion(Region region, List<PresentShape> shapes)
{
    int width = region.Width;
    int height = region.Height;
    int cellCount = width * height;
    int wordCount = (cellCount + 63) / 64;

    // placementsByShape[shapeId] = list of placements for that shape
    var placementsByShape = new List<Placement>[shapes.Count];
    for (int i = 0; i < shapes.Count; i++)
        placementsByShape[i] = new List<Placement>();

    for (int shapeId = 0; shapeId < shapes.Count; shapeId++)
    {
        var shape = shapes[shapeId];

        foreach (var orientation in shape.Orientations)
        {
            // Get bounding box of this orientation
            int maxDr = orientation.Max(p => p.row);
            int maxDc = orientation.Max(p => p.col);

            int maxBaseRow = height - 1 - maxDr;
            int maxBaseCol = width - 1 - maxDc;

            for (int baseRow = 0; baseRow <= maxBaseRow; baseRow++)
            {
                for (int baseCol = 0; baseCol <= maxBaseCol; baseCol++)
                {
                    // Build bitset for this placement
                    ulong[] bits = new ulong[wordCount];

                    foreach (var (dr, dc) in orientation)
                    {
                        int r = baseRow + dr;
                        int c = baseCol + dc;
                        int index = r * width + c;    // flattened index

                        int wordIndex = index / 64;
                        int bitIndex = index % 64;
                        bits[wordIndex] |= 1UL << bitIndex;
                    }

                    placementsByShape[shapeId].Add(new Placement
                    {
                        ShapeId = shapeId,
                        Bits = bits
                    });
                }
            }
        }
    }

    return placementsByShape;
}


bool BackTrackPlace(
    List<int> presentsToPlace,
    int index,
    bool[,] board,
    List<PresentShape> shapes,
    int width,
    int height)
{
    // All presents placed successfully
    if (index == presentsToPlace.Count)
        return true;

    // Find the top-left-most empty cell
    var (hasEmpty, targetRow, targetCol) = FindFirstEmptyCell(board, height, width);

    // Board has no empty cells, but we still have presents -> impossible
    if (!hasEmpty)
        return false;

    int shapeID = presentsToPlace[index];
    PresentShape shape = shapes[shapeID];

    foreach (List<(int row, int col)> orientation in shape.Orientations)
    {
        // To avoid trying the same base position multiple times for this orientation,
        // we can dedupe baseRow/baseCol pairs.
        HashSet<(int br, int bc)> triedBases = new HashSet<(int br, int bc)>();

        foreach ((int dr, int dc) in orientation)
        {
            int baseRow = targetRow - dr;
            int baseCol = targetCol - dc;

            var key = (baseRow, baseCol);
            if (!triedBases.Add(key))
                continue; // already tried this base position for this orientation

            if (CanPlace(orientation, baseRow, baseCol, board, width, height))
            {
                Place(orientation, baseRow, baseCol, board);

                bool success = BackTrackPlace(
                    presentsToPlace,
                    index + 1,
                    board,
                    shapes,
                    width,
                    height);

                if (success)
                    return true;

                Unplace(orientation, baseRow, baseCol, board);
            }
        }
    }

    // No way to place this present so that it covers targetRow/targetCol
    return false;
}


bool CanFitAllPresents(Region region, List<PresentShape> shapes)
{
    int width = region.Width;
    int height = region.Height;

    // Fast area check
    int totalPresentArea = 0;
    for (int shapeId = 0; shapeId < shapes.Count; shapeId++)
    {
        int count = region.Counts[shapeId];
        if (count == 0) continue;

        int shapeArea = shapes[shapeId].Orientations[0].Count;
        totalPresentArea += count * shapeArea;
    }

    int regionArea = width * height;
    if (totalPresentArea > regionArea)
        return false;

    // Build all placements for this region
    var placementsByShape = BuildPlacementsForRegion(region, shapes);

    // Shared board bitset
    int cellCount = width * height;
    int wordCount = (cellCount + 63) / 64;
    ulong[] board = new ulong[wordCount];

    // Remaining counts per shape
    int[] remaining = (int[])region.Counts.Clone();

    // Precompute shape areas for heuristic
    int[] shapeAreaArr = shapes
        .Select(s => s.Orientations[0].Count)
        .ToArray();

    return SearchRegion(board, placementsByShape, remaining, shapeAreaArr);
}

bool SearchRegion(
    ulong[] board,
    List<Placement>[] placementsByShape,
    int[] remaining,
    int[] shapeArea)
{
    // Check if all done
    bool anyLeft = false;
    for (int i = 0; i < remaining.Length; i++)
    {
        if (remaining[i] > 0)
        {
            anyLeft = true;
            break;
        }
    }
    if (!anyLeft)
        return true;

    // Choose next shape to place: largest area among remaining
    int nextShape = -1;
    int bestArea = -1;
    for (int i = 0; i < remaining.Length; i++)
    {
        if (remaining[i] > 0 && shapeArea[i] > bestArea)
        {
            bestArea = shapeArea[i];
            nextShape = i;
        }
    }

    // Safety
    if (nextShape == -1)
        return false;

    // Try all placements for this shape
    foreach (var placement in placementsByShape[nextShape])
    {
        if (Overlaps(board, placement.Bits))
            continue;

        // Use this placement
        ApplyPlacement(board, placement.Bits);
        remaining[nextShape]--;

        if (SearchRegion(board, placementsByShape, remaining, shapeArea))
            return true;

        // Backtrack
        remaining[nextShape]++;
        UndoPlacement(board, placement.Bits);
    }

    return false;
}


int Solve(List<PresentShape> shapes, List<Region> regions)
{
    BuildOrientationsForAllShapes(shapes);
    int successFulRegions = 0;

    foreach (var region in regions)
    {
        bool canFit = CanFitAllPresents(region, shapes);
        if (canFit) successFulRegions++;
    }

    return successFulRegions;
}
//helpers
(bool hasEmpty, int row, int col) FindFirstEmptyCell(bool[,] board, int height, int width)
{
    for (int r = 0; r < height; r++)
    {
        for (int c = 0; c < width; c++)
        {
            if (!board[r, c])
            {
                return (true, r, c);
            }
        }
    }

    return (false, -1, -1);
}

List<(int r, int c)> GetFilledCells(char[,] cells)
{
    var list = new List<(int r, int c)>();
    int h = cells.GetLength(0);
    int w = cells.GetLength(1);

    for (int r = 0; r < h; r++)
    for (int c = 0; c < w; c++)
        if (cells[r, c] == '#')
            list.Add((r, c));

    return list;
}
List<(int r, int c)> Rotate90(List<(int r, int c)> points)
{
    // Rotate around (0,0): (r, c) -> (c, -r)
    var rotated = new List<(int r, int c)>();
    foreach (var (r, c) in points)
        rotated.Add((c, -r));
    return rotated;
}

List<(int r, int c)> FlipHorizontal(List<(int r, int c)> points)
{
    // (r, c) -> (r, -c)
    var flipped = new List<(int r, int c)>();
    foreach (var (r, c) in points)
        flipped.Add((r, -c));
    return flipped;
}

List<(int r, int c)> Normalize(List<(int r, int c)> points)
{
    int minR = points.Min(p => p.r);
    int minC = points.Min(p => p.c);

    var result = new List<(int r, int c)>();
    foreach (var (r, c) in points)
        result.Add((r - minR, c - minC));

    return result;
}

string MakeKey(List<(int r, int c)> points)
{
    return string.Join(";", points
        .OrderBy(p => p.r)
        .ThenBy(p => p.c)
        .Select(p => $"{p.r},{p.c}"));
}

bool CanPlace(List<(int row, int col)> orientation, int baseRow, int baseCol, bool[,] board, int width, int height)
{
    foreach ((int dr, int dc) in orientation)
    {
        var r = baseRow + dr;
        var c = baseCol + dc;

        if ((r < 0) || (r >= height) || (c >= width) || (c < 0))
        {
            return false;
        }

        //overlap board
        if (board[r, c])
        {
            return false;
        }
    }
    return true;
}

void Place(List<(int row, int col)> orientation, int baseRow, int baseCol, bool[,] board)
{
    foreach ((int row, int col) in orientation)
    {
        int r = baseRow + row;
        int c = baseCol + col;
        board[r,c] = true;
    }
}

void Unplace(List<(int row, int col)> orientation, int baseRow, int baseCol, bool[,] board)
{
    foreach ((int row, int col) in orientation)
    {
        int r = baseRow + row;
        int c = baseCol + col;
        board[r,c] = false;
    }
}

bool Overlaps(ulong[] board, ulong[] move)
{
    for (int i = 0; i < board.Length; i++)
    {
        if ((board[i] & move[i]) != 0UL)
            return true;
    }
    return false;
}

void ApplyPlacement(ulong[] board, ulong[] move)
{
    for (int i = 0; i < board.Length; i++)
        board[i] |= move[i];
}

void UndoPlacement(ulong[] board, ulong[] move)
{
    for (int i = 0; i < board.Length; i++)
        board[i] ^= move[i]; // XOR because we only OR'ed once
}

//classes
class PresentShape
{
    public int ID { get; set; }
    public char[,] Cells { get; set; }
    public int Height => Cells.GetLength(0);
    public int Width => Cells.GetLength(1);
    public List<List<(int row, int col)>> Orientations { get; set; } = new List<List<(int row, int col)>>();
}

class Region
{
    public int Width { get; set; }
    public int Height { get; set; }
    public int[] Counts { get; set; }
   
}

class Placement
{
    public int ShapeId { get; set; }
    public ulong[] Bits { get; set; } //which cells this placement occupies
}