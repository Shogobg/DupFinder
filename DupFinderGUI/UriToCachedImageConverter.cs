﻿using System;
using System.Windows.Data;
using System.Windows.Media.Imaging;

namespace DupFinderGUI
{
	public class UriToCachedImageConverter : IValueConverter
	{
		public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
		{
			
			if (value == null)
				return null;

			if (!string.IsNullOrEmpty(value.ToString()))
			{
				try
				{
					BitmapImage bi = new BitmapImage();
					bi.BeginInit();
					bi.UriSource = new Uri(value.ToString());
					bi.CacheOption = BitmapCacheOption.OnLoad;
					bi.EndInit();

					return bi;
				}
				catch(Exception ex)
				{
					// Conversion failed
				}
			}

			return null;
		}

		public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
		{
			throw new NotImplementedException("Two way conversion is not supported.");
		}
	}
}
