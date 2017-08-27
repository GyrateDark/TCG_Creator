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
    public class Serializable_Typeface
    {
        public Serializable_Typeface()
        {
            typeface = null;
        }

        public Serializable_Typeface(Typeface face)
        {
            typeface = face;
        }

        [XmlIgnore]
        public Typeface typeface { get; set; }

        [XmlElement("Typeface")]
        public string SerializeTypefaceAttribute
        {
            get
            {
                return TypefaceXmlConverter.ConvertToString(typeface);
            }
            set
            {
                typeface = TypefaceXmlConverter.ConvertToTypeface(value);
            }
        }

        static public implicit operator Typeface(Serializable_Typeface serial_typeface)
        {
            return serial_typeface.typeface;
        }

        static public implicit operator Serializable_Typeface(Typeface typeface)
        {
            return new Serializable_Typeface(typeface);
        }
    }

    public static class TypefaceXmlConverter
    {
        public static string ConvertToString(Typeface face)
        {
            try
            {
                if (face != null)
                {
                    TypeConverter converter = TypeDescriptor.GetConverter(typeof(Typeface));
                    return converter.ConvertToString(face);
                }
                else
                    return null;
            }
            catch { System.Diagnostics.Debug.WriteLine("Unable to convert"); }
            return null;
        }
        public static Typeface ConvertToTypeface(string faceString)
        {
            try
            {
                TypeConverter converter = TypeDescriptor.GetConverter(typeof(Typeface));
                return (Typeface)converter.ConvertFromString(faceString);
            }
            catch { System.Diagnostics.Debug.WriteLine("Unable to convert"); }
            return null;
        }
    }
}
