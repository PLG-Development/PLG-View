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
using Avalonia.Platform.Storage;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Linq;
using System.ComponentModel;


namespace PLG_View;

public partial class MainWindow : Window
{
    private TransformGroup _transformGroup;
    private ScaleTransform _scaleTransform;
    private TranslateTransform _translateTransform;
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
        SldZoomTf1.PropertyChanged += ZoomChanged;
        SldZoomTf2.PropertyChanged += ZoomChanged;
        ZoomableImage.PointerPressed += Image_PointerPressed;
        ZoomableImage.PointerMoved += Image_PointerMoved;
        ZoomableImage.PointerReleased += Image_PointerReleased;
    
        this.ContextMenu = null;

        BtnNext.Click += BtnNext_Click;
        BtnNextTf1.Click += BtnNext_Click;
        BtnNextTf2.Click += BtnNext_Click;
        BtnPrevious.Click += BtnPrevious_Click;
        BtnPreviousTf1.Click += BtnPrevious_Click;
        BtnPreviousTf2.Click += BtnPrevious_Click;

        this.KeyDown += Window_KeyDown;

        foreach (var item in args)
        {
            Console.WriteLine(item);
        }

        if (args.Length >= 1)
        {
            try
            {
                LoadImageFilesInFolder(Path.GetDirectoryName(args[0]));
                Console.WriteLine(Path.GetDirectoryName(args[0]));
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


    private void Window_KeyDown(object sender, KeyEventArgs e)
    {
        if (e.Key == Key.Right)
        {
            BtnNext_Click(sender, e);
        }
        else if (e.Key == Key.Left)
        {
            BtnPrevious_Click(sender, e);
        }
        else if (e.Key == Key.F11)
        {
            this.WindowState = this.WindowState == WindowState.FullScreen ? WindowState.Normal : WindowState.FullScreen;
        }
        else if (e.KeyModifiers == KeyModifiers.Control && e.Key == Key.O)
        {
            OpenFile_Click(sender, e);
        }
        else if (e.KeyModifiers == KeyModifiers.Control && e.Key == Key.F)
        {
            OpenFolder_Click(sender, e);
        }
        else if (e.KeyModifiers == KeyModifiers.Control && e.Key == Key.T)
        {
            Tafelmodus_Click(sender, e);
        }
        else if (e.KeyModifiers == KeyModifiers.Control && e.Key == Key.S)
        {
            Standardmodus_Click(sender, e);
        }
        else if (e.KeyModifiers == KeyModifiers.Control && e.Key == Key.V)
        {
            VollerModus_Click(sender, e);
        }
        else if (e.KeyModifiers == KeyModifiers.Control && e.Key == Key.OemPlus)
        {
            if(SldZoom.Value <= SldZoom.Maximum - 1)
            {
                SldZoom.Value += 1;
                SldZoomTf1.Value += 1;
                SldZoomTf2.Value += 1;

            }
        }
        else if (e.KeyModifiers == KeyModifiers.Control && e.Key == Key.OemMinus)
        {
            if(SldZoom.Value >= SldZoom.Minimum + 1)
            {
                SldZoom.Value -= 1;
                SldZoomTf1.Value -= 1;
                SldZoomTf2.Value -= 1;
            }
                
        }
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
            Console.WriteLine(Path.GetDirectoryName(filePath));
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
                SldZoomTf1.Value = SldZoom.Maximum;
                SldZoomTf2.Value = SldZoom.Maximum;
            }
            else if (SldZoom.Minimum > scale)
            {
                SldZoom.Value = SldZoom.Minimum;
                SldZoomTf1.Value = SldZoom.Minimum;
                SldZoomTf2.Value = SldZoom.Minimum;
            }
            else
            {
                SldZoom.Value = scale;
                SldZoomTf1.Value = scale;
                SldZoomTf2.Value = scale;
            }

            _scaleTransform.ScaleX = _scaleTransform.ScaleY = scale / 100;
            CenterImage();
        }
    }


    RowDefinitions rows_tafel = new RowDefinitions();
    RowDefinitions rows_standard = new RowDefinitions();

    internal void InitializeGUI()
    {
        ImgLeft.Source = new Bitmap("arrow_left.png");
        ImgRight.Source = new Bitmap("arrow_right.png");
        ImgLeftTf1.Source = new Bitmap("arrow_left.png");
        ImgRightTf1.Source = new Bitmap("arrow_right.png");
        ImgLeftTf2.Source = new Bitmap("arrow_left.png");
        ImgRightTf2.Source = new Bitmap("arrow_right.png");


        rows_tafel.Add(new RowDefinition(new GridLength(0, GridUnitType.Star)));
        rows_tafel.Add(new RowDefinition(new GridLength(10)));
        rows_tafel.Add(new RowDefinition(new GridLength(60)));
        rows_tafel.Add(new RowDefinition(new GridLength(10)));



        rows_standard.Add(new RowDefinition(new GridLength(0, GridUnitType.Star)));
    }

