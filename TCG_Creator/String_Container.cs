using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;

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
                int stopPosition = i.Text.Length + startPosition-1;

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

            Text = "";
        }

        public string FontFamily { get; set; }
        public double FontSize { get; set; }
        public FontStyle StrFontStyle { get; set; }
        public FontWeight StrFontWeight { get; set; }

        

        public Brush TextBrush { get; set; }

        public string Text { get; set; }
    }
}
