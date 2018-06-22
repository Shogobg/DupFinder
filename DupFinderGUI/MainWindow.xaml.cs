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

		public string searchFolder = "";

		string[] FileList;
		private int checkedFiles = 0;
		Stopwatch sw = new Stopwatch();

		private List<Task> workers = new List<Task>();

		private FileInfo[] FileInfoList;

		public MainWindow()
		{

			DuplicatedFiles = new Dictionary<String, DuplicateFileRecord>();
			InitializeComponent();
			DataContext = this;
		}

		private void btnSearch_Click(object sender, RoutedEventArgs e)
		{
			FileList = Directory.GetFiles(searchFolder, "*", SearchOption.AllDirectories);
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
				workers.Add(Task.Factory.StartNew(() => FileWorker(0, FileList.Length - 1), TaskCreationOptions.LongRunning));
				workers.Add(Task.Factory.StartNew(() => FileWorker(FileList.Length / 2, FileList.Length - 1), TaskCreationOptions.LongRunning));

				// Update the data grid only after all records were added to prevent crash
				Task.Factory.ContinueWhenAll(workers.ToArray(), updateUiTask =>
				{
					Dispatcher.BeginInvoke((Action)(() =>
					{
						dataGrid.Items.Refresh();
					}));
				});
			}
		}

		void FileWorker(int start, int end)
		{
			var firstFileWasAdded = false;

			for (var i = start; i < end; i++)
			{
				firstFileWasAdded = false;

				for (var j = i + 1; j < FileList.Length - 1; j++)
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

			Debug.WriteLine("Finished fetching file info");
		}

		private void stopSearchButton_Click(object sender, RoutedEventArgs e)
		{
		}

		private void btnDelete_Click(object sender, RoutedEventArgs e)
		{
			var deleteFilesList = new List<string>();

			foreach (var currentFile in DuplicatedFiles)
			{
				if (currentFile.Value.IsSelected)
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

		private void GridMouseDoubleClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
		{
			//Process.Start(sender.SelectedValue.Key)
			Debug.WriteLine("Clicked");
		}
	}
}
