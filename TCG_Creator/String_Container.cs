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

        public void SetAllInheritValues(InheritPriorities val)
        {
            if (val != InheritPriorities.DoNotInherit)
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

        public String_Container GetInherittedPropertiesMerging(String_Container inherittedSource, String_Container deck)
        {
            String_Container result = Clone();

            result.stringProperties = result.stringProperties.GetInherittedPropertiesMerging(inherittedSource.stringProperties, deck.stringProperties);
            
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

        public InheritPriorities InheritTextVerticalAlignment { get; set; } = InheritPriorities.InheritRegionFirst;
        public VerticalAlignment TextVerticalAlignment { get; set; } = VerticalAlignment.Center;
        public InheritPriorities InheritTextHorizontalAlignment { get; set; } = InheritPriorities.InheritRegionFirst;
        public HorizontalAlignment TextHorizontalAlignment { get; set; } = HorizontalAlignment.Left;

        public InheritPriorities InheritStrokeProperties { get; set; } = InheritPriorities.InheritRegionFirst;
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

        public InheritPriorities InheritFontFamily { get; set; } = InheritPriorities.InheritRegionFirst;
        public InheritPriorities InheritFontSize { get; set; } = InheritPriorities.InheritRegionFirst;
        public InheritPriorities InheritFontStyle { get; set; } = InheritPriorities.InheritRegionFirst;
        public InheritPriorities InheritFontWeight { get; set; } = InheritPriorities.InheritRegionFirst;

        public InheritPriorities InheritFontBrush { get; set; } = InheritPriorities.InheritRegionFirst;

        public String_Properties GetInherittedPropertiesMerging(String_Properties inherittedSource, String_Properties deck)
        {
            String_Properties result = Clone();

            GetInherittedPropertiesMergingOnce(result.InheritFontSize, GetInherittedPropertiesMergingFontSize, result, inherittedSource, deck);
            if (result.InheritFontSize == InheritPriorities.InheritRegionFirst)
            {
                GetInherittedPropertiesMergingFontSize(ref result, inherittedSource);
                if (inherittedSource.InheritFontSize == InheritPriorities.InheritDeckFirst)
                {
                    GetInherittedPropertiesMergingFontSize(ref result, deck);
                }
            }
            else if (result.InheritFontSize == InheritPriorities.InheritDeckFirst)
            {
                GetInherittedPropertiesMergingFontSize(ref result, deck);
                if (deck.InheritFontSize == InheritPriorities.InheritRegionFirst)
                {
                    GetInherittedPropertiesMergingFontSize(ref result, inherittedSource);
                }
            }
            if (result.InheritFontFamily == InheritPriorities.InheritRegionFirst)
            {
                result.FontFamily = inherittedSource.FontFamily;
                if (inherittedSource.InheritFontFamily == InheritPriorities.InheritDeckFirst)
                {
                    result.FontFamily = deck.FontFamily;
                }
            }
            else if (result.InheritFontFamily == InheritPriorities.InheritDeckFirst)
            {
                result.FontFamily = deck.FontFamily;
                if (deck.InheritFontFamily == InheritPriorities.InheritRegionFirst)
                {
                    result.FontFamily = inherittedSource.FontFamily;
                }
            }
            if (result.InheritFontStyle == InheritPriorities.InheritRegionFirst)
            {
                result.SFontStyle = inherittedSource.SFontStyle;
                if (inherittedSource.InheritFontStyle == InheritPriorities.InheritDeckFirst)
                {
                    result.SFontStyle = deck.SFontStyle;
                }
            }
            else if (result.InheritFontStyle == InheritPriorities.InheritDeckFirst)
            {
                result.SFontStyle = deck.SFontStyle;
                if (deck.InheritFontStyle == InheritPriorities.InheritRegionFirst)
                {
                    result.SFontStyle = inherittedSource.SFontStyle;
                }
            }
            if (result.InheritFontWeight == InheritPriorities.InheritRegionFirst)
            {
                result.SFontWeight = inherittedSource.SFontWeight;
                if (inherittedSource.InheritFontWeight == InheritPriorities.InheritDeckFirst)
                {
                    result.SFontWeight = deck.SFontWeight;
                }
            }
            else if (result.InheritFontWeight == InheritPriorities.InheritDeckFirst)
            {
                result.SFontWeight = deck.SFontWeight;
                if (deck.InheritFontWeight == InheritPriorities.InheritRegionFirst)
                {
                    result.SFontWeight = inherittedSource.SFontWeight;
                }
            }
            if (result.InheritFontBrush == InheritPriorities.InheritRegionFirst)
            {
                result.GradientTextBrushStops = inherittedSource.GradientTextBrushStops.Clone();
                result.GradientTextBrushAngle = inherittedSource.GradientTextBrushAngle;
                result.SolidColorTextBrushColor = inherittedSource.SolidColorTextBrushColor;
                result.TextBrushColorMode = inherittedSource.TextBrushColorMode;
                if (inherittedSource.InheritFontBrush == InheritPriorities.InheritDeckFirst)
                {
                    result.GradientTextBrushStops = deck.GradientTextBrushStops.Clone();
                    result.GradientTextBrushAngle = deck.GradientTextBrushAngle;
                    result.SolidColorTextBrushColor = deck.SolidColorTextBrushColor;
                    result.TextBrushColorMode = deck.TextBrushColorMode;
                }
            }
            else if (result.InheritFontBrush == InheritPriorities.InheritDeckFirst)
            {
                result.GradientTextBrushStops = deck.GradientTextBrushStops.Clone();
                result.GradientTextBrushAngle = deck.GradientTextBrushAngle;
                result.SolidColorTextBrushColor = deck.SolidColorTextBrushColor;
                result.TextBrushColorMode = deck.TextBrushColorMode;
                if (deck.InheritFontBrush == InheritPriorities.InheritRegionFirst)
                {
                    result.GradientTextBrushStops = inherittedSource.GradientTextBrushStops.Clone();
                    result.GradientTextBrushAngle = inherittedSource.GradientTextBrushAngle;
                    result.SolidColorTextBrushColor = inherittedSource.SolidColorTextBrushColor;
                    result.TextBrushColorMode = inherittedSource.TextBrushColorMode;
                }
            }
            if (result.InheritStrokeProperties == InheritPriorities.InheritRegionFirst)
            {
                result.TextStrokeColor = inherittedSource.TextStrokeColor;
                result.StrokeOn = inherittedSource.StrokeOn;
                result.StrokeThickness = inherittedSource.StrokeThickness;
                if (inherittedSource.InheritStrokeProperties == InheritPriorities.InheritDeckFirst)
                {
                    result.TextStrokeColor = deck.TextStrokeColor;
                    result.StrokeOn = deck.StrokeOn;
                    result.StrokeThickness = deck.StrokeThickness;
                }
            }
            else if (result.InheritStrokeProperties == InheritPriorities.InheritDeckFirst)
            {
                result.TextStrokeColor = deck.TextStrokeColor;
                result.StrokeOn = deck.StrokeOn;
                result.StrokeThickness = deck.StrokeThickness;
                if (deck.InheritStrokeProperties == InheritPriorities.InheritRegionFirst)
                {
                    result.TextStrokeColor = inherittedSource.TextStrokeColor;
                    result.StrokeOn = inherittedSource.StrokeOn;
                    result.StrokeThickness = inherittedSource.StrokeThickness;
                }
            }
            if (result.InheritTextHorizontalAlignment == InheritPriorities.InheritRegionFirst)
            {
                result.TextHorizontalAlignment = inherittedSource.TextHorizontalAlignment;
                if (inherittedSource.InheritStrokeProperties == InheritPriorities.InheritDeckFirst)
                {
                    result.TextHorizontalAlignment = deck.TextHorizontalAlignment;
                }
            }
            else if (result.InheritTextHorizontalAlignment == InheritPriorities.InheritDeckFirst)
            {
                result.TextHorizontalAlignment = deck.TextHorizontalAlignment;
                if (deck.InheritStrokeProperties == InheritPriorities.InheritRegionFirst)
                {
                    result.TextHorizontalAlignment = inherittedSource.TextHorizontalAlignment;
                }
            }
            if (result.InheritTextVerticalAlignment == InheritPriorities.InheritRegionFirst)
            {
                result.TextVerticalAlignment = inherittedSource.TextVerticalAlignment;
                if (inherittedSource.InheritStrokeProperties == InheritPriorities.InheritDeckFirst)
                {
                    result.TextVerticalAlignment = deck.TextVerticalAlignment;
                }
            }
            else if (result.InheritTextVerticalAlignment == InheritPriorities.InheritDeckFirst)
            {
                result.TextVerticalAlignment = deck.TextVerticalAlignment;
                if (deck.InheritStrokeProperties == InheritPriorities.InheritRegionFirst)
                {
                    result.TextVerticalAlignment = inherittedSource.TextVerticalAlignment;
                }
            }

            return result;
        }

        #region InheritCopyFunctions
        private void GetInherittedPropertiesMergingOnce(ref InheritPriorities prior, Action<String_Properties, String_Properties> PropCopy, String_Properties result, String_Properties source, String_Properties deck)
        {

        }

        private void GetInherittedPropertiesMergingFontFamily(ref String_Properties result, String_Properties source)
        {
            result.FontFamily = source.FontFamily;
            result.InheritFontFamily = source.InheritFontFamily;
        }
        private void GetInherittedPropertiesMergingFontSize(ref String_Properties result, String_Properties source)
        {
            result.FontSize = source.FontSize;
            result.InheritFontSize = source.InheritFontSize;
        }
        private void GetInherittedPropertiesMergingFontStyle(ref String_Properties result, String_Properties source)
        {
            result.SFontStyle = source.SFontStyle;
            result.InheritFontStyle = source.InheritFontStyle;
        }
        private void GetInherittedPropertiesMergingFontWeight(ref String_Properties result, String_Properties source)
        {
            result.SFontWeight = source.SFontWeight;
            result.InheritFontWeight = source.InheritFontWeight;
        }
        #endregion

        public bool IsValid()
        {
            return !(FontFamily == "" || FontSize == 0 || TextBrush == null);
        }

        public void SetAllInheritValues(InheritPriorities val)
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
