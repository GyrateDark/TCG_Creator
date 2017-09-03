using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TCG_Creator
{
    public enum IMAGE_OPTIONS
    {
        Fill,
        Letterbox,
        Unified_Fill,
        None,
        N_IMAGE_OPTIONS
    }

    public enum IMAGE_LOCATION_TYPE
    {
        Absolute,
        Online,
        Relative,
        None
    }

    public class Image_Properties
    {
        public IMAGE_LOCATION_TYPE BackgroundImageLocationType
        {
            get
            {
                string matchedString = BackgroundImageLocation.Replace(" ", "");

                if (matchedString == "")
                {
                    return IMAGE_LOCATION_TYPE.None;
                }
                else if(matchedString.IndexOf("http") == 0)
                {
                    return IMAGE_LOCATION_TYPE.Online;
                }
                else if(matchedString.IndexOf(":/") == 1 || matchedString.IndexOf(":\\") == 1 || matchedString.IndexOf("pack://application:,,,/TCG_Creator;component") == 0)
                {
                    return IMAGE_LOCATION_TYPE.Absolute;
                }
                else
                {
                    return IMAGE_LOCATION_TYPE.Relative;
                }
            }
        }
        public IMAGE_OPTIONS BackgroundImageFillType { get; set; } = IMAGE_OPTIONS.None;
        private string _backgroundImageLocation = "";
        public InheritPriorities InheritBackground { get; set; } = InheritPriorities.InheritRegionFirst;

        public string BackgroundImageLocation
        {
            get { return _backgroundImageLocation; }
            set
            {
                if (_backgroundImageLocation != value)
                {
                    _backgroundImageLocation = value;
                    _backgroundImageLocation = _backgroundImageLocation.Trim();
                }
            }
        } 

        public Image_Properties GetInherittedPropertiesMerging(Image_Properties Source, Image_Properties Deck)
        {
            Image_Properties result = Clone();

            if (InheritBackground == InheritPriorities.InheritRegionFirst)
            {
                result.BackgroundImageFillType = Source.BackgroundImageFillType;
                result.BackgroundImageLocation = Source.BackgroundImageLocation;
                result.InheritBackground = Source.InheritBackground;
                if (result.InheritBackground == InheritPriorities.InheritDeckFirst)
                {
                    result.BackgroundImageFillType = Deck.BackgroundImageFillType;
                    result.BackgroundImageLocation = Deck.BackgroundImageLocation;
                    result.InheritBackground = Deck.InheritBackground;
                }
            }
            else if (InheritBackground == InheritPriorities.InheritDeckFirst)
            {
                result.BackgroundImageFillType = Deck.BackgroundImageFillType;
                result.BackgroundImageLocation = Deck.BackgroundImageLocation;
                result.InheritBackground = Deck.InheritBackground;
                if (result.InheritBackground == InheritPriorities.InheritRegionFirst)
                {
                    result.BackgroundImageFillType = Source.BackgroundImageFillType;
                    result.BackgroundImageLocation = Source.BackgroundImageLocation;
                    result.InheritBackground = Source.InheritBackground;
                }
            }

            return result;
        }

        public void SetAllInheritValues(InheritPriorities val)
        {
            InheritBackground = val;
        }

        public Image_Properties Clone()
        {
            Image_Properties result = (Image_Properties)MemberwiseClone();
            
            result.BackgroundImageLocation = (string)BackgroundImageLocation.Clone();

            return result;
        }
    }
}
