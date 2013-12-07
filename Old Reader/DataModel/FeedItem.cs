using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Newtonsoft.Json.Linq;
using System.Collections.ObjectModel;
using System.Text.RegularExpressions;
#if OLD_READER_WP7
using Old_Reader_WP7;
#else
using Old_Reader;
#endif
using Old_Reader_Utils;

namespace DataModel
{
	public class FeedItem : NotifyPropertyImpl
	{
		private String m_id;
		public String id
		{
			get
			{
				return m_id;
			}
			set
			{
#if OLD_READER_WP7
				NotifyPropertyChanging("id");
#else
				NotifyPropertyChanging();
#endif
				m_id = value;
#if OLD_READER_WP7
				NotifyPropertyChanged("id");
#else
				NotifyPropertyChanged();
#endif
			}
		}

		private String m_title;
		public String title
		{
			get
			{
				return m_title;
			}
			set
			{
#if OLD_READER_WP7
				NotifyPropertyChanging("title");
#else
				NotifyPropertyChanging();
#endif
				m_title = value;
#if OLD_READER_WP7
				NotifyPropertyChanged("title");
#else
				NotifyPropertyChanged();
#endif
			}
		}

		private String m_summary;
		public String summary
		{
			get
			{
				return m_summary;
			}
			set
			{
#if OLD_READER_WP7
				NotifyPropertyChanging("summary");
#else
				NotifyPropertyChanging();
#endif
				m_summary = value;
#if OLD_READER_WP7
				NotifyPropertyChanged("summary");
#else
				NotifyPropertyChanged();
#endif
			}
		}

		private String m_author;
		public String author
		{
			get
			{
				return m_author;
			}
			set
			{
#if OLD_READER_WP7
				NotifyPropertyChanging("author");
#else
				NotifyPropertyChanging();
#endif
				m_author = value;
#if OLD_READER_WP7
				NotifyPropertyChanged("author");
#else
				NotifyPropertyChanged();
#endif
			}
		}

		private String m_href;
		public String href
		{
			get
			{
				return m_href;
			}
			set
			{
#if OLD_READER_WP7
				NotifyPropertyChanging("href");
#else
				NotifyPropertyChanging();
#endif
				m_href = value;
#if OLD_READER_WP7
				NotifyPropertyChanged("href");
#else
				NotifyPropertyChanged();
#endif
			}
		}

		private ObservableCollection<Tag> m_tags;
		public ObservableCollection<Tag> tags
		{
			get
			{
				return m_tags;
			}
			set
			{
#if OLD_READER_WP7
				NotifyPropertyChanging("tags");
#else
				NotifyPropertyChanging();
#endif
				m_tags = value;
#if OLD_READER_WP7
				NotifyPropertyChanged("tags");
#else
				NotifyPropertyChanged();
#endif
			}
		}

		private bool m_isUnread;
		public bool isUnread
		{
			get
			{
				return m_isUnread;
			}
			set
			{
				if (m_isUnread != value)
				{
#if OLD_READER_WP7
					NotifyPropertyChanging("isUnread");
#else
					NotifyPropertyChanging();
#endif
					m_isUnread = value;

					// update the local db as well
					DataStore.CachedFeed cachedFeed = App.ReaderDB.CachedFeeds.FirstOrDefault(cf => cf.ID == this.id);
					if (cachedFeed != null && cachedFeed.Unread != m_isUnread)
					{
						cachedFeed.Unread = m_isUnread;
						App.ReaderDB.SubmitChanges();
					}
#if OLD_READER_WP7
					NotifyPropertyChanged("isUnread");
#else
					NotifyPropertyChanged();
#endif
				}
			}
		}
		public void setReadValSkipDB(bool value)
		{
			NotifyPropertyChanging("isUnread");
			m_isUnread = value;
			NotifyPropertyChanged("isUnread");
		}

		private Subscription m_origin;
		public Subscription origin
		{
			get
			{
				return m_origin;
			}
			set
			{
#if OLD_READER_WP7
				NotifyPropertyChanging("origin");
#else
				NotifyPropertyChanging();
#endif
				m_origin = value;
#if OLD_READER_WP7
				NotifyPropertyChanged("origin");
#else
				NotifyPropertyChanged();
#endif
			}
		}

		private bool m_keepUnread;
		public bool keepUnread
		{
			get
			{
				return m_keepUnread;
			}
			set
			{
#if OLD_READER_WP7
				NotifyPropertyChanging("keepUnread");
#else
				NotifyPropertyChanging();
#endif
				m_keepUnread = value;
#if OLD_READER_WP7
				NotifyPropertyChanged("keepUnread");
#else
				NotifyPropertyChanged();
#endif
			}
		}

		private string m_tagTitle = "";
		public String tagTitle
		{
			get
			{
				if (tags != null)
				{
					return tags[tags.Count - 1].title;
				}
				else if (!String.IsNullOrEmpty(m_tagTitle))
				{
					return m_tagTitle;
				}
				return "None";
			}
			set
			{
				m_tagTitle = value;
			}
		}

		private DateTime m_PublishedTime;
		public DateTime publishedTime
		{
			get
			{
				return m_PublishedTime;
			}
			set
			{
#if OLD_READER_WP7
				NotifyPropertyChanging("publishedTime");
#else
				NotifyPropertyChanging();
#endif
				m_PublishedTime = value;
#if OLD_READER_WP7
				NotifyPropertyChanged("publishedTime");
#else
				NotifyPropertyChanged();
#endif
			}
		}

