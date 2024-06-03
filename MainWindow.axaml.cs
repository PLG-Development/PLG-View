using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using Tmds.DBus.Protocol;

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

        if(args.Length >= 2)
        {
            try
            {
                ZoomableImage.Source = new Bitmap("arrow_left.png");
            } catch
            {
                
            }
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

            _translateTransform.X = (Canvas.Bounds.Width - scaledWidth) / 2;
            _translateTransform.Y = (Canvas.Bounds.Height - scaledHeight) / 2;
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
