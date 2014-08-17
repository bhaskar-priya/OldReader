using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Collections.ObjectModel;
using System.IO.IsolatedStorage;

using Old_Reader;
using Old_Reader_Utils;

namespace DataModel
{
	public class OldReaderContents
	{
		public enum TInitializationStates
		{
			kGettingTagList,
			kGettingSubscription,
			kGettingUnreadCount,
			kGettingUnreadItems,
			kGettingStarredItems
		};

		public delegate void InitializationCompleteHandler(OldReaderContents contents);
		private InitializationCompleteHandler initializationCompleteHandler = null;

		public delegate void InitializationErrorHandler(String szErrorMessage);
		private InitializationErrorHandler initializationErrorHandler = null;

		public delegate void InitializationStatusReceiver(TInitializationStates curstate);
		private InitializationStatusReceiver initializationStatusReceiver = null;

		public ObservableCollection<Tag> Tags
		{
			get;
			set;
		}

		public ObservableCollection<Subscription> Subscriptions
		{
			get;
			set;
		}

		public OldReaderContents()
		{
			try
			{
				String szTagData = DataStore.CacheManager.TagData;
				String szSubData = DataStore.CacheManager.SubscriptionData;
				if (!String.IsNullOrEmpty(szTagData) && !String.IsNullOrEmpty(szSubData))
				{
					Tags = DataModel.Tag.CreateFromResponse(szTagData);
					if (App.AllItemsAtTop)
					{
						Tags.Insert(0, Tag.AllItems);
					}
					else
					{
						Tags.Add(Tag.AllItems);
					}
					Subscriptions = Subscription.CreateFromResponse(szSubData, Tags);
				}
			}
			catch
			{
			}
		}

		public void AddFeeds(List<FeedItem> newFeedItems)
		{
			// save the feeds in the db
			foreach (var newFeedItem in newFeedItems)
			{
				if (App.ReaderDB.CachedFeeds.FirstOrDefault(cf => cf.ID == newFeedItem.id) == null)
				{
					DataStore.CachedFeed cachedFeed = DataStore.CachedFeed.fromFeedItem(newFeedItem);
					App.ReaderDB.CachedFeeds.InsertOnSubmit(cachedFeed);
				}
			}
			App.ReaderDB.SubmitChanges();
		}

		public List<FeedItem> getExistingFeedItemForFeedID(String szFeedId, out int unreadCount, bool bShowRead)
		{
			IEnumerable<DataStore.CachedFeed> src = null;
			if (szFeedId != Tag.AllItems.id)
			{
				if (bShowRead)
				{
					src = App.ReaderDB.CachedFeeds.Where(cf => cf.StreamId == szFeedId).OrderByDescending(cf => cf.publishedTime);
				}
				else
				{
					src = App.ReaderDB.CachedFeeds.Where(cf => cf.StreamId == szFeedId && cf.Unread == true).OrderByDescending(cf => cf.publishedTime);
				}
				if (src.Count() == 0)
				{
					if (bShowRead)
					{
						src = App.ReaderDB.CachedFeeds.Where(cf => cf.Categories.Contains(szFeedId)).OrderByDescending(cf => cf.publishedTime);
					}
					else
					{
						src = App.ReaderDB.CachedFeeds.Where(cf => cf.Categories.Contains(szFeedId) && cf.Unread == true).OrderByDescending(cf => cf.publishedTime);
					}
				}
			}
			else
			{
				if (bShowRead)
				{
					src = App.ReaderDB.CachedFeeds.OrderByDescending(cf => cf.publishedTime);
				}
				else
				{
					src = App.ReaderDB.CachedFeeds.Where(cf => cf.Unread == true).OrderByDescending(cf => cf.publishedTime);
				}
			}

			List<DataModel.FeedItem> cachedFeeds = new List<FeedItem>();

			unreadCount = 0;
			foreach (var curCachedFeed in src)
			{
				if (curCachedFeed.Unread)
				{
					unreadCount++;
				}
				cachedFeeds.Add(curCachedFeed.toFeedItem());
			}

			return cachedFeeds;
		}

