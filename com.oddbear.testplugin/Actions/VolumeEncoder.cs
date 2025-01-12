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

    private static VolumeMock _volumeMock = new();

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

        _volumeMock.PropertyChanged += VolumeMockOnPropertyChanged;
    }

    public override void Dispose()
    {
        _volumeMock.PropertyChanged -= VolumeMockOnPropertyChanged;
    }

    private void VolumeMockOnPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        // We also refresh the state when we get back the real value:
        RefreshState();
    }

    public void KeyPressed(KeyPayload payload)
    {
        if (_settings.Output == DeviceOut.Blend)
        {
            if (_settings.ActionBlend == VolumeActionType.Set)
            {
                if (_settings.SetBlend is not float blend)
                    return;

                if (blend < -1)
                    blend = -1;

                if (blend > 1)
                    blend = 1;

                SetValue(blend);
            }
            else
            {
                var blend = GetValue();
                if (_settings.AdjustBlend is not float adjustmentBlend)
                    return;

                blend += adjustmentBlend;
                if (blend < -1)
                    blend = -1;

                if (blend > 1)
                    blend = 1;

                SetValue(blend);
            }
        }
        else
        {
            if (_settings.ActionVolume == VolumeActionType.Set)
            {
                // TODO: Before we actually set a value on the slider, this one is null.
                // TODO: It would be nice if we had a default, but the default is different based on the output and controller type.
                if (_settings.SetVolume is not int volumeDb)
                    return;

                if (volumeDb < -96)
                    volumeDb = -96;

                if (volumeDb > 0)
                    volumeDb = 0;

                var volume = LookupTable.OutputDbToPercentage(volumeDb) * 100f;
                SetValue(volume);
            }
            else
            {
                if (_settings.AdjustVolume is not int adjustmentDb)
                    return;

                var oldVolume = GetValue() / 100f;
                var oldVolumeDb = LookupTable.OutputPercentageToDb(oldVolume);

                var newVolumeDb = oldVolumeDb + adjustmentDb;
                if (newVolumeDb < -96)
                    newVolumeDb = -96;

                if (newVolumeDb > 0)
                    newVolumeDb = 0;

                var newVolume = LookupTable.OutputDbToPercentage(newVolumeDb) * 100f;
                SetValue(newVolume);
            }
        }

        // We need to refresh the state with the cached value:
        RefreshState();
    }

    public override void DialRotate(DialRotatePayload payload)
    {
        var ticks = payload.Ticks;

        if (_settings.Output == DeviceOut.Blend)
        {
            if (_settings.AdjustBlend is not float steps)
                return;

            var oldBlend = GetValue() * 100;

            var newBlend = oldBlend + steps * ticks;
            if (newBlend < -1)
                newBlend = -1;

            if (newBlend > 1)
                newBlend = 1;

            SetValue(newBlend / 100);
        }
        else
        {
            if (_settings.AdjustVolume is not int steps)
                return;

            var oldVolume = GetValue() / 100f;
            var oldValueDb = LookupTable.OutputPercentageToDb(oldVolume);
            var newVolumeDb = oldValueDb + steps * ticks;

            if (newVolumeDb < -96)
                newVolumeDb = -96;

            if (newVolumeDb > 0)
                newVolumeDb = 0;

            var newVolume = LookupTable.OutputDbToPercentage(newVolumeDb) * 100;
            SetValue(newVolume);
        }
    }

    public override void ReceivedSettings(ReceivedSettingsPayload payload)
    {
        // TODO: Might need to set on each update:
        Tools.AutoPopulateSettings(_settings, payload.Settings);
        //Connection.SetSettingsAsync(JObject.FromObject(_settings));
    }

    private void RefreshState()
    {
        VolumeActionType? GetAction()
        {
            if (_isEncoder)
                return VolumeActionType.Adjust;

            return _settings.Output == DeviceOut.Blend
                ? _settings.ActionBlend
                : _settings.ActionVolume;
        }

        var output = _settings.Output;
        if (GetAction() is not VolumeActionType action)
            return;

        if (_settings.Output == DeviceOut.Blend)
        {
            float OutputBlendToPercentage(float valueBlend)
                => (valueBlend + 1) * 50;

            var blend = GetValue();
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
            //float OutputDbToPercentage(float valueDb)
            //    => (valueDb + 96f) / 96f * 100;

            var percentage = GetValue();
            var volume = LookupTable.OutputPercentageToDb(percentage / 100f);

            Connection.SetFeedbackAsync(new Dictionary<string, string>
            {
                ["value"] = $"{volume:0.0} dB",
                ["indicator"] = percentage.ToString(CultureInfo.InvariantCulture)
            });
        }
    }

    void SetValue(float value)
    {
        switch (_settings.Output)
        {
            case DeviceOut.Blend:
                _volumeMock.MonitorBlend = value;
                break;
            case DeviceOut.Phones:
                _volumeMock.HeadphonesVolume = value;
                break;
            case DeviceOut.MainOut:
                _volumeMock.MainOutVolume = value;
                break;
        }
    }

    float GetValue()
    {
        switch (_settings.Output)
        {
            case DeviceOut.Blend:
                return _volumeMock.MonitorBlend;
            case DeviceOut.Phones:
                return _volumeMock.HeadphonesVolume;
            case DeviceOut.MainOut:
            default:
                return _volumeMock.MainOutVolume;
        }
    }

    #region NotUsed

    public override void DialDown(DialPayload payload)
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
