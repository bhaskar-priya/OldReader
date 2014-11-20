using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO.IsolatedStorage;
using System.Xml.Serialization;
using System.IO;

namespace DataStore
{
	public class CacheManager
	{
		const String tagFileName = "tagList.json";
		const String subscriptionFileName = "subscription.json";
		const String unreadCountFileName = "unreadCount.xml";

		private static IsolatedStorageFile ISF = IsolatedStorageFile.GetUserStoreForApplication();

		public static String TagData
		{
			get
			{
				return ReadFile(tagFileName);
			}
			set
			{
				SaveFile(tagFileName, value);
			}
		}

		public static String SubscriptionData
		{
			get
			{
				return ReadFile(subscriptionFileName);
			}
			set
			{
				SaveFile(subscriptionFileName, value);
			}
		}

		public class IdCount
		{
			public IdCount()
			{ }
			public IdCount(String p_id,int p_unreadCount)
			{
				id = p_id;
				unreadCount = p_unreadCount;
			}
			public String id;
			public int unreadCount;
		}

		public static void serializeMainContentUnreadCounts(DataModel.OldReaderContents content)
		{
			using (IsolatedStorageFileStream oFs = ISF.CreateFile(unreadCountFileName))
			{
				List<IdCount> unreadCounts = new List<IdCount>();
				foreach(var curSub in content.Subscriptions)
				{
					unreadCounts.Add(new IdCount(curSub.id, curSub.unreadCount));
				}

				using (StreamWriter writer = new StreamWriter(oFs))
				{
					XmlSerializer ser = new XmlSerializer(unreadCounts.GetType());
					ser.Serialize(writer, unreadCounts);
					writer.Close();
				}
				oFs.Close();
			}
		}

		public static void deSerializeMainContentUnreadCounts(DataModel.OldReaderContents content)
		{
			try
			{
				using (IsolatedStorageFileStream iFs = ISF.OpenFile(unreadCountFileName, System.IO.FileMode.Open))
				{
					using(StreamReader reader = new StreamReader(iFs))
					{
						XmlSerializer ser = new XmlSerializer(typeof(List<IdCount>));
						List<IdCount> unreadCounts = (List<IdCount>)ser.Deserialize(reader);

						foreach(var unreadId in unreadCounts)
						{
							DataModel.Subscription curSub = content.Subscriptions.FirstOrDefault(s => s.id == unreadId.id);
							if (curSub != null)
							{
								curSub.unreadCount = unreadId.unreadCount;
								foreach (var curCat in curSub.categories)
								{
									curCat.unreadCount += curSub.unreadCount;
								}
								DataModel.Tag.AllItems.unreadCount += curSub.unreadCount;
							}
						}
						reader.Close();
					}
					iFs.Close();
				}
			}
			catch { }
		}

		private static void SaveFile(String fileName, String data)
		{
			using (IsolatedStorageFileStream oFs = ISF.CreateFile(fileName))
			{
				byte[] bytes = System.Text.Encoding.UTF8.GetBytes(data);
				oFs.Write(bytes, 0, bytes.Length);
				oFs.Close();
			}
		}

		private static String ReadFile(String fileName)
		{
			try
			{
				using (IsolatedStorageFileStream iFs = ISF.OpenFile(fileName, System.IO.FileMode.Open))
				{
					byte[] bytes = new byte[iFs.Length];
					iFs.Read(bytes, 0, (int)iFs.Length);
					iFs.Close();
					return System.Text.Encoding.UTF8.GetString(bytes, 0, bytes.Length);
				}
			}
			catch { }
			return "";
		}
	}
}
