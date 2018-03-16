using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace DupFinder
{
	class Program
	{
		static void Main(string[] args)
		{
			Stopwatch sw = Stopwatch.StartNew();
			//checkMD5();

			string[] FileList = Directory.GetFiles("F:\\CommonDownloads");
			for (var i = 0; i < FileList.Length; i++)
			{
				for (var j = i + 1; j < FileList.Length; j++)
				{
					/*if(FileCompare(FileList[i], FileList[j]))
					{
						Console.WriteLine(FileList[i] + " = " + FileList[j]);
					}*/

					if (FilesAreEqual(new FileInfo(FileList[i]), new FileInfo(FileList[j])))
					{
						Console.WriteLine(FileList[i] + " = " + FileList[j]);
					}
				}
			}

			sw.Stop();
			Console.WriteLine(sw.ElapsedMilliseconds);
		}

		static void checkMD5()
		{
			byte[] myHash;

			string[] FileList = Directory.GetFiles("C:\\Users\\Shogo\\Downloads");

			Dictionary<string, int> hashList = new Dictionary<string, int>();

			StringBuilder sb = new StringBuilder();
			int val = 0;

			using (var md5 = MD5.Create())
			{
				for (int i = 0; i < FileList.Length; i++)
				{
					val = 0;
					using (var stream = File.OpenRead(FileList[i]))
					{
						myHash = md5.ComputeHash(stream);
					}

					for (int j = 0; j < myHash.Length; j++)
					{
						sb.Append(myHash[j].ToString("x2"));
					}

					//Console.WriteLine(sb.ToString());
					if (!hashList.TryGetValue(sb.ToString(), out val))
					{
						hashList.Add(sb.ToString(), 1);
					}
					else
					{
						hashList[sb.ToString()]++;
					}

					sb.Clear();
				}
			}

			Console.WriteLine("Files:" + FileList.Length + " Hashes:" + hashList.Count());

			for (int i = 0; i < hashList.Count(); i++)
			{
				if (hashList.ElementAt(i).Value > 1)
				{
					Console.WriteLine("S");
				}
			}
		}

		static bool FileCompare(string file1, string file2)
		{
			int file1byte;
			int file2byte;
			FileStream fs1;
			FileStream fs2;

			// Determine if the same file was referenced two times.
			if (file1 == file2)
			{
				// Return true to indicate that the files are the same.
				return true;
			}

			// Open the two files.
			fs1 = new FileStream(file1, FileMode.Open);
			fs2 = new FileStream(file2, FileMode.Open);

			// Check the file sizes. If they are not the same, the files 
			// are not the same.
			if (fs1.Length != fs2.Length)
			{
				// Close the file
				fs1.Close();
				fs2.Close();

				// Return false to indicate files are different
				return false;
			}

			// Read and compare a byte from each file until either a
			// non-matching set of bytes is found or until the end of
			// file1 is reached.
			do
			{
				// Read one byte from each file.
				file1byte = fs1.ReadByte();
				file2byte = fs2.ReadByte();
			}
			while ((file1byte == file2byte) && (file1byte != -1));

			// Close the files.
			fs1.Close();
			fs2.Close();

			// Return the success of the comparison. "file1byte" is 
			// equal to "file2byte" at this point only if the files are 
			// the same.
			return ((file1byte - file2byte) == 0);
		}

		const int BYTES_TO_READ = sizeof(Int64);

		static bool FilesAreEqual(FileInfo first, FileInfo second)
		{
			if (first.Length != second.Length)
				return false;

			if (first.FullName == second.FullName)
				return true;

			int iterations = (int)Math.Ceiling((double)first.Length / BYTES_TO_READ);

			using (FileStream fs1 = first.OpenRead())
			using (FileStream fs2 = second.OpenRead())
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
