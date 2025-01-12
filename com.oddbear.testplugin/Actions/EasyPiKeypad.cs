using BarRaider.SdTools;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Linq;
using System.Runtime.Serialization;

namespace com.oddbear.testplugin.Actions;

// https://github.com/BarRaider/streamdeck-easypi-v2
[PluginActionId("com.oddbear.testplugin.easypi-keypad")]
public class EasyPiKeypad : KeypadBase
{
    protected class PluginSettings
    {
        public static PluginSettings CreateDefaultSettings()
        {
            PluginSettings instance = new PluginSettings
            {
                Name = "Test",
                ShowName = true,
                FavColor = Color.Green,
                Brightness = 50
            };
            return instance;
        }

        [JsonProperty(PropertyName = "name")]
        public string Name { get; set; }

        [JsonProperty(PropertyName = "show_name")]
        public bool ShowName { get; set; }

        [JsonConverter(typeof(StringEnumConverter))]
        [JsonProperty(PropertyName = "fav_color")]
        public Color FavColor { get; set; }

        [JsonProperty(PropertyName = "brightness")]
        public int Brightness { get; set; }

        public enum Color
        {
            [EnumMember(Value = "red")]
            Red,
            [EnumMember(Value = "green")]
            Green,
            [EnumMember(Value = "blue")]
            Blue
        }
    }

    private readonly PluginSettings settings;

    public EasyPiKeypad(ISDConnection connection, InitialPayload payload)
        : base(connection, payload)
    {
        if (payload.Settings == null || payload.Settings.Count == 0)
        {
            this.settings = PluginSettings.CreateDefaultSettings();
        }
        else
        {
            this.settings = payload.Settings.ToObject<PluginSettings>()!;
        }
    }

    public override void Dispose()
    {
        //
    }

    public override void KeyPressed(KeyPayload payload)
    {
        //
    }

    public override void KeyReleased(KeyPayload payload)
    {
        //
    }

    public override void ReceivedSettings(ReceivedSettingsPayload payload)
    {
        Tools.AutoPopulateSettings(settings, payload.Settings);
        Connection.SetSettingsAsync(JObject.FromObject(settings));
    }

    public override void ReceivedGlobalSettings(ReceivedGlobalSettingsPayload payload)
    {
        //
    }

    public override void OnTick()
    {
        //
    }
}
