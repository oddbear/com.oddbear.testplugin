using System.ComponentModel;
using System.Runtime.Serialization;

namespace com.oddbear.testplugin.Actions.Enums;

public enum DeviceOut
{
    [Description("Main Out")]
    MainOut,
    [Description("Phones")]
    Phones,
    [Description("Blend")]
    Blend
}

public enum VolumeActionType
{
    [EnumMember(Value = "set")]
    Set,
    [EnumMember(Value = "adjust")]
    Adjust,
    [EnumMember(Value = "mute")]
    Mute
}
