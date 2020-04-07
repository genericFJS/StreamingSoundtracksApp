using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace StreamingSoundtracks
{
    class LinkToBitmapConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is null)
                return null;
            var link = value as string;

            BitmapImage bitmap = new BitmapImage();
            bitmap.BeginInit();
            bitmap.UriSource = new Uri(link, UriKind.Absolute);
            bitmap.EndInit();

            return bitmap;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    class MessageToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var message = value as string;
            if (message is null || message.Length == 0)
                return Visibility.Collapsed;
            return Visibility.Visible;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    class IndexStringToIndexConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return int.Parse(value.ToString());
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    class TimeSpanToSecondsConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return ((TimeSpan)value).TotalSeconds;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    class TimeSpanToCurrentQueueConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var timeToCurrentPlaying = (TimeSpan)value;
            var minutes = timeToCurrentPlaying.TotalMinutes;
            if (minutes < 1)
                return string.Format(Properties.Resources.QueueEstimateOneMinute, "< 1");
            else if (minutes >= 60 && minutes < 61)
                return string.Format(Properties.Resources.QueueEstimateOneHour, "1");
            else if (minutes >= 61)
                return string.Format(Properties.Resources.QueueEstimateOneHour, "> 1");
            return string.Format(Properties.Resources.QueueEstimateOneMinute, (int)minutes);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
    class TimeSpanToCurrentHistoryConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var timeToCurrentPlaying = (TimeSpan)value;
            var minutes = timeToCurrentPlaying.TotalMinutes;
            if (minutes < 1)
                return string.Format(Properties.Resources.HistoryEstimateOneMinute, "< 1");
            else if (minutes >= 60 && minutes < 61)
                return string.Format(Properties.Resources.HistoryEstimateOneHour, "1");
            else if (minutes >= 61)
                return string.Format(Properties.Resources.HistoryEstimateOneHour, "> 1");
            return string.Format(Properties.Resources.HistoryEstimateOneMinute, (int)minutes);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    class IsPlayingToIconConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var isPlaying = (bool)value;
            if (isPlaying)
                return FontAwesome.WPF.FontAwesomeIcon.Stop;
            else
                return FontAwesome.WPF.FontAwesomeIcon.Play;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    class IsMutedToBrushConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var isMuted = (bool)value;
            if (isMuted)
                return Brushes.DarkRed;
            else
                return Brushes.Black;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    class BooleanToVisibiltyConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var isTrue = (bool)value;
            if (isTrue)
                return Visibility.Visible;
            return Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