		private void remotingError(String szErrorMessage)
		{
			if (initializationErrorHandler != null)
			{
				Analytics.GAnalytics.trackRemotingErrorEvent(szErrorMessage);
				initializationErrorHandler(szErrorMessage);
			}
		}

		public void Initialize(InitializationCompleteHandler handler, InitializationErrorHandler errorHandler, InitializationStatusReceiver receiver)
		{
			initializationCompleteHandler = handler;
			initializationErrorHandler = errorHandler;
			initializationStatusReceiver = receiver;

			App.Contents = this;
			//tagListComplete(DataStore.CacheManager.TagData);
			initializationStatusReceiver(TInitializationStates.kGettingTagList);
			WS.Remoting rm = new WS.Remoting(App.CurrentService, tagListComplete, remotingError);
			rm.getTagList();
		}

		private static IsolatedStorageFile ISF = IsolatedStorageFile.GetUserStoreForApplication();

		private void tagListComplete(String szResponse)
		{
			DataStore.CacheManager.TagData = szResponse;

			Tags = DataModel.Tag.CreateFromResponse(szResponse);

			if (App.AllItemsAtTop)
			{
				Tags.Insert(0, Tag.AllItems);
			}
			else
			{
				Tags.Add(Tag.AllItems);
			}

			Analytics.GAnalytics.trackOldReaderEvent(OldReaderTrackingConsts.tagListComplete, Tags.Count);

			//subscriptionComplete(DataStore.CacheManager.SubscriptionData);
			// get the subscriptions
			initializationStatusReceiver(TInitializationStates.kGettingSubscription);
			WS.Remoting rm = new WS.Remoting(App.CurrentService, subscriptionComplete, remotingError);
			rm.getSubscriptionList();
		}

		private void subscriptionComplete(String szResponse)
		{
			DataStore.CacheManager.SubscriptionData = szResponse;

			Subscriptions = Subscription.CreateFromResponse(szResponse, Tags);

			Analytics.GAnalytics.trackOldReaderEvent(OldReaderTrackingConsts.subscriptionComplete, Subscriptions.Count);

			initializationStatusReceiver(TInitializationStates.kGettingUnreadCount);
			WS.Remoting rm = new WS.Remoting(App.CurrentService, unreadCountComplete, remotingError);
			rm.getUnreadCount();
		}

		private void unreadCountComplete(String szResponse)
		{
			Tag.AllItems.unreadCount = Utils.handleUnreadCounts(szResponse, Subscriptions, Tags);

			Analytics.GAnalytics.trackOldReaderEvent(OldReaderTrackingConsts.unreadCountComplete, Tag.AllItems.unreadCount);

			//initializationStatusReceiver(TInitializationStates.kGettingUnreadItems);
			//WS.Remoting rm = new WS.Remoting(AllUnreadItemsComplete, remotingError);
			//rm.getAllUnreadItems(Tag.AllItems.unreadCount);

			initializationCompleteHandler(this);
		}

		private void AllUnreadItemsComplete(String szResponse)
		{
			//FeedItems = DataModel.FeedItem.CreateFromResponse(szResponse);
			initializationCompleteHandler(this);
		}

		public ObservableCollection<Object> DisplayObjects
		{
			get
			{
				ObservableCollection<Object> dispObjects = new ObservableCollection<object>();

				if (Subscriptions != null && Tags != null)
				{
					// add the all objects
					foreach (Subscription subObj in Subscriptions.Where(s => s.categories == null || s.categories.Count == 0))
					{
						dispObjects.Add(subObj);
					}

					foreach (var tagObj in Tags)
					{
						dispObjects.Add(tagObj);
					}
				}

				return dispObjects;
			}
		}
	}
}
