using System.Runtime.CompilerServices;
using Harmonize.Channel;
using System.Threading.Channels;

namespace Harmonize.UnboundMemoryChannel;

/// <summary>
/// TODO: Make this the default channel if non is specified in configuration.
/// TODO: Channel interface version check if interface update have breaking changes ?
/// Channel that is unbound to memory size and uses memory as storage.
/// Used by any number of readers and writers concurrently.
/// </summary>
/// <typeparam name="T"></typeparam>
public class UnboundMemoryChannel<T> : IChannel<T>
{
    private readonly Channel<T> _channel;
    private bool _isComplete;
    public Exception? CompleteError { get; private set; }

    /// <summary>
    /// Create a new instance of <see cref="UnboundMemoryChannel{T}"/>
    /// </summary>
    public UnboundMemoryChannel()
    {
        _channel = System.Threading.Channels.Channel.CreateUnbounded<T>();
        _isComplete = false;
    }

    /// <summary>
    /// Dispose this instance
    /// </summary>
    public void Dispose()
    {
        //TODO: When dispose is called should we complete the channel ?
    }

    /// <summary>
    /// Can this channel count number of items in the channel
    /// </summary>
    /// <returns>true if it can else false</returns>
    public bool CanCount()
    {
       return _channel.Reader.CanCount;
    }

    /// <summary>
    /// Can this channel peek at the next item in the channel
    /// </summary>
    /// <returns>True if it can else false</returns>
    public bool CanPeek()
    {
        return _channel.Reader.CanPeek;
    }

    /// <summary>
    /// Do this channel need ack to remove item from channel
    /// </summary>
    /// <returns>True if ack is needed else false</returns>
    public bool NeedAck()
    {
        return false;
    }

    /// <summary>
    /// Ack is not needed for this channel so this will always return true
    /// </summary>
    /// <param name="ackNum"></param>
    /// <returns>Always true ack not needed</returns>
    public bool Ack(ulong ackNum)
    {
        return true;
    }

    /// <summary>
    /// Get number of items in channel.
    /// </summary>
    /// <returns>int number of items</returns>
    public int Count()
    {
        return _channel.Reader.Count;
    }

    /// <summary>
    /// Mark the channel as being complete, meaning no more items will be written to it.
    /// </summary>
    /// <param name="error">Optional Exception indicating a failure that's causing the channel to complete.</param>
    /// <exception cref="InvalidOperationException">The channel has already been marked as complete.</exception>
    public void Complete(Exception? error = null)
    {
        CompleteError = error;
        if (_isComplete)
        {
            throw new InvalidOperationException("Channel is already complete");
        }
        _isComplete = true;
        _channel.Writer.Complete(error);
    }

    /// <summary>
    /// Attempts to mark the channel as being completed, meaning no more data will be written to it.
    /// </summary>
    /// <param name="error"> Indicating the failure causing no more data to be written.</param>
    /// <returns>
    /// true if this operation successfully completes the channel; otherwise, false if the channel could not be marked for completion,
    /// for example due to having already been marked as such, or due to not supporting completion.</returns>
    public bool TryComplete(Exception? error = null)
    {
        var result = _channel.Writer.TryComplete(error);

        if (result)
        {
            _isComplete = true;
            CompleteError = error;
        }
        
        return result;
    }

    /// <summary>
    /// Attempts to write the specified item to the channel.
    /// </summary>
    /// <param name="item">The item to write.</param>
    /// <returns>true if the item was written; otherwise, false if it wasn't written.</returns>
    public bool TryWrite(T item)
    {
        return _channel.Writer.TryWrite(item);
    }

    /// <summary>
    /// Creates an <see cref="IAsyncEnumerable{T}"/> that enables reading all of the data from the channel.
    /// </summary>
    /// <param name="token">Optional <see cref="CancellationToken"/> to cancel the operation</param>
    /// <returns>The created async <see cref="IAsyncEnumerable{T}"/> </returns>
    public async IAsyncEnumerable<(T? item, ulong ackNum)> ReadAllAsync([EnumeratorCancellation]CancellationToken token = default)
    {
        while (await WaitToReadAsync(token).ConfigureAwait(false))
        {
            while (TryRead(out var outPut))
            {
                yield return (outPut.item, outPut.ackNum);
            }
        }
    }

    /// <summary>
    /// Asynchronously reads an item from the channel. 
    /// </summary>
    /// <param name="token">Optional <see cref="CancellationToken"/> to cancel the operation.</param>
    /// <returns><see cref="ValueTask"/> with a <see cref="Tuple"/> (item T and a ulong always 0) given that no ack is needed</returns>
    public ValueTask<(T? item, ulong ackNum)> ReadAsync(CancellationToken token = default)
    {
        return new ValueTask<(T?, ulong)>((_channel.Reader.ReadAsync(token).Result, default));
    }

    /// <summary>
    /// Asynchronously writes an item to the channel.
    /// </summary>
    /// <param name="item">item to write to the channel.</param>
    /// <param name="token">Optional <see cref="CancellationToken"/>to cancel the operation.</param>
    /// <returns>>A <see cref="ValueTask"/> that represents the asynchronous write operation.</returns>
    public async ValueTask WriteAsync(T item, CancellationToken token = default)
    {
        await _channel.Writer.WriteAsync(item, token);
    }

    /// <summary>
    /// Attempts to peek at an item from the channel
    /// </summary>
    /// <param name="item">The peeked item, or a default value if no item could be peeked.</param>
    /// <returns>true if an item was read; otherwise, false if no item was read.</returns>
    public bool TryPeek(out T? item)
    {
        return _channel.Reader.TryPeek(out item);
    }

    /// <summary>
    /// Attempts to read an item from the channel.
    /// This don´t need ack so the ackNum will always be 0
    /// </summary>
    /// <param name="outPut">The read item, or a default value if no item could be read.</param>
    /// <returns>true if an item was read; otherwise, false if no item was read.</returns>
    public bool TryRead(out (T? item, ulong ackNum) outPut)
    {
        outPut.ackNum = default;
        return _channel.Reader.TryRead(out outPut.item);
    }

    /// <summary>
    /// Returns a <see cref="ValueTask{Boolean}"/> that will complete when data is available to read.
    /// </summary>
    /// <param name="token">A <see cref="CancellationToken"/> used to cancel the wait operation.</param>
    /// <returns>
    /// A <see cref="ValueTask{Boolean}"/> that will complete with a <c>true</c> result when data is available to read
    /// or with a <c>false</c> result when no further data will ever be available to be read.
    /// </returns>
    public async ValueTask<bool> WaitToReadAsync(CancellationToken token)
    {
        return await _channel.Reader.WaitToReadAsync(token);
    }

    /// <summary>Returns a<see cref="ValueTask{Boolean}"/> that will complete when space is available to write an item.</summary>
    /// <param name="token">A <see cref="CancellationToken"/> used to cancel the wait operation.</param>
    /// <returns>
    /// A <see cref="ValueTask{Boolean}"/> that will complete with a <c>true</c> result when space is available to write an item
    /// or with a <c>false</c> result when no further writing will be permitted.
    /// </returns>
    public async ValueTask<bool> WaitToWriteAsync(CancellationToken token)
    {
        return await _channel.Writer.WaitToWriteAsync(token);
    }

    /// <summary>
    /// Gets a <see cref="Task"/> that completes when no more data will ever
    /// be available to be read from this channel.
    /// </summary>
    public Task Completion()
    {
        return _channel.Reader.Completion;
    }
}
