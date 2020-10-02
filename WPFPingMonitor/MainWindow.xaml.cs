﻿using System;
using System.Linq;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media;
using System.Windows.Threading;

namespace WpfPingMonitor
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        readonly Pinger myPinger = new Pinger();
        private DateTime lastUpdate;

        public MainWindow()
        {
            InitializeComponent();
            DataContext = myPinger;
            // create a timer to check for new data every 10 milliseconds
            DispatcherTimer plotTimer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(1) };
            plotTimer.Tick += PlotNow;
            plotTimer.Start();

        }
        void PlotNow(object sender, EventArgs e)
        {
            PingResult[] successfulPings = myPinger.PingResults.Where(x => x.Success).ToArray();

            // only update the plot if new data is available
            if (successfulPings.Length == 0)
                return;
            if (successfulPings.Last().Timestamp == lastUpdate)
                return;
            lastUpdate = myPinger.PingResults.Last().Timestamp;

            // clear any old data that may have been plotted
            wpfPlot1.plt.Clear();

            // transform data to double arrays and plot it as a scatter plot
            double[] xs = successfulPings.Select(x => x.OADate).TakeLast(myPinger.Maxpoints).ToArray();
            double[] ys = successfulPings.Select(x => x.Latency).TakeLast(myPinger.Maxpoints).ToArray();
            
            //Set the bar width
            double barWidth = .5 / (24 * 60 * 60); // units are fraction of a day
            //Initialize the plot
            wpfPlot1.plt.PlotBar(xs, ys, barWidth: barWidth);

            // decorate the plot, and disable the mouse since axis limits are set manually
            wpfPlot1.plt.Title("Web Server Latency");
            wpfPlot1.plt.YLabel("Latency (ms)");

            //Lock the Y axis to values of 0 to 100
            wpfPlot1.plt.Axis(y1: 0, y2: 100);
            wpfPlot1.Configure(enablePanning: false, enableRightClickZoom: false);

            // indicate horizontal values are DateTime and add padding to accomodate large tick labels
            wpfPlot1.plt.Ticks(dateTimeX: true);
            wpfPlot1.plt.Layout(y2LabelWidth: 40);

            // the programmer is in control of when the plot is rendered
            wpfPlot1.Render();
        }
        
    }
}
