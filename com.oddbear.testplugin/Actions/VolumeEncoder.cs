using BarRaider.SdTools;
using BarRaider.SdTools.Payloads;
using Newtonsoft.Json;
using System.ComponentModel;
using Newtonsoft.Json.Converters;
using System.Globalization;
using com.oddbear.testplugin.Actions.Enums;
using System.Drawing.Drawing2D;

namespace com.oddbear.testplugin.Actions;

[PluginActionId("com.oddbear.testplugin.volume-encoder")]
public class VolumeEncoder : EncoderBase, IKeypadPlugin
{
    protected class PluginSettings
    {
        [JsonConverter(typeof(StringEnumConverter))]
        [JsonProperty(PropertyName = "output")]
        public DeviceOut Output { get; set; }

        [JsonConverter(typeof(StringEnumConverter))]
        [JsonProperty(PropertyName = "action")]
        public VolumeActionType Action { get; set; }

        // There are 4 states + output and action:
        [JsonProperty(PropertyName = "volume-set")]
        public int SetVolume { get; set; }

        [JsonProperty(PropertyName = "volume-adjust")]
        public int AdjustVolume { get; set; }

        [JsonProperty(PropertyName = "blend-set")]
        public float SetBlend { get; set; }

        [JsonProperty(PropertyName = "blend-adjust")]
        public float AdjustBlend { get; set; }
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
            _settings = new PluginSettings();
        }
        else
        {
            _settings = payload.Settings.ToObject<PluginSettings>()!;
            RefreshState();
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
        switch (_settings.Action)
        {
            case VolumeActionType.Adjust:
                Adjust(1);
                break;
            case VolumeActionType.Set:
                Set();
                break;
        }

        // We need to refresh the state with the cached value:
        RefreshState();
    }

    public override void DialRotate(DialRotatePayload payload)
    {
        Adjust(payload.Ticks);

        // We need to refresh the state with the cached value:
        RefreshState();
    }
    
    private void Adjust(int ticks)
    {
        switch (_settings.Output)
        {
            case DeviceOut.Blend:
                _volumeMock.MonitorBlend = AdjustBlendCalc(_volumeMock.MonitorBlend, _settings.AdjustBlend, ticks);
                return;
            case DeviceOut.Phones:
                _volumeMock.HeadphonesVolume = AdjustVolumeCalc(_volumeMock.HeadphonesVolume, _settings.AdjustVolume, ticks);
                return;
            case DeviceOut.MainOut:
                _volumeMock.MainOutVolume = AdjustVolumeCalc(_volumeMock.MainOutVolume, _settings.AdjustVolume, ticks);
                return;
        }
    }

    private void Set()
    {
        switch (_settings.Output)
        {
            case DeviceOut.Blend:
                _volumeMock.MonitorBlend = SetBlendCalc(_settings.SetBlend);
                break;
            case DeviceOut.Phones:
                _volumeMock.HeadphonesVolume = SetVolumeCalc(_settings.SetVolume);
                break;
            case DeviceOut.MainOut:
                _volumeMock.MainOutVolume = SetVolumeCalc(_settings.SetVolume);
                break;
        }
    }

    private static int SetVolumeCalc(int newVolumeDb)
    {
        if (newVolumeDb < -96)
            newVolumeDb = -96;

        if (newVolumeDb > 0)
            newVolumeDb = 0;

        var newVolumeRaw = LookupTable.OutputDbToPercentage(newVolumeDb);
        // TODO: This creates some rounding errors ex. around - 60 dB with only one tick:
        // TODO: Is rounding errors fixed?
        // TODO: Still rounding errors around -25.4 dB, think I need to have no integer rounding...
        return (int)Math.Round(newVolumeRaw * 100);
    }

    private int AdjustVolumeCalc(int oldVolume, int value, int ticks)
    {
        var adjustment = value * ticks;

        var oldVolumeRaw = oldVolume / 100f;
        var oldVolumeDb = LookupTable.OutputPercentageToDb(oldVolumeRaw);
        var newVolumeDb = (int)Math.Round(oldVolumeDb + adjustment);

        return SetVolumeCalc(newVolumeDb);
    }

    private static float SetBlendCalc(float newBlend)
    {
        if (newBlend < -1)
            newBlend = -1;

        if (newBlend > 1)
            newBlend = 1;

        return newBlend;
    }
    private static float AdjustBlendCalc(float oldBlend, float value, int ticks)
    {
        var adjustment = value * ticks;

        var newBlend = oldBlend + adjustment;
        return SetBlendCalc(newBlend);
    }

    public override void ReceivedSettings(ReceivedSettingsPayload payload)
    {
        // TODO: Might need to set on each update:
        Tools.AutoPopulateSettings(_settings, payload.Settings);
        //Connection.SetSettingsAsync(JObject.FromObject(_settings));
        RefreshState();
    }

    private void RefreshState()
    {
        switch (_settings.Output)
        {
            case DeviceOut.Blend:
                RefreshBlend();
                return;
            case DeviceOut.Phones:
                RefreshVolume(_volumeMock.HeadphonesVolume);
                return;
            case DeviceOut.MainOut:
                RefreshVolume(_volumeMock.MainOutVolume);
                return;
        }
    }

    private void RefreshBlend()
    {
        var blend = _volumeMock.MonitorBlend;

        if (_isEncoder)
        {
            float OutputBlendToPercentage(float valueBlend)
                => (valueBlend + 1) * 50;

            var percentage = OutputBlendToPercentage(blend);

            Connection.SetFeedbackAsync(new Dictionary<string, string>
            {
                ["value"] = $"{blend:0.00}",
                ["indicator"] = percentage.ToString(CultureInfo.InvariantCulture)
            });
        }
        else
        {
            Connection.SetTitleAsync($"{blend:0.00}");
        }
    }

    private void RefreshVolume(int percentage)
    {
        var volume = LookupTable.OutputPercentageToDb(percentage / 100f);
        if (_isEncoder)
        {
            // -96 dB to 0 dB
            //float OutputDbToPercentage(float valueDb)
            //    => (valueDb + 96f) / 96f * 100;

            Connection.SetFeedbackAsync(new Dictionary<string, string>
            {
                ["value"] = $"{volume:0.0} dB",
                ["indicator"] = percentage.ToString(CultureInfo.InvariantCulture)
            });
        }
        else
        {
            Connection.SetTitleAsync($"{volume:0.0} dB");
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
        //Connection.SetSettingsAsync(JObject.FromObject(_settings));
    }

    #endregion
}
