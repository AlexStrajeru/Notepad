using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Media.Animation;

namespace MyNotepad.Core;

// Clasa de baza pentru toate ferestrele aplicatiei.
// Orice fereastra care mosteneste AnimatedWindow primeste automat:
// 1. Bara de titlu inchisa (dark title bar)
// 2. Animatie fade-in la deschidere (fara flash alb)
public class AnimatedWindow : Window
{
    [DllImport("dwmapi.dll")]
    private static extern int DwmSetWindowAttribute(IntPtr hwnd, int attr, ref int value, int size);

    public AnimatedWindow()
    {
        // Ascunde fereastra inainte de primul frame randat (previne flash-ul alb)
        this.Opacity = 0;
        this.ContentRendered += OnContentRendered;
    }

    // Seteaza bara de titlu la dark mode (Windows 10/11)
    protected override void OnSourceInitialized(EventArgs e)
    {
        base.OnSourceInitialized(e);
        var hwnd = new WindowInteropHelper(this).Handle;
        int dark = 1;
        DwmSetWindowAttribute(hwnd, 20, ref dark, sizeof(int)); // Windows 11
        DwmSetWindowAttribute(hwnd, 19, ref dark, sizeof(int)); // Windows 10
    }

    // Fade-in dupa ce primul frame e desenat — garantat fara flash alb
    private void OnContentRendered(object? sender, EventArgs e)
    {
        var ease = new CubicEase { EasingMode = EasingMode.EaseOut };
        this.BeginAnimation(OpacityProperty,
            new DoubleAnimation(0, 1, new Duration(TimeSpan.FromSeconds(0.22)))
            { EasingFunction = ease });
    }
}
