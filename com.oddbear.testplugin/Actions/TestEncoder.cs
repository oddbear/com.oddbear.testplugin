using BarRaider.SdTools;
using BarRaider.SdTools.Events;
using BarRaider.SdTools.Payloads;
using BarRaider.SdTools.Wrappers;
using Newtonsoft.Json;

namespace com.oddbear.testplugin.Actions;

public record LogEvent(DateTime Time, string Event, object payload);

// Flows:
//
// Start:
// - OnTitleParametersDidChange - "State 1" title set, or title if user has set one
// - OnTitleParametersDidChange - "State 2" title set again, or title if user has set one
// Clicked on Property Inspector:
// - OnPropertyInspectorDidAppear - No info in event
// - OnSendToPlugin - { "property_inspector": "propertyInspectorConnected" }

[PluginActionId("com.oddbear.testplugin.test-encoder")]
public class TestEncoder : EncoderBase, IKeypadPlugin
{
    protected class TestSettings
    {
        //[JsonConverter(typeof(StringEnumConverter))]
        [JsonProperty(PropertyName = "intValue")]
        public int IntValue { get; set; }

        [JsonProperty(PropertyName = "stringValue")]
        public string StringValue { get; set; }

        [JsonProperty(PropertyName = "boolValue")]
        public bool BoolValue { get; set; }
    }

    private List<LogEvent> _events = [];

    public TestEncoder(ISDConnection connection, InitialPayload payload)
        : base(connection, payload)
    {
        Connection.OnPropertyInspectorDidAppear += OnPropertyInspectorDidAppear;
        Connection.OnPropertyInspectorDidDisappear += OnPropertyInspectorDidDisappear;

        Connection.OnApplicationDidLaunch += OnApplicationDidLaunch;
        Connection.OnApplicationDidTerminate += OnApplicationDidTerminate;
        
        Connection.OnDeviceDidConnect += OnDeviceDidConnect;
        Connection.OnDeviceDidDisconnect += OnDeviceDidDisconnect;

        Connection.OnSendToPlugin += OnSendToPlugin;
        Connection.OnTitleParametersDidChange += OnTitleParametersDidChange;
        Connection.OnSystemDidWakeUp += OnSystemDidWakeUp;
    }

    public override void Dispose()
    {
        Connection.OnPropertyInspectorDidAppear -= OnPropertyInspectorDidAppear;
        Connection.OnPropertyInspectorDidDisappear -= OnPropertyInspectorDidDisappear;

        Connection.OnApplicationDidLaunch -= OnApplicationDidLaunch;
        Connection.OnApplicationDidTerminate -= OnApplicationDidTerminate;

        Connection.OnDeviceDidConnect -= OnDeviceDidConnect;
        Connection.OnDeviceDidDisconnect -= OnDeviceDidDisconnect;

        Connection.OnSendToPlugin -= OnSendToPlugin;
        Connection.OnTitleParametersDidChange -= OnTitleParametersDidChange;
        Connection.OnSystemDidWakeUp -= OnSystemDidWakeUp;
    }
    
    public override async void DialRotate(DialRotatePayload payload)
    {
        _events.Add(new LogEvent(DateTime.Now, "DialRotate", payload));
        await Connection.SetTitleAsync("DialRotate");
    }

    public override async void DialDown(DialPayload payload)
    {
        _events.Add(new LogEvent(DateTime.Now, "DialDown", payload));
        await Connection.SetTitleAsync("DialDown");

        await Task.Delay(200);
        await Connection.SendToPropertyInspectorAsync(payload.Settings);
    }

    public override async void DialUp(DialPayload payload)
    {
        _events.Add(new LogEvent(DateTime.Now, "DialUp", payload));
        await Connection.SetTitleAsync("DialUp");
    }

    public override async void TouchPress(TouchpadPressPayload payload)
    {
        _events.Add(new LogEvent(DateTime.Now, "TouchPress", payload));
        await Connection.SetTitleAsync("TouchPress");
    }

    public override async void ReceivedSettings(ReceivedSettingsPayload payload)
    {
        _events.Add(new LogEvent(DateTime.Now, "ReceivedSettings", payload));
        await Connection.SetTitleAsync("ReceivedSettings");
    }

