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
		private System.Windows.Forms.FolderBrowserDialog folderDialog = new System.Windows.Forms.FolderBrowserDialog();

		public Dictionary<String, DuplicateFileRecord> DuplicatedFiles { get; set; }

		private BackgroundWorker duplicateFinder = null;

		string searchFolder = "";

		string[] FileList;
		private int checkedFiles = 0;
		Stopwatch sw = new Stopwatch();

		private Task workerOne, workerTwo;

		private FileInfo[] FileInfoList;

		public MainWindow()
		{
			DuplicatedFiles = new Dictionary<String, DuplicateFileRecord>();
			InitializeComponent();
			DataContext = this;
		}

		private void btnSearch_Click(object sender, RoutedEventArgs e)
		{
			FileList = Directory.GetFiles(searchFolder);
			ComparrisonProgress.Maximum = FileList.Length;
			FileInfoList = new FileInfo[FileList.Length];

			for (var i = 0; i < FileList.Length; i++)
			{
				FileInfoList[i] = new FileInfo(FileList[i]);
			}

			sw.Start();

			if (duplicateFinder == null)
			{
				duplicateFinder = new BackgroundWorker();
				duplicateFinder.WorkerSupportsCancellation = true;

				// This is executed in a background thread
				
				workerOne = Task.Factory.StartNew(() => FileWorker(0, FileList.Length / 2 - 1), TaskCreationOptions.LongRunning);
				workerTwo = Task.Factory.StartNew(() => FileWorker(FileList.Length / 2, FileList.Length-1), TaskCreationOptions.LongRunning);
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
					if (FileComparer.FilesAreEqual(FileInfoList[i], FileInfoList[j]))
					{
						if (firstFileWasAdded == false)
						{
							var FirstFile = new DuplicateFileRecord(FileInfoList[i]);
							if (!DuplicatedFiles.ContainsKey(FirstFile.FilePath))
								DuplicatedFiles.Add(FirstFile.FilePath, FirstFile);

							firstFileWasAdded = true;
						}

						var duplicatedFile = new DuplicateFileRecord(FileInfoList[j]);
						if (!DuplicatedFiles.ContainsKey(duplicatedFile.FilePath))
							DuplicatedFiles.Add(duplicatedFile.FilePath, duplicatedFile);
					}
				}

				checkedFiles++;
				
				Dispatcher.BeginInvoke((Action)(() =>
				{
					TimeSpan remainingTime = TimeSpan.FromMilliseconds(sw.ElapsedMilliseconds / checkedFiles * (FileList.Length - checkedFiles));
					lblRemainingTime.Content = "Remaining time: " + remainingTime.ToString(@"dd\.hh\:mm\:ss");
					lblTotalTime.Content = sw.Elapsed.ToString(@"dd\.hh\:mm\:ss");
					ComparrisonProgress.Value = checkedFiles;
					
				}));
			}

			Dispatcher.BeginInvoke((Action)(() =>
			{
				// To do: this may cause a crash that collection was changed
				// while refreshing - research and find a thread-safe fix
				// Probably best to find the duplicates in the background and then show them all
				dataGrid.Items.Refresh();
			}));

			Debug.WriteLine("Finished fetching file info");
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

		private void btnBrowse_Click(object sender, RoutedEventArgs e)
		{
			folderDialog.SelectedPath = searchFolder;
			if (folderDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
				searchFolder = folderDialog.SelectedPath;
		}
	}
}
