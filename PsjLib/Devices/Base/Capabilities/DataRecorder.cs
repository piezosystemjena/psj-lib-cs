namespace PsjLib.Base.Capabilities;

/// <summary>
/// Base capability for device-integrated data recorder acquisition.
/// </summary>
/// <remarks>
/// <para><b>Notes:</b> Recorder capabilities are device-specific (for example supported channels, readback commands, and sample formatting), so derived implementations may override memory, stride, and parsing behavior.</para>
/// </remarks>
public abstract class DataRecorder(CapabilityWriteCallback writeCb, IReadOnlyDictionary<string, string> commands, int samplePeriod)
    : PiezoCapability(writeCb, commands)
{
    /// <summary>
    /// Command token that starts recording.
    /// </summary>
    internal const string CmdStartRecording = "DATA_RECORDER_START";
    /// <summary>
    /// Command token for recorder stride.
    /// </summary>
    internal const string CmdStride = "DATA_RECORDER_STRIDE";
    /// <summary>
    /// Command token for recorder memory length.
    /// </summary>
    internal const string CmdMemoryLength = "DATA_RECORDER_MEMORY_LENGTH";
    /// <summary>
    /// Command token for sample pointer/index.
    /// </summary>
    internal const string CmdPtr = "DATA_RECORDER_PTR";
    /// <summary>
    /// Command token for recorder channel 1 data access.
    /// </summary>
    internal const string CmdGetData1 = "DATA_RECORDER_GET_DATA_1";
    /// <summary>
    /// Command token for recorder channel 2 data access.
    /// </summary>
    internal const string CmdGetData2 = "DATA_RECORDER_GET_DATA_2";

    /// <summary>
    /// Zero-based index representing data recorder channel 1.
    /// </summary>
    public const int Channel1Idx = 0;
    /// <summary>
    /// Zero-based index representing data recorder channel 2.
    /// </summary>
    public const int Channel2Idx = 1;

    /// <summary>
    /// Gets recorder sample period in microseconds.
    /// </summary>
    public int SamplePeriod { get; } = samplePeriod;
    /// <summary>
    /// Gets recorder sample rate in hertz.
    /// </summary>
    public double SampleRate => 1000000.0 / SamplePeriod;

    /// <summary>
    /// Configures recorder memory and stride, then starts acquisition.
    /// </summary>
    /// <param name="memoryLength">Recorder sample capacity to configure.</param>
    /// <param name="stride">Stride between recorded samples.</param>
    /// <remarks>
    /// <para><b>Notes:</b> This operation starts recording immediately after applying memory and stride values.</para>
    /// </remarks>
    public virtual async Task SetAsync(int memoryLength, int stride)
    {
        _ = await WriteAsync(CmdMemoryLength, [memoryLength]).ConfigureAwait(false);
        _ = await WriteAsync(CmdStride, [stride]).ConfigureAwait(false);
        _ = await WriteAsync(CmdStartRecording, [1]).ConfigureAwait(false);
    }

    /// <summary>
    /// Gets configured recorder memory length.
    /// </summary>
    /// <returns>Memory length value.</returns>
    public abstract Task<int> GetMemoryLengthAsync();
    /// <summary>
    /// Gets configured recorder stride.
    /// </summary>
    /// <returns>Stride value.</returns>
    public abstract Task<int> GetStrideAsync();
    /// <summary>
    /// Reads one recorder sample from the given channel.
    /// </summary>
    /// <param name="channel">Channel selector enum.</param>
    /// <param name="index">Optional sample index; if omitted the current pointer may be used.</param>
    /// <returns>Decoded sample value.</returns>
    public abstract Task<double> GetDataAsync(Enum channel, int? index = null);

    /// <summary>
    /// Reads multiple recorder samples sequentially.
    /// </summary>
    /// <param name="channel">Channel selector enum.</param>
    /// <param name="count">Number of samples to read.</param>
    /// <param name="progress">Optional progress callback invoked after each sample.</param>
    /// <returns>Sample list with length <paramref name="count"/>.</returns>
    /// <remarks>
    /// <para><b>Notes:</b> Samples are read one-by-one using <see cref="GetDataAsync(Enum, int?)"/>; this prioritizes portability across devices over maximum bulk-read throughput.</para>
    /// </remarks>
    public virtual async Task<IReadOnlyList<double>> GetAllDataAsync(Enum channel, int count, ProgressCallback? progress = null)
    {
        var result = new List<double>(capacity: count);
        for (var i = 0; i < count; i++)
        {
            result.Add(await GetDataAsync(channel, i).ConfigureAwait(false));
            progress?.Invoke(i + 1, count);
        }

        return result;
    }
}
