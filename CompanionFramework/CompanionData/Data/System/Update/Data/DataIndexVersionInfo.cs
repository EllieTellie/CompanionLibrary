using System;

namespace Companion.Data.System.Update
{
	public class DataIndexVersionInfo
	{
		public string id;
		public string name;
		public string revision;
		public string battleScribeVersion;

		public DataIndexVersionInfo(string id, string name, string revision, string battleScribeVersion)
		{
			this.id = id;
			this.name = name;
			this.revision = revision;
			this.battleScribeVersion = battleScribeVersion;
		}

		public bool MatchesRevision(DataIndexEntry dataIndex)
		{
			// validate id
			if (dataIndex.dataId != id)
				return false;

			// validate revision
			if (dataIndex.dataRevision != revision)
			{
				if (GetRevision() < dataIndex.GetRevision())
					return false;
			}

			return true;
		}

		public int GetRevision()
		{
			if (int.TryParse(revision, out int parsed))
			{
				return parsed;
			}
			else
			{
				return -1;
			}
		}
	}
}
