using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Net.NetworkInformation;
using System.Runtime.CompilerServices;
using System.Windows.Threading;


namespace WpfPingMonitor
{
    public class Pinger : INotifyPropertyChanged
    {
        #region Fields
        const string HostNameOrAddress = "google.com";
        public ObservableCollection<PingResult> PingResults { get; set; } = new ObservableCollection<PingResult>();
        private const int MaxNumberOfDataPoints = 30;
        private const int timeout = 250;
        
        private int _missedcount;
        private int _datapointCount;
        private int _maxpoints;
        private double _average;
        private double _maxPing;
        private double _minPing;

        #endregion

        #region Properties
        public int DatapointCount
        {
            get => _datapointCount;
            set
            {
                _datapointCount = value;
                OnPropertyChanged();
            }
        }
        public int Maxpoints
        {
            get => _maxpoints;
            set
            {

                _maxpoints = value;
                OnPropertyChanged();
            }
        }
        public int MissedCount
        {
            get => _missedcount;
            set
            {

                _missedcount = value;
                OnPropertyChanged();
            }
        }

        public double Average
        {
            get => Math.Round(_average, 1);
            set
            {
                _average = value;
                OnPropertyChanged();
            }
        }
        public double MaxPing
        {
            get => _maxPing;
            set
            {
                _maxPing = value;
                OnPropertyChanged();
            }
        }
        public double MinPing
        {
            get => _minPing;
            set
            {
                _minPing = value;
                OnPropertyChanged();
            }
        }

        #endregion

        /// <summary>
        /// Constructor
        /// </summary>
        public Pinger()
        {
            Maxpoints = 60;
            MaxPing = 0;
            MinPing = timeout;

            DispatcherTimer timer = new DispatcherTimer
            {
                Interval = TimeSpan.FromSeconds(3),
                IsEnabled = true
            };

            PingNow();
            timer.Tick += (sender, args) => PingNow();
        }


        private void PingNow()
        {
            using var pingSender = new Ping();
            PingReply reply = pingSender.Send(HostNameOrAddress, timeout);

            var result = new PingResult
            {
                Timestamp = DateTime.Now,
                Latency = reply.RoundtripTime,
                Success = reply?.Status == IPStatus.Success
            };

            PingResults.Add(result);
            DatapointCount = PingResults.Count;
            Average = PingResults.Average(x => x.Latency);
            if (result.Latency < MinPing && result.Success)
            {
                MinPing = result.Latency;
            }
            if (result.Latency > MaxPing)
            {
                MaxPing = result.Latency;
            }
            if (!result.Success)
            {
                MissedCount++;
            }
        }

        #region Event Handler
        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
        #endregion
    }
}