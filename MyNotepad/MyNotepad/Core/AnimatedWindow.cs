using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Media.Animation;

namespace MyNotepad.Core;

public class AnimatedWindow : Window
{
    [DllImport("dwmapi.dll")]
    private static extern int DwmSetWindowAttribute(IntPtr hwnd, int attr, ref int value, int size);

    public AnimatedWindow()
    {

        this.Opacity = 0;
        this.ContentRendered += OnContentRendered;
    }


    protected override void OnSourceInitialized(EventArgs e)
    {
        base.OnSourceInitialized(e);
        var hwnd = new WindowInteropHelper(this).Handle;
        int dark = 1;
        DwmSetWindowAttribute(hwnd, 20, ref dark, sizeof(int));
        DwmSetWindowAttribute(hwnd, 19, ref dark, sizeof(int));
    }


    private void OnContentRendered(object? sender, EventArgs e)
    {
        var ease = new CubicEase { EasingMode = EasingMode.EaseOut };
        this.BeginAnimation(OpacityProperty,
            new DoubleAnimation(0, 1, new Duration(TimeSpan.FromSeconds(0.22)))
            { EasingFunction = ease });
    }
}
