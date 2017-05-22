using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Windows;

namespace DupFinderGUI
{
	/// <summary>
	/// Interaction logic for MainWindow.xaml
	/// </summary>
	public partial class MainWindow : Window
	{
		public List<string> DuplicatedFiles
		{
			get; set;
		}

		private Thread duplicateFinder = null;

		public MainWindow()
		{
			this.DuplicatedFiles = new List<string>();
			InitializeComponent();
			DataContext = this;
		}

		private void button_Click(object sender, RoutedEventArgs e)
		{
			if (duplicateFinder == null)
			{
				duplicateFinder = new Thread(worker);
				duplicateFinder.IsBackground = true;
				duplicateFinder.Start();
			}
		}

		void worker()
		{
			Stopwatch sw = Stopwatch.StartNew();

			string[] FileList = Directory.GetFiles("C:\\Users\\Shogo\\Downloads"); // F:\\CommonDownloads C:\\Users\\Shogo\\Downloads

			Dispatcher.Invoke(() =>
			{
				ComparrisonProgress.Maximum = FileList.Length;
			});

			TimeSpan remainingTime;

			for (var i = 0; i < FileList.Length; i++)
			{
				for (var j = i + 1; j < FileList.Length; j++)
				{
					if (FilesAreEqual(new FileInfo(FileList[i]), new FileInfo(FileList[j])))
					{
						DuplicatedFiles.Add(FileList[j]);
						
					}
				}

				remainingTime = TimeSpan.FromMilliseconds(sw.ElapsedMilliseconds / (i + 1) * (FileList.Length - i));

				Dispatcher.Invoke(() =>
				{
					ComparrisonProgress.Value = i + 1;
					lblRemainingTime.Content = "Remaining time: " + remainingTime.ToString(@"dd\.hh\:mm\:ss");
					/*
					+ remainingTime.Days
					+ "days " + remainingTime.Hours
					+ ":" + remainingTime.Minutes
					+ ":" + remainingTime.Seconds;
					*/
				});
			}

			Dispatcher.Invoke(() =>
			{
				dataGrid.Items.Refresh();
			});

			sw.Stop();

			Dispatcher.Invoke(() =>
			{
				lblTotalTime.Content = sw.Elapsed.ToString(@"dd\.hh\:mm\:ss");
			});
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
