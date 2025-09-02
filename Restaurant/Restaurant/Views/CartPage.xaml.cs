
using System.Windows.Controls;           // <— pentru Page
using Restaurant.ViewModels;            // <— pentru CartViewModel

namespace Restaurant.Views                // exact acelaşi namespace ca în XAML
{
    public partial class CartPage : Page  // moşteneşte Page
    {
        public CartPage(CartViewModel vm)
        {
            InitializeComponent();
            this.DataContext = vm;         // this.DataContext e disponibil pe Page
        }
    }
}
