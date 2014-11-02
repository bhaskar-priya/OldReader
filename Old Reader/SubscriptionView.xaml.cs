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
using System.Collections.ObjectModel;

using AppNs = Old_Reader;

using Old_Reader_Utils;

namespace Old_Reader
{
	public partial class SubscriptionView : PhoneApplicationPage, INotifyPropertyChanged, INotifyPropertyChanging
	{
		#region INotifyPropertyChange Members

		public event PropertyChangedEventHandler PropertyChanged;

		// Used to notify Silverlight that a property has changed.
		protected void NotifyPropertyChanged([CallerMemberName] String propertyName = "")
		{
			if (PropertyChanged != null)
			{
				PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
			}
		}

		public event PropertyChangingEventHandler PropertyChanging;

		// Used to notify Silverlight that a property has changed.
		protected void NotifyPropertyChanging([CallerMemberName] String propertyName = "")
		{
			if (PropertyChanging != null)
			{
				PropertyChanging(this, new PropertyChangingEventArgs(propertyName));
			}
		}
		#endregion

		private int m_JobsPending = 0;
		private void StartJob()
		{
			m_JobsPending++;
			trayProgress.IsVisible = m_JobsPending != 0;
			(ApplicationBar.Buttons[0] as ApplicationBarIconButton).IsEnabled = false;
			(ApplicationBar.Buttons[1] as ApplicationBarIconButton).IsEnabled = false;
		}

		private void JobComplete()
		{
			m_JobsPending--;
			m_JobsPending = m_JobsPending < 0 ? 0 : m_JobsPending;
			if (m_JobsPending == 0)
			{
				trayProgress.IsVisible = m_JobsPending != 0;
				(ApplicationBar.Buttons[0] as ApplicationBarIconButton).IsEnabled = true;
				(ApplicationBar.Buttons[1] as ApplicationBarIconButton).IsEnabled = true;
			}
		}

		public SubscriptionView()
		{
			InitializeComponent();

			this.DataContext = this;

			(ApplicationBar.Buttons[0] as ApplicationBarIconButton).Text = AppNs.Resources.AppResources.strMarkAllRead;
			(ApplicationBar.Buttons[1] as ApplicationBarIconButton).Text = AppNs.Resources.AppResources.strDownloadOlder;

			NoItemString = AppNs.Resources.AppResources.strNoUnreadItems;

			FeedItems = new ObservableCollection<DataModel.FeedItem>();
		}

		private DataModel.Subscription m_curSub;
		private DataModel.Tag m_CurTag;

		private ObservableCollection<DataModel.FeedItem> m_feedItems;
		public ObservableCollection<DataModel.FeedItem> FeedItems
		{
			get
			{
				return m_feedItems;
			}
			set
			{
				NotifyPropertyChanging();
				m_feedItems = value;
				NotifyPropertyChanged();
			}
		}

		private String m_PageTitle;
		public String PageTitle
		{
			get
			{
				return m_PageTitle;
			}
			set
			{
				NotifyPropertyChanging();
				m_PageTitle = value;
				NotifyPropertyChanged();
			}
		}

		private String m_NoItemString;
		public String NoItemString
		{
			get
			{
				return m_NoItemString;
			}
			set
			{
				NotifyPropertyChanging();
				m_NoItemString = value;
				NotifyPropertyChanged();
			}
		}


		private String m_SrcFeedId = "";
		protected override void OnNavigatedTo(System.Windows.Navigation.NavigationEventArgs e)
		{
			base.OnNavigatedTo(e);
			Analytics.GAnalytics.trackPageView("subscriptionView");

			if (String.IsNullOrEmpty(m_SrcFeedId))
			{
				NavigationContext.QueryString.TryGetValue("feedId", out m_SrcFeedId);

				if (!String.IsNullOrEmpty(m_SrcFeedId))
				{
					DataModel.Subscription curSub = AppNs.App.Contents.Subscriptions.FirstOrDefault(s => s.id == m_SrcFeedId);
					if (curSub != null)
					{
						m_curSub = curSub;
						PageTitle = curSub.title;
					}
					else
					{
						// this is not a subscription but a tag
						m_CurTag = AppNs.App.Contents.Tags.FirstOrDefault(t => t.id == m_SrcFeedId);
						PageTitle = m_CurTag.title;
					}
				}

				int nUnreadCount = 0;
				if (m_curSub != null)
				{
					if (m_curSub.unreadCount > 0)
					{
						nUnreadCount = m_curSub.unreadCount;
					}
					
				}
				else if (m_CurTag != null)
				{
					if (m_CurTag.unreadCount > 0)
					{
						nUnreadCount = m_CurTag.unreadCount;
					}
				}

				// just need to check if the cache has the information
				refreshFeedItems();
				if (nUnreadCount > 0)
				{
					// we already have all the unread items we want
					// need to get the items for this feed
					WS.Remoting rmGetFeeds = new WS.Remoting(FeedListCompleteForUnreadItems);
					rmGetFeeds.getUnreadItemsForSubscription(m_SrcFeedId, nUnreadCount);
					NoItemString = "";
					StartJob();
				}
			}
		}

