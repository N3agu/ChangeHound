using System.Windows;
using ChangeHound.Helpers;

namespace ChangeHound.Behaviors {
    public static class TrayIconBehavior {
        public static readonly DependencyProperty IsEnabledProperty =
            DependencyProperty.RegisterAttached("IsEnabled", typeof(bool), typeof(TrayIconBehavior), new PropertyMetadata(false, OnIsEnabledChanged));
        
        public static void SetIsEnabled(DependencyObject obj, bool value) => obj.SetValue(IsEnabledProperty, value);

        private static void OnIsEnabledChanged(DependencyObject d, DependencyPropertyChangedEventArgs e) {
            if (d is not Window window) return;

            if ((bool)e.NewValue) {
                TrayIconHelper? existingHelper = GetTrayHelper(window);
                if (existingHelper == null) SetTrayHelper(window, new TrayIconHelper(window));
            } else {
                TrayIconHelper? helper = GetTrayHelper(window);
                helper?.Dispose();
                SetTrayHelper(window, null);
            }
        }

        private static readonly DependencyProperty TrayHelperProperty =
            DependencyProperty.RegisterAttached("TrayHelper", typeof(TrayIconHelper), typeof(TrayIconBehavior));

        private static TrayIconHelper GetTrayHelper(DependencyObject obj) => (TrayIconHelper)obj.GetValue(TrayHelperProperty);
        private static void SetTrayHelper(DependencyObject obj, TrayIconHelper? value) => obj.SetValue(TrayHelperProperty, value);
    }
}