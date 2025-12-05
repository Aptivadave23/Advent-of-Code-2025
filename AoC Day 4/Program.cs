using System.Data;

try
{
    var filePath = Path.Combine(AppContext.BaseDirectory, "Files", "Input.txt");
    using StreamReader reader = new(filePath);
    var rolls = reader.ReadToEnd().Split(new[]{"\r\n", "\n"}, StringSplitOptions.RemoveEmptyEntries);

    //put the input into a multi dimensional array
    int rows = rolls.Length;
    int cols = rolls[0].Length;

    var grid = new char[rows, cols];

    for (int r = 0; r < rows; r++)
    {
        for (int c = 0; c < cols; c++)
        {
            grid[r, c] = rolls[r][c];
        }
    }

    var accessibleRolls = InspectAtSpots(grid); ;//the count of rows we can access with the forklift

    //removable rolls
    var totalRolls = RemoveAllWeakRolls(grid);
   
    Console.WriteLine($"Forklift can access {accessibleRolls} rolls");
    Console.WriteLine($"Forklift removed {totalRolls} total rolls");
}
catch (Exception ex)
{
    Console.WriteLine($"An error occurred: {ex.Message}");
}

IEnumerable<(int Row, int Col, char Value)> GetNeighbors(char[,] grid, int row, int col)
{
    int rows = grid.GetLength(0);
    int cols = grid.GetLength(1);

    for (int dr = -1; dr <= 1; dr++)
    {
        for (int dc = -1; dc <= 1; dc++)
        {
            //skip the center cell
            if (dr== 0 && dc== 0) continue;

            int nr = row + dr;
            int nc = col + dc;

            //bounds check
            if (nr >= 0 && nr < rows && nc >= 0 && nc < cols) yield return (nr, nc, grid[nr, nc]);
        }
    }
}

int InspectAtSpots(char[,] grid)
{
    int rows = grid.GetLength(0);
    int cols = grid.GetLength(1);
    int count = 0;
    for (int r = 0; r < rows; r++)
    {
        for (int c = 0; c < cols; c++)
        {
            if (grid[r,c] != '@') continue;//ignore anything that isn't a roll
            var neighbors = GetNeighbors(grid, r, c).ToList();

            int atCount = neighbors.Count(n => n.Value == '@');
            if (atCount < 4) count++;
            else continue;
        }

    }

    return count;
}

int totalRemovedRolls(char[,] grid)
{
    int rows = grid.GetLength(0);
    int cols = grid.GetLength(1);
    var toRemove = new List<(int Row, int Col)>();

    for (int r = 0; r < rows; r++)
    {
        for (int c = 0; c < cols; c++)
        {
            if (grid[r,c] != '@') continue; //ignore anything that isn't a roll
            var neighbors = GetNeighbors(grid, r, c);
            int atCount = neighbors.Count(n => n.Value == '@');
            if (atCount < 4)
            {
               toRemove.Add((r,c));
            }
        }
    }

    foreach (var (r,c) in toRemove)
    {
        grid[r, c] = '.';
    }
    return toRemove.Count();
}

int RemoveAllWeakRolls(char[,] grid)
{
    int totalRemoved = 0;
    int removed;

    do
    {
        removed = totalRemovedRolls(grid);
        totalRemoved += removed;
    } while (removed > 0);
    return totalRemoved;
}

