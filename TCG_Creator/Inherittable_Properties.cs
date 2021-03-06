﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TCG_Creator
{
    public class Inherittable_Properties : ObservableObject
    {
        private Image_Properties _imageProperties = new Image_Properties();
        private String_Container _stringContainer = new String_Container();

        public Image_Properties ImageProperties
        {
            get { return _imageProperties; }
            set
            {
                if (_imageProperties != value)
                {
                    _imageProperties = value;
                    OnPropertyChanged("ImageProperties");
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
                    OnPropertyChanged("StringContainer");
                }
            }
        }

        public String_Properties StringProperties
        {
            get { return _stringContainer.stringProperties; }
            set
            {
                if (_stringContainer.stringProperties != value)
                {
                    _stringContainer.stringProperties = value;
                    OnPropertyChanged("StringProperties");
                }
            }
        }

        public Inherittable_Properties GetInherittedPropertiesMerging(Inherittable_Properties Source)
        {
            Inherittable_Properties result = Clone();

            result.ImageProperties = result._imageProperties.GetInherittedPropertiesMerging(Source._imageProperties);
            result.StringContainer = result._stringContainer.GetInherittedPropertiesMerging(Source._stringContainer);

            return result;
        }

        public void SetAllInheritValues(bool val)
        {
            _imageProperties.SetAllInheritValues(val);
            _stringContainer.SetAllInheritValues(val);
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
