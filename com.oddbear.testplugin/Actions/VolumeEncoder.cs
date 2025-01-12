using BarRaider.SdTools;
using BarRaider.SdTools.Payloads;
using Newtonsoft.Json;
using System.ComponentModel;
using System.Runtime.Serialization;
using Newtonsoft.Json.Converters;
using System.Globalization;
using System.Drawing.Drawing2D;
using Newtonsoft.Json.Linq;

namespace com.oddbear.testplugin.Actions;

[PluginActionId("com.oddbear.testplugin.volume-encoder")]
public class VolumeEncoder : EncoderBase, IKeypadPlugin
{
    protected class PluginSettings
    {
        public static PluginSettings CreateDefaultSettings()
        {
            return new PluginSettings
            {
                //
            };
        }

        [JsonConverter(typeof(StringEnumConverter))]
        [JsonProperty(PropertyName = "output")]
        public DeviceOut Output { get; set; }

        [JsonConverter(typeof(StringEnumConverter))]
        [JsonProperty(PropertyName = "volume-action")]
        public VolumeActionType? ActionVolume { get; set; }

        [JsonProperty(PropertyName = "volume-set")]
        public int? SetVolume { get; set; }

        [JsonProperty(PropertyName = "volume-adjust")]
        public int? AdjustVolume { get; set; }

        [JsonConverter(typeof(StringEnumConverter))]
        [JsonProperty(PropertyName = "blend-action")]
        public VolumeActionType? ActionBlend { get; set; }

        [JsonProperty(PropertyName = "blend-set")]
        public float? SetBlend { get; set; }

        [JsonProperty(PropertyName = "blend-adjust")]
        public float? AdjustBlend { get; set; }
    }

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

    private bool _isEncoder;
    private PluginSettings _settings;

    private static Dictionary<(DeviceOut, VolumeActionType), float> _mock = new ()
    {
        [(DeviceOut.MainOut, VolumeActionType.Set)] = -20f,
        [(DeviceOut.MainOut, VolumeActionType.Adjust)] = -10f,
        [(DeviceOut.Phones, VolumeActionType.Set)] = -30f,
        [(DeviceOut.Phones, VolumeActionType.Adjust)] = +10f,
        [(DeviceOut.Blend, VolumeActionType.Set)] = 0.5f,
        [(DeviceOut.Blend, VolumeActionType.Adjust)] = -0.2f,
    };

    public VolumeEncoder(ISDConnection connection, InitialPayload payload)
        : base(connection, payload)
    {
        _isEncoder = payload.Controller == "Encoder";

        if (payload.Settings == null || payload.Settings.Count == 0)
        {
            _settings = PluginSettings.CreateDefaultSettings();
        }
        else
        {
            _settings = payload.Settings.ToObject<PluginSettings>()!;
        }
    }
    
    public void KeyPressed(KeyPayload payload)
    {
        //_settings
    }

    public override void DialDown(DialPayload payload)
    {
        //_settings
    }

    public override void DialRotate(DialRotatePayload payload)
    {
        var ticks = payload.Ticks;

        if (_settings.Output == DeviceOut.Blend)
        {
            if (_settings.AdjustBlend is not int steps)
                return;

            var oldBlend = _mock[(_settings.Output, VolumeActionType.Adjust)];

            var newBlend = oldBlend + steps * ticks;
            if (newBlend is < -1 or > +1)
                return;

            _mock[(_settings.Output, VolumeActionType.Adjust)] = newBlend;
        }
        else
        {
            if (_settings.AdjustVolume is not int steps)
                return;

            var oldVolume = _mock[(_settings.Output, VolumeActionType.Adjust)];
            var newVolume = oldVolume + steps * ticks;
            if (newVolume is < -96 or > 0)
                return;

            _mock[(_settings.Output, VolumeActionType.Adjust)] = newVolume;
        }

        RefreshState();
    }

    public override void ReceivedSettings(ReceivedSettingsPayload payload)
    {
        // TODO: Might need to set on each update:
        Tools.AutoPopulateSettings(_settings, payload.Settings);
        //Connection.SetSettingsAsync(JObject.FromObject(_settings));
    }

    private void RefreshState()
    {
        VolumeActionType GetAction()
        {
            if (_isEncoder)
                return VolumeActionType.Adjust;

            return _settings.Output == DeviceOut.Blend
                ? _settings.ActionBlend
                : _settings.ActionVolume;
        }

        var output = _settings.Output;
        var action = GetAction();

        //var action = _isEncoder
        //    ? VolumeActionType.Adjust
        //    : output == DeviceOut.Blend
        //        ? _settings.AdjustBlend
        //        : _settings.AdjustVolume;

        if (_settings.Output == DeviceOut.Blend)
        {
            float OutputBlendToPercentage(float valueBlend)
                => (valueBlend + 1) * 50;

            var blend = _mock[(output, action)];
            var percentage = OutputBlendToPercentage(blend);

            Connection.SetFeedbackAsync(new Dictionary<string, string>
            {
                ["value"] = $"{blend:0.00}",
                ["indicator"] = percentage.ToString(CultureInfo.InvariantCulture)
            });
        }
        else
        {
            // -96 dB to 0 dB
            float OutputDbToPercentage(float valueDb)
                => (valueDb + 96f) / 96f * 100;

            var volume = _mock[(output, action)];
            var percentage = OutputDbToPercentage(volume);

            Connection.SetFeedbackAsync(new Dictionary<string, string>
            {
                ["value"] = $"{volume:0.0} dB",
                ["indicator"] = percentage.ToString(CultureInfo.InvariantCulture)
            });
        }
    }

    #region NotUsed

    public override void Dispose()
    {
        //
    }

    public override void DialUp(DialPayload payload)
    {
        //
    }

    public override void TouchPress(TouchpadPressPayload payload)
    {
        //
    }

    public override void ReceivedGlobalSettings(ReceivedGlobalSettingsPayload payload)
    {
        //
    }

    public void KeyReleased(KeyPayload payload)
    {
        //
    }

    public override void OnTick()
    {
        //
    }

    #endregion
}
