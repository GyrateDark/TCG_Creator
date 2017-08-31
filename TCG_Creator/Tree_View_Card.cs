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
    public class Tree_View_Card
    {
        public Tree_View_Card(ref Card_Collection coll)
        {
            _children = new List<Tree_View_Card>();
            _parentId = -1;

            _cardCollection = coll;
        }

        public Tree_View_Card(int id, string name, int parentId, ref Card_Collection coll)
        {
            _children = new List<Tree_View_Card>();
            _id = id;
            _parentId = parentId;
            

            _cardCollection = coll;
        }

        private List<Tree_View_Card> _children;
        private int _id;
        private int _parentId;
        private bool _isSelected;

        private Card_Collection _cardCollection;

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
                }
            }
        }

        #endregion // Properties
    }
}
