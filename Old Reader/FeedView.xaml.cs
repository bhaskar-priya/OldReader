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
using System.Runtime.CompilerServices;
using Microsoft.Phone.Tasks;

using AppNs =
#if OLD_READER_WP7
Old_Reader_WP7;
#else
Old_Reader;
#endif

namespace Old_Reader
{
	public partial class FeedView : PhoneApplicationPage, INotifyPropertyChanged, INotifyPropertyChanging
	{
		#region INotifyPropertyChange Members

		public event PropertyChangedEventHandler PropertyChanged;

		// Used to notify Silverlight that a property has changed.
#if OLD_READER_WP7
		protected void NotifyPropertyChanged(String propertyName)
#else
		protected void NotifyPropertyChanged([CallerMemberName] String propertyName = "")
#endif
		{
			if (PropertyChanged != null)
			{
				PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
			}
		}

		public event PropertyChangingEventHandler PropertyChanging;

		// Used to notify Silverlight that a property has changed.
#if OLD_READER_WP7
		protected void NotifyPropertyChanging(String propertyName)
#else
		protected void NotifyPropertyChanging([CallerMemberName] String propertyName = "")
#endif
		{
			if (PropertyChanging != null)
			{
				PropertyChanging(this, new PropertyChangingEventArgs(propertyName));
			}
		}
		#endregion

		public FeedView()
		{
			InitializeComponent();

			this.DataContext = this;

			(ApplicationBar.Buttons[0] as ApplicationBarIconButton).Text = AppNs.Resources.AppResources.strKeepUnread;
			(ApplicationBar.Buttons[1] as ApplicationBarIconButton).Text = AppNs.Resources.AppResources.strViewFull;
			(ApplicationBar.Buttons[2] as ApplicationBarIconButton).Text = AppNs.Resources.AppResources.strShare;
			(ApplicationBar.Buttons[3] as ApplicationBarIconButton).Text = AppNs.Resources.AppResources.strSaveLocal;

			(ApplicationBar.MenuItems[0] as ApplicationBarMenuItem).Text = AppNs.Resources.AppResources.strOpenInIE;
			(ApplicationBar.MenuItems[1] as ApplicationBarMenuItem).Text = AppNs.Resources.AppResources.strEMailMenuItem;
			(ApplicationBar.MenuItems[2] as ApplicationBarMenuItem).Text = AppNs.App.DarkMode?AppNs.Resources.AppResources.strLightMode:AppNs.Resources.AppResources.strDarkMode;
		}

		private DataModel.FeedItem m_feedItem;
		public DataModel.FeedItem curFeed
		{
			get
			{
				return m_feedItem;
			}
			set
			{
#if OLD_READER_WP7
				NotifyPropertyChanging("curFeed");
#else
				NotifyPropertyChanging();
#endif
				m_feedItem = value;
				handleAppBarButton();
#if OLD_READER_WP7
				NotifyPropertyChanged("curFeed");
#else
				NotifyPropertyChanged();
#endif
			}
		}

		private void handleAppBarButton()
		{
			// change the app bar icon
			String iconUri = "";
			String btnText = "";

			iconUri = Utilities.FeedListIconConvertor.getIconString(m_feedItem.id);

			if(!DataStore.CachedFeed.isStarred(m_feedItem.id))
			{
				btnText = AppNs.Resources.AppResources.strSaveLocal;
			}
			else
			{
				btnText = AppNs.Resources.AppResources.strRemoveLocal;
			}

			(ApplicationBar.Buttons[3] as ApplicationBarIconButton).IconUri = new Uri(iconUri, UriKind.Relative);
			(ApplicationBar.Buttons[3] as ApplicationBarIconButton).Text = btnText;
		}

		private int nCurIdx = -1;
		bool bShowLiveFeed = true;

		protected override void OnNavigatedTo(System.Windows.Navigation.NavigationEventArgs e)
		{
			base.OnNavigatedTo(e);

			if (nCurIdx == -1)
			{
				String szSelIdx = "";
				NavigationContext.QueryString.TryGetValue("selIdx", out szSelIdx);
				int.TryParse(szSelIdx, out nCurIdx);

				String szShowLiveFeed = "";
				if (NavigationContext.QueryString.TryGetValue("live", out szShowLiveFeed))
				{
					Boolean.TryParse(szShowLiveFeed, out bShowLiveFeed);
				}

				(ApplicationBar.Buttons[0] as ApplicationBarIconButton).IsEnabled = bShowLiveFeed;

				showFeedItem(nCurIdx);
			}
		}

