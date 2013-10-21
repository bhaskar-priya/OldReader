using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using System.Collections.ObjectModel;
using DataModel;
using Newtonsoft.Json.Linq;
using WS;
using Coding4Fun.Toolkit.Controls;

#if OLD_READER_WP7
using Old_Reader_WP7;
#endif
using Old_Reader_Utils;

using AppNs =
#if OLD_READER_WP7
 Old_Reader_WP7;
#else
 Old_Reader;
#endif

namespace Old_Reader
{
	public partial class AddSubscription : PhoneApplicationPage
	{
		public AddSubscription()
		{
			InitializeComponent();

			this.DataContext = this;

			(ApplicationBar.Buttons[0] as ApplicationBarIconButton).Text = AppNs.Resources.AppResources.strDoneAppBarButton;
			feedDisplay.NavigateToString(AppNs.Resources.AppResources.strSyncBrowserPrompt);
		}

		public ObservableCollection<Tag> Tags
		{
			get
			{
				return App.Contents.Tags;
			}
		}

		String m_CurTagId = "";

		protected override void OnNavigatedTo(System.Windows.Navigation.NavigationEventArgs e)
		{
			base.OnNavigatedTo(e);
			Analytics.GAnalytics.trackPageView("addSubscription");

			if (String.IsNullOrEmpty(m_CurTagId))
			{
				// get the tag id
				NavigationContext.QueryString.TryGetValue("tagId", out m_CurTagId);

				ctlTagList.SelectedItem = App.Contents.Tags.FirstOrDefault(t => t.id == m_CurTagId);
			}
		}

		private void ApplicationBarDone_Click(object sender, EventArgs e)
		{
			Analytics.GAnalytics.trackOldReaderEvent(OldReaderTrackingConsts.addFeed, 0);

			WS.Remoting rm = new WS.Remoting(AddFeedComplete);
			rm.addSubscription(txtFeedUrl.Text);
			StartJob();
		}

		private void AddFeedComplete(String szResponse)
		{
			Dispatcher.BeginInvoke(() =>
				{
					JobComplete();
					// get the added feed id out
					JObject obj = JObject.Parse(szResponse);
					String szStreamId = (String)obj["streamId"];
					if (!String.IsNullOrEmpty(szStreamId))
					{
						// see if we need to add this to a folder
						if (ctlTagList.SelectedItem != DataModel.Tag.AllItems)
						{
							// move to other folder
							WS.Remoting rm = new WS.Remoting(MoveFeedComplete);
							rm.moveSubscriptionToFolder(szStreamId, (ctlTagList.SelectedItem as DataModel.Tag).id);
							StartJob();
						}
						else
						{
							// all done
							NavigationService.GoBack();
						}
					}
					else
					{
						ToastPrompt toast = new ToastPrompt();
						toast.Title = AppNs.Resources.AppResources.strErrorTitle;
						toast.Message = AppNs.Resources.AppResources.strFeeadAddError;
						toast.Show();
					}
				});
		}

		private void MoveFeedComplete(String szResponse)
		{
			Dispatcher.BeginInvoke(() =>
				{
					JobComplete();
					if (szResponse.ToUpper() != "OK")
					{
						MessageBox.Show(AppNs.Resources.AppResources.strFeedMoveError);
					}
					NavigationService.GoBack();
				});
		}

		private int m_JobsPending = 0;
		private void StartJob()
		{
			m_JobsPending++;
			progressBar.Visibility = System.Windows.Visibility.Visible;
		}

		private void JobComplete()
		{
			m_JobsPending--;
			m_JobsPending = m_JobsPending < 0 ? 0 : m_JobsPending;
			if (m_JobsPending == 0)
			{
				progressBar.Visibility = System.Windows.Visibility.Collapsed;
			}
		}

		private void ApplicationBarSyncIconButton_Click(object sender, EventArgs e)
		{
			try
			{
				String szUrl = txtFeedUrl.Text;
				if (szUrl.IndexOf("http://") != 0)
				{
					szUrl = "http://" + szUrl;
				}
				txtFeedUrl.Text = szUrl;
				feedDisplay.Navigate(new Uri(szUrl));
			}
			catch
			{
			}
		}

		private void txtFeedUrl_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
		{
			if (e.Key == System.Windows.Input.Key.Enter)
			{
				feedDisplay.Navigate(new Uri(txtFeedUrl.Text));
				this.Focus();
			}
		}
	}
}