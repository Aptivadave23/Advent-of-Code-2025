

try
{
    var serverFile = Path.Combine(AppContext.BaseDirectory, "Files", "Servers.txt");
    using StreamReader serverReader = new StreamReader(serverFile);
    var serverResult = serverReader.ReadToEnd().Split(new[] { "\r\n", "\n" }, StringSplitOptions.RemoveEmptyEntries);
    var serverPaths = getServerDictionay(serverResult);
    Console.WriteLine($"Paths from You to Out:  {getYouServerPaths(serverPaths)}");
    Console.WriteLine($"Paths from SVR to Out via DAC/FFT:  {getSVRServerPaths(serverPaths)}");
}
catch (Exception ex)
{
    Console.WriteLine(ex.Message);
}

Dictionary<string, List<string>> getServerDictionay(string[] serversStrings)
{
    var serverPaths = new Dictionary<string, List<string>>(StringComparer.OrdinalIgnoreCase);
    foreach (var serverString in serversStrings)
    {
        if (string.IsNullOrWhiteSpace(serverString)) continue;
        var parts = serverString.Split(':', 2, StringSplitOptions.TrimEntries);
        var serverName = parts[0];

        List<string> connections;
        if (parts.Length == 1 || string.IsNullOrWhiteSpace(parts[1])) connections = new List<string>();
        else connections = parts[1].Split(new[] { ' ', ',' }, StringSplitOptions.RemoveEmptyEntries).ToList();

        serverPaths[serverName] = connections;
    }
    return serverPaths;
}
long getYouServerPaths(Dictionary<string, List<string>> serverPaths)
{
    long count = 0;
   
    var memo = new Dictionary<string, long>(StringComparer.OrdinalIgnoreCase);
    return countPaths("you", serverPaths, memo);
}

long getSVRServerPaths(Dictionary<string, List<string>> serverPaths)
{
   
    var memo = new Dictionary<(string node, int mask), long>();
    return countPathsThruDACFFT("svr",0, serverPaths, memo);
}

long countPaths(string node, Dictionary<string, List<string>> graph, Dictionary<string, long> memo)
{
    if (node.Equals("out", StringComparison.OrdinalIgnoreCase)) return 1;

    if (memo.TryGetValue(node, out var cached)) return cached;

    if(!graph.TryGetValue(node, out var neighbors) || neighbors.Count == 0)
    {
        memo[node] = 0;
        return 0;
    }

    long total = 0;
    foreach (var neighbor in neighbors)
    {
        total += countPaths(neighbor.ToLower(), graph, memo);
    }

    memo[node] = total;
    return total;
}

long countPathsThruDACFFT(string node, int mask, Dictionary<string, List<string>> graph, Dictionary<(string node, int mask), long> memo)
{
    if (node.Equals("out", StringComparison.OrdinalIgnoreCase)) return (mask & 0b11) == 0b11 ? 1 : 0;
    var key = (node, mask);
    if(memo.TryGetValue(key, out var cached)) return cached;

    if (!graph.TryGetValue(node, out var neighbors) || neighbors.Count == 0)
    {
        memo[key] = 0;
        return 0;
    }

    long total = 0;
    foreach (var neighbor in neighbors)
    {
        int newMask = mask;
        if (neighbor.Equals("dac", StringComparison.OrdinalIgnoreCase))
        {
            newMask |= 1;
        }
        else if (neighbor.Equals("fft", StringComparison.OrdinalIgnoreCase))
        {
            newMask |= 2;
        }
        total += countPathsThruDACFFT(neighbor.ToLower(), newMask, graph, memo);
    }
    memo[key] = total;
    return total;
}
