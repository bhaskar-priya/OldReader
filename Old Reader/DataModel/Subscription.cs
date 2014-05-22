using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Newtonsoft.Json.Linq;
using System.Collections.ObjectModel;
using Old_Reader_Utils;

namespace DataModel
{
	public class Subscription : NotifyPropertyImpl
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

		private ObservableCollection<Tag> m_categories;
		public ObservableCollection<Tag> categories
		{
			get
			{
				if (m_categories == null)
				{
					m_categories = new ObservableCollection<Tag>();
				}
				return m_categories;
			}
			set
			{
				NotifyPropertyChanging();
				m_categories = value;
				NotifyPropertyChanged();
			}
		}

		private String m_url;
		public String url
		{
			get
			{
				return m_url;
			}
			set
			{
				NotifyPropertyChanging();
				m_url = value;
				NotifyPropertyChanged();
			}
		}

		private String m_htmlUrl;
		public String htmlUrl
		{
			get
			{
				return m_htmlUrl;
			}
			set
			{
				NotifyPropertyChanging();
				m_htmlUrl = value;
				NotifyPropertyChanged();
			}
		}

		private void IconAvailable(String szPath)
		{
			iconUrl = String.Format(@"isostore:/{0}", szPath.Replace(@"\", "/"));
		}

		private String m_iconUrl;
		public String iconUrl
		{
			get
			{
				return m_iconUrl;
			}
			set
			{
				NotifyPropertyChanging();
				m_iconUrl = value;
				NotifyPropertyChanged();
			}
		}

		private void resetIconPath()
		{
			// start the getting of icon
			DataStore.IconManager iconManager = new DataStore.IconManager(m_htmlUrl, IconAvailable);
			iconManager.Get();
		}

		private int m_unreadCount;
		public int unreadCount
		{
			get
			{
				return m_unreadCount;
			}
			set
			{
				NotifyPropertyChanging();
				m_unreadCount = value > 0 ? value : 0;
				NotifyPropertyChanged();
			}
		}

		public static ObservableCollection<Subscription> CreateFromResponse(String szResponse, ObservableCollection<Tag> tags)
		{
			ObservableCollection<Subscription > subscriptions = new ObservableCollection<Subscription>();

			try
			{
				JObject rootObj = JObject.Parse(szResponse);
				JArray subscriptionList = (JArray)rootObj[OldReaderConsts.subscriptions];
				foreach (JObject curObj in subscriptionList)
				{
					Subscription subObj = new Subscription()
					{
						id = (String)curObj[OldReaderConsts.id],
						title = (String)curObj[OldReaderConsts.title],
						url = (String)curObj[OldReaderConsts.url],
						htmlUrl = (String)curObj[OldReaderConsts.htmlUrl],
						iconUrl = (String)curObj[OldReaderConsts.iconUrl]
					};
					subObj.resetIconPath();
					// get the categories
					JArray subCat = (JArray)curObj[OldReaderConsts.categories];
					foreach (JObject curCat in subCat)
					{
						Tag curTag = null;
						try
						{
							curTag = tags.Single(s => s.id == (String)curCat[OldReaderConsts.id]);
						}
						catch { }
						if (curTag != null)
						{
							subObj.categories.Add(curTag);
							// add this subscription to the tag
							curTag.Subscriptions.Add(subObj);
							curTag.title = (String)curCat[OldReaderConsts.label];
						}
					}
					Tag.AllItems.Subscriptions.Add(subObj);
					subscriptions.Add(subObj);
				}
			}
			catch
			{
			}

			return subscriptions;
		}
	}
}
