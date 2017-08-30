using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Markup;
using System.Windows.Media;
using System.Xml;
using System.Xml.Serialization;

namespace TCG_Creator
{

    public enum TextBrushMode
    {
        Gradient,
        SolidColor
    }

    public class String_Container
    {
        public bool InheritText { get; set; } = true;

        public List<String_Drawing> strings = new List<String_Drawing>();
        public TextAlignment TxtAlign { get; set; } = TextAlignment.Left;
        public String_Properties stringProperties = new String_Properties();

        public string GetAllStrings()
        {
            string result = "";
            foreach (String_Drawing i in strings)
            {
                result += i.Text;
            }

            return result;
        }

        public FormattedText ConvertToFormattedText()
        {
            FormattedText result = new FormattedText(GetAllStrings(), CultureInfo.GetCultureInfo("en-us"), FlowDirection.LeftToRight, new Typeface("Times New Roman"), 32, Brushes.White);

            int startPosition = 0;

            foreach (String_Drawing i in strings)
            {
                int stopPosition = i.Text.Length + startPosition;

                if (!stringProperties.IsValid())
                {
                    throw new ArgumentException("String Properties are not valid");
                }

                result.SetFontFamily(stringProperties.FontFamily, startPosition, stopPosition);
                result.SetFontSize(stringProperties.FontSize, startPosition, stopPosition);
                result.SetFontWeight(stringProperties.SFontWeight, startPosition, stopPosition);
                result.SetFontStyle(stringProperties.SFontStyle, startPosition, stopPosition);

                result.SetForegroundBrush(stringProperties.TextBrush, startPosition, stopPosition);
            }

            result.TextAlignment = TxtAlign;

            return result;
        }

        public void SetAllInheritValues(bool val)
        {
            InheritText = val;

            foreach(String_Drawing i in strings)
            {
                i.properties.SetAllInheritValues(val);
            }
        }

        public String_Container GetInherittedPropertiesMerging(String_Container inherittedSource)
        {
            String_Container result = Clone();

            result.stringProperties = result.stringProperties.GetInherittedPropertiesMerging(inherittedSource.stringProperties);
            
            if (result.InheritText)
            {
                result.strings = inherittedSource.strings;
                result.InheritText = inherittedSource.InheritText;
            }

            return result;
        }

        public String_Container Clone()
        {
            String_Container result = (String_Container)MemberwiseClone();

            result.strings = new List<String_Drawing>(strings.Count);
            for (int i = 0; i < strings.Count; ++i)
            {
                result.strings.Add(strings[i].Clone());
            }

            return result;
        }
    }

    public class String_Drawing
    {
        public String_Drawing()
        {

        }
        public String_Drawing(string text)
        {
            Text = text;
        }

        public String_Properties properties = new String_Properties();

        public string Text { get; set; }

        public String_Drawing Clone()
        {
            String_Drawing result = (String_Drawing)MemberwiseClone();

            result.Text = (string)Text.Clone();
            result.properties = properties.Clone();

            return result;
        }
    }

    public class String_Properties
    {
        public string FontFamily { get; set; } = "Times New Roman";
        public double FontSize { get; set; } = 32;
        public FontStyle SFontStyle { get; set; } = FontStyles.Normal;
        public FontWeight SFontWeight { get; set; } = FontWeights.Normal;

        public TextBrushMode TextBrushColorMode = TextBrushMode.SolidColor;

        [XmlIgnore]
        public GradientBrush GradientTextBrush { get; set; } = new LinearGradientBrush(Colors.Orange, Colors.White, 90.0);

        [XmlIgnore]
        public SolidColorBrush SolidColorTextBrush { get; set; } = Brushes.White;

        [XmlIgnore]
        public Brush TextBrush
        {
            get
            {
                if (TextBrushColorMode == TextBrushMode.Gradient)
                {
                    return GradientTextBrush;
                }
                else
                {
                    return SolidColorTextBrush;
                }
            }
        }

        [Browsable(false), EditorBrowsable(EditorBrowsableState.Never)]
        [XmlElement("GradientTextBrush")]
        public string GradientTextBrushSerialized
        {
            get
            { // serialize
                if (GradientTextBrush == null) return null;
                return Serialize(GradientTextBrush);
            }
            set
            { // deserialize
                if (value == null)
                {
                    GradientTextBrush = null;
                }
                else
                {
                    GradientTextBrush = (GradientBrush)Deserialize(value);
                }
            }
        }