    private void ZoomChanged(object sender, AvaloniaPropertyChangedEventArgs e)
    {
        if (e.Property.Name == "Value")
        {
            double scale = (double)e.NewValue / 100;
            _scaleTransform.ScaleX = _scaleTransform.ScaleY = scale;

            // Bild zentrieren
            CenterImage();

            SldZoomTf1.Value = (double)e.NewValue;
            SldZoom.Value = (double)e.NewValue;
            SldZoomTf2.Value = (double)e.NewValue;
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


// Variables to track drag gesture state
private Point? _start;
private Point _origin;
private bool _isDragging;
private bool _isTouchDragging;


private void Image_PointerPressed(object sender, PointerPressedEventArgs e)
{
    var pointerPoint = e.GetCurrentPoint(Canvas);

    // Check if input is from a touch device or left-click
    if (pointerPoint.Pointer.Type == PointerType.Touch)
    {
        // Initialize dragging for touch
        _start = pointerPoint.Position;
        _origin = new Point(_translateTransform.X, _translateTransform.Y);
        ZoomableImage.Cursor = new Cursor(StandardCursorType.Hand);
        _isDragging = true;
        _isTouchDragging = true;  // Flag for touch dragging

        // Suppress context menu for touch
        e.Handled = true; 
    }
    else if (pointerPoint.Properties.IsLeftButtonPressed)
    {
        // Initialize dragging for mouse left-click
        _start = pointerPoint.Position;
        _origin = new Point(_translateTransform.X, _translateTransform.Y);
        ZoomableImage.Cursor = new Cursor(StandardCursorType.Hand);
        _isDragging = true;

        // Suppress context menu
        e.Handled = true; 
    }

    // Prevent context menu from appearing for right clicks
    if (pointerPoint.Properties.IsRightButtonPressed)
    {
        e.Handled = true;  // Suppress the context menu
    }
}

private void Image_PointerMoved(object sender, PointerEventArgs e)
{
    if (_isDragging && _start.HasValue)
    {
        var currentPosition = e.GetPosition(Canvas);
        var deltaX = currentPosition.X - _start.Value.X;
        var deltaY = currentPosition.Y - _start.Value.Y;

        // Apply translation (dragging effect)
        _translateTransform.X = _origin.X + deltaX;
        _translateTransform.Y = _origin.Y + deltaY;

        // Suppress context menu for touch and drag
        e.Handled = true; 
    }
}

private void Image_PointerReleased(object sender, PointerReleasedEventArgs e)
{
    if (_isDragging)
    {
        // End dragging
        _isDragging = false;
        ZoomableImage.Cursor = new Cursor(StandardCursorType.Arrow);

        // Suppress context menu
        e.Handled = true; 
    }

    // Prevent the context menu from appearing for touch releases
    if (_isTouchDragging)
    {
        _isTouchDragging = false;
        e.Handled = true;  // Explicitly prevent context menu for touch
    }

    // Suppress the context menu on right-click
    var pointerPoint = e.GetCurrentPoint(Canvas);
    if (pointerPoint.Properties.IsRightButtonPressed)
    {
        e.Handled = true;  // Prevent right-click context menu
    }
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
        
        string[] extensions = new[] { "*.png", "*.jpg", "*.jpeg", "*.JPG" };

        if (Directory.Exists(folderPath))
        {
            // Retrieve files for each extension and combine into one array
            _imageFilesInFolder = extensions.SelectMany(ext => Directory.GetFiles(folderPath, ext)).ToArray();
        }
        else
        {
            Console.WriteLine("The directory does not exist.");
        }
    }

    private void Tafelmodus_Click(object sender, RoutedEventArgs e)
    {
        GrdControlsTafel.IsVisible = true;
        GrdControlsStandard.IsVisible = false;

        ScrollViewer.Margin = new Thickness(10,10,10,80);
        CenterImage();
    }

    private void Standardmodus_Click(object sender, RoutedEventArgs e)
    {
        GrdControlsTafel.IsVisible = false;
        GrdControlsStandard.IsVisible = true;
        ScrollViewer.Margin = new Thickness(10,10,10,80);
        CenterImage();
    }

    private void VollerModus_Click(object sender, RoutedEventArgs e)
    {
        GrdControlsTafel.IsVisible = false;
        GrdControlsStandard.IsVisible = false;
        ScrollViewer.Margin = new Thickness(10,10,10,10);
        
    }

    private void Vollbild_Click(object sender, RoutedEventArgs e)
    {
        this.WindowState = WindowState.FullScreen;
    }

    private void Beenden_Click(object sender, RoutedEventArgs e)
    {
        this.Close();
    }

    private async void OpenFolder_Click(object sender, RoutedEventArgs e)
    {

        OpenFolderDialog ofd = new OpenFolderDialog();
        ofd.Title = "Ordner öffnen";

        var result = await ofd.ShowAsync(this);
        
        Console.WriteLine(result);

        if (!string.IsNullOrEmpty(result))
        {
            LoadImageFilesInFolder(result.ToString());
            if(_imageFilesInFolder.Length > 0){
                await OpenImageFileAsync(_imageFilesInFolder[0]);
            } else {
                Console.WriteLine("No images found in folder");
            }
            
        }

    }

    private async void BtnContextMenu_Click(object sender, RoutedEventArgs e)
    { 
        MainContextMenu.Open(this);
    }

    private async void OpenFile_Click(object sender, RoutedEventArgs e)
    {

        var opts = new FilePickerOpenOptions();
        opts.Title = "Bild öffnen";
        opts.AllowMultiple = false;

        var type = new FilePickerFileType("Bilddateien (*.png; *.jpg; *.jpeg; *.webp)");
        type.Patterns = new string[] { "*.png", "*.jpg", "*.jpeg", "*.webp" };
        opts.FileTypeFilter = new FilePickerFileType[] { type };

        IReadOnlyList<IStorageFile> paths = await StorageProvider.OpenFilePickerAsync(opts);

        if (paths?.Count > 0)
        {

            LoadImageFilesInFolder(Path.GetDirectoryName(paths[0].TryGetLocalPath()));
            Console.WriteLine(Path.GetDirectoryName(paths[0].TryGetLocalPath()));
            OpenImageFileAsync(paths[0].TryGetLocalPath());
        }
        //OpenFileDialog ofd = new OpenFileDialog();
        //ofd.Title = "Bild öffnen";
        //ofd.Filters.Add(new FileDialogFilter());
    }
}
