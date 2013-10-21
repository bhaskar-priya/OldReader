using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using System.IO.IsolatedStorage;
using Old_Reader_Utils;

using AppNs =
#if OLD_READER_WP7
Old_Reader_WP7;
#else
Old_Reader;
#endif

namespace Old_Reader
{
	public partial class LoginPage : PhoneApplicationPage
	{
		public LoginPage()
		{
			InitializeComponent();

			this.DataContext = this;

			(ApplicationBar.Buttons[0] as ApplicationBarIconButton).Text = AppNs.Resources.AppResources.strDoneAppBarButton;
		}

		private void ApplicationBarDone_Click(object sender, EventArgs e)
		{
			AppNs.App.UserEMail = txtUserName.Text;
			AppNs.App.UserPassword = txtPasswordBox.Password;
			AppNs.App.AdditionalDownloadCount = moreItemCount.SelectedIndex * 5 + 5;
			AppNs.App.ShowRead = (bool)chkShowReadItem.IsChecked;

			NavigationService.GoBack();
		}

		protected override void OnNavigatedTo(System.Windows.Navigation.NavigationEventArgs e)
		{
			base.OnNavigatedTo(e);
			Analytics.GAnalytics.trackPageView("login");

			if (moreItemCount.Items.Count == 0)
			{
				for (int i = 0; i < 9; i++)
				{
					moreItemCount.Items.Add(5 + i * 5);
				}

				try
				{
					chkShowReadItem.IsChecked = AppNs.App.ShowRead;
					moreItemCount.SelectedIndex = (AppNs.App.AdditionalDownloadCount - 5) / 5;
					txtUserName.Text = AppNs.App.UserEMail;
					txtPasswordBox.Password = AppNs.App.UserPassword;
					
				}
				catch
				{
					// there is nothing saved in here
					moreItemCount.SelectedIndex = 4;
				}
			}
		}
	}
}