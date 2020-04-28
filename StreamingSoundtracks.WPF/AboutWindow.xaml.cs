using System.Windows;

namespace StreamingSoundtracks
{
    /// <summary>
    /// Interaction logic for Window1.xaml
    /// </summary>
    public partial class AboutWindow : Window
    {
        public AboutWindow()
        {
            InitializeComponent();
        }
        private void Hyperlink_RequestNavigate(object sender, System.Windows.Navigation.RequestNavigateEventArgs e)
        {
            App.OpenHyperlink(e.Uri.AbsoluteUri);
            e.Handled = true;
        }
    }
}