		private void handleAppBarButton()
		{
			(ApplicationBar.Buttons[0] as ApplicationBarIconButton).IsEnabled = FeedItems.FirstOrDefault(fi => fi.isUnread) != null;

			(ApplicationBar.MenuItems[0] as ApplicationBarMenuItem).Text = AppNs.App.ShowRead ? AppNs.Resources.AppResources.strShowUnreadOnly : AppNs.Resources.AppResources.strShowAllPosts;
		}

		private void ApplicationBarMarkRead_Click(object sender, EventArgs e)
		{
			WS.Remoting rm = new WS.Remoting(markAllReadComplete);

			List<String> itemIds = new List<string>();
			foreach (var curFeedItem in FeedItems)
			{
				itemIds.Add(curFeedItem.id);
			}
			rm.markFeedItemsRead(itemIds, true);
			StartJob();
		}

		private void markAllReadComplete(String szResponse)
		{
			if (m_curSub != null)
			{
				DataStore.CachedFeed.markAllReadForContainer(m_curSub);
			}
			else if (FeedItems != null)
			{
				// no subscription still we have items.
				// it must be all items
				DataStore.CachedFeed.markAllReadForContainer(m_CurTag);
			}

			Dispatcher.BeginInvoke(() =>
			{
				JobComplete();
				// marked all as read
				foreach (var t in FeedItems)
				{
					t.setReadValSkipDB(false);
				}
				handleAppBarButton();
					
				// reduce the unread count
				if (m_curSub != null)
				{
					int curCount = m_curSub.unreadCount;
					// mark all items unread
					// adjust the count
					m_curSub.unreadCount = 0;
					if (m_curSub.categories != null)
					{
						foreach (DataModel.Tag curTag in m_curSub.categories)
						{
							curTag.unreadCount -= curCount;
						}
					}
					// remove the count from All items as well
					DataModel.Tag.AllItems.unreadCount -= curCount;
				}
				else if (FeedItems != null)
				{
					// no subscription still we have items.
					int curCount = m_CurTag.unreadCount;
					DataModel.Tag.AllItems.unreadCount -= curCount;
					foreach (var curSub in m_CurTag.Subscriptions)
					{
						curSub.unreadCount = 0;
					}
					m_CurTag.unreadCount = 0;
					if (m_CurTag.id == DataModel.Tag.AllItems.id)
					{
						foreach (DataModel.Tag temp in AppNs.App.Contents.Tags)
						{
							temp.unreadCount = 0;
						}
					}
				}
			});
		}

		private void GestureListener_Flick(object sender, FlickGestureEventArgs e)
		{
			if (Old_Reader_Utils.Utils.isSwipeHorizontalEnough(e))
			{
				Object dataCtx = (sender as System.Windows.Controls.StackPanel).DataContext;
				if (dataCtx != null && dataCtx is DataModel.FeedItem)
				{
					DataModel.FeedItem curItem = dataCtx as DataModel.FeedItem;
					curItem.toggleReadState();
					handleAppBarButton();
				}
			}
		}

		private void btnAddToFavorite_click(object sender, RoutedEventArgs e)
		{
			Object objDataSrc = (sender as Button).DataContext;
			if (objDataSrc is DataModel.FeedItem)
			{
				Analytics.GAnalytics.trackFeedSave(true);
				DataModel.FeedItem curFeedItem = objDataSrc as DataModel.FeedItem;
				DataStore.CachedFeed.toggleStarred(curFeedItem.id);
				curFeedItem.id = curFeedItem.id;
			}
		}

