using System.Numerics;

namespace NetGL;

public class RevolvingList<T>
    where T: unmanaged, IDivisionOperators<T, float, T>, IMultiplyOperators<T, float, T>, INumber<T> {
    private readonly int max_size;
    private readonly Queue<T> queue;

    public RevolvingList(int max_size) {
        this.max_size = max_size;
        queue         = new Queue<T>(this.max_size);
    }

    public void add(T item) {
        if (queue.Count == max_size)
            queue.Dequeue(); // Removes the oldest item

        queue.Enqueue(item); // Adds a new item
    }

    public int count => queue.Count;
    public T last => queue.Last();

    public T sum() {
        var result = T.Zero;

        foreach (var item in queue) {
            result += item;
        }

        return result;
    }

    public T minimum() {
        if (queue.Count == 0) return T.Zero;

        var result = queue.First();

        foreach (var item in queue) {
            if (item < result)
                result = item;
        }

        return result;
    }

    public T maximum() {
        if (queue.Count == 0) return T.Zero;

        var result = T.Zero;

        foreach (var item in queue) {
            if (item > result)
                result = item;
        }

        return result;
    }

    public T average() {
        if (queue.Count == 0) return T.Zero;
        return sum() / count;
    }

    public Span<T> as_span() {
        if (queue.Count > 0) return queue.ToArray();
        return new T[1] { T.Zero };
    }

    public override string ToString()
        => $"avg:{average() * 1000:F1}, min:{minimum() * 1000:F1}, max:{maximum() * 1000:F1}";
}