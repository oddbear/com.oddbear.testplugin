using BarRaider.SdTools;
using BarRaider.SdTools.Payloads;
using Newtonsoft.Json;
using System.ComponentModel;
using Newtonsoft.Json.Converters;
using com.oddbear.testplugin.Actions.Enums;
using Newtonsoft.Json.Linq;

namespace com.oddbear.testplugin.Actions;

// To try to just mimic the feel of the interface dial as much as possible:
[PluginActionId("com.oddbear.testplugin.volume-encoder-uc")]
public class VolumeEncoderUc : EncoderBase, IKeypadPlugin
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

    private static VolumeCache _volumeCache = new();

    public VolumeEncoderUc(ISDConnection connection, InitialPayload payload)
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

        _volumeCache.PropertyChanged += VolumeCacheOnPropertyChanged;
    }

    public override void Dispose()
    {
        _volumeCache.PropertyChanged -= VolumeCacheOnPropertyChanged;
    }

    private void VolumeCacheOnPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        // Important use this event instead of calling RefreshState directly when changing state.
        // If not the state will be updated locally and not globally.
        RefreshState();
    }

    public void KeyPressed(KeyPayload payload)
    {
        switch (_settings.Action)
        {
            case VolumeActionType.Set:
                KeypadSet();
                break;
            case VolumeActionType.Adjust:
                KeypadAdjust(1);
                break;
        }
    }

    private void KeypadSet()
    {
        switch (_settings.Output)
        {
            case DeviceOut.Blend:
                _volumeCache.MonitorBlend = SetBlendCalc(_settings.SetBlend);
                break;
            case DeviceOut.Phones:
                _volumeCache.HeadphonesVolume = SetVolumeCalc(_settings.SetVolume);
                break;
            case DeviceOut.MainOut:
                _volumeCache.MainOutVolume = SetVolumeCalc(_settings.SetVolume);
                break;
        }
    }

    private void KeypadAdjust(int ticks)
    {
        switch (_settings.Output)
        {
            case DeviceOut.Blend:
                // I get about 30 ticks for the whole dial on the interface from -1 to +1
                _volumeCache.MonitorBlend = AdjustBlendCalc(_volumeCache.MonitorBlend, _settings.AdjustBlend, ticks);
                return;
            case DeviceOut.Phones:
                _volumeCache.HeadphonesVolume = AdjustVolumeDbCalc(_volumeCache.HeadphonesVolume, _settings.AdjustVolume, ticks);
                return;
            case DeviceOut.MainOut:
                // 0 -> -0.06 -> -0.12
                // -10 -> -9.52 -> -9.07
                // -96 -> -91.9 -> -87.97 -> -84.21
                _volumeCache.MainOutVolume = AdjustVolumeDbCalc(_volumeCache.MainOutVolume, _settings.AdjustVolume, ticks);
                return;
        }
    }

    public override void DialRotate(DialRotatePayload payload)
    {
        // We use fixed values, as these are the same as the ones in UC.
        switch (_settings.Output)
        {
            case DeviceOut.Blend:
                _volumeCache.MonitorBlend = AdjustBlendCalc(_volumeCache.MonitorBlend, 0.02f, payload.Ticks);
                return;
            case DeviceOut.Phones:
                _volumeCache.HeadphonesVolume = AdjustVolumePercentageCalc(_volumeCache.HeadphonesVolume, 0.01f, payload.Ticks);
                return;
            case DeviceOut.MainOut:
                _volumeCache.MainOutVolume = AdjustVolumePercentageCalc(_volumeCache.MainOutVolume, 0.01f, payload.Ticks);
                return;
        }
    }

    private static float SetVolumeCalc(float newVolumeDb)
    {
        if (newVolumeDb < -96)
            newVolumeDb = -96;

        if (newVolumeDb > 0)
            newVolumeDb = 0;

        var newVolumeRaw = LookupTable.OutputDbToPercentage(newVolumeDb);

        return newVolumeRaw;
    }

    private float AdjustVolumeDbCalc(float oldVolumeP, float value, int ticks)
    {
        var oldVolumeDb = LookupTable.OutputPercentageToDb(oldVolumeP);
        var adjustment = value * ticks;

        var newVolumeDb = oldVolumeDb + adjustment;

        if (newVolumeDb < -96)
            newVolumeDb = -96;

        if (newVolumeDb > 0)
            newVolumeDb = 0;

        return LookupTable.OutputDbToPercentage(newVolumeDb);
    }

    private float AdjustVolumePercentageCalc(float oldVolume, float value, int ticks)
    {
        var adjustment = value * ticks;

        var newVolume = oldVolume + adjustment;

        if (newVolume < 0)
            newVolume = 0;

        if (newVolume > 1)
            newVolume = 1;

        return newVolume;
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
        Tools.AutoPopulateSettings(_settings, payload.Settings);
        RefreshState();
    }

    private void RefreshState()
    {
        switch (_settings.Output)
        {
            case DeviceOut.Blend:
                RefreshBlend(_volumeCache.MonitorBlend);
                return;
            case DeviceOut.Phones:
                RefreshVolume(_volumeCache.HeadphonesVolume);
                return;
            case DeviceOut.MainOut:
                RefreshVolume(_volumeCache.MainOutVolume);
                return;
        }
    }

    private void RefreshBlend(float blend)
    {
        if (_isEncoder)
        {
            float OutputBlendToPercentage(float valueBlend)
                => (valueBlend + 1) * 50;

            var percentage = OutputBlendToPercentage(blend);

            Connection.SetFeedbackAsync(JObject.FromObject(new
            {
                value = $"{blend:0.00}",
                indicator = percentage
            }));
        }
        else
        {
            Connection.SetTitleAsync($"{blend:0.00}");
        }
    }

    private void RefreshVolume(float percentage)
    {
        var volumeDb = LookupTable.OutputPercentageToDb(percentage);
        if (_isEncoder)
        {
            var indicatorPercentage = percentage * 100f;

            Connection.SetFeedbackAsync(JObject.FromObject(new
            {
                value = $"{volumeDb:0.00} dB",
                indicator = indicatorPercentage
            }));
        }
        else
        {
            Connection.SetTitleAsync($"{volumeDb:0.00} dB");
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
