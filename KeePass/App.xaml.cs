using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Navigation;
using KeePass.Data;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;

namespace KeePass
{
    public partial class App
    {
        public PhoneApplicationFrame RootFrame { get; private set; }

        public App()
        {
            UnhandledException += Application_UnhandledException;

            if (Debugger.IsAttached)
                Current.Host.Settings.EnableFrameRateCounter = true;

            InitializeComponent();
            InitializePhoneApplication();
        }

        public static void Quit()
        {
            throw new QuitException();
        }

        private void RootFrame_NavigationFailed(
            object sender, NavigationFailedEventArgs e)
        {
            if (Debugger.IsAttached)
                Debugger.Break();
        }

        #region Phone application initialization

        private bool _phoneApplicationInitialized;

        private void InitializePhoneApplication()
        {
            if (_phoneApplicationInitialized)
                return;

            RootFrame = new TransitionFrame();
            RootFrame.Navigated += CompleteInitializePhoneApplication;

            RootFrame.NavigationFailed += RootFrame_NavigationFailed;

            _phoneApplicationInitialized = true;
        }

        private void CompleteInitializePhoneApplication(
            object sender, NavigationEventArgs e)
        {
            RootVisual = RootFrame;
            RootFrame.Navigated -= CompleteInitializePhoneApplication;
        }

        #endregion

        private void Application_Activated(object sender, ActivatedEventArgs e)
        {
            LifeCycle.Activated();
        }

        private void Application_Closing(object sender, ClosingEventArgs e)
        {
            LifeCycle.Closing();
        }

        private void Application_Deactivated(object sender, DeactivatedEventArgs e)
        {
            LifeCycle.Deactivated();
        }

        private void Application_Launching(object sender, LaunchingEventArgs e)
        {
            LifeCycle.Launching();
        }

        private void Application_UnhandledException(
            object sender, ApplicationUnhandledExceptionEventArgs e)
        {
            if (e.ExceptionObject is QuitException)
                return;

            if (Debugger.IsAttached)
                Debugger.Break();
        }

        private class QuitException : Exception {}
    }
}