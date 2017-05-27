using System.IO;

namespace DupFinderGUI
{
	public class DuplicateFileRecord
	{
		private FileInfo info;

		public string FileName { get { return info.Name; } }
		public string Directory { get { return info.DirectoryName; } }
		public string FilePath { get { return info.FullName; } }
		public string Size { get { return (info.Length / 1024).ToString() + "kb"; } }

		public DuplicateFileRecord(FileInfo info)
		{
			this.info = info;
		}
	}
}
