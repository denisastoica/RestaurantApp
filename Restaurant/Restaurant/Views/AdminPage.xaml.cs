using System.Windows.Controls;
using Restaurant.Services;
using Restaurant.ViewModels;

namespace Restaurant.Views
{
    public partial class AdminPage : Page
    {
        public AdminPage(AdminViewModel vm)
        {
            InitializeComponent();
            DataContext = vm;

            // Folosește opțiunile și serviciile din vm, nu doar contextul!
            var ordersService = new OrderService(vm.DbOptions, vm.ConfigService);
            var ordersVm = new AdminOrdersViewModel(vm.DbContext, ordersService);

            var ordersPage = new AdminOrdersPage();
            ordersPage.DataContext = ordersVm;
            AdminOrdersFrame.Content = ordersPage;

            var stockPage = new LowStockProductsPage();
            stockPage.DataContext = new LowStockProductsViewModel(vm.DbContext, vm.ConfigService);
            LowStockFrame.Content = stockPage;
        }
    }
}
