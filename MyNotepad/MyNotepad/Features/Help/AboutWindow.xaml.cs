using System.Diagnostics;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Navigation;
using MyNotepad.Core;

namespace MyNotepad.Features.Help;

// Mosteneste AnimatedWindow — primeste automat fade-in si dark title bar
public partial class AboutWindow : AnimatedWindow
{
    public AboutWindow()
    {
        InitializeComponent();
    }

    private void OK_Click(object sender, RoutedEventArgs e)
    {
        // Fade-out inainte de inchidere
        var ease = new CubicEase { EasingMode = EasingMode.EaseIn };
        var fadeOut = new DoubleAnimation(1, 0, new Duration(TimeSpan.FromSeconds(0.15)))
            { EasingFunction = ease };
        fadeOut.Completed += (_, _) => Close();
        this.BeginAnimation(OpacityProperty, fadeOut);
    }

    private void Hyperlink_RequestNavigate(object sender, RequestNavigateEventArgs e)
    {
        System.Diagnostics.Process.Start(
            new System.Diagnostics.ProcessStartInfo(e.Uri.AbsoluteUri) { UseShellExecute = true });
        e.Handled = true;
    }
}
