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
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        OpenFileDialog dia_open = new OpenFileDialog();
        SaveFileDialog dia_save = new SaveFileDialog();

        public MainWindow(object context)
        {
            DataContext = context;

            InitializeComponent();

            dia_save.Title = "Save Card Database";
            dia_save.DefaultExt = "xml";
            dia_save.AddExtension = true;
            dia_save.Filter = "XML File | *.xml";

            dia_open.Title = "Open Card Database";
            dia_open.DefaultExt = "xml";
            dia_open.Filter = "XML File | *.xml";
        }

        private void Menu_Save_Click(object sender, RoutedEventArgs e)
        {
            dia_save.ShowDialog();

            View_Model model = (View_Model)DataContext;

            try
            {
                model.Xml_Load(dia_save.FileName, true);
            }
            catch
            {
                MessageBox.Show("Error Saving File");
            }
        }

        private void Menu_Open_Click(object sender, RoutedEventArgs e)
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
