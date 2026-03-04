using PsjLib.Base.Capabilities;

namespace PsjLib.NVFamily.NV120CLE;

/// <summary>
/// Single-channel closed-loop NV120CLE channel.
/// </summary>
public sealed class NV120CLEChannel : NVFamilyChannel
{
    /// <summary>
    /// Initializes a new NV120CLE channel and closed-loop capabilities.
    /// </summary>
    /// <param name="id">Channel identifier.</param>
    /// <param name="writeCallback">Channel write callback.</param>
    public NV120CLEChannel(int id, PsjLib.Base.ChannelWriteCallback writeCallback)
        : base(id, writeCallback)
    {
        ClosedLoopController = new ClosedLoopController(CapabilityWriteAsync, new Dictionary<string, string>
        {
            [ClosedLoopController.CmdEnable] = "cloop",
        }, 0);

        ClosedloopUnit = new Unit(CapabilityWriteAsync, new Dictionary<string, string>
        {
            [Unit.CmdUnit] = "unitcl",
        });

        ClosedloopLimits = new Limits(CapabilityWriteAsync, new Dictionary<string, string>
        {
            [Limits.CmdLowerLimit] = "dspclmin",
            [Limits.CmdUpperLimit] = "dspclmax",
        });
    }

    /// <summary>Closed-loop controller capability.</summary>
    public ClosedLoopController ClosedLoopController { get; }
    /// <summary>Closed-loop unit query capability.</summary>
    public Unit ClosedloopUnit { get; }
    /// <summary>Closed-loop lower/upper limit capability.</summary>
    public Limits ClosedloopLimits { get; }
}
