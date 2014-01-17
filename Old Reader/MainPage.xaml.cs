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

using Microsoft.Phone.Scheduler;

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

		private ObservableCollection<DataModel.FeedItem> m_StarredFeeds;
		public ObservableCollection<DataModel.FeedItem> StarredFeeds
		{
			get
			{
				return m_StarredFeeds;
			}
			set
			{
#if OLD_READER_WP7
				NotifyPropertyChanging("StarredFeeds");
#else
				NotifyPropertyChanging();
#endif
				m_StarredFeeds = value;
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
		}

		private PeriodicTask unreadCountTask;

		private bool bLoginComplete = false;
		private void LoginComplete(String szResponse)
		{
			bLoginComplete = true;
			// split the output by \n and then by =
			Dictionary<String, String> loginDetails = Utils.toDictionary(szResponse, '\n', '=');
			App.AuthToken = loginDetails[OldReaderConsts.Auth];
			WS.Remoting.token = App.AuthToken;

			unreadCountTask=ScheduledActionService.Find(OldReaderConsts.unreadCountTask) as PeriodicTask;
			if (unreadCountTask == null)
			{
				unreadCountTask = new PeriodicTask(OldReaderConsts.unreadCountTask);

				if (unreadCountTask != null)
				{
					unreadCountTask.Description = "Fetch the unread feed count";
					try
					{
						Dispatcher.BeginInvoke(() =>
						{
							ScheduledActionService.Add(unreadCountTask);
							//ScheduledActionService.LaunchForTest(OldReaderConsts.unreadCountTask, TimeSpan.FromSeconds(20));
						});
					}
					catch (InvalidOperationException exp)
					{
						MessageBox.Show(exp.Message);
					}
				}
			}
			else
			{
				//ScheduledActionService.LaunchForTest(OldReaderConsts.unreadCountTask, TimeSpan.FromSeconds(10));
			}

			Utils.ClearTileCount();

			Contents.Initialize(InitializationCompleteHandler, InitializationErrorHandler, InitializationStatusReceiver);
		}

		private void InitializationCompleteHandler(DataModel.OldReaderContents contents)
		{
			Dispatcher.BeginInvoke(() =>
				{
					GetStarredFeeds();
					Utils.SyncStarredItems();
					Contents = contents;
					JobComplete();
				});
		}

		private void InitializationStatusReceiver(DataModel.OldReaderContents.TInitializationStates curState)
		{
			Dispatcher.BeginInvoke(() =>
			{
				trayProgress.Text = Utils.getInitializationStateStr(curState);
			});
		}

		private void InitializationErrorHandler(String szErrorMessage)
		{
			Dispatcher.BeginInvoke(() =>
				{
					JobComplete();
					MessageBox.Show(szErrorMessage);
				});
		}

		private int m_JobsPending = 0;
		private void StartJob()
		{
			m_JobsPending++;

			trayProgress.IsVisible = true;

			(ApplicationBar.Buttons[0] as ApplicationBarIconButton).IsEnabled = false;
			(ApplicationBar.Buttons[1] as ApplicationBarIconButton).IsEnabled = false;
		}

		private void JobComplete()
		{
			m_JobsPending--;
			m_JobsPending = m_JobsPending < 0 ? 0 : m_JobsPending;
			if (m_JobsPending == 0)
			{
				trayProgress.IsVisible = false;
				
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
				Contents = new DataModel.OldReaderContents();
				RefreshLocalStarredFeeds();
			}
			else
			{
				DataModel.OldReaderContents tmpContent = Contents;
				Contents = null;
				Contents = tmpContent;
				RefreshLocalStarredFeeds();

				if (AppNs.App.RefreshContents)
				{
					AppNs.App.RefreshContents = false;
					RefreshContent();
				}
			}

			if (!bLoginComplete && !TryLoggingIn())
			{
				txtHelpText.Visibility = Visibility.Visible;
			}
		}

		private void RefreshLocalStarredFeeds()
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

		private void starredFeedsList_SelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			if (starredFeedsList.SelectedIndex >= 0)
			{
				OpenFeedViewForCustomList(StarredFeeds, starredFeedsList.SelectedIndex);
			}
		}

		private void OpenFeedViewForCustomList(ObservableCollection<DataModel.FeedItem> customList, int selIndex)
		{
			App.FeedItems = new List<DataModel.FeedItem>();
			foreach (var curItem in customList)
			{
				App.FeedItems.Add(curItem);
			}
			NavigationService.Navigate(new Uri(String.Format("/FeedView.xaml?selIdx={0}", selIndex), UriKind.Relative));
		}

		private void ApplicationBarRefreshButton_Click(object sender, EventArgs e)
		{
			RefreshContent();
		}

		private void RefreshContent()
		{
			if (mainPanorama.SelectedItem == liveSubsPanaromaItem)
			{
				if (bLoginComplete)
				{
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
			else if (mainPanorama.SelectedItem == starredItemsPanoramaItem)
			{
				GetStarredFeeds();
			}
		}

		private void GetStarredFeeds()
		{
			trayProgress.Text = Utils.getInitializationStateStr(DataModel.OldReaderContents.TInitializationStates.kGettingStarredItems);
			StartJob();
			WS.Remoting rm = new WS.Remoting(StarredItemsComplete);
			rm.getUnreadItemsForSubscription(DataModel.Tag.StarredItems.id, 100);
		}

		private void StarredItemsComplete(String szResponse)
		{
			String continuationId = "";
			List<DataModel.FeedItem> newFeedItems = DataModel.FeedItem.CreateFromResponse(szResponse, out continuationId);
			foreach(var curFeed in newFeedItems)
			{
				DataStore.CachedFeed cachedFeed = App.ReaderDB.CachedFeeds.FirstOrDefault(cf => cf.ID == curFeed.id);
				if (cachedFeed == null)
				{
					App.ReaderDB.CachedFeeds.InsertOnSubmit(DataStore.CachedFeed.fromFeedItem(curFeed));
				}
				else
				{
					cachedFeed.Starred = curFeed.Starred;
				}
			}
			App.ReaderDB.SubmitChanges();

			Dispatcher.BeginInvoke(() =>
			{
				JobComplete();
				RefreshLocalStarredFeeds();
			});
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
				toast.Completed += toast_Completed;
				toast.Title = AppNs.Resources.AppResources.strPressBackToExitTitle;
				toast.Message = AppNs.Resources.AppResources.strPressBackToExit;
				toast.Show();
				e.Cancel = true;
			}
		}

		void toast_Completed(object sender, PopUpEventArgs<string, PopUpResult> e)
		{
			if (e.PopUpResult == PopUpResult.Cancelled || e.PopUpResult == PopUpResult.UserDismissed || e.PopUpResult == PopUpResult.Ok)
			{
				// user messed with the toast
				// Remove the counter
				m_nBackBtnCount = 0;
			}
		}

		private void ApplicationBarAddIconButton_Click(object sender, EventArgs e)
		{
			AppNs.App.RefreshContents = false;
			NavigationService.Navigate(new Uri("/AddSubscription.xaml?tagId=" + DataModel.Tag.AllItems.id, UriKind.Relative));
		}

		private void mainPanorama_SelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			// we have change os page
			m_nBackBtnCount = 0;
		}

		private DataModel.Subscription unsubFeed = null;

		private void menuUnsubscribe_Click(object sender, RoutedEventArgs e)
		{
			if (sender is MenuItem)
			{
				unsubFeed = (sender as MenuItem).DataContext as DataModel.Subscription;
				if (unsubFeed != null)
				{
					String szPrompt = String.Format(AppNs.Resources.AppResources.strConfirmUnsubscribe, unsubFeed.title);
					if (MessageBox.Show(szPrompt, "", MessageBoxButton.OKCancel) == MessageBoxResult.OK)
					{
						WS.Remoting rm = new WS.Remoting(UnsubscribeComplete);
						rm.unsubscribe(unsubFeed.id);
						StartJob();
						Analytics.GAnalytics.trackUnsubscribe();
					}
				}
			}
		}

		private void UnsubscribeComplete(String szResponse)
		{
			Dispatcher.BeginInvoke(() =>
			{
				JobComplete();
				if (szResponse == "OK")
				{
					// refresh the feedlist
					Contents.Subscriptions.Remove(unsubFeed);
					DataModel.Tag.AllItems.unreadCount -= unsubFeed.unreadCount;

					DataModel.OldReaderContents tmpContent = Contents;
					Contents = null;
					Contents = tmpContent;
				}
				else
				{
					ToastPrompt toast = new ToastPrompt();
					toast.Title = AppNs.Resources.AppResources.strErrorTitle;
					toast.Message = String.Format(AppNs.Resources.AppResources.strUnsubscribeFailed, unsubFeed.title);
					toast.Show();
				}
				unsubFeed = null;
			}
			);
		}

		DataModel.Subscription movedFeed = null;
		private DestinationTagChooser destinationSelector;

		private void menuMove_Click(object sender, RoutedEventArgs e)
		{
			if (sender is MenuItem)
			{
				var curFeed = (sender as MenuItem).DataContext as DataModel.Subscription;
				if (curFeed != null)
				{
					movedFeed = curFeed;
					destinationSelector = new DestinationTagChooser(this);
					destinationSelector.CurItem = curFeed;
					destinationSelector.Tags = App.Contents.Tags;
					destinationSelector.SelectedTag = curFeed.categories != null ? curFeed.categories[0] : DataModel.Tag.AllItems;
					destinationSelector.Done += destinationSelector_Done;
					destinationSelector.show();
				}
			}
		}

		void destinationSelector_Done(object sender, DestinationChooserDoneEvent e)
		{
			if (sender is DestinationTagChooser)
			{
				DestinationTagChooser chooser = sender as DestinationTagChooser;
				StartJob();
				WS.Remoting rm = new WS.Remoting(MoveComplete);
				rm.moveSubscriptionToFolder(chooser.CurItem.id, chooser.SelectedTag.id);
			}
		}

		private void MoveComplete(String szResponse)
		{
			Dispatcher.BeginInvoke(() =>
				{
					JobComplete();
					if (szResponse.ToUpper() != "OK")
					{
						MessageBox.Show(AppNs.Resources.AppResources.strFeedMoveError);
					}
					else
					{
						Analytics.GAnalytics.trackSubMove();
						Contents.DisplayObjects.Remove(movedFeed);
						RefreshContent();
					}
				});
		}
	}
}