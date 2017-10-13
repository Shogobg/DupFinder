using System.IO;

namespace DupFinderGUI
{
	public class DuplicateFileRecord
	{
		private FileInfo info;

		public bool IsSelected { get; set; }
		public string FileName { get { return info.Name; } }
		public string Directory { get { return info.DirectoryName; } }
		public string FilePath { get { return info.FullName; } }
		public string Size { get { return (info.Length / 1024).ToString() + "kb"; } }
		public string CreationTime { get { return info.CreationTime.ToString("yyyy-MM-dd HH:mm:ss"); } }

		public DuplicateFileRecord(FileInfo info)
		{
			this.info = info;
		}
	}
}
