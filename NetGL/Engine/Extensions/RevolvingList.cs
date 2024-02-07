namespace NetGL.Engine;

public class RevolvingList<T> where T: unmanaged {
    private readonly int max_size;
    private readonly Queue<T> queue;

    public RevolvingList(int max_size) {
        this.max_size = max_size;
        queue = new Queue<T>(this.max_size);
    }

    public void add(T item) {
        if (queue.Count == max_size)
            queue.Dequeue(); // Removes the oldest item

        queue.Enqueue(item); // Adds a new item
    }

    public int count => queue.Count;
    public T last => queue.Last();

    public Span<T> as_span() => queue.ToArray();
}