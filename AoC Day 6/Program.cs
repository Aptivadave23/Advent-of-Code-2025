try
{
    var mathFile = Path.Combine(AppContext.BaseDirectory, "Files", "Math.txt");
    using StreamReader mathReader = new StreamReader(mathFile);
    var result = mathReader.ReadToEnd().Split(new[] { "\r\n", "\n" }, StringSplitOptions.RemoveEmptyEntries);

    Console.WriteLine($"Math homework answer:  {mathAnswer(result)}");
}
catch (Exception ex)
{
    Console.WriteLine($"An error occurred: {ex.Message}");
}

long mathAnswer(string[] problems)
{
    try
    {
        // Split each line into "columns" (tokens), stripping BOM if present
        var rows = problems
            .Select(line => line
                .TrimStart('\uFEFF') // in case the first line has a BOM
                .Split(' ', StringSplitOptions.RemoveEmptyEntries))
            .ToArray();

        int rowCount = rows.Length;        // e.g., 5 (4 number rows + 1 operator row)
        if (rowCount == 0) return 0;

        int colCount = rows[0].Length;     // e.g., 1000 problems/columns

        long answer = 0;

        // Loop across each "problem" column
        for (int col = 0; col < colCount; col++)
        {
            // Bottom row is the operator (* or +)
            string symbol = rows[rowCount - 1][col];

            long problemAnswer = 0;

            // Now walk the numbers ABOVE the operator
            for (int row = 0; row < rowCount - 1; row++)
            {
                long value = long.Parse(rows[row][col]);

                if (symbol == "+")
                {
                    // addition
                    problemAnswer += value;
                }
                else if (symbol == "*")
                {
                    // multiplication
                    if (problemAnswer == 0)
                        problemAnswer = value;        // first value
                    else
                        problemAnswer *= value;
                }
                else
                {
                    throw new InvalidOperationException(
                        $"Unexpected operator '{symbol}' at row {rowCount - 1}, col {col}");
                }
            }

            // Add this column's result into the grand total
            answer += problemAnswer;
        }

        return answer;
    }
    catch (Exception ex)
    {
        throw new Exception($"mathAnswer failed: {ex.Message}", ex);
    }
}