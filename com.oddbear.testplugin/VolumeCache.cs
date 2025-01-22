using System.ComponentModel;

namespace com.oddbear.testplugin
{
    public class CacheValue<TValue>
    {
        private TValue _value;

        public TValue GetValue()
        {
            return _timer.Enabled
                ? _value
                : _getValue();
        }

        public void SetValue(TValue value)
        {
            // Reset Timer
            _timer.Stop();
            _timer.Start();

            // Set cached value:
            _value = value;

            // Set real value (is eventual consistent):
            _setValue(value);

            // Call callback:
            _valueUpdatedCallback();
        }

        private readonly string _propertyName;
        private readonly Action _valueUpdatedCallback;
        private readonly Func<TValue> _getValue;
        private readonly Action<TValue> _setValue;

        private readonly System.Timers.Timer _timer;

        public CacheValue(string propertyName, Action<string> valueUpdatedCallback, Func<TValue> getValue, Action<TValue> setValue)
        {
            _propertyName = propertyName;
            _valueUpdatedCallback = () => valueUpdatedCallback(propertyName);
            _getValue = getValue;
            _setValue = setValue;

            _timer = new System.Timers.Timer(TimeSpan.FromSeconds(1))
            {
                AutoReset = false
            };
            _timer.Elapsed += (_, _) => _valueUpdatedCallback();
        }

        public void PropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName != _propertyName)
                return;

            if (_timer.Enabled)
                return;

            _valueUpdatedCallback();
        }

        public void Dispose()
        {
            _timer.Stop();
            _timer.Dispose();
        }
    }

    internal class VolumeCache : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler? PropertyChanged;

        private readonly VolumeEngineMock _volumeEngineMock;

        public VolumeCache()
        {
            // Setting starting position in the audio interface:
            _volumeEngineMock = new();
            _volumeEngineMock.Mock[nameof(MainOutVolume)] = 0.80f;
            _volumeEngineMock.Mock[nameof(HeadphonesVolume)] = 0.80f;
            _volumeEngineMock.Mock[nameof(MonitorBlend)] = 0.00f;

            _mainOutVolume = Create(nameof(MainOutVolume), "Out");
            _headphonesVolume = Create(nameof(HeadphonesVolume), "Phones");
            _monitorBlend = Create(nameof(MonitorBlend), "Blend");
        }

        private CacheValue<float> Create(string propertyName, string route)
        {
            var cachedValue = new CacheValue<float>(
                propertyName,
                NamedPropertyChanged,
                () => _volumeEngineMock.GetValue(route),
                value => _volumeEngineMock.SetValue(route, value)
            );
            // We need to register the callback delegate:
            _volumeEngineMock.PropertyChanged += cachedValue.PropertyChanged;
            return cachedValue;
        }

        private void NamedPropertyChanged(string propertyName)
        {
            // When last item is set, we wait and then set it to the true value:
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private readonly CacheValue<float> _mainOutVolume;
        public float MainOutVolume
        {
            get => _mainOutVolume.GetValue();
            set => _mainOutVolume.SetValue(value);
        }

        private readonly CacheValue<float> _headphonesVolume;
        public float HeadphonesVolume
        {
            get => _headphonesVolume.GetValue();
            set => _headphonesVolume.SetValue(value);
        }

        private readonly CacheValue<float> _monitorBlend;
        public float MonitorBlend
        {
            get => _monitorBlend.GetValue();
            set => _monitorBlend.SetValue(value);
        }

        // Mock engine
        protected class VolumeEngineMock : INotifyPropertyChanged
        {
            public event PropertyChangedEventHandler? PropertyChanged;

            // Mock of the "true" values:
            public readonly Dictionary<string, float> Mock = new();

            public float GetValue(string route)
            {
                if (Mock.ContainsKey(route) is false)
                    return default;

                return Mock[route];
            }

            public async void SetValue(string route, float value)
            {
                // Simulate value not truly set until after a delay:
                await Task.Delay(200);
                Mock[route] = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(route));
            }
        }
    }

    internal static class LookupTable
    {
        /// <summary>
        /// Since the algorithm is unknown, we have a sample for each db value for a lookup table.
        /// Then we need to figure out the percentage between the two samples, and estimate the correct db value.
        /// It works good enough, but it's not perfect (and diff gets bigger closer to 0dB, as the samples are farther apart per dB gain).
        /// Algorithm would be some sort of Log function, but it's not a simple one, probably several in ranges.
        /// </summary>
        public static float OutputPercentageToDb(float value)
        {
            if (value > 1)
                return 0;

            if (value < 0)
                return -96;

            return (float)AmpToDb(value);
        }


        public static float OutputDbToPercentage(float valueDb)
        {
            if (valueDb > 0)
                return 1;

            if (valueDb < -96)
                return 0;

            return (float)DbToAmp(valueDb);
        }

        static double AmpToDb(double x)
        {
            double[] coefficients = [
                -95.99887007922007,
                418.68885586964905,
                -898.6390137393848,
                1269.5354747004064,
                -1282.2142140129636,
                909.244250714216,
                -402.9738479769375,
                82.35750439953846,
            ];

            double total = coefficients[0];
            for (int i = 1; i < coefficients.Length; i++)
            {
                total += coefficients[i] * Math.Pow(x, i);
            }

            return (float)total;
        }

        static double DbToAmp(double x)
        {
            double[] coefficients = [
                0.9998937208128663,
                0.16696727653262494,
                0.0451560124215051,
                0.010614715692536959,
                0.0018197758649184232,
                0.00022431014169947388,
                2.0236380882249027E-05,
                1.3655587107300999E-06,
                7.024508008185433E-08,
                2.7956350047628403E-09,
                8.699853778440007E-11,
                2.130941204790007E-12,
                4.119049573188834E-14,
                6.273508696811293E-16,
                7.48077522580852E-18,
                6.897564278816661E-20,
                4.815379452147707E-22,
                2.4590257194098217E-24,
                8.65959363055648E-27,
                1.8786798833567043E-29,
                1.8916395815419544E-32,
            ];

            double total = coefficients[0];
            for (int i = 1; i < coefficients.Length; i++)
            {
                total += coefficients[i] * Math.Pow(x, i);
            }

            return (float)total;
        }
    }

}
