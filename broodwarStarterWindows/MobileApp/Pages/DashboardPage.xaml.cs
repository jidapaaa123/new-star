using MobileApp.ViewModels;
using Microsoft.Maui.Controls;

namespace MobileApp.Pages
{
    public partial class DashboardPage : ContentPage
    {
        public DashboardPage(DashboardPageViewModel vm)
        {
            BindingContext = vm;
            InitializeComponent();
        }

        protected override async void OnDisappearing()
        {
            base.OnDisappearing();

            // Properly dispose the ViewModel when the page is navigated away
            if (BindingContext is DashboardPageViewModel vm)
            {
                await vm.DisposeAsync();
            }
        }
    }
}
