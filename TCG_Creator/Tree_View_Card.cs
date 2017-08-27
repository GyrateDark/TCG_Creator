using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace TCG_Creator
{
    public class Tree_View_Card : ObservableObject
    {
        public Tree_View_Card()
        {
            _children = new List<Tree_View_Card>();
            _parentId = -1;
        }

        public Tree_View_Card(int id, string name, int parentId)
        {
            _children = new List<Tree_View_Card>();
            _id = id;
            _name = name;
            _parentId = parentId;
        }

        private List<Tree_View_Card> _children;
        private int _id;
        private string _name;
        private int _parentId;

        #region Properties

        public List<Tree_View_Card> Children
        {
            get { return _children; }
            set
            {
                if (value != _children)
                {
                    _children = value;
                    OnPropertyChanged("Children");
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
                    OnPropertyChanged("ParentId");
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
                    OnPropertyChanged("Id");
                }
            }
        }

        public string Name
        {
            get { return _name; }
            set
            {
                if (value != _name)
                {
                    _name = value;
                    OnPropertyChanged("Name");
                }
            }
        }

        #endregion // Properties
    }
}
