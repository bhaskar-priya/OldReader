using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.IO;

namespace WS
{
	public class Remoting
	{
		public enum TService
		{
			kTheOldReader,
			kBazqux
		};

		private static String szAPIEndPoint = "http://theoldreader.com/reader/api/0/";

		private static String szBazLoginEndpoint = "https://www.bazqux.com/";
		private static String szBazEndPoint = "https://www.bazqux.com/reader/api/0/";

		public delegate void RemotingComplete(String szResponse);
		private RemotingComplete remotingCompleteHandler = null;

		public delegate void RemotingError(String szErrorMsg);
		private RemotingError remotingErrorHandler = null;

		private TService m_ServiceId;

		public Remoting(TService serviceId)
			: this(serviceId, null, null)
		{
		}

		public Remoting(TService serviceId, RemotingComplete completeHandler)
			: this(serviceId, completeHandler, null)
		{
		}

		public Remoting(TService serviceId, RemotingComplete completeHandler, RemotingError errorHandler)
		{
			remotingCompleteHandler = completeHandler;
			remotingErrorHandler = errorHandler;
			m_ServiceId = serviceId;
		}

		#region HTTP Details
		private static String authHeader = "";

		public static String token
		{
			set
			{
				authHeader = "GoogleLogin auth=" + value;
			}
		}

		private String m_szPostData = "";

		private void Post(String szURI,String szPostData)
		{
			m_szPostData = szPostData;

			System.Uri myUri = new System.Uri(szURI);
			HttpWebRequest myRequest = (HttpWebRequest)HttpWebRequest.Create(myUri);
			myRequest.Method = "POST";
			if (!String.IsNullOrEmpty(authHeader))
			{
				myRequest.Headers[HttpRequestHeader.Authorization] = authHeader;
			}
			myRequest.ContentType = "application/x-www-form-urlencoded";
			myRequest.BeginGetRequestStream(new AsyncCallback(GetRequestStreamCallback), myRequest);
		}

		void GetRequestStreamCallback(IAsyncResult callbackResult)
		{
			HttpWebRequest myRequest = (HttpWebRequest)callbackResult.AsyncState;
			// End the stream request operation
			Stream postStream = myRequest.EndGetRequestStream(callbackResult);

			// Create the post data
			byte[] byteArray = Encoding.UTF8.GetBytes(m_szPostData);

			// Add the post data to the web request
			postStream.Write(byteArray, 0, byteArray.Length);
			postStream.Close();

			// Start the web request
			myRequest.BeginGetResponse(new AsyncCallback(GetResponsetStreamCallback), myRequest);
		}

		void GetResponsetStreamCallback(IAsyncResult callbackResult)
		{
			try
			{
				HttpWebRequest request = (HttpWebRequest)callbackResult.AsyncState;
				HttpWebResponse response = (HttpWebResponse)request.EndGetResponse(callbackResult);
				using (StreamReader httpWebStreamReader = new StreamReader(response.GetResponseStream()))
				{
					string result = httpWebStreamReader.ReadToEnd();
					if (remotingCompleteHandler != null)
					{
						remotingCompleteHandler(result);
					}
				}
			}
			catch (Exception exp)
			{
#if !BACK_AGENT
				Analytics.GAnalytics.sendException(exp.Message, false);
#endif
				if (remotingErrorHandler != null)
				{
					remotingErrorHandler(exp.Message);
				}
			}
		}

		private void Get(String szURI)
		{
			Uri uri = new Uri(szURI);
			WebClient client = new WebClient();
			client.Headers[HttpRequestHeader.Authorization] = authHeader;
			client.DownloadStringCompleted += StringDownloadComplete;
			client.DownloadStringAsync(uri);
		}

		private void StringDownloadComplete(object sender, DownloadStringCompletedEventArgs e)
		{
			if (e.Error == null)
			{
				String Result = e.Result;
				if (remotingCompleteHandler != null)
				{
					remotingCompleteHandler(Result);
				}
			}
			else if (remotingErrorHandler != null)
			{
				remotingErrorHandler(e.Error.Message);
			}
		}
		#endregion

		private String GetApiEndPoint(bool loginApi = false)
		{
			if (m_ServiceId == TService.kTheOldReader)
			{
				return szAPIEndPoint;
			}
			else if (m_ServiceId == TService.kBazqux)
			{
				if (loginApi)
				{
					return szBazLoginEndpoint;
				}
				else
				{
					return szBazEndPoint;
				}
			}
			return szAPIEndPoint;
		}

		public void Login(String szEMail, String szPassword)
		{
			String szLoginAPI = "accounts/ClientLogin";
			String szURI = GetApiEndPoint(true) + szLoginAPI;
			
			Post(szURI, String.Format("client=WPOldReader&accountType=HOSTED_OR_GOOGLE&service=reader&Email={0}&Passwd={1}", szEMail, szPassword));
		}

