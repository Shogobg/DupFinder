using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Windows;

namespace DupFinderGUI
{
	/// <summary>
	/// Interaction logic for MainWindow.xaml
	/// </summary>
	public partial class MainWindow : Window
	{
		public List<DuplicateFileRecord> DuplicatedFiles { get; set; }

		private BackgroundWorker duplicateFinder = null;
		private BackgroundWorker duplicateFinder1 = null;

		string[] FileList;
		private int checkedFiles = 0;

		public MainWindow()
		{
			DuplicatedFiles = new List<DuplicateFileRecord>();
			InitializeComponent();
			DataContext = this;
			FileList = Directory.GetFiles("F:\\CommonDownloads");
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
			
			for (var i = start; i < end; i++)
			{
				for (var j = i + 1; j < FileList.Length; j++)
				{
					if (FileComparer.FilesAreEqual(new FileInfo(FileList[i]), new FileInfo(FileList[j])))
					{
						DuplicatedFiles.Add(new DuplicateFileRecord(new FileInfo(FileList[j])));
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

			for (var i = start; i < FileList.Length; i++)
			{
				for (var j = i + 1; j < FileList.Length; j++)
				{
					if (FileComparer.FilesAreEqual(new FileInfo(FileList[i]), new FileInfo(FileList[j])))
					{
						DuplicatedFiles.Add(new DuplicateFileRecord(new FileInfo(FileList[j])));
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
	}
}
