using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;

namespace TCG_Creator
{
    public class PathCheckBox : CheckBox
    {
        public static DependencyProperty DataProperty = DependencyProperty.Register("Data", typeof(Geometry), typeof(PathCheckBox), new FrameworkPropertyMetadata(new PropertyChangedCallback(Data_Changed)));
        public static DependencyProperty CheckedDataProperty = DependencyProperty.Register("CheckedData", typeof(Geometry), typeof(PathCheckBox), new FrameworkPropertyMetadata(new PropertyChangedCallback(Data_Changed)));
        public static DependencyProperty StrokeDataProperty = DependencyProperty.Register("DataStroke", typeof(Brush), typeof(PathCheckBox), new FrameworkPropertyMetadata(new PropertyChangedCallback(Data_Changed)));

        public Geometry Data
        {
            get { return (Geometry)GetValue(DataProperty); }
            set { SetValue(DataProperty, value); }
        }
        public Geometry CheckedData
        {
            get { return (Geometry)GetValue(CheckedDataProperty); }
            set { SetValue(CheckedDataProperty, value); }
        }

        public Brush DataStroke
        {
            get { return (Brush)GetValue(StrokeDataProperty); }
            set { SetValue(StrokeDataProperty, value); }
        }

        private static void Data_Changed(DependencyObject o, DependencyPropertyChangedEventArgs args)
        {
            PathCheckBox thisClass = (PathCheckBox)o;
            thisClass.SetData();
        }

        private void SetData()
        {

        }
    }
}
