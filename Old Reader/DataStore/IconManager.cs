using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO.IsolatedStorage;
using System.Net;
using System.IO;

namespace DataStore
{
	class IconManager
	{
		private String m_szHost;

		public delegate void IconAvailable(String szPath);
		private IconAvailable iconAvailableHandler = null;

		public IconManager(String szUrl, IconAvailable availHandler)
		{
			Uri uri = new Uri(szUrl);
			m_szHost = uri.Host;

			iconAvailableHandler = availHandler;
		}

		static List<String> enqueuedFiles = new List<string>();

		public void Get()
		{
			String szIconPath = getIconFileNameForHost(m_szHost);
			bool bDoDownload = false;
			lock (enqueuedFiles)
			{
				if (!enqueuedFiles.Contains(szIconPath))
				{
					enqueuedFiles.Add(szIconPath);
					bDoDownload = true;
				}
			}

			if (bDoDownload)
			{
				IsolatedStorageFile isoStore = IsolatedStorageFile.GetUserStoreForApplication();
				if (!isoStore.FileExists(szIconPath))
				{
					String iconPngUrl = String.Format(@"http://www.google.com/s2/favicons?domain={0}", m_szHost);

					HttpWebRequest request = (HttpWebRequest)HttpWebRequest.Create(iconPngUrl);
					request.BeginGetResponse(GetImageCallback, request);
				}
				else
				{
					iconAvailableHandler(szIconPath);
				}
			}
			iconAvailableHandler(szIconPath);
		}

		public static String getIconFileNameForHost(String szHost)
		{
			IsolatedStorageFile isoStore = IsolatedStorageFile.GetUserStoreForApplication();
			if (!isoStore.DirectoryExists("favIcons"))
			{
				isoStore.CreateDirectory("favIcons");
			}

			return String.Format(@"favIcons\{0}.png", szHost);
		}

		private void GetImageCallback(IAsyncResult callbackResult)
		{
			try
			{
				HttpWebRequest request = callbackResult.AsyncState as HttpWebRequest;

				WebResponse response = request.EndGetResponse(callbackResult);

				Stream s = response.GetResponseStream();
				
				// read the png file and write to the isolated storage
				IsolatedStorageFile isoStore = IsolatedStorageFile.GetUserStoreForApplication();
				String szIconPath = getIconFileNameForHost(m_szHost);
				if (!isoStore.FileExists(szIconPath))
				{
					IsolatedStorageFileStream oFs = isoStore.CreateFile(szIconPath);
					byte[] buffer = new byte[512];
					int nReadCount = 0;
					do
					{
						nReadCount = s.Read(buffer, 0, 512);
						if (nReadCount > 0)
						{
							oFs.Write(buffer, 0, nReadCount);
						}
					} while (nReadCount > 0);
				}
				iconAvailableHandler(szIconPath);
			}
			catch
			{
			}
		}
	}
}
