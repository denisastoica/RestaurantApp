using System;
using Microsoft.Extensions.DependencyInjection;
using Restaurant.Views;

namespace Restaurant.ViewModels
{
    public class GuestHomePageViewModel
    {
        public object MeniuView { get; }
        public object CautareView { get; }

        public GuestHomePageViewModel(IServiceProvider provider)
        {
            MeniuView = provider.GetRequiredService<MenuPage>();
            CautareView = provider.GetRequiredService<CautareView>();
        }
    }
}
