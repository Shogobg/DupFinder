﻿using System;
using System.IO;

namespace DupFinderGUI
{
	public partial class FileComparer
	{
		const int BYTES_TO_READ = sizeof(Int64);

		public static bool FilesAreEqual(FileInfo firstFileInfo, FileInfo secondFileInfo)
		{
			if (firstFileInfo.Length != secondFileInfo.Length)
				return false;

			int iterations = (int)Math.Ceiling((double)firstFileInfo.Length / BYTES_TO_READ);

			using (FileStream fs1 = firstFileInfo.OpenRead())
			using (FileStream fs2 = secondFileInfo.OpenRead())
			{
				byte[] one = new byte[BYTES_TO_READ];
				byte[] two = new byte[BYTES_TO_READ];

				for (int i = 0; i < iterations; i++)
				{
					fs1.Read(one, 0, BYTES_TO_READ);
					fs2.Read(two, 0, BYTES_TO_READ);

					if (BitConverter.ToInt64(one, 0) != BitConverter.ToInt64(two, 0))
						return false;
				}
			}

			return true;
		}
	}
}
