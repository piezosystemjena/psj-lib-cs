namespace PsjLib.Base.Capabilities;

public abstract class DataRecorder(CapabilityWriteCallback writeCb, IReadOnlyDictionary<string, string> commands, int samplePeriod)
    : PiezoCapability(writeCb, commands)
{
    public const string CmdStartRecording = "DATA_RECORDER_START";
    public const string CmdStride = "DATA_RECORDER_STRIDE";
    public const string CmdMemoryLength = "DATA_RECORDER_MEMORY_LENGTH";
    public const string CmdPtr = "DATA_RECORDER_PTR";
    public const string CmdGetData1 = "DATA_RECORDER_GET_DATA_1";
    public const string CmdGetData2 = "DATA_RECORDER_GET_DATA_2";

    public const int Channel1Idx = 0;
    public const int Channel2Idx = 1;

    public int SamplePeriod { get; } = samplePeriod;
    public double SampleRate => 1000000.0 / SamplePeriod;

    public virtual async Task SetAsync(int memoryLength, int stride)
    {
        _ = await WriteAsync(CmdMemoryLength, [memoryLength]).ConfigureAwait(false);
        _ = await WriteAsync(CmdStride, [stride]).ConfigureAwait(false);
        _ = await WriteAsync(CmdStartRecording, [1]).ConfigureAwait(false);
    }

    public abstract Task<int> GetMemoryLengthAsync();
    public abstract Task<int> GetStrideAsync();
    public abstract Task<double> GetDataAsync(Enum channel, int? index = null);

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
