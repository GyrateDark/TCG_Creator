using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace TCG_Creator
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        Deck_Edit page_Deck_Edit;
        Landing page_Landing;
        Templates page_Templates;

        public MainWindow(object context)
        {
            DataContext = context;
            page_Deck_Edit = new Deck_Edit(context);
            page_Landing = new Landing(context);
            page_Templates = new Templates(context);

            InitializeComponent();

            Main_Frame.Navigate(page_Landing);
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            Main_Frame.Navigate(page_Deck_Edit);
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            Main_Frame.Navigate(page_Landing);
        }

        private void But_Templates_Click(object sender, RoutedEventArgs e)
        {
            Main_Frame.Navigate(page_Templates);
        }
    }
}
