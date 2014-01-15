using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Newtonsoft.Json.Linq;
using System.Collections.ObjectModel;
#if OLD_READER_WP7
using Old_Reader_WP7;
#endif
using Old_Reader_Utils;

namespace DataModel
{
	public class Tag: NotifyPropertyImpl
	{
		public String id = "";
		
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

		private ObservableCollection<Subscription> m_Subscriptions;
		public ObservableCollection<Subscription> Subscriptions
		{
			get
			{
				return m_Subscriptions;
			}
			set
			{
#if OLD_READER_WP7
				NotifyPropertyChanging("Subscriptions");
#else
				NotifyPropertyChanging();
#endif
				m_Subscriptions = value;
#if OLD_READER_WP7
				NotifyPropertyChanged("Subscriptions");
#else
				NotifyPropertyChanged();
#endif
			}
		}

		private int m_UnreadCount;
		public int unreadCount
		{
			get
			{
				return m_UnreadCount;
			}
			set
			{
#if OLD_READER_WP7
				NotifyPropertyChanging("unreadCount");
#else
				NotifyPropertyChanging();
#endif
				m_UnreadCount = value;
#if OLD_READER_WP7
				NotifyPropertyChanged("unreadCount");
#else
				NotifyPropertyChanged();
#endif
			}
		}

		public String iconUrl
		{
			get
			{
				return "/Toolkit.Content/folder.png";
			}
		}

		public override string ToString()
		{
			return title;
		}

		public static ObservableCollection<Tag> CreateFromResponse(String szResponse)
		{
			ObservableCollection<Tag> tags = new ObservableCollection<Tag>();
			JObject obj = JObject.Parse(szResponse);
			JArray tagList = (JArray)obj[OldReaderConsts.tags];
			foreach (JObject curObj in tagList)
			{
				Tag curTag=new Tag() { id = (String)curObj[OldReaderConsts.id], title = "" };
				if (curTag.id != StarredItems.id)
				{
					tags.Add(curTag);
				}
			}
			return tags;
		}

		public static Tag AllItems = new Tag()
		{
			id = Old_Reader_Utils.OldReaderConsts.allItemsId,
			title = "All Items",
			unreadCount = 0,
			Subscriptions = null
		};

		public static Tag StarredItems = new Tag()
		{
			id = Old_Reader_Utils.OldReaderConsts.starredItemId,
			title = "Starred",
			unreadCount = 0,
			Subscriptions = null
		};
	}
}
