using Avalonia.Controls;
using Avalonia.Media;
using Avalonia.Media.Imaging;

namespace PLG_View;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
        InitializeGUI();
    }

    internal void InitializeGUI()
    {
        ImgLeft.Source = new Bitmap("arrow_left.png");
        ImgRight.Source = new Bitmap("arrow_right.png");
    }

}