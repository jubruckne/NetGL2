namespace NetGL;

public static class BackgroundTaskScheduler {
    private class BackgroundTask {
        public readonly string id;

        internal readonly int priority;
        internal readonly Action threaded;
        internal readonly Action completed;
        internal Thread? worker;

        public BackgroundTask(in string id, Action threaded, Action completed, int priority = 99) {
            this.id = id;
            this.threaded = threaded;
            this.completed = completed;
            this.priority = priority;
        }

        public override string ToString() => id;
    }

    private static readonly List<BackgroundTask> scheduled_tasks;
    private static readonly List<BackgroundTask> running_tasks;
    private static readonly List<BackgroundTask> completed_tasks;

    static BackgroundTaskScheduler() {
        scheduled_tasks = [];
        running_tasks = [];
        completed_tasks = [];
    }

    public static void schedule(in string id, Action threaded, Action completed, int priority = 99) {
        Console.WriteLine($"Scheduling thread: " + id);
        scheduled_tasks.Add(new BackgroundTask(id, threaded, completed, priority));
    }

    internal static void process_scheduled_tasks() {
        scheduled_tasks.Sort((x, y) => x.priority - y.priority);

        while(running_tasks.Count <= 4 && scheduled_tasks.pop(out var task)) {
            Console.WriteLine($"Starting thread for {task}");
            running_tasks.Add(task);

            task.worker = new(task.threaded.Invoke);
            task.worker.Priority = task.priority > 1
                ? ThreadPriority.Lowest
                : ThreadPriority.Normal;

            task.worker.Start();
        }
    }

    internal static void process_completed_tasks() {
        if (running_tasks.pop(t => !t.worker!.IsAlive, out var task)) {
            Console.WriteLine($"Thread for {task} completed!");

            completed_tasks.Add(task);

            Console.WriteLine($"Running completion function for {task}");

            task.completed();
        }
    }
}