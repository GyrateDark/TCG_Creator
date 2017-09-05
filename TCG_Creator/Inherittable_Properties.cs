using System;
using System.Collections.Generic;
using System.Windows.Media;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace TCG_Creator
{
    public class InheritPriorities
    {
        public const int InheritRegionFirst = 0;
        public const int InheritDeckFirst = 1;
        public const int DoNotInherit = 2;

        private int _val = InheritRegionFirst;

        public static implicit operator bool?(InheritPriorities value)
        {
            if (value._val == InheritRegionFirst)
            {
                return true;
            }
            else if (value._val == InheritDeckFirst)
            {
                return null;
            }
            else
            {
                return false;
            }
        }
        public static implicit operator InheritPriorities(bool? value)
        {
            InheritPriorities result = new InheritPriorities();

            if (value == true)
            {
                result._val = InheritRegionFirst;
            }
            else if (value == null)
            {
                result._val = InheritDeckFirst;
            }
            else
            {
                result._val = DoNotInherit;
            }

            return result;
        }
        public static implicit operator InheritPriorities(int value)
        {
            InheritPriorities result = new InheritPriorities();

            result._val = value;

            return result;
        }
        public static implicit operator int(InheritPriorities value)
        {
            return value._val;
        }

        public int Value
        {
            get { return _val; }
            set { _val = value; }
        }
    }


    public class Inherittable_Properties
    {
        private Image_Properties _imageProperties = new Image_Properties();
        private String_Container _stringContainer = new String_Container();

        public bool InheritDeckFirst = true;

        public Image_Properties ImageProperties
        {
            get { return _imageProperties; }
            set
            {
                if (_imageProperties != value)
                {
                    _imageProperties = value;
                }
            }
        }

        public String_Container StringContainer
        {
            get { return _stringContainer; }
            set
            {
                if (_stringContainer != value)
                {
                    _stringContainer = value;
                }
            }
        }

        [XmlIgnore]
        public String_Properties StringProperties
        {
            get { return _stringContainer.stringProperties; }
            set
            {
                if (_stringContainer.stringProperties != value)
                {
                    _stringContainer.stringProperties = value;
                }
            }
        }

        public Inherittable_Properties GetInherittedPropertiesMerging(Inherittable_Properties Source, Inherittable_Properties Deck)
        {
            Inherittable_Properties result = Clone();

            if (Deck == null)
            {
                Deck = new Inherittable_Properties();

                Deck.ImageProperties = null;
                Deck.StringProperties = null;
            }
            if (Source == null)
            {
                Source = new Inherittable_Properties();

                Source.ImageProperties = null;
                Source.StringProperties = null;
            }

            result.ImageProperties = result._imageProperties.GetInherittedPropertiesMerging(Source.ImageProperties, Deck.ImageProperties);
            result.StringContainer = result._stringContainer.GetInherittedPropertiesMerging(Source.StringContainer, Deck.StringContainer);

            return result;
        }

        public void SetAllInheritValues(InheritPriorities val)
        {
            _imageProperties.SetAllInheritValues(val);
            _stringContainer.SetAllInheritValues(val);
        }

        public IList<Color> GetUsedColors()
        {
            IList<Color> result = new List<Color>();

            if (StringProperties.InheritFontBrush == InheritPriorities.DoNotInherit)
            {
                if (StringProperties.TextBrushColorMode == TextBrushMode.SolidColor)
                {
                    result.Add(StringProperties.SolidColorTextBrushColor);
                }
                else if (StringProperties.TextBrushColorMode == TextBrushMode.Gradient)
                {
                    foreach (GradientStop i in StringProperties.GradientTextBrushStops)
                    {
                        result.Add(i.Color);
                    }
                }
            }

            return result;
        }

        public  Inherittable_Properties Clone()
        {
            Inherittable_Properties result = (Inherittable_Properties)MemberwiseClone();

            result._imageProperties = _imageProperties.Clone();
            result._stringContainer = _stringContainer.Clone();

            return result;
        }
    }
}
