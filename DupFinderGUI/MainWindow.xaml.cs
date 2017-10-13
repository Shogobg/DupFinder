using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media.Imaging;

namespace DupFinderGUI
{
	/// <summary>
	/// Interaction logic for MainWindow.xaml
	/// </summary>
	public partial class MainWindow : Window
	{
		public Dictionary<String, DuplicateFileRecord> DuplicatedFiles { get; set; }

		private BackgroundWorker duplicateFinder = null;
		private BackgroundWorker duplicateFinder1 = null;

		string[] FileList;
		private int checkedFiles = 0;

		public MainWindow()
		{
			DuplicatedFiles = new Dictionary<String, DuplicateFileRecord>();
			InitializeComponent();
			DataContext = this;
			FileList = Directory.GetFiles("F:\\PCBackup\\Hen\\dup");
			ComparrisonProgress.Maximum = FileList.Length;
		}

		private void button_Click(object sender, RoutedEventArgs e)
		{
			if (duplicateFinder == null)
			{
				duplicateFinder = new BackgroundWorker();
				duplicateFinder.WorkerSupportsCancellation = true;

				// This is executed in a background thread
				duplicateFinder.DoWork += (s, args) => {
					worker(0, FileList.Length / 2 - 1);
				};

				duplicateFinder1 = new BackgroundWorker();
				duplicateFinder1.WorkerSupportsCancellation = true;
				duplicateFinder1.DoWork += (s, args) => {
					worker2();
				};
				// This runs in the main UI thread
				duplicateFinder1.RunWorkerCompleted += (s, args) => {
					if (args.Error != null)  // if an exception occurred during DoWork,
						MessageBox.Show(args.Error.ToString());  // do your error handling here

					dataGrid.Items.Refresh();
				};

				duplicateFinder1.RunWorkerAsync();

				// This runs in the main UI thread
				duplicateFinder.RunWorkerCompleted += (s, args) => {
					if (args.Error != null)  // if an exception occurred during DoWork,
						MessageBox.Show(args.Error.ToString());  // do your error handling here

					dataGrid.Items.Refresh();
				};

				duplicateFinder.RunWorkerAsync();
			}
		}

		void worker(int start, int end)
		{
			Stopwatch sw = Stopwatch.StartNew();
			
			TimeSpan remainingTime;

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
							DuplicatedFiles.Add(FirstFile.FilePath, FirstFile);
							firstFileWasAdded = true;
						}

						var duplicatedFile = new DuplicateFileRecord(new FileInfo(FileList[j]));
						DuplicatedFiles.Add(duplicatedFile.FilePath, duplicatedFile);
					}
				}

				remainingTime = TimeSpan.FromMilliseconds(sw.ElapsedMilliseconds / (i + 1) * (FileList.Length - i));

				checkedFiles++;

				Dispatcher.Invoke(() =>
				{
					ComparrisonProgress.Value = checkedFiles;
					lblRemainingTime.Content = "Remaining time: " + remainingTime.ToString(@"dd\.hh\:mm\:ss");
				});
			}

			sw.Stop();

			Dispatcher.Invoke(() =>
			{
				lblTotalTime.Content = sw.Elapsed.ToString(@"dd\.hh\:mm\:ss");
			});
		}

		void worker2()
		{
			var start = FileList.Length / 2;
			var firstFileWasAdded = false;

			for (var i = start; i < FileList.Length; i++)
			{
				firstFileWasAdded = false;

				for (var j = i + 1; j < FileList.Length; j++)
				{
					if (FileComparer.FilesAreEqual(new FileInfo(FileList[i]), new FileInfo(FileList[j])))
					{
						if(firstFileWasAdded == false)
						{
							var FirstFile = new DuplicateFileRecord(new FileInfo(FileList[i]));
							DuplicatedFiles.Add(FirstFile.FilePath, FirstFile);
							firstFileWasAdded = true;
						}

						var duplicatedFile = new DuplicateFileRecord(new FileInfo(FileList[j]));
						DuplicatedFiles.Add(duplicatedFile.FilePath, duplicatedFile);
					}
				}

				checkedFiles++;

				Dispatcher.Invoke(() =>
				{
					ComparrisonProgress.Value = checkedFiles;
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

	public class UriToCachedImageConverter : IValueConverter
	{
		public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
		{
			if (value == null)
				return null;

			if (!string.IsNullOrEmpty(value.ToString()))
			{
				BitmapImage bi = new BitmapImage();
				bi.BeginInit();
				bi.UriSource = new Uri(value.ToString());
				bi.CacheOption = BitmapCacheOption.OnLoad;
				bi.EndInit();
				return bi;
			}

			return null;
		}

		public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
		{
			throw new NotImplementedException("Two way conversion is not supported.");
		}
	}
}
