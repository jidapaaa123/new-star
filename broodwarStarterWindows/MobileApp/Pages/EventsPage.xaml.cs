using MobileApp.ViewModels;

namespace MobileApp.Pages;

public partial class EventsPage : ContentPage
{
    private readonly EventsPageViewModel _viewModel;

    public EventsPage(EventsPageViewModel viewModel)
    {
        InitializeComponent();
        _viewModel = viewModel;
        BindingContext = _viewModel;
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        _viewModel.RefreshEventsCommand.Execute(null);
    }
}
