using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using Tmds.DBus.Protocol;
using MsBox.Avalonia;
using System;
using Avalonia.Interactivity;
using System.IO;
using System.Threading.Tasks;

namespace PLG_View;

public partial class MainWindow : Window
{
    private TransformGroup _transformGroup;
    private ScaleTransform _scaleTransform;
    private TranslateTransform _translateTransform;
    private Point _origin;
    private Point _start;
    private bool _isDragging;
    private string _currentFilePath;
    private string[] _imageFilesInFolder;

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

        
        BtnNext.Click += BtnNext_Click;
        BtnPrevious.Click += BtnPrevious_Click;

        foreach (var item in args)
        {
            Console.WriteLine(item);
        }

        if (args.Length >= 1)
        {
            try
            {
                LoadImageFilesInFolder(Path.GetDirectoryName(args[0]));
                OpenImageFileAsync(args[0]);
            }
            catch (Exception ex)
            {
                var box = MessageBoxManager.GetMessageBoxStandard("Error while loading image", "Loading the file caused an error:" + ex.Message);
                box.ShowAsync();
            }
        }

        this.SizeChanged += (sender, e) => AdjustZoomToFit(ZoomableImage.Source as Bitmap);
        SetCanvasBackground();
    }

    private void SetCanvasBackground()
    {
        
    }

    public async Task OpenFileAsync(string filePath)
        {
            try
            {
                var bitmap = await Task.Run(() => new Bitmap(filePath));
                ZoomableImage.Source = bitmap;
                AdjustZoomToFit(bitmap);
                _currentFilePath = filePath;

                // Lade die Bilddateien im selben Ordner
                LoadImageFilesInFolder(Path.GetDirectoryName(filePath));
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Fehler beim Öffnen der Datei: {ex.Message}");
            }
        }
    private void AdjustZoomToFit(Bitmap bitmap)
    {
        if (bitmap != null && Canvas.Bounds.Width > 0 && Canvas.Bounds.Height > 0)
        {
            double scaleX = Canvas.Bounds.Width / bitmap.Size.Width;
            double scaleY = Canvas.Bounds.Height / bitmap.Size.Height;
            double scale = Math.Min(scaleX, scaleY) * 100;

            if (SldZoom.Maximum < scale)
            {
                SldZoom.Value = SldZoom.Maximum;
            }
            else if (SldZoom.Minimum > scale)
            {
                SldZoom.Value = SldZoom.Minimum;
            }
            else
            {
                SldZoom.Value = scale;
            }

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




    private async void BtnNext_Click(object sender, RoutedEventArgs e)
        {
            if (_imageFilesInFolder == null || _imageFilesInFolder.Length == 0)
                return;

            int currentIndex = Array.IndexOf(_imageFilesInFolder, _currentFilePath);
            int nextIndex = (currentIndex + 1) % _imageFilesInFolder.Length;

            await OpenImageFileAsync(_imageFilesInFolder[nextIndex]);
        }

        private async void BtnPrevious_Click(object sender, RoutedEventArgs e)
        {
            if (_imageFilesInFolder == null || _imageFilesInFolder.Length == 0)
                return;

            int currentIndex = Array.IndexOf(_imageFilesInFolder, _currentFilePath);
            int previousIndex = (currentIndex - 1 + _imageFilesInFolder.Length) % _imageFilesInFolder.Length;

            await OpenImageFileAsync(_imageFilesInFolder[previousIndex]);
        }
    private async Task OpenImageFileAsync(string filePath)
        {
            try
            {
                var bitmap = await Task.Run(() => new Bitmap(filePath));
                ZoomableImage.Source = bitmap;
                AdjustZoomToFit(bitmap);
                _currentFilePath = filePath;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Fehler beim Öffnen der Datei: {ex.Message}");
            }
        }
        
    private void LoadImageFilesInFolder(string folderPath)
    {
        try
        {
            _imageFilesInFolder = Directory.GetFiles(folderPath, "*.png");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error loading image files: {ex.Message}");
        }
    }
}
