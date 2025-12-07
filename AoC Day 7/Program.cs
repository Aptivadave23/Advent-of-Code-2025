try
{
    var beamFile = Path.Combine(AppContext.BaseDirectory, "Files", "Input.txt");
    using StreamReader beamReader = new StreamReader(beamFile);
    var beam = beamReader.ReadToEnd().Split(new[] { "\r\n", "\n" }, StringSplitOptions.RemoveEmptyEntries);

    Console.WriteLine($"Beam splits:  {beamSplitCount(beam)}");
    Console.WriteLine($"Total Universes:  {CountTimelines(beam)}");
}
catch (Exception ex)
{
    Console.WriteLine($"An error occurred: {ex.Message}");
}

int beamSplitCount(string[] beam)
{

    int rowCount = beam.Length;
    int colCount = beam[0].Length;

  
    
    int splitCount = 0;
    var currentBeams = new HashSet<int>();
    bool started = false;//have we found 'S'?

    //first, find the start of the beam
    //we know the start because the string will contain an S
    //then we can move on and find the splits
    foreach (var b in beam)
    {
        if (!started)
        {
            //look for the start
            int starCol = b.ToLower().IndexOf('s');
            if (starCol >= 0)
            {
                //intialize the beam
                currentBeams.Add(starCol);
                started = true;
            }
            continue;
        }

        //can't talk, I'm shifting into "beam mode"
        var nextBeams = new HashSet<int>();

        foreach (var c in currentBeams)
        {
            char cell = b[c];
            if (cell == '^')
            {
                //We have a split
                splitCount++;

                if (c - 1 >= 0) nextBeams.Add(c - 1);
                if (c + 1 < colCount) nextBeams.Add(c + 1);
            }
            else nextBeams.Add(c);//beam continues
        }

        currentBeams = nextBeams;
    }
    return splitCount;
}

long CountTimelines(string[] beam)
{
    int rows = beam.Length;
    int cols = beam[0].Length;

    // 1. Find 'S'
    int startRow = -1;
    int startCol = -1;

    for (int r = 0; r < rows; r++)
    {
        int idx = beam[r].IndexOf('S');
        if (idx >= 0)
        {
            startRow = r;
            startCol = idx;
            break;
        }
    }

    if (startRow == -1)
        throw new InvalidOperationException("No 'S' found in input.");

    // 2. current[c] = how many timelines are in column c at this row
    long[] current = new long[cols];
    long[] next = new long[cols];

    // The particle starts *below* S in the same column
    if (startRow + 1 < rows)
        current[startCol] = 1;

    // 3. Walk down row by row, propagating counts
    for (int r = startRow + 1; r < rows - 1; r++)
    {
        Array.Clear(next, 0, cols);
        string row = beam[r];

        for (int c = 0; c < cols; c++)
        {
            long count = current[c];
            if (count == 0) continue;

            char cell = row[c];

            if (cell == '^')
            {
                // Split this many timelines to left and right
                if (c - 1 >= 0)
                    next[c - 1] += count;
                if (c + 1 < cols)
                    next[c + 1] += count;
            }
            else
            {
                // Continue straight down
                next[c] += count;
            }
        }

        // Swap current/next
        var temp = current;
        current = next;
        next = temp;
    }

    // 4. Sum all timelines that make it to the last row
    long total = 0;
    for (int c = 0; c < cols; c++)
        total += current[c];

    return total;
}
