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
using System.Windows.Shapes;

namespace TCG_Creator
{
    /// <summary>
    /// Interaction logic for Template_Edit.xaml
    /// </summary>
    public partial class CardDisplay : VirtualizingStackPanel
    {
        private bool addNewCardRegion = false;
        private bool moveCardRegion = false;
        Point originalMousePosition;
        View_Model model;

        public CardDisplay()
        {
            InitializeComponent();
        }

        private void Main_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (model == null)
            {
                model = (View_Model)DataContext;
            }
            if (model.IsAddingNewCardRegion)
            {
                addNewCardRegion = true;
                originalMousePosition = e.GetPosition(Main);

                model.SelectionRegionLeft = originalMousePosition.X;
                model.SelectionRegionTop = originalMousePosition.Y;
                selectionBox.Width = 0;
                selectionBox.Height = 0;

                selectionBox.Visibility = Visibility.Visible;
            }
            else if (model.IsMovingSelectedCardRegion)
            {
                moveCardRegion = true;
                originalMousePosition = e.GetPosition(Main);

                model.SelectionRegionLeft = originalMousePosition.X;
                model.SelectionRegionTop = originalMousePosition.Y;
                selectionBox.Width = 0;
                selectionBox.Height = 0;

                selectionBox.Visibility = Visibility.Visible;
            }

        }

        private void Main_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (addNewCardRegion)
            {
                addNewCardRegion = false;
                selectionBox.Visibility = Visibility.Collapsed;
                model.SelectionRegionWidth = selectionBox.Width;
                model.SelectionRegionHeight = selectionBox.Height;
                model.AddNewCardRegion();
            }
            else if (moveCardRegion)
            {
                moveCardRegion = false;
                selectionBox.Visibility = Visibility.Collapsed;
                model.SelectionRegionWidth = selectionBox.Width;
                model.SelectionRegionHeight = selectionBox.Height;
                model.MoveSelectedRegion();
            }
        }

        private void Main_MouseMove(object sender, MouseEventArgs e)
        {
            if (addNewCardRegion || moveCardRegion)
            {
                Point mousePos = e.GetPosition(Main);

                if (originalMousePosition.X < mousePos.X)
                {
                    Canvas.SetLeft(selectionBox, originalMousePosition.X);
                    selectionBox.Width = mousePos.X - originalMousePosition.X;
                }
                else
                {
                    Canvas.SetLeft(selectionBox, mousePos.X);
                    selectionBox.Width = originalMousePosition.X - mousePos.X;
                }

                if (originalMousePosition.Y < mousePos.Y)
                {
                    Canvas.SetTop(selectionBox, originalMousePosition.Y);
                    selectionBox.Height = mousePos.Y - originalMousePosition.Y;
                }
                else
                {
                    Canvas.SetTop(selectionBox, mousePos.Y);
                    selectionBox.Height = originalMousePosition.Y - mousePos.Y;
                }
            }
        }

        
    }
}
