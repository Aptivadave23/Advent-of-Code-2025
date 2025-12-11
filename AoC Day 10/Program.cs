using System.Security.Cryptography.X509Certificates;

try
{
    var inputFile = Path.Combine(AppContext.BaseDirectory, "Files", "Input.txt");
    using StreamReader inputReader = new StreamReader(inputFile);
    var inputResult = inputReader.ReadToEnd().Split(new[] { "\r\n", "\n" }, StringSplitOptions.RemoveEmptyEntries);

   Console.WriteLine($"Button Press counts for question 1:  {switchCount(inputResult)}");
   Console.WriteLine($"Joltage Press counts for question 2:  {getJoltagePresses(inputResult)}");

}
catch (Exception ex)
{
    Console.WriteLine(ex.Message);
}
//Question 1
long switchCount(string[] input)
{
    long count = 0;
   
    //get each string so that we can get our light diagram and wiring schematics
    foreach (string inputItem in input)
    {
        //split the string via spaces
        var machine = inputItem.Split(' ');

        //now, get the light diagram
        var lightDiagram = PatternToMask(machine[0]);
        List<int> buttonMasks = new List<int>();
        //get the combinations of switches and create masks
        for (int i = 1; i < machine.Length; i++)
        {
            //if we're at the {, jump out
            if (machine[i].Substring(0, 1) == "{") break;
            //get your indices
            int[] indices = machine[i].Trim('(', ')')
                .Split(',', StringSplitOptions.RemoveEmptyEntries)
                .Select(int.Parse)
                .ToArray();
            if (indices.Length == 0) continue;
            int mask = buttonMask(indices);
            buttonMasks.Add(mask);
        }

        count = count + getMinPressesForMachine(lightDiagram, buttonMasks);

    }

    
    return count;
}

int PatternToMask(string rawPattern)
{
    string pattern = rawPattern.Trim('[', ']');

    int mask = 0;

    for (int i = 0; i < pattern.Length; i++)
    {
        if (pattern[i] == '#')
        {
            //set bit
            mask |= (1 << i);
        }
    }

    return mask;
}

int buttonMask(params int[] indices)
{
    int mask = 0;
    foreach (var index in indices)
    {
        mask |= (1 <<index);
    }

    return mask;
}

int getMinPressesForMachine(int targetMask, List<int> buttonMasks)
{
    int n = buttonMasks.Count;
    int best = int.MaxValue;

    for (int combo = 0; combo < (1 << n); combo++)
    {
        int current = 0;
        int pressCount = 0;

        for (int j = 0; j < n; j++)
        {
            if ((combo & (1 << j)) != 0)
            {
                current ^= buttonMasks[j];
                pressCount++;
            }
        }

        if (current == targetMask && pressCount < best)
        {
            best = pressCount;
        }
    }

    return best;
}

//question 2
long getJoltagePresses(string[] input)
{
    long count = 0;
    foreach (var inputItem in input)
    {
        //split the string via spaces
        var machine = inputItem.Split(' ');

        int joltageIndex = -1;
        for (int i = 1; i < machine.Length; i++)
        {
            if (machine[i].StartsWith("{"))
            {
                joltageIndex = i;
                break;
            }
        }

        int[] target = parseJoltageTargets(machine[joltageIndex]);

        List<int[]> buttons = new List<int[]>();

        for (int i = 1; i < joltageIndex; i++)
        {
            int[] indices = machine[i].Trim('(', ')')
                .Split(',', StringSplitOptions.RemoveEmptyEntries)
                .Select(int.Parse)
                .ToArray();
            if (indices.Length == 0) continue;
            buttons.Add(indices);
        }

        int pressesForMachine = minJoltagePressesForMachine(target, buttons);
        count += pressesForMachine;
    }
    return count;
}

int[] parseJoltageTargets(string rawTargets)
{
    string targets = rawTargets.Trim('{', '}');
    return targets.Split(',', StringSplitOptions.RemoveEmptyEntries)
        .Select(int.Parse)
        .ToArray();
}

int minJoltagePressesForMachine(int[] target, List<int[]> buttons)
{
    int k = target.Length;
    int n = buttons.Count;

    // current counters we’re building as we choose button counts
    int[] current = new int[k];

    int best = int.MaxValue;

    // For each button j, we can never press it more than
    // the smallest target of any counter it touches
    int[] maxPress = new int[n];
    for (int j = 0; j < n; j++)
    {
        int limit = int.MaxValue;
        foreach (int idx in buttons[j])
        {
            limit = Math.Min(limit, target[idx]);
        }
        maxPress[j] = limit;
    }

    void dfs(int buttonIndex, int pressesSoFar)
    {
        // prune if this branch is already no better than best found
        if (pressesSoFar >= best)
            return;

        // if we've assigned counts for all buttons, check if we hit the target
        if (buttonIndex == n)
        {
            for (int i = 0; i < k; i++)
            {
                if (current[i] != target[i])
                    return;
            }

            if (pressesSoFar < best)
                best = pressesSoFar;

            return;
        }

        int[] button = buttons[buttonIndex];

        // Try p presses of this button: p = 0..maxPress[buttonIndex]
        // We apply incrementally and backtrack.

        // Option 1: press this button 0 times
        dfs(buttonIndex + 1, pressesSoFar);

        int timesApplied = 0;

        for (int p = 1; p <= maxPress[buttonIndex]; p++)
        {
            timesApplied++;

            bool overshoot = false;

            // apply one more press
            foreach (int idx in button)
            {
                current[idx]++;

                if (current[idx] > target[idx])
                {
                    overshoot = true;
                }
            }

            if (overshoot)
            {
                // undo that last press
                foreach (int idx in button)
                {
                    current[idx]--;
                }
                timesApplied--;
                break; // no point trying larger p, they'll be worse
            }

            dfs(buttonIndex + 1, pressesSoFar + p);
        }

        // backtrack: undo all presses we applied for this button
        if (timesApplied > 0)
        {
            foreach (int idx in button)
            {
                current[idx] -= timesApplied;
            }
        }
    }

    dfs(0, 0);

    if (best == int.MaxValue)
        throw new Exception("No valid joltage configuration found for this machine.");

    return best;
}


bool allZeros(int[] arr)
{
    for (int i = 0; i < arr.Length; i++)
        if (arr[i] != 0) return false;
    return true;
}

bool arraysEqual(int[] a, int[] b)
{
    if (a.Length != b.Length) return false;
    for (int i = 0; i < a.Length; i++)
        if (a[i] != b[i]) return false;
    return true;
}

int[] copyArray(int[] a)
{
    int[] copy = new int[a.Length];
    for (int i = 0; i < a.Length; i++)
        copy[i] = a[i];
    return copy;
}

