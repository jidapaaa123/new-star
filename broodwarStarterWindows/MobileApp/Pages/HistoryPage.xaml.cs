using MobileApp.ViewModels;
using Microsoft.Maui.Controls;

namespace MobileApp.Pages
{
    public partial class HistoryPage : ContentPage
    {
        public HistoryPage(HistoryPageViewModel vm)
        {
            InitializeComponent();
            BindingContext = vm;
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();
            if (BindingContext is HistoryPageViewModel vm)
            {
                await vm.LoadMatchesCommand.ExecuteAsync(null);
            }
        }
    }
}

