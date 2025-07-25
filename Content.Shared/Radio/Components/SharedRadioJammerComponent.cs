using Robust.Shared.Serialization;
using Robust.Shared.GameStates;

namespace Content.Shared.RadioJammer;

/// <summary>
/// When activated (<see cref="ActiveRadioJammerComponent"/>) prevents from sending messages in range
/// Suit sensors will also stop working.
/// </summary>
[NetworkedComponent, RegisterComponent]
public sealed partial class RadioJammerComponent : Component
{
    [DataDefinition]
    public partial struct RadioJamSetting
    {
        /// <summary>
        /// Power usage per second when enabled.
        /// </summary>
        [DataField(required: true)]
        public float Wattage;

        /// <summary>
        /// Range of the jammer.
        /// </summary>
        [DataField(required: true)]
        public float Range;

        /// <summary>
        /// The message that is displayed when switched 
        /// to this setting.
        /// </summary>
        [DataField(required: true)]
        public LocId Message;

        /// <summary>
        /// Name of the setting.
        /// </summary>
        [DataField(required: true)]
        public LocId Name;
    }

    /// <summary>
    /// List of all the settings for the radio jammer.
    /// </summary>
    [DataField(required: true), ViewVariables(VVAccess.ReadOnly)]
    public RadioJamSetting[] Settings = default!;

    /// <summary>
    /// Index of the currently selected setting.
    /// </summary>
    [DataField]
    public int SelectedPowerLevel = 1;
}

[Serializable, NetSerializable]
public enum RadioJammerChargeLevel : byte
{
    Low,
    Medium,
    High
}

[Serializable, NetSerializable]
public enum RadioJammerLayers : byte
{
    LED
}

[Serializable, NetSerializable]
public enum RadioJammerVisuals : byte
{
    ChargeLevel,
    LEDOn
}
