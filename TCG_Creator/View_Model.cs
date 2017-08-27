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

        private Card_Collection _cardCollection = new Card_Collection();
        private ICommand _getCardCommand;
        private ICommand _saveCardCommand;
        private ICommand _treeViewSelectedNode;

        private IList<Tree_View_Card> _treeViewCards;

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
            { return _treeViewCards; }
            set
            {
                if (_treeViewCards != value)
                {
                    _treeViewCards = value;
                    OnPropertyChanged("Get_Tree_View_Cards");
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
            NotifyCollectionChanged();
        }

        #region Private Helpers

        private void AddNewCard()
        {
            Card newCard = new Card();

            newCard.IsTemplateCard = true;
            if (_treeViewCards != null)
            {
                Card parentCard = CurrentCardCollection.Find_Card_In_Collection(find_selected(_treeViewCards));
                newCard = parentCard;
                newCard.ParentCard = parentCard.Id;
            }

            CurrentCardCollection.Add_Card_To_Collection(newCard);
            NotifyCollectionChanged();
        }

        private void SaveCardCollection()
        {
            // You would implement your Product save here
        }

        private void NotifyCollectionChanged()
        {
            Get_Tree_View_Cards = _cardCollection.Get_Tree_View_Template_Cards(ref _cardCollection);

            OnPropertyChanged("CurrentCardCollection");
        }

        private int find_selected(IList<Tree_View_Card> cards)
        {
            int result = -1;
            foreach (Tree_View_Card i in cards)
            {
                if (i.IsSelected == true)
                {
                    return i.Id;
                }
                else if (i.Children.Count >= 1)
                {
                    result = find_selected(i.Children);

                    if (result != -1)
                    {
                        return result;
                    }
                }
            }

            return result;
        }

        #endregion
    }
}
