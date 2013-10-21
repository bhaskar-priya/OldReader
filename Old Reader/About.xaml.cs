using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using Microsoft.Phone.Tasks;
using System.Reflection;

namespace Old_Reader
{
	public partial class About : PhoneApplicationPage
	{
		public About()
		{
			InitializeComponent();

			txtVersionBox.Text = new AssemblyName(Assembly.GetExecutingAssembly().FullName).Version.ToString();
			Analytics.GAnalytics.trackPageView("about");
		}

		private void btnSendFeedback_Click(object sender, RoutedEventArgs e)
		{
			EmailComposeTask task = new EmailComposeTask()
			{
				To = "bhaskar.priya@gmail.com",
				Subject = "WP Old Reader Feedback"
			};
			task.Show();
		}
	}
}