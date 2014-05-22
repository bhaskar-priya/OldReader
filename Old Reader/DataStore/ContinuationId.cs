using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data.Linq.Mapping;

using Old_Reader;

namespace DataStore
{
	[Table]
	public class ContinuationId
	{
		[Column(IsPrimaryKey = true, CanBeNull = false, IsDbGenerated = false)]
		public String ID { get; set; }

		[Column(CanBeNull = true, IsDbGenerated = false)]
		public String continuationID { get; set; }

		public static String getContinuationIdFor(String id)
		{
			ContinuationId ctid = App.ReaderDB.ContinuationIds.FirstOrDefault(cid => cid.ID == id);
			if (ctid != null)
			{
				return ctid.continuationID;
			}
			return "";
		}

		public static void setContinuationId(String szId, String szcontinuationID)
		{
			ContinuationId ctid = App.ReaderDB.ContinuationIds.FirstOrDefault(cid => cid.ID == szId);
			if (ctid != null)
			{
				ctid.continuationID = szcontinuationID;
			}
			else
			{
				ctid = new ContinuationId()
				{
					ID = szId,
					continuationID = szcontinuationID
				};
				App.ReaderDB.ContinuationIds.InsertOnSubmit(ctid);
			}

			App.ReaderDB.SubmitChanges();
		}
	}
}
