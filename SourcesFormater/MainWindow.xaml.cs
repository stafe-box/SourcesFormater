// Copyright (c) Microsoft Corporation and Contributors.
// Licensed under the MIT License.

using Microsoft.UI.Composition.SystemBackdrops;
using Microsoft.UI.Xaml;
using System.Runtime.InteropServices; // For DllImport
using WinRT;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using Microsoft.UI.Composition;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.ApplicationModel.DataTransfer;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace SourcesFormater
{
    /// <summary>
    /// An empty window that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainWindow : Window
    {
        WindowsSystemDispatcherQueueHelper m_wsdqHelper; // See below for implementation.
        MicaController m_backdropController;
        SystemBackdropConfiguration m_configurationSource;
        private int _mode = 0;
        public MainWindow()
        {
            IntPtr hWnd = WinRT.Interop.WindowNative.GetWindowHandle(this);
            var windowId = Microsoft.UI.Win32Interop.GetWindowIdFromWindow(hWnd);
            var appWindow = Microsoft.UI.Windowing.AppWindow.GetFromWindowId(windowId);
            this.InitializeComponent();
            ExtendsContentIntoTitleBar = true;
            SetTitleBar(AppTitleBar);
            appWindow.Resize(new Windows.Graphics.SizeInt32 { Width = 400, Height = 500 });
            TrySetSystemBackdrop();
            Type.ItemsSource = new string[] { "Автоматический", "Ручной" };
        }

        private void myButton_Click(object sender, RoutedEventArgs e)
        {
            string url = URL.Text;
            switch (_mode)
            {
                case 0:
                    WebPageParser webPage = new WebPageParser(url);
                    Result.Text += webPage.GOST + "\n";
                    break;
                case 1:
                    string currentDate = DateTime.Now.ToString("dd.MM.yyyy");
                    Result.Text += (Title.Text + ". [Электронный ресурс]. URL: " + url + " Дата обращения: " + currentDate + "\n");
                    break;
            }
            Console.Beep();
        }

        private void ClearButton_Click(Object sender, RoutedEventArgs e) 
        {
            Result.Text = "";
        }

        bool TrySetSystemBackdrop()
        {
            if (Microsoft.UI.Composition.SystemBackdrops.MicaController.IsSupported())
            {
                m_wsdqHelper = new WindowsSystemDispatcherQueueHelper();
                m_wsdqHelper.EnsureWindowsSystemDispatcherQueueController();

                // Create the policy object.
                m_configurationSource = new SystemBackdropConfiguration();
                this.Activated += Window_Activated;
                this.Closed += Window_Closed;
                ((FrameworkElement)this.Content).ActualThemeChanged += Window_ThemeChanged;

                // Initial configuration state.
                m_configurationSource.IsInputActive = true;
                SetConfigurationSourceTheme();

                m_backdropController = new Microsoft.UI.Composition.SystemBackdrops.MicaController();

                // Enable the system backdrop.
                // Note: Be sure to have "using WinRT;" to support the Window.As<...>() call.
                m_backdropController.AddSystemBackdropTarget(this.As<Microsoft.UI.Composition.ICompositionSupportsSystemBackdrop>());
                m_backdropController.SetSystemBackdropConfiguration(m_configurationSource);
                return true; // succeeded
            }

            return false; // Mica is not supported on this system
        }

        private void Window_Activated(object sender, Microsoft.UI.Xaml.WindowActivatedEventArgs args)
        {
            m_configurationSource.IsInputActive = args.WindowActivationState != WindowActivationState.Deactivated;
        }

        private void Window_Closed(object sender, WindowEventArgs args)
        {
            // Make sure any Mica/Acrylic controller is disposed
            // so it doesn't try to use this closed window.
            if (m_backdropController != null)
            {
                m_backdropController.Dispose();
                m_backdropController = null;
            }
            this.Activated -= Window_Activated;
            m_configurationSource = null;
        }

        private void Window_ThemeChanged(FrameworkElement sender, object args)
        {
            if (m_configurationSource != null)
            {
                SetConfigurationSourceTheme();
            }
        }

        private void SetConfigurationSourceTheme()
        {
            switch (((FrameworkElement)this.Content).ActualTheme)
            {
                case ElementTheme.Dark: m_configurationSource.Theme = Microsoft.UI.Composition.SystemBackdrops.SystemBackdropTheme.Dark; break;
                case ElementTheme.Light: m_configurationSource.Theme = Microsoft.UI.Composition.SystemBackdrops.SystemBackdropTheme.Light; break;
                case ElementTheme.Default: m_configurationSource.Theme = Microsoft.UI.Composition.SystemBackdrops.SystemBackdropTheme.Default; break;
            }
        }
        private void FontsChanger()
        {
            var fonts = new Windows.Globalization.Fonts.LanguageFontGroup("ru-ru");
            var traditionalDocumentFont = fonts.TraditionalDocumentFont;
            var modernDocumentFont = fonts.ModernDocumentFont;

            // Obtain two properties of the traditional document font.
            var traditionalDocumentFontFontFamily = traditionalDocumentFont.FontFamily;   // "MS Mincho"
            var traditionalDocumentFontScaleFactor = traditionalDocumentFont.ScaleFactor; // 100

            // Obtain two properties of the modern document font.
            var modernDocumentFontFontFamily = modernDocumentFont.FontFamily;             // "Meiryo"
            var modernDocumentFontScaleFactor = modernDocumentFont.ScaleFactor;           // 90
        }

        private void Type_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            _mode = Type.SelectedIndex;
            switch (_mode)
            {
                case 0:
                    Title.Visibility = Visibility.Collapsed;
                    break;
                case 1:
                    Title.Visibility = Visibility.Visible;
                    break;
            }
        }

        private void Copy_Click(object sender, RoutedEventArgs e)
        {
            DataPackage data = new DataPackage();
            data.SetText(Result.Text);
            Clipboard.SetContent(data);
        }
    }
}
