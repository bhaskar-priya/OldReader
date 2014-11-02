using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Newtonsoft.Json.Linq;
using System.Collections.ObjectModel;
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
				NotifyPropertyChanging();
				m_title = value;
				NotifyPropertyChanged();
			}
		}

		private ObservableCollection<Subscription> m_Subscriptions;
		public ObservableCollection<Subscription> Subscriptions
		{
			get
			{
				if (m_Subscriptions == null)
				{
					m_Subscriptions = new ObservableCollection<Subscription>();
				}
				return m_Subscriptions;
			}
			set
			{
				NotifyPropertyChanging();
				m_Subscriptions = value;
				NotifyPropertyChanged();
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
				NotifyPropertyChanging();
				m_UnreadCount = value > 0 ? value : 0;
				NotifyPropertyChanged();
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
			try
			{
				JObject obj = JObject.Parse(szResponse);
				JArray tagList = (JArray)obj[OldReaderConsts.tags];
				foreach (JObject curObj in tagList)
				{
					Tag curTag = new Tag() { id = (String)curObj[OldReaderConsts.id], title = "" };
					if (curTag.id != StarredItems.id)
					{
						tags.Add(curTag);
					}
				}
			}
			catch
			{
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
