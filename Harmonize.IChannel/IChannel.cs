using System.Runtime.CompilerServices;

namespace Harmonize.Channel;

public interface IChannel<T> : IDisposable
{

    /// <summary>
    /// Gets whether <see cref="Count"/> is available for use on implementing instance.
    /// </summary>
    bool CanCount();

    /// <summary>
    /// Gets whether <see cref="TryPeek"/> is available for use on implementing instance.
    /// </summary>
    bool CanPeek();

    /// <summary>
    /// If reading channel item needs ack to be removed from channel this will return true
    /// </summary>
    /// <returns>true if ack is needed to remove item from channel else false</returns>
    bool NeedAck();

    /// <summary>
    /// Ack a message from the channel and under lying queue.
    /// </summary>
    /// <returns>Ack number</returns>
    bool Ack(ulong ackNum);

    /// <summary>
    /// Gets the current number of items available from this channel reader.
    /// <remarks>
    /// This will return -1 if count is not supported by the channel.
    /// </remarks>
    /// </summary>
    int Count();

    /// <summary>Mark the channel as being complete, meaning no more items will be written to it.</summary>
    /// <param name="error">Optional Exception indicating a failure that's causing the channel to complete.</param>
    /// <exception cref="InvalidOperationException">The channel has already been marked as complete.</exception>
    void Complete(Exception? error = null);

    /// <summary>Attempts to mark the channel as being completed, meaning no more data will be written to it.</summary>
    /// <param name="error">An <see cref="Exception"/> indicating the failure causing no more data to be written, or null for success.</param>
    /// <returns>
    /// <TODO>Change exception type to some internal like ChannelCompleteException</TODO>
    /// true if this operation successfully completes the channel; otherwise, false if the channel could not be marked for completion,
    /// for example due to having already been marked as such, or due to not supporting completion.
    /// </returns>
    bool TryComplete(Exception? error = null);

    /// <summary>Attempts to write the specified item to the channel.</summary>
    /// <param name="item">The item to write to the channel.</param>
    /// <returns>true if the item was written; otherwise, false.</returns>
    bool TryWrite(T item);

    /// <summary>Creates an <see cref="IAsyncEnumerable{T}"/> that enables reading all of the data from the channel.</summary>
    /// <param name="token">Optional <see cref="CancellationToken"/> used to cancel the enumeration.</param>
    /// <remarks>
    /// Each <see cref="IAsyncEnumerator{T}.MoveNextAsync"/> call that returns <c>true</c> will read the next item out of the channel.
    /// <see cref="IAsyncEnumerator{T}.MoveNextAsync"/> will return false once no more data is or will ever be available to read.
    /// </remarks>
    /// <returns>The created async enumerable.</returns>
    IAsyncEnumerable<(T? item, ulong ackNum)> ReadAllAsync(CancellationToken token = default);

    /// <summary>Asynchronously reads an item from the channel.</summary>
    /// <param name="token">A <see cref="CancellationToken"/> used to cancel the read operation.</param>
    /// <returns>A <see cref="ValueTask{TResult}"/> that represents the asynchronous read operation.</returns>
    ValueTask<(T? item, ulong ackNum)> ReadAsync(CancellationToken token = default);

    /// <summary>Asynchronously writes an item to the channel.</summary>
    /// <param name="item">The value to write to the channel.</param>
    /// <param name="token">A <see cref="CancellationToken"/> used to cancel the write operation.</param>
    /// <returns>A <see cref="ValueTask"/> that represents the asynchronous write operation.</returns>
    ValueTask WriteAsync(T item, CancellationToken token);

    /// <summary>Attempts to peek at an item from the channel.</summary>
    /// <param name="item">The peeked item, or a default value if no item could be peeked.</param>
    /// <returns>true if an item was read; otherwise, false.</returns>
    bool TryPeek(out T? item);

    /// <summary>Attempts to read an item from the channel.</summary>
    /// <param name="outPut">The read item, or a default value if no item could be read.</param>
    /// <returns>true if an item was read; otherwise, false.</returns>
    bool TryRead(out (T? item, ulong ackNum) outPut);

    /// <summary>Returns a <see cref="ValueTask{Boolean}"/> that will complete when data is available to read.</summary>
    /// <param name="token">A <see cref="CancellationToken"/> used to cancel the wait operation.</param>
    /// <returns>
    /// A <see cref="ValueTask{Boolean}"/> that will complete with a <c>true</c> result when data is available to read
    /// or with a <c>false</c> result when no further data will ever be available to be read.
    /// </returns>
    ValueTask<bool> WaitToReadAsync(CancellationToken token);

    /// <summary>Returns a <see cref="ValueTask{Boolean}"/> that will complete when space is available to write an item.</summary>
    /// <param name="token">A <see cref="CancellationToken"/> used to cancel the wait operation.</param>
    /// <returns>
    /// A <see cref="ValueTask{Boolean}"/> that will complete with a <c>true</c> result when space is available to write an item
    /// or with a <c>false</c> result when no further writing will be permitted.
    /// </returns>
    ValueTask<bool> WaitToWriteAsync(CancellationToken token);

    /// <summary>
    /// Gets a <see cref="Task"/> that completes when no more data will ever
    /// be available to be read from this channel.
    /// </summary>
    Task Completion();

}

