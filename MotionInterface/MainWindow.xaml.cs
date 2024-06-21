using Microsoft.Extensions.DependencyInjection;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using MotionInterface.Lib.Service;

namespace MotionInterface;

/// <summary>
/// Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();

        var serviceCollection = new ServiceCollection();
        serviceCollection.AddWpfBlazorWebView();
        serviceCollection.AddMasaBlazor();
        serviceCollection.AddSingleton<CommandCommunicationService>();
        serviceCollection.AddSingleton<DataCommunicationService>();
        serviceCollection.AddSingleton<DataLogService>();

#if DEBUG
        serviceCollection.AddBlazorWebViewDeveloperTools();
#endif

        Resources.Add("services", serviceCollection.BuildServiceProvider());
    }
}