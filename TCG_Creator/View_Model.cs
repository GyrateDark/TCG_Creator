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
using System.Windows.Media.Imaging;
using System.Net;
using Xceed.Wpf.Toolkit;

namespace TCG_Creator
{
    public class View_Model : ObservableObject
    {
        public View_Model()
        {
            _richTextBox.Background = new SolidColorBrush(Color.FromArgb(180, 100, 100, 100));
            
            RichTextBoxFormatBar formatBar = new RichTextBoxFormatBar();
            RichTextBoxFormatBarManager.SetFormatBar(_richTextBox, formatBar);

            NotifyCollectionChanged();
        }

        #region Fields

        private Card_Collection _cardCollection = new Card_Collection();
        private ICommand _getCardCommand;
        private ICommand _saveCardCommand;

        private IList<Tree_View_Card> _treeViewCards;

        private Size _renderSize = new Size(825, 1125);

        public PercentageConverter percentConvertor = new PercentageConverter();

        private int _selectedRegion = -1;
        private int _nextRegionId = 0;

        private Xceed.Wpf.Toolkit.RichTextBox _richTextBox = new Xceed.Wpf.Toolkit.RichTextBox();

        private bool _gradientBrushRequested = false;
        private bool _richTextBoxEditingTools = false;
        private bool _deleteNextSelectedRegion = false;

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
                        if (i.strings != null)
                        {
                            _richTextBox.Document = i.ConvertFromStringContainerToFlowDocument();
                        }

                        frameworkElement = _richTextBox;
                    }
                    else
                    {
                        frameworkElement = new Rectangle();

                        ((Rectangle)frameworkElement).Fill = Brushes.Transparent;
                        ((Rectangle)frameworkElement).Style = (Style)Application.Current.Resources["Card_Rectangle"];
                    }

                    if (i.inheritted)
                    {

                    }

                    frameworkElement.Height = i.ideal_location.Height * CardRenderHeight;
                    frameworkElement.Width = i.ideal_location.Width * CardRenderWidth;

                    frameworkElement.AllowDrop = true;
                    
                    frameworkElement.DragEnter += FrameworkElement_DragEnter;
                    frameworkElement.Drop += FrameworkElement_Drop;

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

        private void FrameworkElement_Drop(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.StringFormat))
            {
                string data = (string)e.Data.GetData(DataFormats.StringFormat);
                Card_Region selectedRegion = Find_SelectedCard_Region((int)((FrameworkElement)sender).DataContext);

                selectedRegion.background_location_type = IMAGE_LOCATION_TYPE.Online;
                selectedRegion.background_image_filltype = IMAGE_OPTIONS.Unified_Fill;
                selectedRegion.background_location = data;

                Notify_Drawing_Card_Changed();
            }
            else if (e.Data.GetDataPresent(DataFormats.Bitmap))
            {
                string data = (string)e.Data.GetData(DataFormats.Bitmap);
            }
        }
        private void FrameworkElement_DragEnter(object sender, DragEventArgs e)
        {
            e.Effects = DragDropEffects.Copy;
        }

        private void FrameworkElement_MouseUp(object sender, MouseButtonEventArgs e)
        {
            if (DeleteNextSelectedRegion)
            {
                int regionId = (int)((FrameworkElement)sender).DataContext;

                if (SelectedRegion == regionId)
                {
                    DisplayRichTextBoxEditingTools = false;
                    SelectedRegion = -1;
                }

                Card selectedCard = Find_Selected_Card();

                for (int i = 0; i < selectedCard.Regions.Count; ++i)
                {
                     if (selectedCard.Regions[i].id == regionId)
                    {
                        selectedCard.Regions.RemoveAt(i);
                        break;
                    }
                }

                DeleteNextSelectedRegion = false;
                Notify_Drawing_Card_Changed();
                return;
            }

            if (SelectedRegion != -1)
            {
                DisplayRichTextBoxEditingTools = false;
                Find_SelectedCard_Region(SelectedRegion).ConvertFromFlowDocumentToStringContainer(_richTextBox.Document);
                SelectedRegion = -1;
            }
            else
            {
                DisplayRichTextBoxEditingTools = true;
                SelectedRegion = (int)((FrameworkElement)sender).DataContext;
            }
            Notify_Drawing_Card_Changed();
        }

        public void AddNewCardRegion(Rect location)
        {
            Card selectedCard = Find_Selected_Card();

            selectedCard.Regions.Add(new Card_Region(ref _nextRegionId));

            selectedCard.Regions.Last().ideal_location = location;

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

            _cardCollection = null;
            _treeViewCards = null;
            _selectedRegion = -1;


            _cardCollection = (Card_Collection)xmlSerializer.Deserialize(xmlReader);
            NotifyCollectionChanged();

            foreach (Card i in _cardCollection.CardCollection)
            {
                foreach (Card_Region j in i.Regions)
                {
                    if (j.id >= _nextRegionId)
                    {
                        _nextRegionId = j.id + 1;
                    }
                }
            }
        }

        public void Tree_View_Selected_Item_Changed()
        {
            Notify_Drawing_Card_Changed();
        }

        #region Visibility Controls and Checkbox States
        public Visibility VisOfGradientBrushes
        {
            get
            {
                if (GradientBrushRequested && DisplayRichTextBoxEditingTools)
                {
                    return Visibility.Visible;
                }
                else
                {
                    return Visibility.Collapsed;
                }
            }
        }
        public Visibility VisOfRichTextEditing
        {
            get
            {
                if (DisplayRichTextBoxEditingTools)
                {
                    return Visibility.Visible;
                }
                else
                {
                    return Visibility.Collapsed;
                }
            }
        }

        public bool GradientBrushRequested
        {
            get { return _gradientBrushRequested; }
            set
            {
                if (_gradientBrushRequested != value)
                {
                    _gradientBrushRequested = value;
                    OnPropertyChanged("GradientBrushRequested");
                    OnPropertyChanged("VisOfGradientBrushes");
                }
            }
        }
        public bool DisplayRichTextBoxEditingTools
        {
            get { return _richTextBoxEditingTools; }
            set
            {
                if (_richTextBoxEditingTools != value)
                {
                    _richTextBoxEditingTools = value;
                    OnPropertyChanged("DisplayRichTextBoxEditingTools");
                    OnPropertyChanged("VisOfGradientBrushes");
                    OnPropertyChanged("VisOfRichTextEditing");
                }
            }
        }
        public bool DeleteNextSelectedRegion
        {
            get { return _deleteNextSelectedRegion; }
            set
            {
                if (_deleteNextSelectedRegion != value)
                {
                    _deleteNextSelectedRegion = value;
                    OnPropertyChanged("DeleteNextSelectedRegion");
                }
            }
        }
        #endregion

        #region Private Helpers

        private void AddNewCard()
        {
            Card newCard = new Card();

            newCard.IsTemplateCard = true;
            if (_treeViewCards != null && find_selected(_treeViewCards) >= 0)
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
            else
            {
                newCard.Regions.Add(new Card_Region(ref _nextRegionId));
                newCard.Regions[0].ideal_location = new Rect(0, 0, 1, 1);
            }

            CurrentCardCollection.Add_Card_To_Collection(newCard);
            NotifyCollectionChanged();
        }

        private void SaveCardCollection()
        {
            // You would implement your Product save here
        }
        
        private Card_Region Find_SelectedCard_Region(int regionId)
        {
            Card selectedCard = Find_Selected_Card();

            foreach (Card_Region i in selectedCard.Regions)
            {
                if (i.id == regionId)
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
