using PsjLib.Base;
using PsjLib.Base.Capabilities;

namespace PsjLib.DDriveFamily.Capabilities;

public enum DDriveDataRecorderChannel
{
    Position = DataRecorder.Channel1Idx,
    Voltage = DataRecorder.Channel2Idx,
}

public sealed class DDriveDataRecorder(CapabilityWriteCallback writeCb, IReadOnlyDictionary<string, string> commands, int samplePeriod)
    : DataRecorder(writeCb, commands, samplePeriod)
{
    public override Task<int> GetMemoryLengthAsync() => Task.FromResult(500000);
    public override Task<int> GetStrideAsync() => Task.FromResult(0);

    public override async Task<double> GetDataAsync(Enum channel, int? index = null)
    {
        if (index is not null)
        {
            _ = await WriteAsync(CmdPtr, [index.Value]).ConfigureAwait(false);
        }

        var channelIdx = Convert.ToInt32(channel);
        var cmd = channelIdx == Channel1Idx ? CmdGetData1 : CmdGetData2;
        var result = await WriteAsync(cmd, [0, 1]).ConfigureAwait(false);

        if (channelIdx == (int)DDriveDataRecorderChannel.Position)
        {
            return ParsePosValue(result[0]);
        }

        return ParseVoltageValue(result[0]);
    }

    private static double ParsePosValue(string raw)
    {
        var value = Convert.ToInt32(raw, 16);
        return (160.0 / 65535.0) * value - 30.0;
    }

    private static double ParseVoltageValue(string raw)
    {
        var value = Convert.ToInt32(raw, 16);
        return (165.0 / 65535.0) * value - 27.5;
    }
}
