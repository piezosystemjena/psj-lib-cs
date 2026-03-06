using PsjLib.Base;
using PsjLib.Base.Capabilities;

namespace PsjLib.DDriveFamily.Capabilities;

/// <summary>
/// d-Drive data recorder channel selectors.
/// </summary>
public enum DDriveDataRecorderChannel
{
    /// <summary>
    /// Position recorder channel.
    /// </summary>
    Position = DataRecorder.Channel1Idx,
    /// <summary>
    /// Voltage recorder channel.
    /// </summary>
    Voltage = DataRecorder.Channel2Idx,
}

/// <summary>
/// d-Drive recorder implementation with fixed memory configuration and hex sample decoding.
/// </summary>
/// <remarks>
/// <para><b>Notes:</b> d-Drive records position and actuator voltage channels simultaneously with device-specific hexadecimal sample encoding.</para>
/// <para><b>Notes:</b> Some configuration values are write-only on hardware; readback methods may return sentinel or fixed values.</para>
/// </remarks>
public sealed class DDriveDataRecorder(CapabilityWriteCallback writeCb, IReadOnlyDictionary<string, string> commands, int samplePeriod)
    : DataRecorder(writeCb, commands, samplePeriod)
{
    /// <inheritdoc/>
    /// <remarks>
    /// <para><b>Notes:</b> d-Drive does not expose a memory-length read command; this returns hardware maximum capacity.</para>
    /// </remarks>
    public override Task<int> GetMemoryLengthAsync() => Task.FromResult(500000);
    /// <inheritdoc/>
    /// <remarks>
    /// <para><b>Notes:</b> d-Drive does not expose stride readback; track configured stride externally if needed.</para>
    /// </remarks>
    public override Task<int> GetStrideAsync() => Task.FromResult(0);

    /// <inheritdoc/>
    /// <remarks>
    /// <para><b>Notes:</b> d-Drive channel values are returned as hexadecimal strings and converted into engineering units by this implementation.</para>
    /// </remarks>
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
