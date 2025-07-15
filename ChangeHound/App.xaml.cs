using System.Windows;
using ChangeHound.ViewModels;
using ChangeHound.Views;

namespace ChangeHound {
    public partial class App : System.Windows.Application {
        protected override async void OnStartup(StartupEventArgs e) {
            base.OnStartup(e);

            var mainView = new MainView();
            mainView.Show();

            // create async the MainViewModel
            mainView.DataContext = await MainViewModel.CreateAsync();
        }
    }
}
