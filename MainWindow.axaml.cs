using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using Tmds.DBus.Protocol;
using MsBox.Avalonia;
using System;

namespace PLG_View;

public partial class MainWindow : Window
{
    private TransformGroup _transformGroup;
    private ScaleTransform _scaleTransform;
    private TranslateTransform _translateTransform;
    private Point _origin;
    private Point _start;
    private bool _isDragging;

    public MainWindow(string[] args)
    {
        InitializeComponent();
        InitializeGUI();

        DataContext = this;

        _transformGroup = new TransformGroup();
        _scaleTransform = new ScaleTransform();
        _translateTransform = new TranslateTransform();
        _transformGroup.Children.Add(_scaleTransform);
        _transformGroup.Children.Add(_translateTransform);

        ZoomableImage.RenderTransform = _transformGroup;

        SldZoom.PropertyChanged += ZoomChanged;
        ZoomableImage.PointerPressed += Image_PointerPressed;
        ZoomableImage.PointerMoved += Image_PointerMoved;
        ZoomableImage.PointerReleased += Image_PointerReleased;

        foreach (var item in args)
        {
            Console.WriteLine(item);
        }

        if (args.Length >= 1)
        {
            try
            {
                var img = new Bitmap(args[0]);
                ZoomableImage.Source = img;
                this.Opened += (sender, e) => AdjustZoomToFit(img);

            }
            catch (Exception ex)
            {
                var box = MessageBoxManager
            .GetMessageBoxStandard("Error while loading image", "Loading the file caused an error:" + ex.Message);
                box.ShowAsync();
            }
        }
    }

    private void AdjustZoomToFit(Bitmap bitmap)
    {
        if (bitmap != null && Canvas.Bounds.Width > 0 && Canvas.Bounds.Height > 0)
        {
            double scaleX = Canvas.Bounds.Width / bitmap.Size.Width;
            double scaleY = Canvas.Bounds.Height / bitmap.Size.Height;
            double scale = Math.Min(scaleX, scaleY) * 100;

            SldZoom.Value = scale;
            _scaleTransform.ScaleX = _scaleTransform.ScaleY = scale / 100;
            CenterImage();
        }
    }

    internal void InitializeGUI()
    {
        ImgLeft.Source = new Bitmap("arrow_left.png");
        ImgRight.Source = new Bitmap("arrow_right.png");
    }

    private void ZoomChanged(object sender, AvaloniaPropertyChangedEventArgs e)
    {
        if (e.Property.Name == "Value")
        {
            double scale = (double)e.NewValue / 100;
            _scaleTransform.ScaleX = _scaleTransform.ScaleY = scale;

            // Bild zentrieren
            CenterImage();
        }
    }

    private void CenterImage()
    {
        if (ZoomableImage.Source != null)
        {
            double scaledWidth = ZoomableImage.Source.Size.Width * _scaleTransform.ScaleX;
            double scaledHeight = ZoomableImage.Source.Size.Height * _scaleTransform.ScaleY;

            double offsetX = (Canvas.Bounds.Width - scaledWidth) / 2;
            double offsetY = (Canvas.Bounds.Height - scaledHeight) / 2;

            _translateTransform.X = offsetX;
            _translateTransform.Y = offsetY;
        }
    }

    private void Image_PointerPressed(object sender, PointerPressedEventArgs e)
    {
        var pointerPoint = e.GetCurrentPoint(Canvas);
        if (pointerPoint.Properties.IsLeftButtonPressed)
        {
            _start = pointerPoint.Position;
            _origin = new Point(_translateTransform.X, _translateTransform.Y);
            ZoomableImage.Cursor = new Cursor(StandardCursorType.Hand);
            _isDragging = true;
        }
    }

    private void Image_PointerMoved(object sender, PointerEventArgs e)
    {
        if (_isDragging)
        {
            var currentPosition = e.GetPosition(Canvas);
            var deltaX = currentPosition.X - _start.X;
            var deltaY = currentPosition.Y - _start.Y;

            _translateTransform.X = _origin.X + deltaX;
            _translateTransform.Y = _origin.Y + deltaY;
        }
    }

    private void Image_PointerReleased(object sender, PointerReleasedEventArgs e)
    {
        _isDragging = false;
        ZoomableImage.Cursor = new Cursor(StandardCursorType.Arrow);
    }
}
