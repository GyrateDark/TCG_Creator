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
        public IMAGE_LOCATION_TYPE BackgroundImageLocationType { get; set; } = IMAGE_LOCATION_TYPE.None;
        public IMAGE_OPTIONS BackgroundImageFillType { get; set; } = IMAGE_OPTIONS.None;

        public string BackgroundImageLocation { get; set; } = "";

        public bool InheritBackground { get; set; } = true;

        public Image_Properties GetInherittedPropertiesMerging(Image_Properties Source)
        {
            Image_Properties result = Clone();

            if (InheritBackground)
            {
                result.BackgroundImageFillType = Source.BackgroundImageFillType;
                result.BackgroundImageLocation = Source.BackgroundImageLocation;
                result.BackgroundImageLocationType = Source.BackgroundImageLocationType;
            }

            return result;
        }

        public void SetAllInheritValues(bool val)
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
