//read the file

using System.Collections;
using System.Runtime.CompilerServices;
using System.Security.Cryptography.X509Certificates;

try
{
    var filePath = Path.Combine(AppContext.BaseDirectory, "Files", "Input.txt");
    using StreamReader reader = new(filePath);
    //string text = reader.ReadToEnd();
    //Console.WriteLine(text);
    var batteries = reader.ReadToEnd().Split('\n');
    var count = 0;
    var output = 0;
    foreach (var batteriesItem in batteries)
    {
        count++;
        Console.WriteLine($"Line {count.ToString()}:  {jolts(batteriesItem)}");
        output += jolts(batteriesItem);
    }

    Console.WriteLine($"Output jolts:  {output.ToString()}");

    //get big jolts
    count = 0;
    double bigOutput = 0;
    foreach (var bigBattery in batteries)
    {
        count++;
        Console.WriteLine($"Line {count.ToString()}:  {largeJolts(bigBattery)}");
        bigOutput += largeJolts(bigBattery);
    }

    Console.WriteLine($"Big jolts:  {bigOutput.ToString()}");
}
catch (IOException ex)
{
    Console.WriteLine("The file could not be reached");
    Console.WriteLine(ex.Message);
}

int jolts(string battery)
{
    //get the length of the battery string
    var batLength = battery.Trim().Length;
    if (batLength > 0 || !string.IsNullOrEmpty(battery.Trim()))
    {
        int bestFirst = -1;
        int bestValue = -1;

        foreach (var b in battery)
        {
            if(!char.IsDigit(b))
                continue;
            int d = b - '0';
            //if you already have at least 1 digit, we can create the pair
            if (bestFirst != -1)
            {
                int candidate = bestFirst * 10 + d;
                if (candidate > bestValue) bestValue = candidate;
            }

            //update the best first digit for future pairs
            if(d>bestFirst) bestFirst = d;
        }

       //if we never find a pair, this means there wasn't enough digits to worry about
       if (bestValue == -1) return 0;
        else return bestValue;
    }
    else return 0;
}

long largeJolts(string battery)
{
    var digits = new string(battery.Where(char.IsDigit).ToArray());
    if (digits.Length == 0) return 0;

    int keep = 12;
    if (digits.Length <= keep) return long.Parse(digits);//nothing to remove

    int remove = digits.Length - keep;//how many digits we can keep

    var stack = new System.Text.StringBuilder();

    foreach (var c in digits)
    {
        while (remove > 0 && stack.Length > 0 && stack[stack.Length - 1] < c)
        {
            stack.Length--; //pop
            remove--;
        }

        stack.Append(c);
    }

    //if there is still stuff to remove, chop from the end
    if(remove > 0) stack.Length -= remove;

    //ensure we only keep the first 'keep' digits
    if(stack.Length > keep) stack.Length = keep;

    var returnString = stack.ToString();
    return long.Parse(returnString);

}