        [Browsable(false), EditorBrowsable(EditorBrowsableState.Never)]
        [XmlElement("SolidColorTextBrush")]
        public string SolidColorTextBrushSerialized
        {
            get
            { // serialize
                if (SolidColorTextBrush == null) return null;
                return Serialize(SolidColorTextBrush);
            }
            set
            { // deserialize
                if (value == null)
                {
                    SolidColorTextBrush = null;
                }
                else
                {
                    SolidColorTextBrush = (SolidColorBrush)Deserialize(value);
                }
            }
        }

        private string Serialize(object toSerialize)
        {
            XmlWriterSettings settings = new XmlWriterSettings();
            // You might want to wrap these in #if DEBUG's 
            settings.Indent = true;
            settings.NewLineOnAttributes = true;
            // this gets rid of the XML version 
            settings.ConformanceLevel = ConformanceLevel.Fragment;
            // buffer to a stringbuilder
            StringBuilder sb = new StringBuilder();
            XmlWriter writer = XmlWriter.Create(sb, settings);
            // Need moar documentation on the manager, plox MSDN
            XamlDesignerSerializationManager manager = new XamlDesignerSerializationManager(writer);
            manager.XamlWriterMode = XamlWriterMode.Expression;
            // its extremely rare for this to throw an exception
            XamlWriter.Save(toSerialize, manager);

            return sb.ToString();
        }

        /// <summary>
        /// Deserializes an object from xaml.
        /// </summary>
        /// <param name="xamlText">The xaml text.</param>
        /// <returns>The deserialized object</returns>
        /// <exception cref="XmlException">Thrown if the serialized text is not well formed XML</exception>
        /// <exception cref="XamlParseException">Thrown if unable to deserialize from xaml</exception>
        private object Deserialize(string xamlText)
        {
            XmlDocument doc = new XmlDocument();
            // may throw XmlException
            doc.LoadXml(xamlText);
            // may throw XamlParseException
            return XamlReader.Load(new XmlNodeReader(doc));
        }


        public bool InheritFontFamily { get; set; } = true;
        public bool InheritFontSize { get; set; } = true;
        public bool InheritFontStyle { get; set; } = true;
        public bool InheritFontWeight { get; set; } = true;

        public bool InheritFontBrush { get; set; } = true;

        public String_Properties GetInherittedPropertiesMerging(String_Properties inherittedSource)
        {
            String_Properties result = Clone();

            if (result.InheritFontSize)
            {
                result.FontSize = inherittedSource.FontSize;
                result.InheritFontSize = inherittedSource.InheritFontSize;
            }
            if (result.InheritFontFamily)
            {
                result.FontFamily = inherittedSource.FontFamily;
                result.InheritFontFamily = inherittedSource.InheritFontFamily;
            }
            if (result.InheritFontStyle)
            {
                result.SFontStyle = inherittedSource.SFontStyle;
                result.InheritFontStyle = inherittedSource.InheritFontStyle;
            }
            if (result.InheritFontWeight)
            {
                result.SFontWeight = inherittedSource.SFontWeight;
                result.InheritFontWeight = inherittedSource.InheritFontWeight;
            }
            if (result.InheritFontBrush)
            {
                result.GradientTextBrush = inherittedSource.GradientTextBrush.Clone();
                result.SolidColorTextBrush = inherittedSource.SolidColorTextBrush.Clone();
                result.TextBrushColorMode = inherittedSource.TextBrushColorMode;
                result.InheritFontBrush = inherittedSource.InheritFontBrush;
            }

            return result;
        }

        public bool IsValid()
        {
            return !(FontFamily == "" || FontSize == 0 || TextBrush == null);
        }

        public void SetAllInheritValues(bool val)
        {
            InheritFontFamily = val;
            InheritFontSize = val;
            InheritFontStyle = val;
            InheritFontWeight = val;
        }

        public String_Properties Clone()
        {
            String_Properties result = (String_Properties)MemberwiseClone();

            result.FontFamily = (string)FontFamily.Clone();
            result.GradientTextBrush = GradientTextBrush.Clone();
            result.SolidColorTextBrush = SolidColorTextBrush.Clone();

            return result;
        }
    }

}
