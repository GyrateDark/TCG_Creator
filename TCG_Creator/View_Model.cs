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

        private double _imageHeight = 1125;
        private double _imageWidth = 825;
        private Size _imageSize;

        public PercentageConverter percentConvertor = new PercentageConverter();

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

        public IList<Rectangle> Drawing_Card_Rectangles
        {
            get
            {
                IList<Rectangle> result = new List<Rectangle>();

                Card selectedCard = Find_Selected_Card();

                foreach (Card_Region i in selectedCard.Regions)
                {
                    Rectangle rectangle = new Rectangle();

                    rectangle.Height = i.ideal_location.Height * Image_Height;
                    rectangle.Width = i.ideal_location.Width * Image_Width;

                    rectangle.Fill = Brushes.Transparent;

                    BindingOperations.SetBinding(rectangle, Canvas.TopProperty, Bind_Card_Canvas_Property_With_Percent_Convertor("ActualHeight", i.ideal_location.Y));
                    BindingOperations.SetBinding(rectangle, Canvas.LeftProperty, Bind_Card_Canvas_Property_With_Percent_Convertor("ActualWidth", i.ideal_location.X));

                    BindingOperations.SetBinding(rectangle, Rectangle.HeightProperty, Bind_Card_Canvas_Property_With_Percent_Convertor("ActualHeight", i.ideal_location.Height));
                    BindingOperations.SetBinding(rectangle, Rectangle.WidthProperty, Bind_Card_Canvas_Property_With_Percent_Convertor("ActualWidth", i.ideal_location.Width));

                    result.Add(rectangle);
                }

                return result;
            }
        }

        public DrawingGroup Drawing_Card
        {
            get { return Find_Selected_Card().Render_Card(new Rect(0, 0, Image_Width, Image_Height), ref _cardCollection); }
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

        public double Image_Height
        {
            get
            {
                return _imageHeight;
            }
            set
            {
                if (_imageHeight != value)
                {
                    _imageHeight = value;
                    OnPropertyChanged("Image_Height");
                    Notify_Drawing_Card_Changed();
                }
            }
        }

        public double Image_Width
        {
            get
            {
                return _imageWidth;
            }
            set
            {
                if (_imageWidth != value)
                {
                    _imageWidth = value;
                    OnPropertyChanged("Image_Width");
                    Notify_Drawing_Card_Changed();
                }
            }
        }

        public Size Image_Size
        {
            get
            { 
                return _imageSize;
            }
            set
            {
                if (_imageSize != value)
                {
                    _imageSize = value;
                    OnPropertyChanged("Image_Size");
                    Notify_Drawing_Card_Changed();
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
                newCard.ParentCard = parentCard.Id;
            }

            CurrentCardCollection.Add_Card_To_Collection(newCard);
            NotifyCollectionChanged();
        }

        private void SaveCardCollection()
        {
            // You would implement your Product save here
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
            OnPropertyChanged("Drawing_Card");
            OnPropertyChanged("Drawing_Card_Rectangles");
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

        private Binding Bind_Card_Canvas_Property_With_Percent_Convertor(string canvasProperty, double convertorParameter)
        {
            Binding bindingBase = new Binding("{Binding Converter ={ StaticResource PercentageConverter}, ElementName = canvas, Path = ActualWidth, ConverterParameter = 0.1}");

            bindingBase.Converter = percentConvertor;
            bindingBase.ElementName = "Card_Canvas";
            bindingBase.Path = (PropertyPath)(new PropertyPathConverter().ConvertFromString(canvasProperty));
            bindingBase.ConverterParameter = convertorParameter;

            return bindingBase;

            
        }

        #endregion
    }
}
