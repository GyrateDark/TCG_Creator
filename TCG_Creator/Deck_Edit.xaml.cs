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
    /// Interaction logic for Deck_Edit.xaml
    /// </summary>
    public partial class Deck_Edit : Page
    {
        public Deck_Edit(object context)
        {
            DataContext = context;
            InitializeComponent();

            lbl_Deck_Name.Content = "DECK NAME";
            lbl_Game_Name.Content = "GAME NAME";
        }
    }
}
