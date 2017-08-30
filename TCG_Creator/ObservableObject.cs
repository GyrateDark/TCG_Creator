using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;
using System.Diagnostics;
using System.Reflection;

namespace TCG_Creator
{
    public abstract class ObservableObject : INotifyPropertyChanged
    {
        #region INotifyPropertyChanged Members

        int currentNumberOfUnsavedChanges = 0;
        const int MAX_NUMBER_OF_UNSAVED_CHANGES = 1;
        protected bool SkipSave = false;

        /// <summary>
        /// Raised when a property on this object has a new value.
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        /// Raises this object's PropertyChanged event.
        /// </summary>
        /// <param name="propertyName">The property that has a new value.</param>
        protected void RaiseOnPropertyChanged(string propertyName)
        {
            this.VerifyPropertyName(propertyName);

            if (this.PropertyChanged != null)
            {
                var e = new PropertyChangedEventArgs(propertyName);
                this.PropertyChanged(this, e);
            }
        }

        protected virtual void Save(string file)
        {
            return;
        }

        protected void OnPropertyChanged(string propertyName, List<string> calledProperties = null)
        {
            if (calledProperties == null)
            {
                calledProperties = new List<string>();
            }

            ++currentNumberOfUnsavedChanges;

            if (currentNumberOfUnsavedChanges >= MAX_NUMBER_OF_UNSAVED_CHANGES && !SkipSave)
            {
                currentNumberOfUnsavedChanges = 0;

                Save(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "\\TCG_Creator\\");
            }

            calledProperties.Add(propertyName);

            List<PropertyInfo> pInfo = GetType().GetProperties().ToList();

            if (pInfo != null)
            {
                bool addedProperty = true;
                while (addedProperty)
                {
                    addedProperty = false;
                    for(int i = 0; i < pInfo.Count; ++i)
                    {
                        foreach (DependsUponAttribute j in pInfo[i].GetCustomAttributes(false).OfType<DependsUponAttribute>())
                        {
                            foreach (string k in calledProperties)
                            {
                                if (j.Properties.Contains(k))
                                {
                                    calledProperties.Add(pInfo[i].Name);
                                    pInfo.RemoveAt(i);
                                    --i;
                                    addedProperty = true;
                                    break;
                                }
                            }
                            if (addedProperty)
                            {
                                break;
                            }
                        }
                    }
                }

                foreach (string i in calledProperties)
                {
                    RaiseOnPropertyChanged(i);
                }
            }
        }

        #endregion // INotifyPropertyChanged Members

        #region Debugging Aides

        /// <summary>
        /// Warns the developer if this object does not have
        /// a public property with the specified name. This
        /// method does not exist in a Release build.
        /// </summary>
        [Conditional("DEBUG")]
        [DebuggerStepThrough]
        public virtual void VerifyPropertyName(string propertyName)
        {
            // Verify that the property name matches a real,
            // public, instance property on this object.
            if (TypeDescriptor.GetProperties(this)[propertyName] == null)
            {
                string msg = "Invalid property name: " + propertyName;

                if (this.ThrowOnInvalidPropertyName)
                    throw new Exception(msg);
                else
                    Debug.Fail(msg);
            }
        }

        /// <summary>
        /// Returns whether an exception is thrown, or if a Debug.Fail() is used
        /// when an invalid property name is passed to the VerifyPropertyName method.
        /// The default value is false, but subclasses used by unit tests might
        /// override this property's getter to return true.
        /// </summary>
        protected virtual bool ThrowOnInvalidPropertyName { get; private set; }

        #endregion // Debugging Aides
    }

    [AttributeUsageAttribute(AttributeTargets.Property, AllowMultiple = true)]
    public class DependsUponAttribute : Attribute
    {
        private List<string> properties = new List<string>();

        public DependsUponAttribute(params string[] dp)
        {
            properties.AddRange(dp);
        }

        public string[] Properties
        {
            get
            {
                return properties.ToArray();
            }
        }
    }
}
