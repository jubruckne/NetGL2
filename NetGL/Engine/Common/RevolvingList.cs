using System.Numerics;

namespace NetGL;

public class RevolvingList<T>
    where T: unmanaged, IDivisionOperators<T, float, T>, IMultiplyOperators<T, float, T>, INumber<T> {
    private readonly int max_size;
    private readonly Queue<T> queue;
    private readonly T[] array;

    private T r_sum = T.Zero;
    private T r_min = T.Zero;
    private T r_max = T.Zero;

    public RevolvingList(int max_size) {
        this.max_size = max_size;
        queue         = new Queue<T>(this.max_size);
        array         = new T[this.max_size];
    }

    public void add(T item) {
        if (queue.Count == max_size) {
            var removed = queue.Dequeue();
            r_sum -= removed;
        }

        queue.Enqueue(item);
        r_sum += item;

        update_min_max();
    }

    private void update_min_max() {
        if (queue.Count == 0) return;

        var min = queue.First();
        var max = queue.First();

        foreach (var item in queue) {
            if (item < min)
                min = item;
            else if (item > max)
                max = item;
        }
    }

    public int count => queue.Count;

    public T sum() => r_sum;
    public T average() => queue.Count == 0 ? T.Zero : sum() / count;
    public T minimum() => r_min;
    public T maximum() => r_max;

    public Span<T> as_span() {
        queue.CopyTo(array, 0);
        return array;
    }

    public override string ToString()
        => $"avg:{average() * 1000:F1}, min:{r_min * 1000:F1}, max:{r_max * 1000:F1}";
}