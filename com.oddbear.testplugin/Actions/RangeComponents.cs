using BarRaider.SdTools;
using BarRaider.SdTools.Payloads;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using com.oddbear.testplugin.Actions.Enums;

namespace com.oddbear.testplugin.Actions;

// To try to just mimic the feel of the interface dial as much as possible:
[PluginActionId("com.oddbear.testplugin.range-components")]
public class RangeComponents : EncoderBase, IKeypadPlugin
{
    protected class PluginSettings
    {
        [JsonConverter(typeof(StringEnumConverter))]
        [JsonProperty(PropertyName = "output")]
        public DeviceOut Output { get; set; }

        [JsonProperty(PropertyName = "testValue")]
        public float TestValue { get; set; }

        [JsonProperty(PropertyName = "is_okay")]
        public bool isOkay { get; set; }
    }

    private PluginSettings _settings;

    public RangeComponents(ISDConnection connection, InitialPayload payload)
        : base(connection, payload)
    {
        if (payload.Settings == null || payload.Settings.Count == 0)
        {
            _settings = new PluginSettings();
        }
        else
        {
            _settings = payload.Settings.ToObject<PluginSettings>()!;
        }
    }

    public override void Dispose()
    {

    }


    public void KeyPressed(KeyPayload payload)
    {
    }

    public override void DialRotate(DialRotatePayload payload)
    {
    }

    public override void ReceivedSettings(ReceivedSettingsPayload payload)
    {
        Tools.AutoPopulateSettings(_settings, payload.Settings);
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
