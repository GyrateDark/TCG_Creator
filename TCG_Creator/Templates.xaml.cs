using Microsoft.Win32;
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
    /// Interaction logic for Templates.xaml
    /// </summary>
    public partial class Templates : Page
    {
        OpenFileDialog dia_open = new OpenFileDialog();
        SaveFileDialog dia_save = new SaveFileDialog();

        public Templates(object context)
        {
            DataContext = context;
            InitializeComponent();

            Tree_View.SelectedItemChanged += new RoutedPropertyChangedEventHandler<Object>(Tree_View_Selection_Changed);

            dia_save.Title = "Save Card Templates";
            dia_save.DefaultExt = "xml";
            dia_save.AddExtension = true;
            dia_save.Filter = "XML File | *.xml";

            dia_open.Title = "Load Card Templates";
            dia_open.DefaultExt = "xml";
            dia_open.Filter = "XML File | *.xml";
        }

        void Tree_View_Selection_Changed(Object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            View_Model model = (View_Model)DataContext;
            
            model.Tree_View_Selected_Item_Changed();
        }

        private void but_temp_save_Click(object sender, RoutedEventArgs e)
        {
            dia_save.ShowDialog();

            View_Model model = (View_Model)DataContext;

            model.Xml_Save(dia_save.FileName, true);
        }

        private void but_temp_load_Click(object sender, RoutedEventArgs e)
        {
            dia_open.ShowDialog();

            View_Model model = (View_Model)DataContext;

            try
            {
                model.Xml_Load(dia_open.FileName, true);
            }
            catch
            {
                MessageBox.Show("Error Loading File");
            }
        }
    }
}
