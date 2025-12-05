using System.Collections;
using System.Reflection.Metadata.Ecma335;

try
{
    //get ranges
    var rangesFile = Path.Combine(AppContext.BaseDirectory, "Files", "IDRanges.txt");
    using StreamReader rangesReader = new StreamReader(rangesFile);
    var ranges = rangesReader.ReadToEnd().Split(new[] { "\r\n", "\n" }, StringSplitOptions.RemoveEmptyEntries);

    //get ingredient IDs
    var ingredientsFile = Path.Combine(AppContext.BaseDirectory, "Files", "Ingredients.txt");
    using StreamReader ingredientsReader = new StreamReader(ingredientsFile);
    var ingredients = ingredientsReader.ReadToEnd().Split(new[] { "\r\n", "\n" }, StringSplitOptions.RemoveEmptyEntries);

    var freshCount = 0;
    foreach (var ingredient in ingredients)
    {
        if (freshIngredient(ingredient, ranges)) freshCount++;
    }

    Console.WriteLine($"Number of fresh ingredients:  {freshCount}");

    var totalIngredients = totalFreshIngredients(ranges);
    Console.WriteLine($"Total number of all fresh ingredients:  {totalIngredients}");
}
catch (Exception ex)
{
    Console.WriteLine($"An error occurred: {ex.Message}");
}

bool freshIngredient(string ingredient, string[] IDRanges)
{
    var found = false;
    foreach (var range in IDRanges)
    {
        //split the ranges
        var ranges = range.Split("-", StringSplitOptions.TrimEntries);
        //check and see if the ingredient falls between the range
        if (long.Parse(ingredient) >= long.Parse(ranges[0]) &&
            long.Parse(ingredient) <= long.Parse(ranges[1])) found = true;
        else continue;
    }
    return found;
}

long totalFreshIngredients(string[] ingredientIDs)
{
    var ranges = ingredientIDs
        .Select(line =>
        {
            var parts = line.Split('-', StringSplitOptions.TrimEntries);
            long start = long.Parse(parts[0]);
            long end = long.Parse(parts[1]);
            return (start, end);
        })
        .OrderBy(r => r.start)
        .ToList();

    if (ranges.Count == 0) return 0;

    long total = 0;

    long currentStart = ranges[0].start;
    long currentEnd = ranges[0].end;

    foreach (var r in ranges.Skip(1))
    {
        //overlapping or directly adjacent ranges:  merge them
        if (r.start <= currentEnd)
        {
            if (r.end > currentEnd) currentEnd = r.end;
        }// currentEnd = r.end;
        else
        {
            //close out the previous merge range
            total += currentEnd - currentStart + 1;
            //start new merge
            currentStart = r.start;
            currentEnd = r.end;
        }
    }
    //add the last merged range
    total += currentEnd - currentStart + 1;

    return total;
}