		public void getTagList()
		{
			String szAPI = "tag/list?output=json";
			String szURI = GetApiEndPoint() + szAPI;

			Get(szURI);
		}

		public void getSubscriptionList()
		{
			String szAPI = "subscription/list?output=json";
			String szURI = GetApiEndPoint() + szAPI;

			Get(szURI);
		}

		public void getUnreadCount()
		{
			String szAPI = "unread-count?output=json";
			String szURI = GetApiEndPoint() + szAPI;

			Get(szURI);
		}

		public void getUserInfo()
		{
			String szAPI = "user-info?output=json";
			String szURI = GetApiEndPoint() + szAPI;

			Get(szURI);
		}

		public void getMoreItemsForSubscription(String szSubName, int nItemCount, String nContinuation)
		{
			String szURI = String.Format("{0}stream/contents?output=json&n={2}&c={3}&s={1}", GetApiEndPoint(), szSubName, nItemCount, nContinuation);
			Get(szURI);
		}

		public void getItemsForSubscription(String szSubName, int nItemCount)
		{
			String szURI = String.Format("{0}stream/contents?output=json&n={2}&s={1}", GetApiEndPoint(), szSubName, nItemCount);
			Get(szURI);
		}

		public void getUnreadItemsForSubscription(String szSubName, int nItemCount)
		{
			String szURI = null;
			if (m_ServiceId == TService.kTheOldReader)
			{
				szURI = String.Format("{0}stream/contents?output=json&xt=user/-/state/com.google/read&n={2}&s={1}", GetApiEndPoint(), szSubName, nItemCount);
			}
			else if (m_ServiceId == TService.kBazqux)
			{
				szURI = String.Format("{0}stream/contents?output=json&n={2}&s={1}", GetApiEndPoint(), szSubName, nItemCount);
			}
			if (szURI != null)
			{
				Get(szURI);
			}
		}

		public void getAllUnreadItems(int nItemCount)
		{
			String szURI = String.Format("{0}stream/contents?output=json&xt=user/-/state/com.google/read&n={2}&s={1}", GetApiEndPoint(), "user/-/state/com.google/reading-list", nItemCount);
			Get(szURI);
		}

		public void markFeedItemRead(String szItemId, bool bRead)
		{
			String szPostData = String.Format("{0}=user/-/state/com.google/read&i={1}", bRead ? "a" : "r", szItemId);

			Post(GetApiEndPoint() + "edit-tag", szPostData);
		}

		public void markFeedItemsRead(List<String> szItemIds, bool bRead)
		{
			changeTagOfItems(Old_Reader_Utils.OldReaderConsts.readItemTag, szItemIds, bRead);
		}

		public void markAllItemsAsRead(String szFeedId)
		{
			String szPostData = String.Format("s={0}", szFeedId);

			Post(GetApiEndPoint() + "mark-all-as-read", szPostData);
		}

		public void starItem(String szFeedId, bool bStar)
		{
			String szPostData = String.Format("{0}={2}&i={1}", bStar ? "a" : "r", szFeedId, Old_Reader_Utils.OldReaderConsts.starredItemId);

			Post(GetApiEndPoint() + "edit-tag", szPostData);
		}

		public void starItems(List<String> szFeedIds, bool bStar)
		{
			changeTagOfItems(Old_Reader_Utils.OldReaderConsts.starredItemId, szFeedIds, bStar);
		}

		private void changeTagOfItems(String szTagName, List<String> szFeedIds, bool bAdd)
		{
			if (szFeedIds != null && szFeedIds.Count > 0)
			{
				StringBuilder sb = new StringBuilder();
				sb.AppendFormat("{0}={1}", bAdd ? "a" : "r", szTagName);

				foreach (String curItemId in szFeedIds)
				{
					sb.AppendFormat("&i={0}", curItemId);
				}

				String szPostData = sb.ToString();

				Post(GetApiEndPoint() + "edit-tag", szPostData);
			}
		}

		public void addSubscription(String szFeedUrl)
		{
			String szPostData = "";

			Post(GetApiEndPoint() + "subscription/quickadd?quickadd=" + szFeedUrl, szPostData);
		}

		public void moveSubscriptionToFolder(String feedId, String folderId)
		{
			String szPostData = "";
			if (!String.IsNullOrEmpty(folderId) && folderId != Old_Reader_Utils.OldReaderConsts.allItemsId)
			{
				szPostData = String.Format("ac=edit&s={0}&a={1}", feedId, folderId);
			}
			else
			{
				szPostData = String.Format("ac=edit&s={0}&r={1}", feedId, folderId);
			}

			Post(GetApiEndPoint() + "subscription/edit", szPostData);
		}

		public void unsubscribe(String feedId)
		{
			String szPostData = "";
			szPostData = String.Format("ac=unsubscribe&s={0}", feedId);

			Post(GetApiEndPoint() + "subscription/edit", szPostData);
		}
	}
}
