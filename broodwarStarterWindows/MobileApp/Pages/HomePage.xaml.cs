using MobileApp.ViewModels;

namespace MobileApp.Pages
{
    public partial class HomePage : ContentPage
    {
        public HomePage(HomePageViewModel vm)
        {
            BindingContext = vm;
            InitializeComponent();
        }
    }
}