try
{
    var comboFile = Path.Combine(AppContext.BaseDirectory, "Files", "Combo.txt");
    using StreamReader comboReader = new StreamReader(comboFile);
    var results = comboReader.ReadToEnd().Split(new[] { "\r\n", "\n" }, StringSplitOptions.RemoveEmptyEntries); 
    Console.WriteLine($"Number of times past zero:  {safeCombo(results)}");

    Console.WriteLine($"Number of times dial passes zero:  {newSafeCombo(results)}");
}
catch (Exception ex)
{
    Console.WriteLine($"An error occurred: {ex.Message}");
}

int safeCombo(string[] directions)
{
    var count = 0;
    var position = 50;

    foreach (var direction in directions)
    {
        char d = char.ToLower(direction[0]);      // 'l' or 'r'
        int steps = int.Parse(direction.Substring(1));

        // We only care about net movement on a 0-99 dial
        steps %= 100;

        if (d == 'l')
        {
            position = (position - steps) % 100;
            if (position < 0) position += 100;    // fix negative modulo
        }
        else if (d == 'r')
        {
            position = (position + steps) % 100;
            position %= 100;
        }
        else
        {
            throw new Exception("not a valid direction");
        }

        if (position == 0)
        {
            count++;
        }
    }

    return count;
}


int newSafeCombo(string[] directions)
{
    int position = 50;
    int passes = 0;

    foreach (var direction in directions)
    {
        char d = char.ToLower(direction[0]);
        int steps = int.Parse(direction.Substring(1));

        if (d == 'r')
        {
            // Count how many times we hit 0 while moving right
            passes += (position + steps) / 100;

            // Final position after the move
            position = (position + steps) % 100;
        }
        else if (d == 'l')
        {
            // Count how many times we hit 0 while moving left
            if (position == 0)
            {
                // Starting on 0: we only hit it once every full revolution
                passes += steps / 100;
            }
            else if (steps >= position)
            {
                // First time we reach 0 is after 'position' steps,
                // then every 100 steps after that
                passes += 1 + (steps - position) / 100;
            }
            // else: steps < position → we never reach 0 this move

            // Final position after the move (mod 100)
            int stepsMod = steps % 100;
            position -= stepsMod;
            if (position < 0) position += 100;
        }
        else
        {
            throw new Exception("not a valid direction");
        }
    }

    return passes;
}

