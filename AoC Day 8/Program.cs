using System.Reflection.Metadata.Ecma335;
using System.Security.Cryptography.X509Certificates;

try
{
    var boxesFile = Path.Combine(AppContext.BaseDirectory, "Files", "boxes.txt");
    using StreamReader boxesReader = new StreamReader(boxesFile);
    var boxesResult = boxesReader.ReadToEnd().Split(new[] {"\r\n", "\n"}, StringSplitOptions.RemoveEmptyEntries);

    //get the boxes
    List<Box> boxes = new List<Box>();
    foreach (var line in boxesResult)
    {
        var parts = line.Split(',');
        boxes.Add(new Box
        {
            x = int.Parse(parts[0]),
            y = int.Parse(parts[1]),
            z = int.Parse(parts[2])
        });
    }

    // create the edges
    List<Edge> edges = new List<Edge>();
    for (int i = 0; i < boxes.Count; i++)
    {
        for (int j = i + 1; j < boxes.Count; j++)
        {
            var bi = boxes[i];
            var bj = boxes[j]; //unfortunate variable name :(
            long dx = bi.x - bj.x;
            long dy = bi.y - bj.y;
            long dz = bi.z - bj.z;

            long dist2 = dx * dx + dy * dy + dz * dz;
            edges.Add(new Edge
            {
                a = i,
                b = j,
                Dist2 = dist2
            });
        }
    }

    //sort edges by distance
    edges.Sort((e1, e2) => e1.Dist2.CompareTo(e2.Dist2));

    //Kruskal's algorithm to find MST
    var dsu = new DisjointSet(boxes.Count);
    int K = 1000;
    int considered = 0;
    int edgeIndex = 0;

    while (considered < K && edgeIndex < edges.Count)
    {
        var e = edges[edgeIndex];
        edgeIndex++;
        considered++;

        //Union returns true if actually merged to circuts
        dsu.Union(e.a, e.b);
    }

    List<int> componentSizes = new List<int>();
    HashSet<int> seenRoots = new HashSet<int>();

    for (int i = 0; i < dsu.Count; i++)
    {
        int root = dsu.Find(i);
        if (seenRoots.Add(root)) componentSizes.Add(dsu.GetSizeOfRoot(root));
    }

    componentSizes.Sort();
    componentSizes.Reverse();

    int c1 = componentSizes[0];
    int c2 = componentSizes[1];
    int c3 = componentSizes[2];

    long result = (long)c1 * c2 * c3;
    Console.WriteLine($"The result of multiplying together the largest circuits: {result}");

    //now,find the value of the extension cord we would need to connect all the circuits
    int components = boxes.Count;
    var dsu2 = new DisjointSet(boxes.Count);
    int lastA = -1;
    int lastB = -1;

    foreach (var e in edges)
    {
        //only care about edges that actually merge components
        if (dsu2.Union(e.a, e.b))
        {
            components--;

            //this edge successfully joined to previously unconnected components
            lastA = e.a;
            lastB = e.b;

            if (components == 1)
            {
                //all boxes now in a single circuit
                break;
            }
        }
    }

    int x1 = boxes[lastA].x;
    int y1 = boxes[lastB].x;
    long unConnectedResult = (long) x1 * y1;
    Console.WriteLine($"The result of the extension cord to connect all circuits: {unConnectedResult}");

}
catch (Exception ex)
{
    Console.WriteLine($"An error occurred: {ex.Message}");
}






//classes
public class Box
{
    public int x;
    public int y;
    public int z;
}

public class Edge
{
    public int a; //index of box a
    public int b; //index of box b
    public long Dist2; //squared distance
}

public class DisjointSet
{
    private readonly int[] parent;
    private readonly int[] size;

    public DisjointSet(int n)
    {
        parent = new int[n];
        size = new int[n];
        for (int i = 0; i < n; i++)
        {
            parent[i] = i;
            size[i] = 1;
        }
    }

    public int Find(int x)
    {
        if (parent[x] != x)
        {
            parent[x] = Find(parent[x]); //path compression
        }
        return parent[x];
    }

    public bool Union(int x, int y)
    {
        int ra = Find(x);
        int rb = Find(y);
        if (ra == rb) return false;

        if (size[ra] < size[rb]) (ra, rb) = (rb, ra);

        parent[rb] = ra;
        size[ra] += size[rb];
        return true;
    }

    public int Count => parent.Length;

    public int GetSizeOfRoot(int root)
    {
        return size[root];
    }
}