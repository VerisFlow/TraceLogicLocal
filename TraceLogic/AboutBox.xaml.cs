using System;
using System.Reflection;
using System.Windows;

namespace TraceLogic
{
    public partial class AboutBox : Window
    {
        public AboutBox()
        {
            InitializeComponent();

            // This ensures the AboutBox uses the same icon as the MainWindow
            if (Application.Current.MainWindow != null && Application.Current.MainWindow != this)
            {
                this.Icon = Application.Current.MainWindow.Icon;
            }

            // Get the assembly version and display it
            Version? version = Assembly.GetExecutingAssembly().GetName().Version; // CORRECTED LINE

            if (version != null)
            {
                VersionTextBlock.Text = $"Version {version.Major}.{version.Minor}.{version.Build}";
            }
        }
    }
}