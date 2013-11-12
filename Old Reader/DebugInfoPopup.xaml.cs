using System;
using System.Diagnostics;
using System.Windows.Threading;
using Microsoft.Phone.Info;

namespace MemoryLeak.WP8
{
    public sealed partial class DebugInfoPopup
    {
        public DebugInfoPopup()
        {
            InitializeComponent();
        }

        private readonly DispatcherTimer timer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(500) };
        public void PopupControl_OnOpened(object sender, EventArgs e)
        {
            timer.Start();
            timer.Tick += UpdateMemoryInfo;
        }

        private void UpdateMemoryInfo(object sender, EventArgs e)
        {
            try
            {
                GC.Collect();
                GC.WaitForPendingFinalizers();
                // keep the popup open
                PopupControl.IsOpen = true;
            }
            catch (Exception ex)
            {
                // GC.Collect was throwing exceptions in some rare scenarios
                Debugger.Break();
            }
			double usage = (long)DeviceExtendedProperties.GetValue("ApplicationCurrentMemoryUsage");
			MemoryBlock.Text = string.Format("Memory: {0:N3} MB", (usage / 1024) / 1024.0);
        }
    }
}