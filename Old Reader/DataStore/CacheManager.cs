using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO.IsolatedStorage;

namespace DataStore
{
	public class CacheManager
	{
		const String tagFileName = "tagList.json";
		const String subscriptionFileName = "subscription.json";

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

		private static void SaveFile(String fileName, String data)
		{
			IsolatedStorageFileStream oFs = ISF.CreateFile(fileName);
			byte[] bytes = System.Text.Encoding.UTF8.GetBytes(data);
			oFs.Write(bytes, 0, bytes.Length);
			oFs.Close();
		}

		private static String ReadFile(String fileName)
		{
			IsolatedStorageFileStream iFs = ISF.OpenFile(fileName, System.IO.FileMode.Open);
			byte[] bytes = new byte[iFs.Length];
			iFs.Read(bytes, 0, (int)iFs.Length);
			iFs.Close();
			return System.Text.Encoding.UTF8.GetString(bytes, 0, bytes.Length);
		}
	}
}
