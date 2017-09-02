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
    public enum InheritTextOptions
    {
        AddInherittedTextAfter,
        AddInherittedTextBefore,
        UseOnlyInherittedText,
        UseOnlyLocalText,
        N_INHERIT_TEXT_OPTIONS
    }

    public class String_Container
    {
        public InheritTextOptions InheritText { get; set; } = InheritTextOptions.UseOnlyInherittedText;

        public List<String_Drawing> strings = new List<String_Drawing>();
        public TextAlignment TxtAlign { get; set; } = TextAlignment.Left;
        public String_Properties stringProperties = new String_Properties(true);

        public string GetAllStrings()
        {
            string result = "";
            foreach (String_Drawing i in strings)
            {
                result += i.Text;
            }

            return result;
        }

        public FormattedText ConvertToFormattedText(double PPI)
        {
            FormattedText result = new FormattedText(GetAllStrings(), CultureInfo.GetCultureInfo("en-us"), FlowDirection.LeftToRight, new Typeface("Times New Roman"), 32, Brushes.White);

            int startPosition = 0;

            foreach (String_Drawing i in strings)
            {
                int stopPosition = i.Text.Length;

                if (!stringProperties.IsValid())
                {
                    throw new ArgumentException("String Properties are not valid");
                }

                FontFamily fontFamily = new FontFamily(stringProperties.FontFamily);
                if (stringProperties.FontFamily.IndexOf("#") == 0)
                {
                    fontFamily = new FontFamily(new Uri("pack://application:,,,/"), "/Resources/Fonts/"+stringProperties.FontFamily);
                }

                result.SetFontFamily(fontFamily, startPosition, stopPosition);
                result.SetFontSize(stringProperties.FontSize*PPI/300, startPosition, stopPosition);
                result.SetFontWeight(stringProperties.SFontWeight, startPosition, stopPosition);
                result.SetFontStyle(stringProperties.SFontStyle, startPosition, stopPosition);

                result.SetForegroundBrush(stringProperties.TextBrush, startPosition, stopPosition);
                startPosition = stopPosition;
            }

            result.TextAlignment = TxtAlign;

            return result;
        }

        public void SetAllInheritValues(bool val)
        {
            if (val)
            {
                InheritText = InheritTextOptions.UseOnlyInherittedText;
            }
            else
            {
                InheritText = InheritTextOptions.UseOnlyLocalText;
            }

            foreach(String_Drawing i in strings)
            {
                i.properties.SetAllInheritValues(val);
            }
            stringProperties.SetAllInheritValues(val);
        }

        public String_Container GetInherittedPropertiesMerging(String_Container inherittedSource)
        {
            String_Container result = Clone();

            result.stringProperties = result.stringProperties.GetInherittedPropertiesMerging(inherittedSource.stringProperties);
            
            if (result.InheritText == InheritTextOptions.AddInherittedTextAfter)
            {
                result.strings.AddRange(inherittedSource.strings);
                result.InheritText = inherittedSource.InheritText;
            }
            else if (result.InheritText == InheritTextOptions.AddInherittedTextBefore)
            {
                result.strings.InsertRange(0, inherittedSource.strings);
                result.InheritText = inherittedSource.InheritText;
            }
            else if (result.InheritText == InheritTextOptions.UseOnlyInherittedText)
            {
                result.strings = inherittedSource.strings;
                result.InheritText = inherittedSource.InheritText;
            }
            else if (result.InheritText == InheritTextOptions.UseOnlyLocalText)
            {

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

        public String_Properties properties = new String_Properties(true);

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
        public String_Properties()
        {
            
        }

        public String_Properties(bool initCollection)
        {
            if (initCollection)
            {
                GradientTextBrushStops = DefaultGradientTextBrushStops;
            }
        }

        public string FontFamily { get; set; } = "Times New Roman";
        public double FontSize { get; set; } = 32;
        public FontStyle SFontStyle { get; set; } = FontStyles.Normal;
        public FontWeight SFontWeight { get; set; } = FontWeights.Normal;

        public TextBrushMode TextBrushColorMode = TextBrushMode.SolidColor;

        public GradientStopCollection GradientTextBrushStops { get; set; } = new GradientStopCollection();
        public double GradientTextBrushAngle { get; set; } = 90;

        public Color SolidColorTextBrushColor { get; set; } = Colors.White;

        public bool InheritTextVerticalAlignment { get; set; } = true;
        public VerticalAlignment TextVerticalAlignment { get; set; } = VerticalAlignment.Center;
        public bool InheritTextHorizontalAlignment { get; set; } = true;
        public HorizontalAlignment TextHorizontalAlignment { get; set; } = HorizontalAlignment.Left;

        public bool InheritStrokeProperties { get; set; } = true;
        public bool StrokeOn { get; set; } = true;
        public double StrokeThickness { get; set; } = 3.0;
        public Color TextStrokeColor { get; set; } = Colors.Black;

        [XmlIgnore]
        public GradientStopCollection DefaultGradientTextBrushStops
        {
            get
            {
                return new GradientStopCollection
            {
                new GradientStop(Colors.Orange, 0),
                new GradientStop(Colors.Red, 1)
            };
            }
        }

        [XmlIgnore]
        public Brush TextBrush
        {
            get
            {
                if (TextBrushColorMode == TextBrushMode.Gradient)
                {
                    return new LinearGradientBrush(GradientTextBrushStops, GradientTextBrushAngle);
                }
                else
                {
                    return new SolidColorBrush(SolidColorTextBrushColor);
                }
            }
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
                result.GradientTextBrushStops = inherittedSource.GradientTextBrushStops.Clone();
                result.GradientTextBrushAngle = inherittedSource.GradientTextBrushAngle;
                result.SolidColorTextBrushColor = inherittedSource.SolidColorTextBrushColor;
                result.TextBrushColorMode = inherittedSource.TextBrushColorMode;
                result.InheritFontBrush = inherittedSource.InheritFontBrush;
            }
            if (result.InheritStrokeProperties)
            {
                result.TextStrokeColor = inherittedSource.TextStrokeColor;
                result.StrokeOn = inherittedSource.StrokeOn;
                result.StrokeThickness = inherittedSource.StrokeThickness;
                result.InheritStrokeProperties = inherittedSource.InheritStrokeProperties;
            }
            if (result.InheritTextHorizontalAlignment)
            {
                result.TextHorizontalAlignment = inherittedSource.TextHorizontalAlignment;
                result.InheritTextHorizontalAlignment = inherittedSource.InheritTextHorizontalAlignment;
            }
            if (result.InheritTextVerticalAlignment)
            {
                result.TextVerticalAlignment = inherittedSource.TextVerticalAlignment;
                result.InheritTextVerticalAlignment = inherittedSource.InheritTextVerticalAlignment;
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
            InheritFontBrush = val;
            InheritStrokeProperties = val;
            InheritTextHorizontalAlignment = val;
            InheritTextVerticalAlignment = val;
        }

        public String_Properties Clone()
        {
            String_Properties result = (String_Properties)MemberwiseClone();

            result.FontFamily = (string)FontFamily.Clone();
            result.GradientTextBrushStops = GradientTextBrushStops.Clone();

            return result;
        }
    }

}
