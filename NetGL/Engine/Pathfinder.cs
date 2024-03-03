using System.Diagnostics.CodeAnalysis;

namespace NetGL;

public class Pathfinder<T> where T: IEquatable<T> {
    public delegate float HeuristicDelegate(T a, T b);
    public delegate IEnumerable<(T neighbor, float cost)> NeighborsDelegate(T node);

    private readonly HeuristicDelegate heuristic;
    private readonly NeighborsDelegate neighbors;

    public Pathfinder(HeuristicDelegate heuristic, NeighborsDelegate neighbors) {
        this.heuristic = heuristic;
        this.neighbors = neighbors;
    }

    public bool find_path(in T start, in T goal, [MaybeNullWhen(false)] out IReadOnlyList<T> path) {
        var openSet = new PriorityQueue<T, float>();
        var cameFrom = new Dictionary<T, T>();
        var gScore = new Dictionary<T, float> {{start, 0}};
        var fScore = new Dictionary<T, float> {{start, heuristic(start, goal)}};

        openSet.Enqueue(start, fScore[start]);

        while (openSet.Count != 0) {
            var current = openSet.Dequeue();
            if(current.Equals(goal)) {
                path = reconstruct_path(cameFrom, current);
                return true;
            }

            foreach (var (neighbor, cost) in neighbors(current)) {
                var tentativeGScore = gScore[current] + cost;
                if (gScore.TryGetValue(neighbor, out var value) && !(tentativeGScore < value)) continue;

                cameFrom[neighbor] = current;
                gScore[neighbor] = tentativeGScore;
                fScore[neighbor] = tentativeGScore + heuristic(neighbor, goal);

                if (!openSet.UnorderedItems.Any(item => item.Element.Equals(neighbor))) {
                    openSet.Enqueue(neighbor, fScore[neighbor]);
                }
            }
        }

        path = null;
        return false;
    }

    private IReadOnlyList<T> reconstruct_path(in Dictionary<T, T> came_from, T current) {
        var total_path = new List<T> {current};
        while (came_from.ContainsKey(current)) {
            current = came_from[current];
            total_path.Insert(0, current);
        }
        return total_path;
    }
}