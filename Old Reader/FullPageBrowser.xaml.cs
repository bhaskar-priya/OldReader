using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using System.ComponentModel;

namespace Old_Reader
{
	public partial class FullPageBrowser : PhoneApplicationPage
	{
		public FullPageBrowser()
		{
			InitializeComponent();
		}

		private String m_targetUrl = "";

		protected override void OnNavigatedTo(System.Windows.Navigation.NavigationEventArgs e)
		{
			base.OnNavigatedTo(e);
			Analytics.GAnalytics.trackPageView("fullBrowser");

			if (String.IsNullOrEmpty(m_targetUrl))
			{
				NavigationContext.QueryString.TryGetValue("url", out m_targetUrl);
				contentDisplay.Navigate(new Uri(m_targetUrl));
			}
		}

		protected override void OnBackKeyPress(System.ComponentModel.CancelEventArgs e)
		{
			base.OnBackKeyPress(e);

			if(contentDisplay.CanGoBack)
			{
				contentDisplay.GoBack();
				e.Cancel = true;
			}
			else
			{
				contentDisplay.Navigate(new Uri("about:blank"));
			}
		}

		private void contentDisplay_Navigating(object sender, NavigatingEventArgs e)
		{
			trayProgress.IsVisible = true;
		}

		private void contentDisplay_Navigated(object sender, NavigationEventArgs e)
		{
			trayProgress.IsVisible = false;
		}
	}
}