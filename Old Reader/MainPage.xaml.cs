using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
#if OLD_READER_WP7
using Old_Reader_WP7.Resources;
#else
using Old_Reader.Resources;
#endif
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.IO.IsolatedStorage;
using Old_Reader_Utils;
using System.Collections.ObjectModel;
using System.Data.Linq;
using System.Data.Linq.Mapping;
using Coding4Fun.Toolkit.Controls;

using AppNs =
#if OLD_READER_WP7
 Old_Reader_WP7;
#else
 Old_Reader;
#endif


#if OLD_READER_WP7
namespace Old_Reader_WP7
#else
namespace Old_Reader
#endif
{
	public partial class MainPage : PhoneApplicationPage, INotifyPropertyChanged, INotifyPropertyChanging
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

		public DataModel.OldReaderContents Contents
		{
			get
			{
				return App.Contents;
			}
			set
			{
#if OLD_READER_WP7
				NotifyPropertyChanging("Contents");
#else
				NotifyPropertyChanging();
#endif
				App.Contents = value;
#if OLD_READER_WP7
				NotifyPropertyChanged("Contents");
#else
				NotifyPropertyChanged();
#endif
			}
		}

		private ObservableCollection<DataModel.FeedItem> m_LocalFeeds;
		public ObservableCollection<DataModel.FeedItem> StarredFeeds
		{
			get
			{
				return m_LocalFeeds;
			}
			set
			{
#if OLD_READER_WP7
				NotifyPropertyChanging("StarredFeeds");
#else
				NotifyPropertyChanging();
#endif
				m_LocalFeeds = value;
#if OLD_READER_WP7
				NotifyPropertyChanged("StarredFeeds");
#else
				NotifyPropertyChanged();
#endif
			}
		}

		// Constructor
		public MainPage()
		{
			InitializeComponent();

			this.DataContext = this;

			(ApplicationBar.Buttons[0] as ApplicationBarIconButton).Text = AppNs.Resources.AppResources.strRefreshAppButton;
			(ApplicationBar.Buttons[1] as ApplicationBarIconButton).Text = AppNs.Resources.AppResources.strAddFeed;
			(ApplicationBar.Buttons[1] as ApplicationBarIconButton).IsEnabled = false;
			(ApplicationBar.MenuItems[0] as ApplicationBarMenuItem).Text = AppNs.Resources.AppResources.strSettingsMenu;
			(ApplicationBar.MenuItems[1] as ApplicationBarMenuItem).Text = AppNs.Resources.AppResources.strStatusMenu;
			(ApplicationBar.MenuItems[2] as ApplicationBarMenuItem).Text = AppNs.Resources.AppResources.strAboutMenu;

#if DEBUG
			AppNs.App.userSettings[OldReaderConsts.EMail] = "bhaskar.priya@gmail.com";
			AppNs.App.userSettings[OldReaderConsts.Password] = "random12!";
#endif
		}

		private bool bLoginComplete = false;
		private void LoginComplete(String szResponse)
		{
			bLoginComplete = true;
			// split the output by \n and then by =
			Dictionary<String, String> loginDetails = Utils.toDictionary(szResponse, '\n', '=');
			WS.Remoting.token = loginDetails[OldReaderConsts.Auth];

			DataModel.OldReaderContents tmpContents = new DataModel.OldReaderContents();
			tmpContents.Initialize(InitializationCompleteHandler, InitializationErrorHandler, InitializationStatusReceiver);
		}

		private void InitializationCompleteHandler(DataModel.OldReaderContents contents)
		{
			Dispatcher.BeginInvoke(() =>
				{
					if (StarredFeeds == null || StarredFeeds.Count == 0)
					{
						StarredFeeds = new ObservableCollection<DataModel.FeedItem>();
						refreshLocalFeeds();
					}
					txtProgressState.Visibility = System.Windows.Visibility.Collapsed;
					Contents = contents;
					JobComplete();
				});
		}

		private void InitializationStatusReceiver(DataModel.OldReaderContents.TInitializationStates curState)
		{
			Dispatcher.BeginInvoke(() =>
			{
				txtProgressState.Visibility = System.Windows.Visibility.Visible;
				txtProgressState.Text = Utils.getInitializationStateStr(curState);
			});
		}

		private void InitializationErrorHandler(String szErrorMessage)
		{
			Dispatcher.BeginInvoke(() =>
				{
					if (!bLoginComplete)
					{
						// login failed
						JobComplete();
					}
					MessageBox.Show(szErrorMessage);
				});
		}

		private int m_JobsPending = 0;
		private void StartJob()
		{
			m_JobsPending++;
			progressBar.Visibility = System.Windows.Visibility.Visible;
			(ApplicationBar.Buttons[0] as ApplicationBarIconButton).IsEnabled = false;
			(ApplicationBar.Buttons[1] as ApplicationBarIconButton).IsEnabled = false;
		}

		private void JobComplete()
		{
			m_JobsPending--;
			m_JobsPending = m_JobsPending < 0 ? 0 : m_JobsPending;
			if (m_JobsPending == 0)
			{
				progressBar.Visibility = System.Windows.Visibility.Collapsed;
				(ApplicationBar.Buttons[0] as ApplicationBarIconButton).IsEnabled = true;
				(ApplicationBar.Buttons[1] as ApplicationBarIconButton).IsEnabled = true;
			}
		}

		protected override void OnNavigatedTo(System.Windows.Navigation.NavigationEventArgs e)
		{
			base.OnNavigatedTo(e);
			m_nBackBtnCount = 0;
			Analytics.GAnalytics.trackPageView("main");

			if (Contents == null)
			{
				if (!TryLoggingIn())
				{
					txtHelpText.Visibility = Visibility.Visible;
				}
			}
			else
			{
				DataModel.OldReaderContents tmpContent = Contents;
				Contents = null;
				Contents = tmpContent;
				refreshLocalFeeds();
			}
		}

		private void refreshLocalFeeds()
		{
			StarredFeeds = new ObservableCollection<DataModel.FeedItem>();
			// get the local feeds from database
			var localFeedItems = from DataStore.CachedFeed curItem in App.ReaderDB.CachedFeeds where curItem.Starred == true select curItem;
			foreach (var curCachedFeedItem in localFeedItems)
			{
				StarredFeeds.Add(curCachedFeedItem.toFeedItem());
			}
		}

		private void subscriptionList_SelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			if (subscriptionList.SelectedItem is DataModel.Tag)
			{
				DataModel.Tag curTag = subscriptionList.SelectedItem as DataModel.Tag;
				if (curTag.id != DataModel.Tag.AllItems.id)
				{
					NavigationService.Navigate(new Uri("/TagView.xaml?tagId=" + (subscriptionList.SelectedItem as DataModel.Tag).id, UriKind.Relative));
				}
				else
				{
					NavigationService.Navigate(new Uri("/SubscriptionView.xaml?feedId=" + curTag.id, UriKind.Relative));
				}
			}
			else if (subscriptionList.SelectedItem is DataModel.Subscription)
			{
				NavigationService.Navigate(new Uri("/SubscriptionView.xaml?feedId=" + (subscriptionList.SelectedItem as DataModel.Subscription).id, UriKind.Relative));
			}
		}

		private void savedFeedsList_SelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			if (savedFeedsList.SelectedIndex >= 0)
			{
				App.FeedItems = new List<DataModel.FeedItem>();
				foreach (var curItem in StarredFeeds)
				{
					App.FeedItems.Add(curItem);
				}
				NavigationService.Navigate(new Uri(String.Format("/FeedView.xaml?selIdx={0}&live=false", savedFeedsList.SelectedIndex), UriKind.Relative));
			}
		}

		private void ApplicationBarRefreshButton_Click(object sender, EventArgs e)
		{
			if (mainPanorama.SelectedItem == liveSubsPanaromaItem)
			{
				if (bLoginComplete)
				{
					Contents = null;
					DataModel.OldReaderContents tmpContents = new DataModel.OldReaderContents();
					tmpContents.Initialize(InitializationCompleteHandler, InitializationErrorHandler, InitializationStatusReceiver);
					StartJob();
				}
				else
				{
					if (!TryLoggingIn())
					{
						NavigationService.Navigate(new Uri("/LoginPage.xaml", UriKind.Relative));
					}
				}
			}
			else if (mainPanorama.SelectedItem == localContentPanaromaItem)
			{
				refreshLocalFeeds();
			}
		}

		private bool TryLoggingIn()
		{
			if (!String.IsNullOrEmpty(App.UserEMail) && !String.IsNullOrEmpty(App.UserPassword))
			{
				txtHelpText.Visibility = Visibility.Collapsed;
				bLoginComplete = false;
				WS.Remoting rm = new WS.Remoting(LoginComplete, InitializationErrorHandler);
				rm.Login(App.UserEMail, App.UserPassword);
				StartJob();
				return true;
			}
			return false;
		}

		private void ApplicationBarSettingsMenuItem_Click(object sender, EventArgs e)
		{
			NavigationService.Navigate(new Uri("/LoginPage.xaml", UriKind.Relative));
		}

		private void ApplicationBarOldReaderStatusMenuItem_Click(object sender, EventArgs e)
		{
			NavigationService.Navigate(new Uri("/FullPageBrowser.xaml?url=" + "http://theoldreader.com/reader/api/0/status", UriKind.Relative));
		}

		private void ApplicationBarAboutMenuItem_Click(object sender, EventArgs e)
		{
			NavigationService.Navigate(new Uri("/About.xaml", UriKind.Relative));
		}

		private void btnAddToFavorite_Click(object sender, RoutedEventArgs e)
		{
			Object objDataSrc = (sender as Button).DataContext;
			if (objDataSrc is DataModel.FeedItem)
			{
				DataModel.FeedItem curFeedItem = objDataSrc as DataModel.FeedItem;
				DataStore.CachedFeed.toggleStarred(curFeedItem.id);
				// force icon update
				curFeedItem.id = curFeedItem.id;
			}
		}

		int m_nBackBtnCount = 0;
		protected override void OnBackKeyPress(CancelEventArgs e)
		{
			base.OnBackKeyPress(e);
			m_nBackBtnCount++;
			if (m_nBackBtnCount < 2)
			{
				ToastPrompt toast = new ToastPrompt();
				toast.Title = AppNs.Resources.AppResources.strPressBackToExitTitle;
				toast.Message = AppNs.Resources.AppResources.strPressBackToExit;
				toast.Show();
				e.Cancel = true;
			}
		}

		private void ApplicationBarAddIconButton_Click(object sender, EventArgs e)
		{
			NavigationService.Navigate(new Uri("/AddSubscription.xaml?tagId=" + DataModel.Tag.AllItems.id, UriKind.Relative));
		}
	}
}