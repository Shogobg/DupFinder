using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Windows;
using System.Threading.Tasks;

namespace DupFinderGUI
{
	/// <summary>
	/// Interaction logic for MainWindow.xaml
	/// </summary>
	public partial class MainWindow : Window
	{
		public Dictionary<String, DuplicateFileRecord> DuplicatedFiles { get; set; }

		private BackgroundWorker duplicateFinder = null;

		string[] FileList;
		private int checkedFiles = 0;
		Stopwatch sw = new Stopwatch();

		public MainWindow()
		{
			DuplicatedFiles = new Dictionary<String, DuplicateFileRecord>();
			InitializeComponent();
			DataContext = this;
			FileList = Directory.GetFiles("F:\\CommonDownloads");
			ComparrisonProgress.Maximum = FileList.Length;
		}

		private async void button_Click(object sender, RoutedEventArgs e)
		{
			sw.Start();

			if (duplicateFinder == null)
			{
				duplicateFinder = new BackgroundWorker();
				duplicateFinder.WorkerSupportsCancellation = true;

				// This is executed in a background thread
				Task.Run(() => FileWorker(0, FileList.Length/2 - 1));
				Task.Run(() => FileWorker(FileList.Length / 2, FileList.Length-1));
				
			}
		}

		void FileWorker(int start, int end)
		{
			var firstFileWasAdded = false;

			for (var i = start; i < end; i++)
			{
				firstFileWasAdded = false;

				for (var j = i + 1; j < FileList.Length; j++)
				{
					if (FileComparer.FilesAreEqual(new FileInfo(FileList[i]), new FileInfo(FileList[j])))
					{
						if (firstFileWasAdded == false)
						{
							var FirstFile = new DuplicateFileRecord(new FileInfo(FileList[i]));
							if (!DuplicatedFiles.ContainsKey(FirstFile.FilePath))
								DuplicatedFiles.Add(FirstFile.FilePath, FirstFile);

							firstFileWasAdded = true;
						}

						var duplicatedFile = new DuplicateFileRecord(new FileInfo(FileList[j]));
						if (!DuplicatedFiles.ContainsKey(duplicatedFile.FilePath))
							DuplicatedFiles.Add(duplicatedFile.FilePath, duplicatedFile);
					}
				}
				
				checkedFiles++;

				Dispatcher.Invoke(() =>
				{
					TimeSpan remainingTime = TimeSpan.FromMilliseconds(sw.ElapsedMilliseconds / checkedFiles * (FileList.Length - checkedFiles));
					lblRemainingTime.Content = "Remaining time: " + remainingTime.ToString(@"dd\.hh\:mm\:ss");
					lblTotalTime.Content = sw.Elapsed.ToString(@"dd\.hh\:mm\:ss");
					ComparrisonProgress.Value = checkedFiles;
					dataGrid.Items.Refresh();
				});
			}
		}
		
		private void stopSearchButton_Click(object sender, RoutedEventArgs e)
		{

		}

		private void btnDelete_Click(object sender, RoutedEventArgs e)
		{
			var deleteFilesList = new List<string>();

			foreach(var currentFile in DuplicatedFiles)
			{
				if(currentFile.Value.IsSelected)
				{
					deleteFilesList.Add(currentFile.Value.FilePath);
				}
			}

			foreach (var filePath in deleteFilesList)
			{
				File.Delete(filePath);
				DuplicatedFiles.Remove(filePath);
			}

			Dispatcher.Invoke(() =>
			{
				dataGrid.Items.Refresh();
			});
		}
	}
}
