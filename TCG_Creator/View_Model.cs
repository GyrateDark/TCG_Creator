using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Windows.Media;
using System.Xaml;
using System.Xml;
using System.Xml.Serialization;
using System.Windows.Controls;


namespace TCG_Creator
{
    public class View_Model : ObservableObject
    {
        #region Fields

        private int _collectionId;
        private Card_Collection _cardCollection = new Card_Collection();
        private ICommand _getCardCommand;
        private ICommand _saveCardCommand;
        private ICommand _treeViewSelectedNode;

        private string _selectedTreeViewNode;

        #endregion

        #region Public Properties/Commands

        public Card_Collection CurrentCardCollection
        {
            get { return _cardCollection; }
            set
            {
                if (value != _cardCollection)
                {
                    _cardCollection = value;
                    NotifyCollectionChanged();
                }
            }
        }

        public ICommand SaveCardCommand
        {
            get
            {
                if (_saveCardCommand == null)
                {
                    _saveCardCommand = new RelayCommand(
                        param => SaveCardCollection(),
                        param => (_saveCardCommand != null)
                    );
                }
                return _saveCardCommand;
            }
        }

        public ICommand AddCardCommand
        {
            get
            {
                if (_getCardCommand == null)
                {
                    _getCardCommand = new RelayCommand(
                        param => AddNewCard()
                    );
                }
                return _getCardCommand;
            }
        }

        public ICommand TreeViewSelectedNodeChanged
        {
            get
            {
                if (_treeViewSelectedNode == null)
                {
                    _treeViewSelectedNode = new RelayCommand(
                        param => SaveCardCollection()
                    );
                }
                return _treeViewSelectedNode;
            }
        }

        public DrawingGroup Drawing_Card
        {
            get { return CurrentCardCollection.CardCollection[0].Render_Card(new System.Windows.Rect(0, 0, 100, 100), ref _cardCollection); }
        }

        public IList<Tree_View_Card> Get_Tree_View_Cards
        {
            get
            {
                var result = new List<string>();

                var tree_view_cards = _cardCollection.Get_Tree_View_Template_Cards();

                return tree_view_cards;
            }
        }

        public string SelectedTreeViewNode
        {
            get { return _selectedTreeViewNode; }
            set
            {
                if (_selectedTreeViewNode != value)
                {
                    _selectedTreeViewNode = value;
                    OnPropertyChanged("SelectedTreeViewNode");
                }
            }
        }

        #endregion

        public void Xml_Save(string file, bool only_templates)
        {
            XmlWriterSettings xmlWriterSettings = new XmlWriterSettings();

            xmlWriterSettings.ConformanceLevel = ConformanceLevel.Auto;
            xmlWriterSettings.Indent = true;
            xmlWriterSettings.NewLineOnAttributes = true;

            XmlWriter xmlWriter = XmlWriter.Create(file, xmlWriterSettings);
            XmlSerializer xmlSerializer = new XmlSerializer(typeof(Card_Collection));
            
            xmlSerializer.Serialize(xmlWriter, _cardCollection);
        }

        public void Xml_Load(string file, bool only_templates)
        {
            XmlReader xmlReader = XmlReader.Create(file);
            XmlSerializer xmlSerializer = new XmlSerializer(typeof(Card_Collection));

            _cardCollection = (Card_Collection)xmlSerializer.Deserialize(xmlReader);
        }

        #region Private Helpers

        private void AddNewCard()
        {
            Card newCard = new Card();

            newCard.IsTemplateCard = true;

            //newCard.ParentCard = SelectedTreeViewNode.Length;

            CurrentCardCollection.Add_Card_To_Collection(newCard);
            NotifyCollectionChanged();
        }

        private void SaveCardCollection()
        {
            // You would implement your Product save here
        }

        private void Seleclted()
        {
            // You would implement your Product save here
        }

        private void NotifyCollectionChanged()
        {
            OnPropertyChanged("CurrentCardCollection");
            OnPropertyChanged("Get_Tree_View_Cards");
        }

        #endregion
    }
}
