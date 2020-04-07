using System.Windows.Controls;

namespace StreamingSoundtracks
{
    /// <summary>
    /// Interaction logic for CurrentPlayingControl.xaml
    /// </summary>
    public partial class CurrentPlayingControl : UserControl
    {
        public CurrentPlayingControl()
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
