using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using System.Xml.Serialization;

namespace TCG_Creator
{
    public class Serializable_Brush
    {
        public Serializable_Brush()
        {
            brush = null;
        }

        public Serializable_Brush(Brush pbrush)
        {
            brush = pbrush;
        }

        [XmlIgnore]
        public Brush brush { get; set; }

        [XmlElement("Brush")]
        public string SerializeTypefaceAttribute
        {
            get
            {
                return BrushXmlConverter.ConvertToString(brush);
            }
            set
            {
                brush = BrushXmlConverter.ConvertToBrush(value);
            }
        }

        static public implicit operator Brush(Serializable_Brush serial_brush)
        {
            if (serial_brush == null)
                return null;
            else
                return serial_brush.brush;
        }

        static public implicit operator Serializable_Brush(Brush pbrush)
        {
            return pbrush;
        }
    }

    public static class BrushXmlConverter
    {
        public static string ConvertToString(Brush pbrush)
        {
            try
            {
                if (pbrush != null)
                {
                    TypeConverter converter = TypeDescriptor.GetConverter(typeof(Brush));
                    return converter.ConvertToString(pbrush);
                }
                else
                    return null;
            }
            catch { System.Diagnostics.Debug.WriteLine("Unable to convert"); }
            return null;
        }
        public static Brush ConvertToBrush(string brushString)
        {
            try
            {
                TypeConverter converter = TypeDescriptor.GetConverter(typeof(Brush));
                return (Brush)converter.ConvertFromString(brushString);
            }
            catch { System.Diagnostics.Debug.WriteLine("Unable to convert"); }
            return null;
        }
    }
}
