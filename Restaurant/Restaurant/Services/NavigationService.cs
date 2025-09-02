using System;
using System.Windows.Controls;
using Microsoft.Extensions.DependencyInjection;

namespace Restaurant.Services
{
    public class NavigationService
    {
        private readonly IServiceProvider _provider;
        public Frame? Frame { get; set; }

        public NavigationService(IServiceProvider provider)
        {
            _provider = provider;
        }
        public void Navigate<TPage>() where TPage : Page
        {
            if (Frame == null)
                throw new InvalidOperationException("Frame nu este setat.");

            var page = _provider.GetRequiredService<TPage>();
            Frame.Navigate(page);
        }
        public void GoBack()
        {
            if (Frame == null)
                throw new InvalidOperationException("Frame nu este setat.");

            if (Frame.CanGoBack)
                Frame.GoBack();
        }
    }
}
