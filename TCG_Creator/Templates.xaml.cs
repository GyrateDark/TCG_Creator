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

        private bool captureNextMouseEvent = false;
        bool mouseDown = false; // Set to 'true' when mouse is held down.
        Point mouseDownPos; // The point where the mouse button was clicked down.

        private void Grid_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (!captureNextMouseEvent)
            {
                return;
            }
            // Capture and track the mouse.
            mouseDown = true;
            mouseDownPos = e.GetPosition(theGrid);
            theGrid.CaptureMouse();

            // Initial placement of the drag selection box.         
            Canvas.SetLeft(selectionBox, mouseDownPos.X);
            Canvas.SetTop(selectionBox, mouseDownPos.Y);
            selectionBox.Width = 0;
            selectionBox.Height = 0;

            // Make the drag selection box visible.
            selectionBox.Visibility = Visibility.Visible;
        }

        private void Grid_MouseUp(object sender, MouseButtonEventArgs e)
        {
            if (mouseDown)
            {
                // Release the mouse capture and stop tracking it.
                mouseDown = false;
                captureNextMouseEvent = false;
                theGrid.ReleaseMouseCapture();

                // Hide the drag selection box.
                selectionBox.Visibility = Visibility.Collapsed;

                Point mouseUpPos = e.GetPosition(theGrid);

                View_Model model = (View_Model)DataContext;

                Rect rect = new Rect(mouseDownPos, mouseUpPos);

                rect.X /= model.CardRenderWidth;
                rect.Y /= model.CardRenderHeight;
                rect.Width /= model.CardRenderWidth;
                rect.Height /= model.CardRenderHeight;

                But_Delete_Region.IsEnabled = true;
                model.AddNewCardRegion(rect);
            }
        }

        private void Grid_MouseMove(object sender, MouseEventArgs e)
        {
            if (mouseDown)
            {
                // When the mouse is held down, reposition the drag selection box.

                Point mousePos = e.GetPosition(theGrid);

                if (mouseDownPos.X < mousePos.X)
                {
                    Canvas.SetLeft(selectionBox, mouseDownPos.X);
                    selectionBox.Width = mousePos.X - mouseDownPos.X;
                }
                else
                {
                    Canvas.SetLeft(selectionBox, mousePos.X);
                    selectionBox.Width = mouseDownPos.X - mousePos.X;
                }

                if (mouseDownPos.Y < mousePos.Y)
                {
                    Canvas.SetTop(selectionBox, mouseDownPos.Y);
                    selectionBox.Height = mousePos.Y - mouseDownPos.Y;
                }
                else
                {
                    Canvas.SetTop(selectionBox, mousePos.Y);
                    selectionBox.Height = mouseDownPos.Y - mousePos.Y;
                }
            }

        }

        private void But_Add_Region_Click(object sender, RoutedEventArgs e)
        {
            captureNextMouseEvent = true;

            But_Delete_Region.IsEnabled = false;
        }

        private void But_Delete_Region_Click(object sender, RoutedEventArgs e)
        {
            View_Model model = (View_Model)DataContext;

            model.DeleteNextSelectedRegion = true;
        }
    }
}
