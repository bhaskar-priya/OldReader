using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Data.Linq.Mapping;

#if OLD_READER_WP7
using Old_Reader_WP7;
#else
using Old_Reader;
#endif

namespace DataStore
{
	[Table]
	public class CachedFeed
	{
		[Column(IsPrimaryKey = true, CanBeNull = false, IsDbGenerated = false)]
		public String ID { get; set; }

		[Column(CanBeNull = false, IsDbGenerated = false, DbType = "ntext", UpdateCheck = UpdateCheck.Never)]
		public String Content { get; set; }

		[Column(CanBeNull = false, IsDbGenerated = false)]
		public String Author { get; set; }

		[Column(CanBeNull = false, IsDbGenerated = false)]
		public String href { get; set; }

		[Column(CanBeNull = false, IsDbGenerated = false)]
		public String Title { get; set; }

		[Column(CanBeNull = false, IsDbGenerated = false)]
		public DateTime publishedTime { get; set; }

		[Column(CanBeNull = false, IsDbGenerated = false)]
		public DateTime crawledtime { get; set; }

		[Column(CanBeNull = false, IsDbGenerated = false)]
		public String Categories { get; set; }

		[Column(CanBeNull = false, IsDbGenerated = false)]
		public String StreamId { get; set; }

		[Column(CanBeNull = false, IsDbGenerated = false)]
		public bool Unread { get; set; }

		[Column(CanBeNull = false, IsDbGenerated = false)]
		public bool dirty { get; set; }

		[Column(CanBeNull = false, IsDbGenerated = false)]
		public bool Starred { get; set; }

		public static CachedFeed fromFeedItem(DataModel.FeedItem feedItem)
		{
			CachedFeed cachced = new CachedFeed()
			{
				ID = feedItem.id,
				Content = feedItem.summary,
				Author = feedItem.author,
				href = feedItem.href,
				Title = feedItem.title,
				publishedTime = feedItem.publishedTime,
				crawledtime = feedItem.publishedTime,
				Unread = feedItem.isUnread,
				dirty = false,
				Starred = feedItem.Starred
			};

			if (feedItem.origin != null)
			{
				cachced.StreamId = feedItem.origin.id;
			}

			if (feedItem.tags != null)
			{
				foreach (var curTag in feedItem.tags)
				{
					cachced.Categories += curTag.id + "\n";
				}
			}

			return cachced;
		}

		public DataModel.FeedItem toFeedItem()
		{
			DataModel.FeedItem newFeedItem = new DataModel.FeedItem(this.ID, this.Content, this.Author, this.href,
				this.Title, this.Unread, this.publishedTime, this.Starred);

			foreach (var curCat in Categories.Split('\n'))
			{
				if (!String.IsNullOrEmpty(curCat))
				{
					DataModel.Tag newTag = App.Contents.Tags.FirstOrDefault(t => String.CompareOrdinal(t.id, curCat) == 0);
					if (newTag != null)
					{
						if (newFeedItem.tags == null)
						{
							newFeedItem.tags = new ObservableCollection<DataModel.Tag>();
						}
						newFeedItem.tags.Add(newTag);
					}
				}
			}

			newFeedItem.origin = App.Contents.Subscriptions.FirstOrDefault(s => String.CompareOrdinal(s.id, this.StreamId) == 0);

			return newFeedItem;
		}

		public static void toggleStarred(String szItemId)
		{
			CachedFeed cachedFeed = App.ReaderDB.CachedFeeds.FirstOrDefault(cf => cf.ID == szItemId);
			if (cachedFeed != null)
			{
				cachedFeed.Starred = !cachedFeed.Starred;
				App.ReaderDB.SubmitChanges();

				WS.Remoting rm = new WS.Remoting();
				rm.starItem(cachedFeed.ID, cachedFeed.Starred);
			}
		}

		public static bool isStarred(String szItemId)
		{
			CachedFeed cachedFeed = App.ReaderDB.CachedFeeds.FirstOrDefault(cf => cf.ID == szItemId);
			if (cachedFeed != null)
			{
				return cachedFeed.Starred;
			}
			return false;
		}

		public static void markAllRead()
		{
			foreach (var cachedFeed in App.ReaderDB.CachedFeeds.Where(cf => cf.Unread == true))
			{
				cachedFeed.Unread = false;
			}
			App.ReaderDB.SubmitChanges();
		}

		public static void markAllReadForSubscription(String szBubId)
		{
			foreach (var cachedFeed in App.ReaderDB.CachedFeeds.Where(
				cf => cf.Unread == true && cf.StreamId == szBubId))
			{
				cachedFeed.Unread = false;
			}
			App.ReaderDB.SubmitChanges();
		}

		public static implicit operator DataModel.FeedItem(CachedFeed f)  // implicit digit to byte conversion operator
		{
			return f.toFeedItem();
		}

	}
}
