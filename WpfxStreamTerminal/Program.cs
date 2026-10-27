using Microsoft.Extensions.DependencyInjection;
using System.Windows;
using WpfxStreamTerminal.ViewModels;

namespace WpfxStreamTerminal;

public class Program
{
    [STAThread]
    public static void Main()
    {
        var services = new ServiceCollection();
        services.AddSingleton<MainViewModel>();
        services.AddSingleton<MainWindow>();

        var sp = services.BuildServiceProvider();

        var app = new Application();

        var win = sp.GetRequiredService<MainWindow>();

        app.Run(win);
    }
}
