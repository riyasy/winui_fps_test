using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace winui_fps_test
{
    /// <summary>
    /// An empty window that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            this.Title = "FPS Test";

            // Set window size
            this.AppWindow.ResizeClient(new Windows.Graphics.SizeInt32(600, 400));
        }

        private void OnAnimatedCanvasTestClicked(object sender, RoutedEventArgs e)
        {
            var window = new AnimatedCanvasTestWindow();
            window.Activate();
        }

        private void OnCompTargetRenderingClicked(object sender, RoutedEventArgs e)
        {
            var window = new CompTargetRenderingAnimatorWindow();
            window.Activate();
        }

        private void OnDispatcherQueueTimerClicked(object sender, RoutedEventArgs e)
        {
            var window = new DispatcherQueueAnimatorWindow();
            window.Activate();
        }
    }
}
