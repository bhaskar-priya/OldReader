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

		private ObservableCollection<Tag> m_categories;
		public ObservableCollection<Tag> categories
		{
			get
			{
				return m_categories;
			}
			set
			{
#if OLD_READER_WP7
				NotifyPropertyChanging("categories");
#else
				NotifyPropertyChanging();
#endif
				m_categories = value;
#if OLD_READER_WP7
				NotifyPropertyChanged("categories");
#else
				NotifyPropertyChanged();
#endif
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
#if OLD_READER_WP7
				NotifyPropertyChanging("url");
#else
				NotifyPropertyChanging();
#endif
				m_url = value;
#if OLD_READER_WP7
				NotifyPropertyChanged("url");
#else
				NotifyPropertyChanged();
#endif
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
#if OLD_READER_WP7
				NotifyPropertyChanging("htmlUrl");
#else
				NotifyPropertyChanging();
#endif
				m_htmlUrl = value;
#if OLD_READER_WP7
				NotifyPropertyChanged("htmlUrl");
#else
				NotifyPropertyChanged();
#endif
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
#if OLD_READER_WP7
				NotifyPropertyChanging("iconUrl");
#else
				NotifyPropertyChanging();
#endif
				m_iconUrl = value;
#if OLD_READER_WP7
				NotifyPropertyChanged("iconUrl");
#else
				NotifyPropertyChanged();
#endif
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
#if OLD_READER_WP7
				NotifyPropertyChanging("unreadCount");
#else
				NotifyPropertyChanging();
#endif
				m_unreadCount = value;
#if OLD_READER_WP7
				NotifyPropertyChanged("unreadCount");
#else
				NotifyPropertyChanged();
#endif
			}
		}

		public static ObservableCollection<Subscription> CreateFromResponse(String szResponse, ObservableCollection<Tag> tags)
		{
			ObservableCollection<Subscription > subscriptions = new ObservableCollection<Subscription>();

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
				foreach(JObject curCat in subCat)
				{
					if (subObj.categories == null)
					{
						subObj.categories = new ObservableCollection<Tag>();
					}
					Tag curTag = tags.Single(s => s.id == (String)curCat[OldReaderConsts.id]);
					if (curTag != null)
					{
						subObj.categories.Add(curTag);
						// add this subscription to the tag
						if (curTag.Subscriptions == null)
						{
							curTag.Subscriptions = new ObservableCollection<Subscription>();
						}
						curTag.Subscriptions.Add(subObj);
						curTag.title = (String)curCat[OldReaderConsts.label];
					}
				}

				subscriptions.Add(subObj);
			}

			return subscriptions;
		}
	}
}
