using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace TCG_Creator
{
    public class Tree_View_Card : ITreeViewItemModel
    {
        public Tree_View_Card(ref Card_Collection coll)
        {
            _children = new List<Tree_View_Card>();
            _parent = null;
            _parentId = -1;

            _cardCollection = coll;
        }

        public Tree_View_Card(int id, string name, int parentId, ref Card_Collection coll)
        {
            _children = new List<Tree_View_Card>();
            _parent = null;
            _id = id;
            _parentId = parentId;
            

            _cardCollection = coll;
        }

        private List<Tree_View_Card> _children;
        private Tree_View_Card _parent;
        private int _id;
        private int _parentId;
        private bool _isSelected = false;

        private Card_Collection _cardCollection;

        public event PropertyChangedEventHandler PropertyChanged;

        #region Properties

        public List<Tree_View_Card> Children
        {
            get { return _children; }
            set
            {
                if (value != _children)
                {
                    _children = value;
                }
            }
        }
        public Tree_View_Card Parent
        {
            get { return _parent; }
            set
            {
                if (value != _parent)
                {
                    _parent = value;
                }
            }
        }

        public int ParentId
        {
            get { return _parentId; }
            set
            {
                if (value != _parentId)
                {
                    _parentId = value;
                }
            }
        }

        public int Id
        {
            get { return _id; }
            set
            {
                if (value != _id)
                {
                    _id = value;
                }
            }
        }

        public string Name
        {
            get
            {
                if (Id != -2)
                {
                    return _cardCollection.Find_Card_In_Collection(Id).Name;
                }
                else
                {
                    return "<New>";
                }
            }
            set
            {
                if (value != Name)
                {
                    _cardCollection.Find_Card_In_Collection(Id).Name = value;
                }
            }
        }

        public string DisplayName
        { get { return Name; } }


        public bool IsSelected
        {
            get { return _isSelected; }
            set
            {
                if (value != _isSelected)
                {
                    _isSelected = value;
                    RaisePropertyChanged("IsSelected");
                }
            }
        }

        public string SelectedValuePath { get { return DisplayName; } }

        public string DisplayValuePath { get { return DisplayName; } }

        private bool _isExpanded = true;
        public bool IsExpanded
        {
            get { return _isExpanded; }
            set
            {
                if (_isExpanded != value)
                {
                    _isExpanded = value;
                    RaisePropertyChanged("IsExpanded");
                }
            }
        }

        public IEnumerable<ITreeViewItemModel> GetHierarchy()
        {
            return GetAscendingHierarchy().Reverse();
        }
        private IEnumerable<Tree_View_Card> GetAscendingHierarchy()
        {
            var vm = this;

            yield return vm;
            while (vm.Parent != null)
            {
                yield return vm.Parent;
                vm = vm.Parent;
            }
        }


        public IEnumerable<ITreeViewItemModel> GetChildren()
        {
            return Children;
        }

        protected virtual void RaisePropertyChanged(string propertyName)
        {
            if (this.PropertyChanged != null)
            {
                this.PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }


        #endregion // Properties
    }
}
