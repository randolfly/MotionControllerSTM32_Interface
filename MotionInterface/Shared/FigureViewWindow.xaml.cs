using System.Diagnostics;
using System.Globalization;
using System.Windows.Input;
using System.Windows.Threading;
using ScottPlot;
using ScottPlot.Plottables;

namespace MotionInterface.Shared;

/// <summary>
///     Interaction logic for FigureViewWindow.xaml
/// </summary>
public partial class FigureViewWindow : System.Windows.Window
{
    private const int Maxlength = 500_0000;
    // private const int Maxlength = 500;

    public static double LogPeriod = 5;
    private readonly DispatcherTimer _renderTimer;

    private int _nextDataIndex;
    // todo: change to ring-buffer and add stream storage functionality
    private readonly double[] LogData = new double[Maxlength];

    public FigureViewWindow()
    {
        InitializeComponent();
        DataContext = this;

        MaxRenderLengthTextBox.Text = (MaxRenderLength * LogPeriod).ToString(CultureInfo.InvariantCulture);
        SignalPlot = FigureView.Plot.Add.Signal(LogData);
        ScottPlot.TickGenerators.NumericAutomatic customTickGenerator = new()
        {
            LabelFormatter = CustomTickFormatter
        };
        FigureView.Plot.Axes.Bottom.TickGenerator = customTickGenerator;
        FigureView.Refresh();

        // create a timer to update the GUI
        _renderTimer = new DispatcherTimer
        {
            Interval = TimeSpan.FromMilliseconds(20)
        };
        _renderTimer.Tick += UpdateFigure!;
        _renderTimer.Start();
    }


    private int MaxRenderLength { get; set; } = 2_000;

    // private Crosshair Crosshair { get; }
    private Signal SignalPlot { get; set; }

    private void UpdateFigure(object sender, EventArgs e)
    {
        FigureView.Plot.Axes.AutoScale();
        FigureView.Refresh();
    }

    public void ScaleRangeToMax()
    {
        SignalPlot.MaxRenderIndex = _nextDataIndex - 1;
        SignalPlot.MinRenderIndex = 0;
        FigureView.Plot.Axes.AutoScale();
        FigureView.Refresh();
        _renderTimer.Stop();
    }

    // create a custom formatter as a static class
    private static string CustomTickFormatter(double position)
    {
        return $"{position * LogPeriod}";
    }

    public void UpdateDataList(List<float> data)
    {
        data.ForEach(UpdateData);
    }
    
    public void UpdateData(float data)
    {
        if (_nextDataIndex >= Maxlength)
        {
            // todo: throw new OverflowException("data array isn't long enough to accomodate new data");
            // clear all data and begin new figure
            _nextDataIndex = 0;
        }
        
        LogData[_nextDataIndex] = data;
        SignalPlot.MaxRenderIndex = _nextDataIndex;
        if (_nextDataIndex > MaxRenderLength)
            SignalPlot.MinRenderIndex = _nextDataIndex - MaxRenderLength;
        else
            SignalPlot.MinRenderIndex = 0;

        _nextDataIndex += 1;

        Debug.WriteLine($"nextDataIndex: {_nextDataIndex}");
    }


    // private void OnMouseMove(object sender, MouseEventArgs e)
    // {
    //     // var pixelX = (int)e.MouseDevice.GetPosition(FigureView).X;
    //     // var pixelY = (int)e.MouseDevice.GetPosition(FigureView).Y;
    //
    //     var (coordinateX, coordinateY) = FigureView.GetMouseCoordinates();
    //
    //     Crosshair.X = coordinateX;
    //     Crosshair.Y = coordinateY;
    //
    //     FigureView.Refresh();
    // }

    // private void OnMouseEnter(object sender, MouseEventArgs e)
    // {
    //     Crosshair.IsVisible = true;
    // }

    // private void OnMouseLeave(object sender, MouseEventArgs e)
    // {
    //     Crosshair.IsVisible = false;
    // }

    public void CloseWindow()
    {
        Close();
    }

    private void OnMaxRenderLengthChanged(object sender, KeyEventArgs e)
    {
        if (e.Key == Key.Enter)
            MaxRenderLength = Convert.ToInt32(double.Parse(MaxRenderLengthTextBox.Text) / LogPeriod);
    }
}