		private void GestureListener_Flick(object sender, FlickGestureEventArgs e)
		{
			if (Old_Reader_Utils.Utils.isSwipeHorizontalEnough(e))
			{
				int nNewIdx = nCurIdx;
				if (e.HorizontalVelocity < 0)
				{
					Analytics.GAnalytics.trackFeedSwipe(true);
					nNewIdx++;
				}
				else if (e.HorizontalVelocity > 0)
				{
					Analytics.GAnalytics.trackFeedSwipe(false);
					nNewIdx--;
				}

				int nMaxFeedCount = AppNs.App.FeedItems.Count;
				if (nCurIdx != nNewIdx && nNewIdx >= 0 && nNewIdx < nMaxFeedCount)
				{
					nCurIdx = nNewIdx;
					showFeedItem(nNewIdx);
				}
			}
		}

		String GetMarkup()
		{
			if (AppNs.App.DarkMode)
			{
				String szStart = "<body style=\"background-color:black; color:white\">";
				String szEnd = "</body>";

				return szStart + ConvertExtendedASCII(curFeed.summary) + szEnd;
			}
			else
			{
				return ConvertExtendedASCII(curFeed.summary);
			}
		}

		void showFeedItem(int nIdx)
		{
			Analytics.GAnalytics.trackPageView("FeedView");
			curFeed = AppNs.App.FeedItems[nIdx];
			if (curFeed != null)
			{
				contentDisplay.NavigateToString(GetMarkup());

				if (bShowLiveFeed)
				{
					curFeed.markRead();
				}
			}
		}

		private static string ConvertExtendedASCII(string HTML)
		{
			string retVal = "";
			char[] s = HTML.ToCharArray();

			foreach (char c in s)
			{
				if (Convert.ToInt32(c) > 127)
					retVal += "&#" + Convert.ToInt32(c) + ";";
				else
					retVal += c;
			}

			return retVal;
		}


		private void ApplicationBarFullView_Click(object sender, EventArgs e)
		{
			NavigationService.Navigate(new Uri("/FullPageBrowser.xaml?url=" + curFeed.href, UriKind.Relative));
		}

		private void ApplicationBarShare_Click(object sender, EventArgs e)
		{
			ShareLinkTask shareLinkTask = new ShareLinkTask();
			shareLinkTask.Title = curFeed.title;
			shareLinkTask.LinkUri = new Uri(curFeed.href);
			shareLinkTask.Message = "";

			shareLinkTask.Show();

			Analytics.GAnalytics.trackFeedShare();
		}

		private void ApplicationBarKeepUnread_Click(object sender, EventArgs e)
		{
			if (bShowLiveFeed)
			{
				Analytics.GAnalytics.trackKeepUnread();
				curFeed.keepUnread = true;
				curFeed.markUnRead();
			}
		}

		private void ApplicationBarSaveButton_Click(object sender, EventArgs e)
		{
			Analytics.GAnalytics.trackFeedSave(false);
			DataStore.CachedFeed.toggleStarred(curFeed.id);
			handleAppBarButton();
		}

		private void ApplicationBarOpenInIEMenuItem_Click(object sender, EventArgs e)
		{
			Analytics.GAnalytics.trackFeedOpenInIE();
			WebBrowserTask webBrowserTask = new WebBrowserTask();
			webBrowserTask.Uri = new Uri(curFeed.href);
			webBrowserTask.Show();
		}

		private void ApplicationBarEMailMenuItem_Click(object sender, EventArgs e)
		{
			Analytics.GAnalytics.trackFeedEmailed();
			EmailComposeTask email = new EmailComposeTask();
			email.Subject = curFeed.title;
			email.Body = curFeed.href;
			email.Show();
		}

		private void ApplicationBarDarkModeMenuItem_Click(object sender, EventArgs e)
		{
			AppNs.App.DarkMode = !AppNs.App.DarkMode;
			(ApplicationBar.MenuItems[2] as ApplicationBarMenuItem).Text = AppNs.App.DarkMode ? AppNs.Resources.AppResources.strLightMode : AppNs.Resources.AppResources.strDarkMode;
			contentDisplay.NavigateToString(GetMarkup());
		}
	}
}