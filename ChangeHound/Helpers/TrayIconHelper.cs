﻿using ChangeHound.Common;
using ChangeHound.ViewModels;
using System.IO;
using System.Windows;

namespace ChangeHound.Helpers {
    public class TrayIconHelper : IDisposable {
        #region Fields
        private readonly Window _window;
        private readonly NotifyIcon _notifyIcon;
        private bool _isExiting = false;
        #endregion

        #region Constructor & Lifecycle
        public TrayIconHelper(Window window) {
            _window = window;
            _window.Closing += OnClosing;

            _notifyIcon = new NotifyIcon();
            _notifyIcon.DoubleClick += (s, e) => ShowWindow();

            try {
                Stream? iconStream = System.Windows.Application.GetResourceStream(new Uri("/logo.ico", UriKind.Relative)).Stream;
                _notifyIcon.Icon = new Icon(iconStream);
            }
            catch (Exception) {
                System.Windows.MessageBox.Show("Icon was not found in the application's resources");
            }

            _notifyIcon.ContextMenuStrip = new ContextMenuStrip();
            _notifyIcon.ContextMenuStrip.Items.Add("Show", null, (s, e) => ShowWindow());
            _notifyIcon.ContextMenuStrip.Items.Add("Exit", null, (s, e) => ExitApplication());

            // tray icon always visible
            _notifyIcon.Visible = true;

            EventAggregator.Subscribe<NotificationMessage>(ShowNotification);
        }

        public void Dispose() {
            _window.Closing -= OnClosing;
            _notifyIcon.Dispose();
        }
        #endregion

        #region Private Methods
        private void ShowNotification(NotificationMessage message) {
            _notifyIcon.ShowBalloonTip(3000, message.Title, message.Message, ToolTipIcon.Info);
        }

        private void OnClosing(object? sender, System.ComponentModel.CancelEventArgs e) {
            // cleanup on intentional exit
            if (_isExiting) {
                _notifyIcon.Dispose();
                return;
            }

            if (_window.DataContext is not MainViewModel mainViewModel) return;

            if (mainViewModel.MinimizeToTray) {
                // cancel the close event
                e.Cancel = true;

                // hide the window
                _window.Hide();
            }
        }

        private void ShowWindow() {
            _window.Show();
            _window.WindowState = WindowState.Normal;
            _window.Activate();
        }

        private void ExitApplication() {
            _isExiting = true;
            System.Windows.Application.Current.Shutdown();
        }
        #endregion
    }
}
