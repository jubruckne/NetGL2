namespace NetGL;

public static class BackgroundTaskScheduler {
    private interface IBackgroundTask {
        int priority { get; }
        Thread? worker { get; set; }

        void invoke_threaded();
        void invoke_completed();
    }

    public class BackgroundTask<TResult> : IBackgroundTask {
        public int priority { get; }
        public Thread? worker { get; set; }
        private readonly string id;
        private TResult data = default!;

        private readonly Func<TResult> threaded;
        private readonly Action<TResult> completed;

        void IBackgroundTask.invoke_threaded() => data = threaded();
        void IBackgroundTask.invoke_completed() => completed(data);

        public BackgroundTask(in string id, Func<TResult> threaded, Action<TResult> completed, int priority = 99) {
            this.id = id;
            this.threaded = threaded;
            this.completed = completed;
            this.priority = priority;
        }

        public override string ToString() => id;
    }

    private static readonly List<IBackgroundTask> scheduled_tasks;
    private static readonly List<IBackgroundTask> running_tasks;
    private static readonly List<IBackgroundTask> completed_tasks;

    public static int worker_count { get; set; }

    static BackgroundTaskScheduler() {
        scheduled_tasks = [];
        running_tasks = [];
        completed_tasks = [];
    }

    public static void schedule(string id, Action threaded, Action completed, int priority = 99) {
        scheduled_tasks.Add(new BackgroundTask<int>(id, run_threaded, run_completed, priority));

        void run_completed(int obj) => completed();
        int run_threaded() {
            threaded();
            return 0;
        }

        scheduled_tasks.Sort(static (x, y) => x.priority - y.priority);
    }

    public static void schedule<TResult>(string id, Func<TResult> threaded, Action<TResult> completed, int priority = 99) {
        scheduled_tasks.Add(new BackgroundTask<TResult>(id, threaded, completed, priority));
        scheduled_tasks.Sort(static (x, y) => x.priority - y.priority);
    }

    internal static void process_scheduled_tasks() {
        completed_tasks.Clear();

        while(running_tasks.Count <= 4 && scheduled_tasks.pop(out var task)) {
            //Console.WriteLine($"task: {task} started.");
            running_tasks.Add(task);

            task.worker = new(task.invoke_threaded);
            task.worker.Name = "[WORKER]";

            task.worker.Priority = task.priority switch {
                0 => ThreadPriority.Normal,
                1 => ThreadPriority.BelowNormal,
                2 => ThreadPriority.Lowest,
                _ => ThreadPriority.Lowest
            };

            task.worker.Start();
        }
    }

    internal static void process_completed_tasks() {
        if (running_tasks.pop(static t => !t.worker!.IsAlive, out var task)) {
            //Console.WriteLine($"task: {task} completed.");

            completed_tasks.Add(task);
            task.invoke_completed();
            task.worker = null;
        }
    }
}