    public override async void ReceivedGlobalSettings(ReceivedGlobalSettingsPayload payload)
    {
        _events.Add(new LogEvent(DateTime.Now, "ReceivedGlobalSettings", payload));
        await Connection.SetTitleAsync("ReceivedGlobalSettings");
    }

    public async void KeyPressed(KeyPayload payload)
    {
        _events.Add(new LogEvent(DateTime.Now, "KeyPressed", payload));
        await Connection.SetTitleAsync("KeyPressed");

        await Task.Delay(200);
        await Connection.SendToPropertyInspectorAsync(payload.Settings);

        //await Task.Delay(200);
        //await Connection.SetSettingsAsync(payload.Settings);
    }

    public async void KeyReleased(KeyPayload payload)
    {
        _events.Add(new LogEvent(DateTime.Now, "KeyReleased", payload));
        await Connection.SetTitleAsync("KeyReleased");
    }

    private async void OnPropertyInspectorDidAppear(object? sender, SDEventReceivedEventArgs<PropertyInspectorDidAppear> e)
    {
        _events.Add(new LogEvent(DateTime.Now, "OnPropertyInspectorDidAppear", e.Event));
        await Connection.SetTitleAsync("OnPropertyInspectorDidAppear");
    }

    private async void OnPropertyInspectorDidDisappear(object? sender, SDEventReceivedEventArgs<PropertyInspectorDidDisappear> e)
    {
        _events.Add(new LogEvent(DateTime.Now, "OnPropertyInspectorDidDisappear", e.Event));
        await Connection.SetTitleAsync("OnPropertyInspectorDidDisappear");
    }

    private async void OnApplicationDidLaunch(object? sender, SDEventReceivedEventArgs<ApplicationDidLaunch> e)
    {
        _events.Add(new LogEvent(DateTime.Now, "OnApplicationDidLaunch", e.Event));
        await Connection.SetTitleAsync("OnApplicationDidLaunch");
    }

    private async void OnApplicationDidTerminate(object? sender, SDEventReceivedEventArgs<ApplicationDidTerminate> e)
    {
        _events.Add(new LogEvent(DateTime.Now, "OnApplicationDidTerminate", e.Event));
        await Connection.SetTitleAsync("OnApplicationDidTerminate");
    }

    private async void OnDeviceDidConnect(object? sender, SDEventReceivedEventArgs<DeviceDidConnect> e)
    {
        _events.Add(new LogEvent(DateTime.Now, "OnDeviceDidConnect", e.Event));
        await Connection.SetTitleAsync("OnDeviceDidConnect");
    }

    private async void OnDeviceDidDisconnect(object? sender, SDEventReceivedEventArgs<DeviceDidDisconnect> e)
    {
        _events.Add(new LogEvent(DateTime.Now, "OnDeviceDidDisconnect", e.Event));
        await Connection.SetTitleAsync("OnDeviceDidDisconnect");
    }

    private async void OnSendToPlugin(object? sender, SDEventReceivedEventArgs<SendToPlugin> e)
    {
        _events.Add(new LogEvent(DateTime.Now, "OnSendToPlugin", e.Event));
        await Connection.SetTitleAsync("OnSendToPlugin");
    }

    private async void OnTitleParametersDidChange(object? sender, SDEventReceivedEventArgs<TitleParametersDidChange> e)
    {
        // Called 2 times:
        _events.Add(new LogEvent(DateTime.Now, "OnTitleParametersDidChange", e.Event));
        await Connection.SetTitleAsync("OnTitleParametersDidChange");
    }

    private async void OnSystemDidWakeUp(object? sender, SDEventReceivedEventArgs<SystemDidWakeUp> e)
    {
        _events.Add(new LogEvent(DateTime.Now, "OnSystemDidWakeUp", e.Event));
        await Connection.SetTitleAsync("OnSystemDidWakeUp");
    }

    public override void OnTick()
    {
        // This is disabled as it's called every 1 second:
        //_events.Add(new LogEvent(DateTime.Now, "OnTick", null));
        //await Connection.SetTitleAsync("OnTick");

        // This is however a nice place to put a breakpoint to inspect the _events list
    }
}