		public bool Starred { get; set; }

		public FeedItem(String p_Id, String p_Content, String p_Author, String p_href, String p_Title,
			bool p_Unread, DateTime p_PublishedTime, bool bStarred)
		{
			m_id = p_Id;
			m_summary = p_Content;
			m_author = p_Author;
			m_href = p_href;
			m_title = p_Title;
			m_isUnread = p_Unread;
			m_PublishedTime = p_PublishedTime;
			Starred = bStarred;
		}

		public static List<FeedItem> CreateFromResponse(String szResponse, out String continuationId)
		{
			List<FeedItem> feedItems = new List<FeedItem>();

			JObject rootObj = JObject.Parse(szResponse);
			Regex rx = new Regex(@"&#x([\d]+);");
			Regex rxCyrillic = new Regex(@"\\u([0-9a-zA-Z]{4})");

			continuationId = (String)rootObj[OldReaderConsts.continuation];

			if (rootObj[OldReaderConsts.items] != null)
			{
				foreach (JObject curFeedObj in (JArray)rootObj[OldReaderConsts.items])
				{
					FeedItem newFeedItem = new FeedItem(
						getFeedID((string)curFeedObj[OldReaderConsts.id]),
						rxCyrillic.Replace((String)(((JObject)curFeedObj[OldReaderConsts.summary])[OldReaderConsts.content]), new MatchEvaluator(ConvertEscapedText)),
						(String)curFeedObj[OldReaderConsts.author],
						(String)(((JArray)curFeedObj[OldReaderConsts.canonical])[0][OldReaderConsts.href]),
						rx.Replace((String)curFeedObj[OldReaderConsts.title], new MatchEvaluator(ConvertEscapedText)),
						true, DateTime.Now, false);

					String strPublished = (string)curFeedObj[OldReaderConsts.published];
					String strCrawlTime = (String)curFeedObj[OldReaderConsts.crawlTimeMsec];

					int nPublished = int.Parse(strPublished);
					int nCrawled = (int)(Int64.Parse(strCrawlTime) / 1000);
					DateTime dtDateTime = new DateTime(1970,1,1,0,0,0,0);
					newFeedItem.publishedTime = dtDateTime.AddSeconds(nCrawled>nPublished?nCrawled:nPublished).ToLocalTime();

					JArray catArr = (JArray)curFeedObj[OldReaderConsts.categories];
					foreach (String curCatObj in catArr)
					{
						Tag newTag = App.Contents.Tags.FirstOrDefault(t => t.id == curCatObj);
						if (newTag != null)
						{
							if (newFeedItem.tags == null)
							{
								newFeedItem.tags = new ObservableCollection<Tag>();
							}
							newFeedItem.tags.Add(newTag);
						}
						if (curCatObj == readItemTag)
						{
							// this feed is already read
							newFeedItem.isUnread = false;
						}
						if (curCatObj == DataModel.Tag.StarredItems.id)
						{
							newFeedItem.Starred = true;
						}
					}

					String szOrigin = (String)((JObject)curFeedObj[OldReaderConsts.origin])[OldReaderConsts.streamId];
					newFeedItem.origin = App.Contents.Subscriptions.FirstOrDefault(s => s.id == szOrigin);

					feedItems.Add(newFeedItem);
				}
			}
			return feedItems;
		}

		static string ConvertEscapedText(Match m)
		{
			if(m.Groups.Count>1)
			{
#if OLD_READER_WP7
				return "" + Convert.ToChar(Convert.ToInt32(m.Groups[1].ToString(),16));
#else
				return "" + char.ConvertFromUtf32(Convert.ToInt32(m.Groups[1].ToString(),16));
#endif
			}
			return "";
		}

		private static String getFeedID(String Id)
		{
			String[] parts = Id.Split('/');
			return parts.Last();
		}

		public override int GetHashCode()
		{
			return id.GetHashCode();
		}

		public bool Equals(FeedItem otherFeed)
		{
			return id == otherFeed.id;
		}

		public override bool Equals(object obj)
		{
			// Try to cast the object to compare to to be a Person
			if (obj != null && obj is FeedItem)
			{
				return Equals(obj as FeedItem);
			}
			return false;
		}

		public void toggleReadState()
		{
			if (isUnread)
			{
				// mark as read in case its not to be kept unread
				if (!keepUnread)
				{
					markRead();
				}
			}
			else
			{
				markUnRead();
			}
		}

		public void markRead()
		{
			WS.Remoting rm = new WS.Remoting();
			rm.markFeedItemRead(id, true);
			
			if (isUnread && !keepUnread)
			{
				isUnread = false;
				foreach (DataModel.Tag curTag in tags)
				{
					curTag.unreadCount--;
					curTag.unreadCount = curTag.unreadCount < 0 ? 0 : curTag.unreadCount;
				}
				origin.unreadCount--;
				origin.unreadCount = origin.unreadCount < 0 ? 0 : origin.unreadCount;
			}
		}

		public void markUnRead()
		{
			if (!isUnread)
			{
				// mark this on unread in case it has been marked read
				isUnread = true;
				if (tags != null)
				{
					foreach (DataModel.Tag curTag in tags)
					{
						curTag.unreadCount++;
					}
				}
				if (origin != null)
				{
					origin.unreadCount++;
				}
				WS.Remoting rm = new WS.Remoting();
				rm.markFeedItemRead(id, false);
			}
		}

		public const String readItemTag = "user/-/state/com.google/read";
	}
}
