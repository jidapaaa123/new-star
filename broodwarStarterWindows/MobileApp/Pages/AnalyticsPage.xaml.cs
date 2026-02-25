using MobileApp.ViewModels;
using Microsoft.Maui.Controls;

namespace MobileApp.Pages
{
    public partial class AnalyticsPage : ContentPage
    {
        public AnalyticsPage(AnalyticsPageViewModel vm)
        {
            InitializeComponent();
            BindingContext = vm;
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();
            if (BindingContext is AnalyticsPageViewModel vm)
            {
                await vm.RenderChartsAsync(WinLossChartContainer, WorkerChartContainer);
            }
        }
    }
}
