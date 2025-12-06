try
{
    var inventory =
        "2157315-2351307,9277418835-9277548385,4316210399-4316270469,5108-10166,872858020-872881548,537939-575851,712-1001,326613-416466,53866-90153,907856-1011878,145-267,806649-874324,6161532344-6161720341,1-19,543444404-543597493,35316486-35418695,20-38,84775309-84908167,197736-309460,112892-187377,336-552,4789179-4964962,726183-793532,595834-656619,1838-3473,3529-5102,48-84,92914229-92940627,65847714-65945664,64090783-64286175,419838-474093,85-113,34939-52753,14849-30381";

    var inventoryRanges = inventory.Split(',');

    Console.WriteLine($"Total bad inventory:  {badInventory(inventoryRanges)}");

    Console.WriteLine($"Updated bad inventory:  {updatedBadInventory(inventoryRanges)}");
}
catch (Exception ex)
{
    Console.WriteLine($"An error occurred: {ex.Message}");
}

long badInventory(string[] ranges)
{
    long count = 0;
    //loop through the ranges
    foreach (string range in ranges)
    {
        //get the upper bound and lower bound of each range
        var r = range.Split('-', StringSplitOptions.TrimEntries);//r[0] is the lower bound of the range, r[1] the upper bound

        //now, loop through the range so we can check each number in the range
        for (long x = long.Parse(r[0]); x <= long.Parse(r[1]); x++)
        {
            var number = x.ToString().ToCharArray();
            //get the length of the char array
            var l = number.Length;
            //now, split the array in half
            if (l % 2 == 0) //we know we can split the number equally in half
            {
                var leftSide = x.ToString().Substring(0, l / 2);
                var rightSide = x.ToString().Substring(l / 2);
                if (leftSide == rightSide) count = count + x;
            }
            else continue;//if we can, we know that both sides are not dups
        }
    }
    return count;
}

long updatedBadInventory(string[] ranges)
{
    long count = 0;
    //loop through the ranges
    foreach (string range in ranges)
    {
        //get the upper bound and lower bound of each range
        var r = range.Split('-', StringSplitOptions.TrimEntries);//r[0] is the lower bound of the range, r[1] the upper bound
        //now, loop through the range so we can check each number in the range
        for (long x = long.Parse(r[0]); x <= long.Parse(r[1]); x++)
        {
            var number = x.ToString().ToCharArray();
            int length = number.Length;

            //look for at least 2 characters to have "something repeated"
            if (length < 2) continue;

            //find all possible pattern lengths
            //start at 1 so we allow things like 11, 99, 111, etc
            for (int patternLen = 1; patternLen <= length/2; patternLen++)
            {
                //total length must be an exact multiple of the pattern length
                if(length%patternLen !=0) continue;

                bool matches = true;

                //compare each position with the corresponding char in the pattern
                for (int i = patternLen; i < length; i++)
                {
                    if (number[i] != number[i % patternLen])
                    {
                        matches = false; break;
                    }
                }

                if (matches)
                {
                    int repeats = length / patternLen;
                    if (repeats >= 2)
                    {
                        count = count + x;
                        break;
                    }
                }
            }
        }

    }

    return count;
}