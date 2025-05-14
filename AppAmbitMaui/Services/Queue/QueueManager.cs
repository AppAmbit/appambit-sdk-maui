using System.Diagnostics;

namespace AppAmbit.Services.Queue;

internal class QueueManager
{
    private readonly Queue<Func<Task>> _queue = new();
    private readonly object _lock = new();

    public void EnqueueTaskAsync(Func<Task> request)
    {
        lock (_lock)
        {
            _queue.Enqueue(request);
        }
    }

    public IEnumerable<Func<Task>> DequeueTaksAll()
    {
        lock (_lock)
        {
            while (_queue.Count > 0)
            {
                yield return _queue.Dequeue();
            }
        }
    }

    public bool HasPendingRequest
    {
        get
        {
            lock (_lock)
            {
                return _queue.Count > 0;
            }
        }
    }
}
