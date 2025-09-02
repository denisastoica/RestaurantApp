using System.Windows.Controls;
using Restaurant.ViewModels;

namespace Restaurant.Views
{
    public partial class OrdersPage : Page
    {
        public OrdersPage(OrdersViewModel vm)
        {
            InitializeComponent();
            DataContext = vm;
        }
    }
}
