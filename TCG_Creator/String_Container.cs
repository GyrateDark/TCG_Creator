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
    public class String_Container
    {
        public List<String_Drawing> strings = new List<String_Drawing>();
        public TextAlignment TxtAlign { get; set; } = TextAlignment.Left;

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

                result.SetFontFamily(i.FontFamily, startPosition, stopPosition);
                result.SetFontSize(i.FontSize, startPosition, stopPosition);
                result.SetFontWeight(i.StrFontWeight, startPosition, stopPosition);
                result.SetFontStyle(i.StrFontStyle, startPosition, stopPosition);

                result.SetForegroundBrush(i.TextBrush, startPosition, stopPosition);
            }

            result.TextAlignment = TxtAlign;

            return result;
        }
    }

    public class String_Drawing
    {
        public String_Drawing()
        {
            init();
        }
        public String_Drawing(string text)
        {
            init();

            Text = text;
        }
        public String_Drawing(string text, int fontSize, string fontFamily, FontStyle fontStyle, FontWeight fontWeight, Brush brush)
        {
            init();

            FontFamily = fontFamily;
            FontSize = fontSize;
            StrFontStyle = fontStyle;
            StrFontWeight = fontWeight;

            TextBrush = brush;

            Text = text;
        }

        private void init()
        {
            FontFamily = "";
            FontSize = 32;
            StrFontStyle = FontStyles.Normal;
            StrFontWeight = FontWeights.Normal;

            TextBrush = Brushes.White;
            string tmp = Serialize(TextBrush);
            Text = "";
        }

        public string FontFamily { get; set; }
        public double FontSize { get; set; }
        public FontStyle StrFontStyle { get; set; }
        public FontWeight StrFontWeight { get; set; }

        public string Text { get; set; }

        [XmlIgnore]
        public Brush TextBrush { get; set; }

        [Browsable(false), EditorBrowsable(EditorBrowsableState.Never)]
        [XmlElement("TextBrush")]
        public string TextBrushSerialized
        {
            get
            { // serialize
                if (TextBrush == null) return null;
                return Serialize(TextBrush);
            }
            set
            { // deserialize
                if (value == null)
                {
                    TextBrush = null;
                }
                else
                {
                    TextBrush = (Brush)Deserialize(value);
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
    }
}