		private void ApplicationBarDownloadMoreIconButton_Click(object sender, EventArgs e)
		{
			int nMoreDownload = AppNs.App.AdditionalDownloadCount;
			String continuationId = "";

			if (m_curSub != null)
			{
				continuationId = DataStore.ContinuationId.getContinuationIdFor(m_curSub.id);
			}
			else
			{
				// this is all items
				continuationId = DataStore.ContinuationId.getContinuationIdFor(m_CurTag.id);
			}

			WS.Remoting rmGetFeeds = new WS.Remoting(FeedListComplete);
			rmGetFeeds.getMoreItemsForSubscription(m_SrcFeedId, nMoreDownload, continuationId);
			StartJob();
		}

		private void FeedListCompleteForUnreadItems(String szResponse)
		{
			/*
			 *  since we are getting all the unread items from the server mark any 
			 *  unread item from cached item set not in the new items read
			 * */

			String continuationId = "";
			List<DataModel.FeedItem> newFeedItems = DataModel.FeedItem.CreateFromResponse(szResponse, out continuationId);

			List<DataModel.FeedItem> itemsToRemove = new List<DataModel.FeedItem>();
			foreach (var existingItem in FeedItems)
			{
				if (existingItem.isUnread && !newFeedItems.Contains(existingItem))
				{
					itemsToRemove.Add(existingItem);
					existingItem.markRead(false);
				}
			}

			foreach(var curItem in itemsToRemove)
			{
				FeedItems.Remove(curItem);
			}

			handleNewItems(newFeedItems, continuationId);
		}

		private void FeedListComplete(String szResponse)
		{
			String continuationId = "";
			List<DataModel.FeedItem> newFeedItems = DataModel.FeedItem.CreateFromResponse(szResponse, out continuationId);

			handleNewItems(newFeedItems, continuationId);
		}

		void handleNewItems(List<DataModel.FeedItem> newFeedItems, String continuationId)
		{
			if (m_curSub != null)
			{
				DataStore.ContinuationId.setContinuationId(m_curSub.id, continuationId);
			}
			else
			{
				// this is all items
				DataStore.ContinuationId.setContinuationId(m_CurTag.id, continuationId);
			}

			AppNs.App.Contents.AddFeeds(newFeedItems.ToList());

			var merged = FeedItems.Union(newFeedItems);

			FeedItems = new ObservableCollection<DataModel.FeedItem>();

			foreach (var curFeedObj in merged.OrderByDescending(f => f.publishedTime))
			{
				FeedItems.Add(curFeedObj);
			}

			JobComplete(); 
			handleAppBarButton();
			
			NoItemString = AppNs.Resources.AppResources.strNoUnreadItems;

			// adjust the unread counts if needed
			if(m_CurTag!=null)
			{
				foreach(DataModel.Subscription curSub in m_CurTag.Subscriptions)
				{
					curSub.unreadCount = FeedItems.Count(cf => cf.isUnread && cf.origin.id == curSub.id);
				}
			}
			else if(m_curSub!=null)
			{
				m_curSub.unreadCount = FeedItems.Count(cf => cf.isUnread);
			}
		}

		private void ApplicationBarShowReadItemsMenuItem_Click(object sender, EventArgs e)
		{
			Analytics.GAnalytics.trackAppBarMenuItems(OldReaderTrackingConsts.showReadItems, AppNs.App.ShowRead ? 1 : 0);

			AppNs.App.ShowRead = !AppNs.App.ShowRead;
			refreshFeedItems();
		}

		private void refreshFeedItems()
		{
			int cachedUnreadCount = 0;
			
			List<DataModel.FeedItem> cachedFeeds = AppNs.App.Contents.getExistingFeedItemForFeedID(m_SrcFeedId, out cachedUnreadCount, AppNs.App.ShowRead);

			FeedItems = new ObservableCollection<DataModel.FeedItem>(cachedFeeds);

			handleAppBarButton();
		}

		private void feedList_Tap(object sender, System.Windows.Input.GestureEventArgs e)
		{
			if (feedList.SelectedIndex >= 0)
			{
				AppNs.App.FeedItems = new List<DataModel.FeedItem>();
				foreach (var t in FeedItems)
				{
					AppNs.App.FeedItems.Add(t);
				}
				NavigationService.Navigate(new Uri("/FeedView.xaml?selIdx=" + feedList.SelectedIndex, UriKind.Relative));
			}
		}
	}
}