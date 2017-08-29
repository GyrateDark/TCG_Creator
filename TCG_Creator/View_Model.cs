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
using System.Windows;
using System.Windows.Shapes;
using System.Windows.Data;
using System.Windows.Documents;

namespace TCG_Creator
{
    public class View_Model : ObservableObject
    {
        #region Fields

        private Card_Collection _cardCollection = new Card_Collection();
        private ICommand _getCardCommand;
        private ICommand _saveCardCommand;

        private IList<Tree_View_Card> _treeViewCards;

        private Size _renderSize = new Size(825, 1125);

        public PercentageConverter percentConvertor = new PercentageConverter();

        private int _selectedRegion = -1;

        private string _selectedRegionText = "";

        private Canvas _cardCanvas = new Canvas();
        private FlowDocument _richTextDocument;

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


        public IList<FrameworkElement> Drawing_Card_Elements
        {
            get
            {
                IList<FrameworkElement> result = new List<FrameworkElement>();

                Card selectedCard = Find_Selected_Card();
                DrawingGroup cardDrawing = selectedCard.Render_Card(new Rect(0, 0, CardRenderWidth, CardRenderHeight), ref _cardCollection);

                FrameworkElement frameworkElement = new Rectangle();

                frameworkElement.Height = CardRenderHeight;
                frameworkElement.Width = CardRenderWidth;

                DrawingBrush rectFill = new DrawingBrush(cardDrawing);
                rectFill.Stretch = Stretch.None;
                ((Rectangle)frameworkElement).Fill = rectFill;

                Style BaseStyle = new Style();

                ((Rectangle)frameworkElement).Style = BaseStyle;

                result.Add(frameworkElement);

                foreach (Card_Region i in selectedCard.Regions)
                {
                    if (SelectedRegion == i.id)
                    {
                        frameworkElement = new RichTextBox();
                        
                        if (i.text != null)
                        {
                            _richTextDocument = i.ConvertFromFormattedTextToFlowDocument();
                            ((RichTextBox)frameworkElement).Document = _richTextDocument;
                        }
                    }
                    else
                    {
                        frameworkElement = new Rectangle();

                        ((Rectangle)frameworkElement).Fill = Brushes.Transparent;
                    }

                    if (i.inheritted)
                    {

                    }

                    frameworkElement.Height = i.ideal_location.Height * CardRenderHeight;
                    frameworkElement.Width = i.ideal_location.Width * CardRenderWidth;

                    Thickness margin = new Thickness();

                    margin.Left = i.ideal_location.X * CardRenderWidth;
                    margin.Top = i.ideal_location.Y * CardRenderHeight;

                    frameworkElement.Margin = margin;

                    //DrawingBrush rectFill = new DrawingBrush(i.Draw_Region(new Rect(rectangle.Margin.Left, rectangle.Margin.Top, rectangle.Width, rectangle.Height)));
                    //rectFill.Stretch = Stretch.None;
                    //rectangle.Fill = rectFill;

                    frameworkElement.MouseUp += FrameworkElement_MouseUp; ;
                    frameworkElement.DataContext = i.id;

                    result.Add(frameworkElement);
                }
                return result;
            }
        }

        private void FrameworkElement_MouseUp(object sender, MouseButtonEventArgs e)
        {
            if (SelectedRegion != -1)
            {
                Find_SelectedCard_Region().ConvertFromFlowDocumentToFormattedText(_richTextDocument);
                SelectedRegion = -1;
            }
            else
            {
                SelectedRegion = (int)((FrameworkElement)sender).DataContext;
            }
            Notify_Drawing_Card_Changed();
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

        public int SelectedRegion
        {
            get
            {
                return _selectedRegion;
            }
            set
            {
                if (_selectedRegion != value)
                {
                    _selectedRegion = value;
                    OnPropertyChanged("SelectedRegion");
                }
            }
        }

        public double CardRenderHeight
        {
            get
            {
                return _renderSize.Height;
            }
        }

        public double CardRenderWidth
        {
            get
            {
                return _renderSize.Width;
            }
        }

        public string SelectedRegionText
        {
            get { return _selectedRegionText; }
            set
            {
                if (_selectedRegionText != value)
                {
                    _selectedRegionText = value;
                    OnPropertyChanged("SelectedRegionText");
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

        public void Tree_View_Selected_Item_Changed()
        {
            Notify_Drawing_Card_Changed();
        }

        #region Private Helpers

        private void AddNewCard()
        {
            Card newCard = new Card();

            newCard.IsTemplateCard = true;
            if (_treeViewCards != null)
            {
                Card parentCard = Find_Selected_Card();
                newCard = new Card(parentCard);
                for (int i = 0; i < newCard.Regions.Count; ++i)
                {
                    if (newCard.Regions[i].Should_Inherit_Region())
                    {
                        newCard.Regions[i].inheritted = true;
                    }
                    else
                    {
                        newCard.Regions.RemoveAt(i);
                        --i;
                    }
                }
                newCard.ParentCard = parentCard.Id;
            }

            CurrentCardCollection.Add_Card_To_Collection(newCard);
            NotifyCollectionChanged();
        }

        private void SaveCardCollection()
        {
            // You would implement your Product save here
        }
        
        private Card_Region Find_SelectedCard_Region()
        {
            Card selectedCard = Find_Selected_Card();

            foreach (Card_Region i in selectedCard.Regions)
            {
                if (i.id == SelectedRegion)
                {
                    return i;
                }
            }

            return new Card_Region();
        }

        private Card Find_Selected_Card()
        {
            if (_treeViewCards == null)
            {
                Card temp = new Card();
                return temp;
            }
            return CurrentCardCollection.Find_Card_In_Collection(find_selected(_treeViewCards));
        }

        private void NotifyCollectionChanged()
        {
            Get_Tree_View_Cards = _cardCollection.Get_Tree_View_Template_Cards(ref _cardCollection);

            OnPropertyChanged("CurrentCardCollection");
        }

        private void Notify_Drawing_Card_Changed()
        {
            OnPropertyChanged("Drawing_Card_Elements");
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
