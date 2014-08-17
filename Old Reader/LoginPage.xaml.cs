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

using AppNs = Old_Reader;

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
			AppNs.App.RetentionDays = Old_Reader_Utils.Utils.DaysToString[timeToKeepOldItems.SelectedItem.ToString()];
			AppNs.App.AllItemsAtTop = (bool)chkAllItemsOntop.IsChecked;
			switch(serviceIdPicker.SelectedIndex)
			{
				case 0:
					App.CurrentService = WS.Remoting.TService.kTheOldReader;
					break;
				case 1:
					App.CurrentService = WS.Remoting.TService.kBazqux;
					break;
			}
			
			NavigationService.GoBack();
		}

		protected override void OnNavigatedTo(System.Windows.Navigation.NavigationEventArgs e)
		{
			base.OnNavigatedTo(e);
			Analytics.GAnalytics.trackPageView("login");

			if (serviceIdPicker.Items.Count==0)
			{
				serviceIdPicker.Items.Add("The Old Reader");
				serviceIdPicker.Items.Add("bazqux");
			}

			switch(App.CurrentService)
			{
				case WS.Remoting.TService.kTheOldReader:
					serviceIdPicker.SelectedIndex = 0;
					break;
				case WS.Remoting.TService.kBazqux:
					serviceIdPicker.SelectedIndex = 1;
					break;
			}

			if (moreItemCount.Items.Count == 0)
			{
				for (int i = 0; i < 9; i++)
				{
					moreItemCount.Items.Add(5 + i * 5);
				}

				try
				{
					chkShowReadItem.IsChecked = AppNs.App.ShowRead;
					txtUserName.Text = AppNs.App.UserEMail;
					txtPasswordBox.Password = AppNs.App.UserPassword;
					chkAllItemsOntop.IsChecked = AppNs.App.AllItemsAtTop;
					moreItemCount.SelectedIndex = (AppNs.App.AdditionalDownloadCount - 5) / 5;
				}
				catch
				{
					// there is nothing saved in here
					moreItemCount.SelectedIndex = 4;
				}
			}

			if (timeToKeepOldItems.Items.Count == 0)
			{
				foreach (var a in Utils.DaysToString)
				{
					timeToKeepOldItems.Items.Add(a.Key);
				}

				int nDaysCount = AppNs.App.RetentionDays;
				int nSelIdx = 0;
				foreach (var a in Utils.DaysToString)
				{
					if (a.Value == nDaysCount)
					{
						break;
					}
					else
					{
						nSelIdx++;
					}
				}

				timeToKeepOldItems.SelectedIndex = nSelIdx;
			}
		}
	}
}