using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Newtonsoft.Json.Linq;
using System.Collections.ObjectModel;
using System.Text.RegularExpressions;
using Old_Reader;
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
				NotifyPropertyChanging();
				m_id = value;
				NotifyPropertyChanged();
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
				NotifyPropertyChanging();
				m_title = value;
				NotifyPropertyChanged();
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
				NotifyPropertyChanging();
				m_summary = value;
				NotifyPropertyChanged();
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
				NotifyPropertyChanging();
				m_author = value;
				NotifyPropertyChanged();
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
				NotifyPropertyChanging();
				m_href = value;
				NotifyPropertyChanged();
			}
		}

		private ObservableCollection<Tag> m_tags;
		public ObservableCollection<Tag> tags
		{
			get
			{
				if (m_tags == null)
				{
					m_tags = new ObservableCollection<Tag>();
				}
				return m_tags;
			}
			set
			{
				NotifyPropertyChanging();
				m_tags = value;
				NotifyPropertyChanged();
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
					NotifyPropertyChanging();
					m_isUnread = value;

					// update the local db as well
					DataStore.CachedFeed cachedFeed = App.ReaderDB.CachedFeeds.FirstOrDefault(cf => cf.ID == this.id);
					if (cachedFeed != null && cachedFeed.Unread != m_isUnread)
					{
						cachedFeed.Unread = m_isUnread;
						App.ReaderDB.SubmitChanges();
					}
					NotifyPropertyChanged();
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
				NotifyPropertyChanging();
				m_origin = value;
				NotifyPropertyChanged();
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
				NotifyPropertyChanging();
				m_keepUnread = value;
				NotifyPropertyChanged();
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
				NotifyPropertyChanging();
				m_PublishedTime = value;
				NotifyPropertyChanged();
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

			try
			{
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
						DateTime dtDateTime = new DateTime(1970, 1, 1, 0, 0, 0, 0);
						newFeedItem.publishedTime = dtDateTime.AddSeconds(nCrawled > nPublished ? nCrawled : nPublished).ToLocalTime();

						JArray catArr = (JArray)curFeedObj[OldReaderConsts.categories];
						foreach (String curCatObj in catArr)
						{
							Tag newTag = App.Contents.Tags.FirstOrDefault(t => t.id == curCatObj);
							if (newTag != null)
							{
								newFeedItem.tags.Add(newTag);
							}
							if (curCatObj == Old_Reader_Utils.OldReaderConsts.readItemTag)
							{
								// this feed is already read
								newFeedItem.isUnread = false;
							}
							if (curCatObj == DataModel.Tag.StarredItems.id)
							{
								newFeedItem.Starred = true;
							}
						}

						try
						{
							String szOrigin = (String)((JObject)curFeedObj[OldReaderConsts.origin])[OldReaderConsts.streamId];
							newFeedItem.origin = App.Contents.Subscriptions.FirstOrDefault(s => s.id == szOrigin);
						}
						catch (NullReferenceException)
						{
							newFeedItem.origin = null;
						}

						feedItems.Add(newFeedItem);
					}
				}
			}
			catch
			{
				continuationId = "";
			}

			return feedItems;
		}

		static string ConvertEscapedText(Match m)
		{
			if(m.Groups.Count>1)
			{
				try
				{
					return "" + char.ConvertFromUtf32(Convert.ToInt32(m.Groups[1].ToString(), 16));
				}
				catch
				{
					return m.Groups[1].ToString();
				}
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
	}
}
