namespace NLDMAP.Web.Administration.Caching;

// The loader belongs to the session. A caller cancels only its own wait.
internal sealed class BoundedQueryCache<TKey, TValue>(int capacity) where TKey : notnull
{
    private readonly object gate = new();
    private readonly Dictionary<TKey, Entry> entries = new();
    private long clock;

    internal int Count { get { lock (gate) { return entries.Count; } } }

    public Task<CachedResponse<TValue>> GetAsync(
        TKey key,
        Func<CancellationToken, Task<TValue>> loader,
        bool forceRefresh,
        CancellationToken sessionToken,
        CancellationToken callerToken)
    {
        callerToken.ThrowIfCancellationRequested();
        sessionToken.ThrowIfCancellationRequested();
        Task<CachedResponse<TValue>> result;
        Entry entry;
        TaskCompletionSource<CachedResponse<TValue>>? start = null;

        lock (gate)
        {
            if (!entries.TryGetValue(key, out entry!))
            {
                entry = new Entry();
                entries.Add(key, entry);
            }

            entry.LastUse = ++clock;

            if (!forceRefresh && entry.Value is not null)
            {
                return Task.FromResult(entry.Value);
            }

            if (entry.Pending is not null && (!forceRefresh || entry.IsRefresh))
            {
                result = entry.Pending.Task;
            }
            else
            {
                // An explicit refresh supersedes an older initial load.
                RejectPending(entry);
                start = new(TaskCreationOptions.RunContinuationsAsynchronously);
                entry.Pending = start;
                entry.IsRefresh = forceRefresh;
                result = start.Task;
                ObserveFailure(result);
            }

            Trim();
        }

        if (start is not null)
        {
            _ = PopulateAsync(key, entry, start, loader, sessionToken);
        }

        return result.WaitAsync(callerToken);
    }

    private async Task PopulateAsync(
        TKey key, Entry entry, TaskCompletionSource<CachedResponse<TValue>> pending,
        Func<CancellationToken, Task<TValue>> loader, CancellationToken sessionToken)
    {
        try
        {
            var value = await loader(sessionToken);
            sessionToken.ThrowIfCancellationRequested();
            var response = new CachedResponse<TValue>(value, DateTimeOffset.UtcNow);

            lock (gate)
            {
                if (!entries.TryGetValue(key, out var current)
                    || !ReferenceEquals(current, entry)
                    || !ReferenceEquals(entry.Pending, pending))
                {
                    pending.TrySetException(new CacheInvalidatedException());
                    return;
                }

                entry.Value = response;
                entry.Pending = null;
                pending.TrySetResult(response);
            }
        }
        catch (OperationCanceledException) when (sessionToken.IsCancellationRequested)
        {
            FinishFailure(key, entry, pending);
            pending.TrySetCanceled(sessionToken);
        }
        catch (Exception error)
        {
            FinishFailure(key, entry, pending);
            pending.TrySetException(error);
        }
    }

    private void FinishFailure(TKey key, Entry entry,
        TaskCompletionSource<CachedResponse<TValue>> pending)
    {
        lock (gate)
        {
            if (entries.TryGetValue(key, out var current)
                && ReferenceEquals(current, entry) && ReferenceEquals(entry.Pending, pending))
            {
                entry.Pending = null;
                if (entry.Value is null)
                {
                    entries.Remove(key);
                }
            }
        }
    }

    public void Put(TKey key, TValue value)
    {
        lock (gate)
        {
            if (entries.TryGetValue(key, out var previous))
            {
                RejectPending(previous);
            }
            entries[key] = new Entry
            {
                Value = new CachedResponse<TValue>(value, DateTimeOffset.UtcNow),
                LastUse = ++clock
            };
            Trim();
        }
    }

    public CachedResponse<TValue>? Peek(TKey key)
    {
        lock (gate) { return entries.GetValueOrDefault(key)?.Value; }
    }

    public void RemoveWhere(Func<TKey, bool> predicate)
    {
        lock (gate)
        {
            foreach (var key in entries.Keys.Where(predicate).ToArray())
            {
                RejectPending(entries[key]);
                entries.Remove(key);
            }
        }
    }

    public void Clear() => RemoveWhere(_ => true);

    private void Trim()
    {
        while (entries.Count > capacity)
        {
            var oldest = entries.MinBy(pair => pair.Value.LastUse);
            RejectPending(oldest.Value);
            entries.Remove(oldest.Key);
        }
    }

    private static void RejectPending(Entry entry) =>
        entry.Pending?.TrySetException(new CacheInvalidatedException());

    private static void ObserveFailure(Task task) =>
        _ = task.ContinueWith(completed => { _ = completed.Exception; },
            CancellationToken.None, TaskContinuationOptions.OnlyOnFaulted, TaskScheduler.Default);

    private sealed class Entry
    {
        public CachedResponse<TValue>? Value { get; set; }
        public TaskCompletionSource<CachedResponse<TValue>>? Pending { get; set; }
        public bool IsRefresh { get; set; }
        public long LastUse { get; set; }
    }
}
