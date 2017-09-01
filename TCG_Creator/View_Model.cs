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
using System.Collections.ObjectModel;

namespace TCG_Creator
{
    public class View_Model : ObservableObject
    {
        public View_Model()
        {
            _richTextBox.Background = new SolidColorBrush(Color.FromArgb(180, 100, 100, 100));
            
            RichTextBoxFormatBar formatBar = new RichTextBoxFormatBar();
            RichTextBoxFormatBarManager.SetFormatBar(_richTextBox, formatBar);

            Xml_Load(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData)+"\\TCG_Creator\\autosave.xml", false);

            OnPropertyChanged("CurrentCardCollection");

            Binding tmpBinding = new Binding();
            tmpBinding.Source = RegionText;
            tmpBinding.Path = new PropertyPath("Text");
            tmpBinding.Mode = BindingMode.TwoWay;
            tmpBinding.NotifyOnSourceUpdated = true;
            _textBox.SetBinding(TextBox.TextProperty, tmpBinding);
            _textBox.TextWrapping = TextWrapping.WrapWithOverflow;
        }

        #region Fields

        private Card_Collection _cardCollection = new Card_Collection();
        private ICommand _addNewTemplateCardCommand;
        private ICommand _saveTemplateCardCommand;
        private ICommand _addNewRegionCommand;
        private ICommand _deleteSelectedTemplateCard;
        private ICommand _deleteSelectedRegion;
        private ICommand _moveSelectedRegionCommand;

        private Xceed.Wpf.Toolkit.RichTextBox _richTextBox = new Xceed.Wpf.Toolkit.RichTextBox();
        private TextBox _textBox = new TextBox();

        private bool _richTextBoxEditingTools = false;
        private bool _deleteNextSelectedRegion = false;
        private bool _hideTextEditBox = false;

        private bool _clickedOnRegion = false;

        private Rect _selectionBox = new Rect(0, 0, 100, 100);
        private Card_Region _lastSelectedCardRegion = new Card_Region();

        private bool _isAddingNewCardRegion = false;
        private bool _isMovingSelectedCardRegion = false;

        private Templates _templates = new Templates();
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
                    OnPropertyChanged("CurrentCardCollection");
                }
            }
        }

        public ICommand SaveTemplateCardCommand
        {
            get
            {
                if (_saveTemplateCardCommand == null)
                {
                    _saveTemplateCardCommand = new RelayCommand(
                        param => SaveTemplateCard(),
                        param => (_saveTemplateCardCommand != null)
                    );
                }
                return _saveTemplateCardCommand;
            }
        }

        public ICommand AddTemplateCardCommand
        {
            get
            {
                if (_addNewTemplateCardCommand == null)
                {
                    _addNewTemplateCardCommand = new RelayCommand(
                        param => AddNewTemplateCard()
                    );
                }
                return _addNewTemplateCardCommand;
            }
        }
        public ICommand AddNewRegionCommand
        {
            get
            {
                if (_addNewRegionCommand == null)
                {
                    _addNewRegionCommand = new RelayCommand(
                        param => StartAddRegion()
                    );
                }
                return _addNewRegionCommand;
            }
        }
        public ICommand DeleteSelectedTemplateCardCommand
        {
            get
            {
                if (_deleteSelectedTemplateCard == null)
                {
                    _deleteSelectedTemplateCard = new RelayCommand(
                        param => SaveTemplateCard()
                    );
                }
                return _deleteSelectedTemplateCard;
            }
        }
        public ICommand DeleteSelectedRegionCommand
        {
            get
            {
                if (_deleteSelectedRegion == null)
                {
                    _deleteSelectedRegion = new RelayCommand(
                        param => DeleteSelectedRegion()
                    );
                }
                return _deleteSelectedRegion;
            }
        }
        public ICommand MoveSelectedRegionCommand
        {
            get
            {
                if (_moveSelectedRegionCommand == null)
                {
                    _moveSelectedRegionCommand = new RelayCommand(
                        param => StartMoveSelectedRegion()
                    );
                }
                return _moveSelectedRegionCommand;
            }
        }
        private void DeleteSelectedRegion()
        {
            if (SelectedRegionId >= 1)
            {
                int regionIdToBeDeleted = SelectedRegionId;
                SelectedRegionId = 0;

                for (int i = 0; i < CurrentlySelectedCard.Regions.Count; ++i)
                {
                    if (CurrentlySelectedCard.Regions[i].id == regionIdToBeDeleted)
                    {
                        CurrentlySelectedCard.Regions.RemoveAt(i);
                        break;
                    }
                }

                OnPropertyChanged("CurrentlySelectedCard");
            }
        }

        [DependsUpon("CurrentlySelectedCard")]
        [DependsUpon("CurrentlySelectedRegion")]
        [DependsUpon("ShowAllRegions")]
        [DependsUpon("HideTextEditBox")]
        [DependsUpon("CardRenderHeight")]
        [DependsUpon("CardRenderWidth")]
        public IList<FrameworkElement> Drawing_Card_Elements
        {
            get
            {
                IList<FrameworkElement> result = new List<FrameworkElement>();

                Card selectedCard = Find_Selected_Card();
                DrawingGroup cardDrawing = selectedCard.Render_Card(new Rect(0, 0, CardRenderWidth, CardRenderHeight), ref _cardCollection, PPI);

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
                    if (SelectedRegionId == i.id)
                    {
                        if (HideTextEditBox || !_clickedOnRegion || CurrentlySelectedRegion.IsLocked)
                        {
                            frameworkElement = new Rectangle();

                            ((Rectangle)frameworkElement).Fill = Brushes.Transparent;
                            ((Rectangle)frameworkElement).Style = (Style)Application.Current.Resources["Selected_Card_Rectangle"];
                        }
                        else
                        {
                            frameworkElement = _textBox;
                            _textBox.Text = RegionText;
                            _textBox.FontSize = FontSize;
                        }
                    }
                    else
                    {
                        frameworkElement = new Rectangle();

                        ((Rectangle)frameworkElement).Fill = Brushes.Transparent;
                        ((Rectangle)frameworkElement).Style = (Style)Application.Current.Resources["Card_Rectangle"];

                        Binding tmpBinding = new Binding();
                        tmpBinding.Source = i;
                        tmpBinding.Path = new PropertyPath("IsMouseOver");
                        tmpBinding.Mode = BindingMode.TwoWay;
                        tmpBinding.UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged;

                        ((Rectangle)frameworkElement).SetBinding(Rectangle.TagProperty, tmpBinding);

                        if (ShowAllRegions)
                        {
                            ((Rectangle)frameworkElement).Style = (Style)Application.Current.Resources["Show_All_Card_Rectangle"];
                        }
                    }

                    frameworkElement.Height = i.ideal_location.Height * CardRenderHeight;
                    frameworkElement.Width = i.ideal_location.Width * CardRenderWidth;

                    frameworkElement.AllowDrop = true;
                    
                    frameworkElement.DragEnter += FrameworkElement_DragEnter;
                    frameworkElement.Drop += FrameworkElement_Drop;

                    frameworkElement.ToolTip = i.DisplayName;

                    Thickness margin = new Thickness();

                    margin.Left = i.ideal_location.X * CardRenderWidth;
                    margin.Top = i.ideal_location.Y * CardRenderHeight;

                    frameworkElement.Margin = margin;

                    //DrawingBrush rectFill = new DrawingBrush(i.Draw_Region(new Rect(rectangle.Margin.Left, rectangle.Margin.Top, rectangle.Width, rectangle.Height)));
                    //rectFill.Stretch = Stretch.None;
                    //rectangle.Fill = rectFill;
                    frameworkElement.MouseUp += FrameworkElement_MouseUp;
                    frameworkElement.DataContext = i;

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
                Card_Region selectedRegion = (Card_Region)((FrameworkElement)sender).DataContext;
                
                selectedRegion.DesiredInherittedProperties.ImageProperties.BackgroundImageFillType = IMAGE_OPTIONS.Unified_Fill;
                selectedRegion.DesiredInherittedProperties.ImageProperties.BackgroundImageLocation = data;

                OnPropertyChanged("Drawing_Card_Elements");
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
            if (!IsAddingNewCardRegion && !IsMovingSelectedCardRegion)
            {
                if (DeleteNextSelectedRegion)
                {
                    int regionId = ((Card_Region)((FrameworkElement)sender).DataContext).id;

                    if (SelectedRegionId == regionId)
                    {
                        DisplayRichTextBoxEditingTools = false;
                        SelectedRegionId = -1;
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
                    OnPropertyChanged("Drawing_Card_Elements");
                    return;
                }

                if (SelectedRegionId != -1)
                {
                    DisplayRichTextBoxEditingTools = false;
                    // Find_SelectedCard_Region(SelectedRegion).ConvertFromFlowDocumentToStringContainer(_richTextBox.Document);
                    if (!HideTextEditBox && ! CurrentlySelectedRegionIsLocked)
                    {
                        CurrentlySelectedRegion.DesiredInherittedProperties.StringContainer.strings = new List<String_Drawing>();
                        CurrentlySelectedRegion.DesiredInherittedProperties.StringContainer.strings.Add(new String_Drawing(_textBox.Text));
                    }

                    SelectedRegionId = -1;
                }
                else if (((Card_Region)((FrameworkElement)sender).DataContext).id != 0)
                {
                    DisplayRichTextBoxEditingTools = true;
                    SelectedRegionId = ((Card_Region)((FrameworkElement)sender).DataContext).id;
                    _clickedOnRegion = true;
                }
                OnPropertyChanged("Drawing_Card_Elements");
            }
        }


        public void AddNewCardRegion()
        {
            Rect location = new Rect(SelectionRegionLeft, SelectionRegionTop, SelectionRegionWidth, SelectionRegionHeight);

            location.X /= CardRenderWidth;
            location.Width /= CardRenderWidth;
            location.Y /= CardRenderHeight;
            location.Height /= CardRenderHeight;

            if (location.X < 0)
            {
                location.X = 0;
            }
            if (location.Y < 0)
            {
                location.Y = 0;
            }
            if (location.X + location.Width > 1.0)
            {
                location.Width = 1.0 - location.X;
            }
            if (location.Y + location.Height > 1.0)
            {
                location.Height = 1.0 - location.Y;
            }

            Card selectedCard = Find_Selected_Card();

            selectedCard.Regions.Add(new Card_Region(ref CurrentCardCollection.NextRegionId));

            selectedCard.Regions.Last().ideal_location = location;

            OnPropertyChanged("Drawing_Card_Elements");
        }
        public void StartAddRegion()
        {
            IsAddingNewCardRegion = true;
        }
        public double SelectionRegionLeft
        {
            get { return _selectionBox.X; }
            set
            {
                if (_selectionBox.X != value)
                {
                    _selectionBox.X = value;
                    OnPropertyChanged("SelectionRegionLeft");
                }
            }
        }
        public double SelectionRegionTop
        {
            get { return _selectionBox.Y; }
            set
            {
                if (_selectionBox.Y != value)
                {
                    _selectionBox.Y = value;
                    OnPropertyChanged("SelectionRegionTop");
                }
            }
        }
        public double SelectionRegionWidth
        {
            get { return _selectionBox.Width; }
            set
            {
                if (_selectionBox.Width != value)
                {
                    _selectionBox.Width = value;
                    OnPropertyChanged("SelectionRegionWidth");
                }
            }
        }
        public double SelectionRegionHeight
        {
            get { return _selectionBox.Height; }
            set
            {
                if (_selectionBox.Height != value)
                {
                    _selectionBox.Height = value;
                    OnPropertyChanged("SelectionRegionHeight");
                }
            }
        }
        public bool IsAddingNewCardRegion
        {
            get { return _isAddingNewCardRegion; }
            set
            {
                if(_isAddingNewCardRegion != value)
                {
                    _isAddingNewCardRegion = value;
                    OnPropertyChanged("IsAddingNewCardRegion");
                }
            }
        }


        public bool IsMovingSelectedCardRegion
        {
            get { return _isMovingSelectedCardRegion; }
            set
            {
                if (_isMovingSelectedCardRegion != value)
                {
                    _isMovingSelectedCardRegion = value;
                    OnPropertyChanged("IsMovingSelectedCardRegion");
                }
            }
        }
        public void StartMoveSelectedRegion()
        {
            IsMovingSelectedCardRegion = true;
        }
        public void MoveSelectedRegion()
        {
            Rect location = new Rect(SelectionRegionLeft, SelectionRegionTop, SelectionRegionWidth, SelectionRegionHeight);

            location.X /= CardRenderWidth;
            location.Width /= CardRenderWidth;
            location.Y /= CardRenderHeight;
            location.Height /= CardRenderHeight;

            if (location.X < 0)
            {
                location.X = 0;
            }
            if (location.Y < 0)
            {
                location.Y = 0;
            }
            if (location.X + location.Width > 1.0)
            {
                location.Width = 1.0 - location.X;
            }
            if (location.Y + location.Height > 1.0)
            {
                location.Height = 1.0 - location.Y;
            }

            CurrentlySelectedRegion.ideal_location = location;

            List<Card_Region> allRelatedRegions = GetAllRelatedRegions(CurrentlySelectedRegion.id, CurrentlySelectedCard.Id, CurrentlySelectedCard.ParentCard);

            foreach (Card_Region i in allRelatedRegions)
            {
                i.ideal_location = location;
            }

            OnPropertyChanged("Drawing_Card_Elements");
        }


        [DependsUpon("CurrentlySelectedRegion")]
        public bool IsValidToAddRegion
        {
            get
            {
                bool result = true;

                result &= !IsMovingSelectedCardRegion;
                result &= !IsAddingNewCardRegion;
                result &= !CurrentlySelectedCard.IsBaseTemplate;

                return result;
            }
        }
        [DependsUpon("CurrentlySelectedRegion")]
        public bool IsValidToDeleteRegion
        {
            get
            {
                bool result = true;

                result &= !IsMovingSelectedCardRegion;
                result &= !IsAddingNewCardRegion;
                result &= SelectedRegionId >= 1;
                result &= !CurrentlySelectedCard.IsBaseTemplate;

                return result;
            }
        }
        [DependsUpon("CurrentlySelectedRegion")]
        public bool IsValidToLockRegion
        {
            get
            {
                bool result = true;
                
                result &= SelectedRegionId >= 1;
                result &= !CurrentlySelectedCard.IsBaseTemplate;

                return result;
            }
        }
        [DependsUpon("CurrentlySelectedRegion")]
        public bool IsValidToMoveRegion
        {
            get
            {
                bool result = true;

                result &= !IsMovingSelectedCardRegion;
                result &= !IsAddingNewCardRegion;
                result &= SelectedRegionId >= 1;
                result &= !CurrentlySelectedCard.IsBaseTemplate;

                return result;
            }
        }
        


        [DependsUpon("CurrentlySelectedRegion")]
        public string RegionText
        {
            get
            {
                return CurrentlySelectedRegion.DesiredInherittedProperties.StringContainer.GetAllStrings();
            }
            set
            {
                if (RegionText != value)
                {
                    CurrentlySelectedRegion.DesiredInherittedProperties.StringContainer.strings.Clear();
                    CurrentlySelectedRegion.DesiredInherittedProperties.StringContainer.strings.Add(new String_Drawing(value));
                    OnPropertyChanged("RegionText");
                }
            }
        }
        
        [DependsUpon("CurrentCardCollection")]
        public List<Tree_View_Card> TreeViewCards
        {
            get
            {
                List<Tree_View_Card> result = new List<Tree_View_Card>(CurrentCardCollection.Get_Tree_View_Template_Cards(ref _cardCollection));
                return result;
            }
        }
        [DependsUpon("CurrentCardCollection")]
        public Tree_View_Card CurrentlySelectedTreeViewCard
        {
            get
            {
                return FindSelectedTreeViewCard(TreeViewCards);
            }
            set
            {
                if (value != null)
                {
                    if (_cardCollection.SelectedCardId != value.Id)
                    {
                        _cardCollection.SelectedCardId = value.Id;
                        OnPropertyChanged("CurrentlySelectedTreeViewCard");
                    }
                }
            }
        }
        public bool CurrentlySelectedTreeViewCardChanged
        {
            get { return false; }
            set
            {
                RaiseOnPropertyChanged("CurrentlySelectedTreeViewCard");
            }
        }
        [DependsUpon("CurrentlySelectedTreeViewCard")]
        public string CurrentlySelectedTreeViewCardDisplayName
        {
            get
            {
                return CurrentlySelectedTreeViewCard.DisplayName;
            }
        }

        [DependsUpon("CurrentlySelectedCard")]
        public IList<Card_Region> CurrentlySelectedCardRegions
        {
            get
            {
                var temp = new List<Card_Region>(CurrentlySelectedCard.Regions);

                if (temp.Count >= 1)
                {
                    temp.RemoveAt(0);
                }

                if (temp.Count == 0)
                {
                    temp.Add(new Card_Region());
                    temp[0].id = -1;
                }

                return temp;
            }
        }

        
        [DependsUpon("CurrentlySelectedCard")]
        public int SelectedRegionId
        {
            get
            {
                return _cardCollection.SelectedRegionId;
            }
            set
            {
                if (_cardCollection.SelectedRegionId != value)
                {
                    _cardCollection.SelectedRegionId = value;
                    _lastSelectedCardRegion = CurrentlySelectedRegion;
                    _clickedOnRegion = false;
                    OnPropertyChanged("SelectedRegionId");
                }
            }
        }
        [DependsUpon("SelectedRegionId")]
        [DependsUpon("CurrentlySelectedCard")]
        public Card_Region CurrentlySelectedRegion
        {
            get
            {
                return Find_SelectedCard_Region(SelectedRegionId);
            }
            set
            {
                if (value != null)
                {
                    if (value.id != SelectedRegionId)
                    {
                        SelectedRegionId = value.id;
                        _lastSelectedCardRegion = value;
                    }
                }
            }
        }
        [DependsUpon("CurrentlySelectedRegion")]
        public string CurrentlySelectedRegionDescription
        {
            get { return CurrentlySelectedRegion.description; }
            set
            {
                if (CurrentlySelectedRegion.description != value)
                {
                    CurrentlySelectedRegion.description = value;
                    OnPropertyChanged("CurrentlySelectedRegionDescription");
                }
            }
        }
        [DependsUpon("CurrentlySelectedRegion")]
        [DependsUpon("CurrentlySelectedRegionDescription")]
        public string CurrentlySelectedRegionTitle
        {
            get
            {
                string result = "";

                if (SelectedRegionId == -1)
                {
                    result = "No Region Selected";
                }
                else
                {
                    result = RegionTitle(SelectedRegionId);
                }

                return result;
            }
        }

        [DependsUpon("CurrentCardCollection")]
        public List<Deck> CurrentDecks
        {
            get { return CurrentCardCollection.Decks; }
        }
        [DependsUpon("CurrentDecks")]
        public int CurrentlySelectedDeckId
        {
            get { return CurrentCardCollection.SelectedDeckId; }
            set
            {
                if (CurrentlySelectedDeckId != value)
                {
                    CurrentCardCollection.SelectedDeckId = value;
                    OnPropertyChanged("CurrentlySelectedDeckId");
                }
            }
        }
        [DependsUpon("CurrentDecks")]
        [DependsUpon("CurrentlySelectedDeckId")]
        public Deck CurrentlySelectedDeck
        {
            get
            {
                if (CurrentlySelectedDeckId >= 1)
                {
                    for (int i = 0; i < CurrentDecks.Count; ++i)
                    {
                        if (CurrentDecks[i].Id == CurrentlySelectedDeckId)
                        {
                            return CurrentDecks[i];
                        }
                    }
                    throw new Exception("Selected deck does not exist.");
                }
                return new Deck();
            }
        }

        [DependsUpon("CurrentCardCollection")]
        [DependsUpon("CurrentlySelectedTreeViewCard")]
        public Card CurrentlySelectedCard
        {
            get
            {
                return Find_Selected_Card();
            }
        }

        [DependsUpon("CurrentlySelectedCard")]
        public double CardPhysicalHeight
        {
            get
            {
                return CurrentlySelectedCard.PhysicalSizeHeight;
            }
            set
            {
                if (CurrentlySelectedCard.PhysicalSizeHeight != value)
                {
                    CurrentlySelectedCard.PhysicalSizeHeight = value;
                    OnPropertyChanged("CardPhysicalHeight");
                }
            }
        }
        [DependsUpon("CurrentlySelectedCard")]
        public double CardPhysicalWidth
        {
            get
            {
                return CurrentlySelectedCard.PhysicalSizeWidth;
            }
            set
            {
                if (CurrentlySelectedCard.PhysicalSizeWidth != value)
                {
                    CurrentlySelectedCard.PhysicalSizeWidth = value;
                    OnPropertyChanged("CardPhysicalWidth");
                }
            }
        }

        public double MaxLevelOfDetail
        {
            get
            {
                return 3000;
            }
        }
        public double MinLevelOfDetail
        {
            get
            {
                return 1;
            }
        }

        [DependsUpon("CardPhysicalHeight")]
        [DependsUpon("LevelOfDetail")]
        public double CardRenderHeight
        {
            get
            {
                return CardPhysicalHeight * PPI;
            }
        }
        [DependsUpon("CardPhysicalHeight")]
        [DependsUpon("LevelOfDetail")]
        public double CardRenderWidth
        {
            get
            {
                return CardPhysicalWidth * PPI;
            }
        }

        #endregion

        protected override void Save(string file)
        {
            Xml_Save(file);
        }

        

        #region Card Region Properties
        [DependsUpon("GradientBrushRequested")]
        [DependsUpon("DisplayRichTextBoxEditingTools")]
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
        [DependsUpon("DisplayRichTextBoxEditingTools")]
        public bool VisOfRichTextEditing
        {
            get
            {
                if (DisplayRichTextBoxEditingTools)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
        }

        [DependsUpon("CurrentlySelectedRegion")]
        public bool GradientBrushRequested
        {
            get { return CurrentlySelectedRegion.DesiredInherittedProperties.StringProperties.TextBrushColorMode != TextBrushMode.SolidColor; }
            set
            {
                if ((CurrentlySelectedRegion.DesiredInherittedProperties.StringProperties.TextBrushColorMode != TextBrushMode.SolidColor) != value)
                {
                    if (value)
                    {
                        CurrentlySelectedRegion.DesiredInherittedProperties.StringProperties.TextBrushColorMode = TextBrushMode.Gradient;
                    }
                    else
                    {
                        CurrentlySelectedRegion.DesiredInherittedProperties.StringProperties.TextBrushColorMode = TextBrushMode.SolidColor;
                    }

                    OnPropertyChanged("GradientBrushRequested");
                }
            }
        }
        [DependsUpon("GradientBrushRequested")]
        public bool NotGradientBrushRequested
        {
            get { return !GradientBrushRequested; }
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
                }
            }
        }
        
        [DependsUpon("DeleteNextSelectedRegion")]
        public bool ButAddRegionEnabled
        {
            get { return !DeleteNextSelectedRegion; }
        }
        public bool DeleteNextSelectedRegion
        {
            get { return _deleteNextSelectedRegion; }
            set
            {
                if (_deleteNextSelectedRegion != value)
                {
                    _deleteNextSelectedRegion = value;
                    OnPropertyChanged("ButAddRegionEnabled");
                }
            }
        }
        public bool HideTextEditBox
        {
            get { return _hideTextEditBox; }
            set
            {
                if (_hideTextEditBox != value)
                {
                    _hideTextEditBox = value;
                    OnPropertyChanged("HideTextEditBox");
                }
            }
        }
        public bool ShowAllRegions
        {
            get { return _cardCollection.ShowAllRegions; }
            set
            {
                if (_cardCollection.ShowAllRegions != value)
                {
                    _cardCollection.ShowAllRegions = value;
                    OnPropertyChanged("ShowAllRegions");
                }
            }
        }

        [DependsUpon("CurrentlySelectedRegion")]
        public bool InheritFontFamily
        {
            get { return CurrentlySelectedRegion.DesiredInherittedProperties.StringProperties.InheritFontFamily; }
            set
            {
                if (CurrentlySelectedRegion.DesiredInherittedProperties.StringProperties.InheritFontFamily != value)
                {
                    CurrentlySelectedRegion.DesiredInherittedProperties.StringProperties.InheritFontFamily = value;
                    OnPropertyChanged("InheritFontFamily");
                }
            }
        }
        [DependsUpon("InheritFontFamily")]
        public bool NotInheritFontFamily
        {
            get
            {
                return !InheritFontFamily;
            }
        }
        [DependsUpon("CurrentlySelectedRegion")]
        public string SelectedFontFamily
        {
            get { return CurrentlySelectedRegion.DesiredInherittedProperties.StringProperties.FontFamily; }
            set
            {
                if (CurrentlySelectedRegion.DesiredInherittedProperties.StringProperties.FontFamily != value)
                {
                    CurrentlySelectedRegion.DesiredInherittedProperties.StringProperties.FontFamily = value;

                    OnPropertyChanged("SelectedFontFamily");
                }
            }
        }
        public IList<string> AllFontFamilies
        {
            get
            {
                SortedList<string, FontFamily> sortedResult = new SortedList<string, FontFamily>();

                foreach( FontFamily i in Fonts.SystemFontFamilies)
                {
                    sortedResult.Add(i.Source, i);
                }

                foreach (FontFamily fontFamily in Fonts.GetFontFamilies(new Uri("pack://application:,,,/"), "./Resources/Fonts/"))
                {
                    sortedResult.Add(fontFamily.Source.Remove(0, 18), fontFamily);
                }
                
                return sortedResult.Keys;
            }
        }

        [DependsUpon("CurrentlySelectedRegion")]
        public bool InheritFontSize
        {
            get { return CurrentlySelectedRegion.DesiredInherittedProperties.StringProperties.InheritFontSize; }
            set
            {
                if (CurrentlySelectedRegion.DesiredInherittedProperties.StringProperties.InheritFontSize != value)
                {
                    CurrentlySelectedRegion.DesiredInherittedProperties.StringProperties.InheritFontSize = value;

                    OnPropertyChanged("InheritFontSize");
                }
            }
        }
        [DependsUpon("InheritFontSize")]
        public bool NotInheritFontSize
        {
            get { return !InheritFontSize; }
        }
        [DependsUpon("CurrentlySelectedRegion")]
        public double FontSize
        {
            get { return CurrentlySelectedRegion.DesiredInherittedProperties.StringProperties.FontSize; }
            set
            {
                if (CurrentlySelectedRegion.DesiredInherittedProperties.StringProperties.FontSize != value)
                {
                    CurrentlySelectedRegion.DesiredInherittedProperties.StringProperties.FontSize = value;

                    OnPropertyChanged("FontSize");
                }
            }
        }

        [DependsUpon("CurrentlySelectedRegion")]
        public bool InheritFontStyle
        {
            get { return CurrentlySelectedRegion.DesiredInherittedProperties.StringProperties.InheritFontStyle; }
            set
            {
                if (CurrentlySelectedRegion.DesiredInherittedProperties.StringProperties.InheritFontStyle != value)
                {
                    CurrentlySelectedRegion.DesiredInherittedProperties.StringProperties.InheritFontStyle = value;

                    OnPropertyChanged("InheritFontStyle");
                }
            }
        }
        [DependsUpon("InheritFontStyle")]
        public bool NotInheritFontStyle
        {
            get { return !InheritFontStyle; }
        }
        [DependsUpon("CurrentlySelectedRegion")]
        [DependsUpon("Italic")]
        public FontStyle SelectedFontStyle
        {
            get { return CurrentlySelectedRegion.DesiredInherittedProperties.StringProperties.SFontStyle; }
            set
            {
                if (CurrentlySelectedRegion.DesiredInherittedProperties.StringProperties.SFontStyle != value)
                {
                    CurrentlySelectedRegion.DesiredInherittedProperties.StringProperties.SFontStyle = value;

                    OnPropertyChanged("SelectedFontStyle");
                }
            }
        }

        [DependsUpon("CurrentlySelectedRegion")]
        public bool InheritFontWeight
        {
            get { return CurrentlySelectedRegion.DesiredInherittedProperties.StringProperties.InheritFontWeight; }
            set
            {
                if (CurrentlySelectedRegion.DesiredInherittedProperties.StringProperties.InheritFontWeight != value)
                {
                    CurrentlySelectedRegion.DesiredInherittedProperties.StringProperties.InheritFontWeight = value;

                    OnPropertyChanged("InheritFontWeight");
                }
            }
        }
        [DependsUpon("InheritFontWeight")]
        public bool NotInheritFontWeight
        {
            get { return !InheritFontWeight; }
        }
        [DependsUpon("CurrentlySelectedRegion")]
        [DependsUpon("Bold")]
        public FontWeight SelectedFontWeight
        {
            get { return CurrentlySelectedRegion.DesiredInherittedProperties.StringProperties.SFontWeight; }
            set
            {
                if (CurrentlySelectedRegion.DesiredInherittedProperties.StringProperties.SFontWeight != value)
                {
                    CurrentlySelectedRegion.DesiredInherittedProperties.StringProperties.SFontWeight = value;

                    OnPropertyChanged("SelectedFontWeight");
                }
            }
        }

        [DependsUpon("CurrentlySelectedRegion")]
        public bool InheritTextBrush
        {
            get { return CurrentlySelectedRegion.DesiredInherittedProperties.StringProperties.InheritFontBrush; }
            set
            {
                if (CurrentlySelectedRegion.DesiredInherittedProperties.StringProperties.InheritFontBrush != value)
                {
                    CurrentlySelectedRegion.DesiredInherittedProperties.StringProperties.InheritFontBrush = value;

                    OnPropertyChanged("InheritTextBrush");
                }
            }
        }
        [DependsUpon("InheritTextBrush")]
        public bool NotInheritTextBrush
        {
            get { return !InheritTextBrush; }
        }
        [DependsUpon("CurrentlySelectedRegion")]
        [DependsUpon("GradientBrushRequested")]
        public Color SelectedTextFontBrushColor
        {
            get
            {
                return CurrentlySelectedRegion.DesiredInherittedProperties.StringProperties.SolidColorTextBrushColor;
            }
            set
            {
                if (CurrentlySelectedRegion.DesiredInherittedProperties.StringProperties.SolidColorTextBrushColor != value)
                {
                    CurrentlySelectedRegion.DesiredInherittedProperties.StringProperties.SolidColorTextBrushColor = value;
                    OnPropertyChanged("SelectedTextFontBrushColor");
                }
            }
        }
        
        [DependsUpon("CurrentlySelectedRegion")]
        public double SelectedGradientFontBrushAngle
        {
            get
            {
                return CurrentlySelectedRegion.DesiredInherittedProperties.StringProperties.GradientTextBrushAngle;
            }
            set
            {
                if (CurrentlySelectedRegion.DesiredInherittedProperties.StringProperties.GradientTextBrushAngle != value)
                {
                    CurrentlySelectedRegion.DesiredInherittedProperties.StringProperties.GradientTextBrushAngle = value;
                    OnPropertyChanged("CurrentlySelectedRegion");
                }
            }
        }
        [DependsUpon("CurrentlySelectedRegion")]
        public Color SelectedGradientFontBrush1
        {
            get
            {
                return CurrentlySelectedRegion.DesiredInherittedProperties.StringProperties.GradientTextBrushStops.First().Color;
            }
            set
            {
                if (CurrentlySelectedRegion.DesiredInherittedProperties.StringProperties.GradientTextBrushStops.First().Color != value)
                {
                    GradientStop tmp = new GradientStop(value, 0);

                    CurrentlySelectedRegion.DesiredInherittedProperties.StringProperties.GradientTextBrushStops.RemoveAt(0);
                    CurrentlySelectedRegion.DesiredInherittedProperties.StringProperties.GradientTextBrushStops.Insert(0, tmp);

                    OnPropertyChanged("SelectedGradientFontBrush1");
                }
            }
        }
        [DependsUpon("CurrentlySelectedRegion")]
        public double SelectedGradientFontBrushOffset1
        {
            get
            {
                return CurrentlySelectedRegion.DesiredInherittedProperties.StringProperties.GradientTextBrushStops.First().Offset;
            }
            set
            {
                if (CurrentlySelectedRegion.DesiredInherittedProperties.StringProperties.GradientTextBrushStops.First().Offset != value)
                {
                    GradientStop tmp = new GradientStop(CurrentlySelectedRegion.DesiredInherittedProperties.StringProperties.GradientTextBrushStops.First().Color, value);

                    CurrentlySelectedRegion.DesiredInherittedProperties.StringProperties.GradientTextBrushStops.RemoveAt(0);
                    CurrentlySelectedRegion.DesiredInherittedProperties.StringProperties.GradientTextBrushStops.Insert(0, tmp);

                    OnPropertyChanged("SelectedGradientFontBrushOffset1");
                }
            }
        }
        [DependsUpon("CurrentlySelectedRegion")]
        public Color SelectedGradientFontBrush2
        {
            get
            {
                return CurrentlySelectedRegion.DesiredInherittedProperties.StringProperties.GradientTextBrushStops.Last().Color;
            }
            set
            {
                if (CurrentlySelectedRegion.DesiredInherittedProperties.StringProperties.GradientTextBrushStops.Last().Color != value)
                {
                    GradientStop tmp = new GradientStop(value, 1);

                    CurrentlySelectedRegion.DesiredInherittedProperties.StringProperties.GradientTextBrushStops.RemoveAt(1);
                    CurrentlySelectedRegion.DesiredInherittedProperties.StringProperties.GradientTextBrushStops.Add(tmp);

                    OnPropertyChanged("SelectedGradientFontBrush2");
                }
            }
        }
        [DependsUpon("CurrentlySelectedRegion")]
        public double SelectedGradientFontBrushOffset2
        {
            get
            {
                return CurrentlySelectedRegion.DesiredInherittedProperties.StringProperties.GradientTextBrushStops.Last().Offset;
            }
            set
            {
                if (CurrentlySelectedRegion.DesiredInherittedProperties.StringProperties.GradientTextBrushStops.Last().Offset != value)
                {
                    GradientStop tmp = new GradientStop(CurrentlySelectedRegion.DesiredInherittedProperties.StringProperties.GradientTextBrushStops.Last().Color, value);

                    CurrentlySelectedRegion.DesiredInherittedProperties.StringProperties.GradientTextBrushStops.RemoveAt(1);
                    CurrentlySelectedRegion.DesiredInherittedProperties.StringProperties.GradientTextBrushStops.Add(tmp);

                    OnPropertyChanged("SelectedGradientFontBrushOffset2");
                }
            }
        }

        [DependsUpon("CurrentlySelectedRegion")]
        public bool InheritText
        {
            get { return CurrentlySelectedRegion.DesiredInherittedProperties.StringContainer.InheritText; }
            set
            {
                if (CurrentlySelectedRegion.DesiredInherittedProperties.StringContainer.InheritText != value)
                {
                    CurrentlySelectedRegion.DesiredInherittedProperties.StringContainer.InheritText = value;
                    OnPropertyChanged("InheritText");
                }
            }
        }
        [DependsUpon("CurrentlySelectedRegion")]
        public bool InheritRegionBeforeCard
        {
            get { return CurrentlySelectedRegion.InheritRegionBeforeCard; }
            set
            {
                if (CurrentlySelectedRegion.InheritRegionBeforeCard != value)
                {
                    CurrentlySelectedRegion.InheritRegionBeforeCard = value;
                    OnPropertyChanged("InheritRegionBeforeCard");
                }
            }
        }

        [DependsUpon("SelectedFontWeight")]
        public bool Bold
        {
            get
            {
                return SelectedFontWeight == FontWeights.Bold;
            }
            set
            {
                if ((SelectedFontWeight == FontWeights.Bold) != value)
                {
                    if (value)
                    {
                        SelectedFontWeight = FontWeights.Bold;
                    }
                    else
                    {
                        SelectedFontWeight = FontWeights.Normal;
                    }

                    OnPropertyChanged("Bold");
                }
            }
        }
        [DependsUpon("SelectedFontStyle")]
        public bool Italic
        {
            get
            {
                return SelectedFontStyle == FontStyles.Italic;
            }
            set
            {
                if ((SelectedFontStyle == FontStyles.Italic) != value)
                {
                    if (value)
                    {
                        SelectedFontStyle = FontStyles.Italic;
                    }
                    else
                    {
                        SelectedFontStyle = FontStyles.Italic;
                    }

                    OnPropertyChanged("Italic");
                }
            }
        }

        [DependsUpon("CurrentlySelectedCard")]
        [DependsUpon("SelectedGradientFontBrush1")]
        public ObservableCollection<ColorItem> GetUsedCardColors
        {
            get
            {
                IList<Color> tmp = CurrentlySelectedCard.GetUsedColors;

                IList<ColorItem> result = new List<ColorItem>();

                foreach (Color i in tmp)
                {
                    result.Add(new ColorItem(i, ""));
                }

                return new ObservableCollection<ColorItem>(result);
            }
        }
        public IList<string> AllBackgroundImageFillType
        {
            get
            {
                IList<string> result = new List<String>();

                for (int i = 0; i < (int)IMAGE_OPTIONS.N_IMAGE_OPTIONS; ++i)
                {
                    result.Add("");
                }

                result[(int)IMAGE_OPTIONS.Fill] = "Fill";
                result[(int)IMAGE_OPTIONS.Letterbox] = "Letterbox";
                result[(int)IMAGE_OPTIONS.Unified_Fill] = "Unified Fill";
                result[(int)IMAGE_OPTIONS.None] = "None";

                return result;
            }
        }

        [DependsUpon("CurrentlySelectedRegion")]
        public bool InheritBackground
        {
            get { return CurrentlySelectedRegion.DesiredInherittedProperties.ImageProperties.InheritBackground; }
            set
            {
                if (CurrentlySelectedRegion.DesiredInherittedProperties.ImageProperties.InheritBackground != value)
                {
                    CurrentlySelectedRegion.DesiredInherittedProperties.ImageProperties.InheritBackground = value;

                    OnPropertyChanged("InheritBackground");
                }
            }
        }
        [DependsUpon("InheritBackground")]
        public bool NotInheritBackground
        {
            get { return !InheritBackground; }
        }
        [DependsUpon("CurrentlySelectedRegion")]
        public string BackgroundImageFillType
        {
            get { return AllBackgroundImageFillType[(int)CurrentlySelectedRegion.DesiredInherittedProperties.ImageProperties.BackgroundImageFillType]; }
            set
            {
                IMAGE_OPTIONS real_value = (IMAGE_OPTIONS)AllBackgroundImageFillType.IndexOf(value);

                if (CurrentlySelectedRegion.DesiredInherittedProperties.ImageProperties.BackgroundImageFillType != real_value)
                {
                    CurrentlySelectedRegion.DesiredInherittedProperties.ImageProperties.BackgroundImageFillType = real_value;

                    OnPropertyChanged("BackgroundImageFillType");
                }
            }
        }
        [DependsUpon("CurrentlySelectedRegion")]
        public string BackgroundImageLocation
        {
            get { return CurrentlySelectedRegion.DesiredInherittedProperties.ImageProperties.BackgroundImageLocation; }
            set
            {
                if (CurrentlySelectedRegion.DesiredInherittedProperties.ImageProperties.BackgroundImageLocation != value)
                {
                    CurrentlySelectedRegion.DesiredInherittedProperties.ImageProperties.BackgroundImageLocation = value;

                    OnPropertyChanged("BackgroundImageLocation");
                }
            }
        }

        [DependsUpon("CurrentlySelectedRegion")]
        public bool InheritTextHorizontalAlignment
        {
            get { return CurrentlySelectedRegion.DesiredInherittedProperties.StringProperties.InheritTextHorizontalAlignment; }
            set
            {
                if (CurrentlySelectedRegion.DesiredInherittedProperties.StringProperties.InheritTextHorizontalAlignment != value)
                {
                    CurrentlySelectedRegion.DesiredInherittedProperties.StringProperties.InheritTextHorizontalAlignment = value;
                    OnPropertyChanged("InheritTextHorizontalAlignment");
                }
            }
        }
        [DependsUpon("InheritTextHorizontalAlignment")]
        public bool NotInheritTextHorizontalAlignment
        {
            get { return !InheritTextHorizontalAlignment; }
        }
        [DependsUpon("CurrentlySelectedRegion")]
        public HorizontalAlignment TextHorizonalAlignment
        {
            get { return CurrentlySelectedRegion.DesiredInherittedProperties.StringProperties.TextHorizontalAlignment; }
            set
            {
                if (CurrentlySelectedRegion.DesiredInherittedProperties.StringProperties.TextHorizontalAlignment != value)
                {
                    CurrentlySelectedRegion.DesiredInherittedProperties.StringProperties.TextHorizontalAlignment = value;
                    OnPropertyChanged("TextHorizontalAlignment");
                }
            }
        }

        [DependsUpon("CurrentlySelectedRegion")]
        public bool InheritTextVerticalAlignment
        {
            get { return CurrentlySelectedRegion.DesiredInherittedProperties.StringProperties.InheritTextVerticalAlignment; }
            set
            {
                if (CurrentlySelectedRegion.DesiredInherittedProperties.StringProperties.InheritTextVerticalAlignment != value)
                {
                    CurrentlySelectedRegion.DesiredInherittedProperties.StringProperties.InheritTextVerticalAlignment = value;
                    OnPropertyChanged("InheritTextVerticalAlignment");
                }
            }
        }
        [DependsUpon("InheritTextVerticalAlignment")]
        public bool NotInheritTextVerticalAlignment
        {
            get { return !InheritTextVerticalAlignment; }
        }
        [DependsUpon("CurrentlySelectedRegion")]
        public VerticalAlignment TextVerticalAlignment
        {
            get { return CurrentlySelectedRegion.DesiredInherittedProperties.StringProperties.TextVerticalAlignment; }
            set
            {
                if (CurrentlySelectedRegion.DesiredInherittedProperties.StringProperties.TextVerticalAlignment != value)
                {
                    CurrentlySelectedRegion.DesiredInherittedProperties.StringProperties.TextVerticalAlignment = value;
                    OnPropertyChanged("TextVerticalAlignment");
                }
            }
        }

        [DependsUpon("CurrentlySelectedRegion")]
        public bool InheritStrokeProperties
        {
            get { return CurrentlySelectedRegion.DesiredInherittedProperties.StringProperties.InheritStrokeProperties; }
            set
            {
                if (CurrentlySelectedRegion.DesiredInherittedProperties.StringProperties.InheritStrokeProperties != value)
                {
                    CurrentlySelectedRegion.DesiredInherittedProperties.StringProperties.InheritStrokeProperties = value;
                    OnPropertyChanged("InheritStrokeProperties");
                }
            }
        }
        [DependsUpon("InheritStrokeProperties")]
        public bool NotInheritStrokeProperties
        {
            get { return !InheritStrokeProperties; }
        }
        [DependsUpon("CurrentlySelectedRegion")]
        public bool StrokeOn
        {
            get { return CurrentlySelectedRegion.DesiredInherittedProperties.StringProperties.StrokeOn; }
            set
            {
                if (CurrentlySelectedRegion.DesiredInherittedProperties.StringProperties.StrokeOn != value)
                {
                    CurrentlySelectedRegion.DesiredInherittedProperties.StringProperties.StrokeOn = value;
                    OnPropertyChanged("StrokeOn");
                }
            }
        }
        [DependsUpon("CurrentlySelectedRegion")]
        public Color TextStrokeColor
        {
            get { return CurrentlySelectedRegion.DesiredInherittedProperties.StringProperties.TextStrokeColor; }
            set
            {
                if (CurrentlySelectedRegion.DesiredInherittedProperties.StringProperties.TextStrokeColor != value)
                {
                    CurrentlySelectedRegion.DesiredInherittedProperties.StringProperties.TextStrokeColor = value;
                    OnPropertyChanged("TextStrokeColor");
                }
            }
        }
        [DependsUpon("CurrentlySelectedRegion")]
        public double StrokeThickness
        {
            get { return CurrentlySelectedRegion.DesiredInherittedProperties.StringProperties.StrokeThickness; }
            set
            {
                if (CurrentlySelectedRegion.DesiredInherittedProperties.StringProperties.StrokeThickness != value)
                {
                    CurrentlySelectedRegion.DesiredInherittedProperties.StringProperties.StrokeThickness = value;
                    OnPropertyChanged("StrokeThickness");
                }
            }
        }

        [DependsUpon("SelectedRegionId")]
        [DependsUpon("CurrentlySelectedCard")]
        [DependsUpon("CurrentlySelectedRegionIsLocked")]
        public bool CardEditEnable
        {
            get
            {
                return SelectedRegionId >= 1 && SelectedRegionId < CurrentlySelectedCard.Regions.Count && !CurrentlySelectedRegionIsLocked;
            }
        }
        [DependsUpon("CurrentlySelectedRegion")]
        public bool CurrentlySelectedRegionIsLocked
        {
            get
            {
                return CurrentlySelectedRegion.IsLocked;
            }
            set
            {
                if (CurrentlySelectedRegion.IsLocked != value)
                {
                    CurrentlySelectedRegion.IsLocked = value;
                    OnPropertyChanged("CurrentlySelectedRegionIsLocked");
                }
            }
        }


        [DependsUpon("CurrentCardCollection")]
        public bool ShowTemplateSettings
        {
            get { return CurrentCardCollection.ShowTemplateSettings; }
            set
            {
                if (CurrentCardCollection.ShowTemplateSettings != value)
                {
                    CurrentCardCollection.ShowTemplateSettings = value;
                    OnPropertyChanged("ShowTemplateSettings");
                }
            }
        }
        [DependsUpon("ShowTemplateSettings")]
        public Visibility VisShowTemplateSettings
        {
            get
            {
                if (ShowTemplateSettings)
                {
                    return Visibility.Visible;
                }
                else
                {
                    return Visibility.Collapsed;
                }
            }
        }

        


        #region Template Settings
        [DependsUpon("CurrentlySelectedCard")]
        public bool TemplateInheritFontFamily
        {
            get { return CurrentlySelectedCard.Regions[0].DesiredInherittedProperties.StringProperties.InheritFontFamily; }
            set
            {
                if (CurrentlySelectedCard.Regions[0].DesiredInherittedProperties.StringProperties.InheritFontFamily != value)
                {
                    CurrentlySelectedCard.Regions[0].DesiredInherittedProperties.StringProperties.InheritFontFamily = value;
                    OnPropertyChanged("TemplateInheritFontFamily");
                }
            }
        }
        [DependsUpon("TemplateInheritFontFamily")]
        public bool NotTemplateInheritFontFamily
        {
            get
            {
                return !TemplateInheritFontFamily;
            }
        }
        [DependsUpon("CurrentlySelectedCard")]
        public FontFamily TemplateSelectedFontFamily
        {
            get { return new FontFamily(CurrentlySelectedCard.Regions[0].DesiredInherittedProperties.StringProperties.FontFamily); }
            set
            {
                if (CurrentlySelectedCard.Regions[0].DesiredInherittedProperties.StringProperties.FontFamily != value.Source)
                {
                    CurrentlySelectedCard.Regions[0].DesiredInherittedProperties.StringProperties.FontFamily = value.Source;

                    OnPropertyChanged("TemplateSelectedFontFamily");
                }
            }
        }

        [DependsUpon("CurrentlySelectedCard")]
        public bool TemplateInheritFontSize
        {
            get { return CurrentlySelectedCard.Regions[0].DesiredInherittedProperties.StringProperties.InheritFontSize; }
            set
            {
                if (CurrentlySelectedCard.Regions[0].DesiredInherittedProperties.StringProperties.InheritFontSize != value)
                {
                    CurrentlySelectedCard.Regions[0].DesiredInherittedProperties.StringProperties.InheritFontSize = value;

                    OnPropertyChanged("TemplateInheritFontSize");
                }
            }
        }
        public bool NotTemplateInheritFontSize
        {
            get { return !TemplateInheritFontSize; }
        }
        [DependsUpon("CurrentlySelectedCard")]
        public double TemplateFontSize
        {
            get { return CurrentlySelectedCard.Regions[0].DesiredInherittedProperties.StringProperties.FontSize; }
            set
            {
                if (CurrentlySelectedCard.Regions[0].DesiredInherittedProperties.StringProperties.FontSize != value)
                {
                    CurrentlySelectedCard.Regions[0].DesiredInherittedProperties.StringProperties.FontSize = value;

                    OnPropertyChanged("TemplateFontSize");
                }
            }
        }

        [DependsUpon("CurrentlySelectedCard")]
        public bool TemplateInheritFontStyle
        {
            get { return CurrentlySelectedCard.Regions[0].DesiredInherittedProperties.StringProperties.InheritFontStyle; }
            set
            {
                if (CurrentlySelectedCard.Regions[0].DesiredInherittedProperties.StringProperties.InheritFontStyle != value)
                {
                    CurrentlySelectedCard.Regions[0].DesiredInherittedProperties.StringProperties.InheritFontStyle = value;

                    OnPropertyChanged("TemplateInheritFontStyle");
                }
            }
        }
        public bool NotTemplateInheritFontStyle
        {
            get { return !TemplateInheritFontStyle; }
        }
        [DependsUpon("CurrentlySelectedCard")]
        [DependsUpon("TemplateItalic")]
        public FontStyle TemplateSelectedFontStyle
        {
            get { return CurrentlySelectedCard.Regions[0].DesiredInherittedProperties.StringProperties.SFontStyle; }
            set
            {
                if (CurrentlySelectedCard.Regions[0].DesiredInherittedProperties.StringProperties.SFontStyle != value)
                {
                    CurrentlySelectedCard.Regions[0].DesiredInherittedProperties.StringProperties.SFontStyle = value;

                    OnPropertyChanged("TemplateSelectedFontStyle");
                }
            }
        }

        [DependsUpon("CurrentlySelectedCard")]
        public bool TemplateInheritFontWeight
        {
            get { return CurrentlySelectedCard.Regions[0].DesiredInherittedProperties.StringProperties.InheritFontWeight; }
            set
            {
                if (CurrentlySelectedCard.Regions[0].DesiredInherittedProperties.StringProperties.InheritFontWeight != value)
                {
                    CurrentlySelectedCard.Regions[0].DesiredInherittedProperties.StringProperties.InheritFontWeight = value;

                    OnPropertyChanged("TemplateInheritFontWeight");
                }
            }
        }
        public bool NotTemplateInheritFontWeight
        {
            get { return !TemplateInheritFontWeight; }
        }
        [DependsUpon("CurrentlySelectedCard")]
        [DependsUpon("TemplateBold")]
        public FontWeight TemplateSelectedFontWeight
        {
            get { return CurrentlySelectedCard.Regions[0].DesiredInherittedProperties.StringProperties.SFontWeight; }
            set
            {
                if (CurrentlySelectedCard.Regions[0].DesiredInherittedProperties.StringProperties.SFontWeight != value)
                {
                    CurrentlySelectedCard.Regions[0].DesiredInherittedProperties.StringProperties.SFontWeight = value;

                    OnPropertyChanged("TemplateSelectedFontWeight");
                }
            }
        }

        [DependsUpon("TemplateSelectedFontWeight")]
        public bool TemplateBold
        {
            get
            {
                return TemplateSelectedFontWeight == FontWeights.Bold;
            }
            set
            {
                if ((TemplateSelectedFontWeight == FontWeights.Bold) != value)
                {
                    if (value)
                    {
                        TemplateSelectedFontWeight = FontWeights.Bold;
                    }
                    else
                    {
                        TemplateSelectedFontWeight = FontWeights.Normal;
                    }

                    OnPropertyChanged("TemplateBold");
                }
            }
        }
        [DependsUpon("TemplateSelectedFontStyle")]
        public bool TemplateItalic
        {
            get
            {
                return TemplateSelectedFontStyle == FontStyles.Italic;
            }
            set
            {
                if ((TemplateSelectedFontStyle == FontStyles.Italic) != value)
                {
                    if (value)
                    {
                        TemplateSelectedFontStyle = FontStyles.Italic;
                    }
                    else
                    {
                        TemplateSelectedFontStyle = FontStyles.Italic;
                    }

                    OnPropertyChanged("TemplateItalic");
                }
            }
        }
        #endregion

        #endregion

        #region Public GUI Properties
        [DependsUpon("CurrentDecks")]
        public IList<TabItem> DeckTabsWithTemplate
        {
            get
            {
                IList<TabItem> result = new List<TabItem>();

                result.Add(new TabItem
                {
                    Header = "Templates",
                    Content = _templates
                });

                foreach (Deck i in CurrentDecks)
                {
                    result.Add(new TabItem
                    {
                        Header = i.DisplayName,
                        Content = _templates
                    });
                }

                return result;
            }
        }

        [DependsUpon("CurrentlySelectedDeck")]
        public int SelectedTabIndex
        {
            get
            {
                if (CurrentlySelectedDeckId == 0)
                {
                    return 0;
                }
                else if (CurrentlySelectedDeckId == -1)
                {
                    throw new Exception("Invalid deck id for tab index.");
                }

                for (int i = 0; i < CurrentDecks.Count; ++i)
                {
                    if (CurrentDecks[i].Id == CurrentlySelectedDeckId)
                    {
                        return i;
                    }
                }

                return 0;
            }
            set
            {
                if (SelectedTabIndex != value)
                {
                    if (value >= CurrentDecks.Count+1)
                    {
                        throw new ArgumentException("Tab index " + value.ToString() + " is larger than the number of decks (" + CurrentDecks.Count.ToString() + ").");
                    }
                    else if (value < 0)
                    {
                        return;
                    }

                    CurrentlySelectedDeckId = CurrentDecks[value-1].Id;
                    OnPropertyChanged("SelectedTabIndex");
                }
            }
        }

        #region Public GUI Events
        public void RegionListMouseEnter(object sender, MouseEventArgs e)
        {
            Card_Region reg = (Card_Region)((ListBoxItem)sender).Content;

            reg.IsMouseOver = true;
        }
        public void RegionListMouseLeave(object sender, MouseEventArgs e)
        {
            Card_Region reg = (Card_Region)((ListBoxItem)sender).Content;

            reg.IsMouseOver = false;
        }
        public double PPI
        {
            get { return CurrentCardCollection.PPI; }
            set
            {
                if (value != PPI)
                {
                    CurrentCardCollection.PPI = value;
                    OnPropertyChanged("PPI");
                }
            }
        }
        #endregion
        #endregion

        #region Private Helpers
        private void AddNewTemplateCard()
        {
            AddNewCard(true);
        }
        private void AddNewCard(bool IsTemplate = false)
        {
            Card newCard = new Card();

            newCard.IsTemplateCard = IsTemplate;
            if (CurrentCardCollection.SelectedCardId != -1)
            {
                Card parentCard = Find_Selected_Card();
                newCard = new Card(parentCard);
                
                newCard.ParentCard = parentCard.Id;
            }
            else
            {
                newCard.Regions.Add(new Card_Region(ref CurrentCardCollection.NextRegionId));
                newCard.Regions[0].ideal_location = new Rect(0, 0, 1, 1);
            }

            CurrentCardCollection.Add_Card_To_Collection(newCard);
            OnPropertyChanged("CurrentCardCollection");
        }
        private void DeleteSelectedTemplateCard()
        {
            // You would implement your Product save here
            _deleteSelectedTemplateCard = null;
        }
        private void SaveTemplateCard()
        {
            // You would implement your Product save here
            _saveTemplateCardCommand = null;
        }
        
        
        private string RegionTitle(int regionId)
        {
            string result = "";

            if (regionId == 0)
            {
                result = "Base Card";
            }
            else
            {
                result = "Region " + regionId.ToString();
                if (Find_SelectedCard_Region(regionId).description != "")
                {
                    result += " - " + Find_SelectedCard_Region(regionId).description;
                }
            }
            return result;
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
            
            return _lastSelectedCardRegion;
        }
        private Card Find_Selected_Card()
        {
            if (CurrentCardCollection.SelectedCardId == -1)
            {
                Card temp = new Card();

                temp.Regions.Add(new Card_Region());

                return temp;
            }
            else if (CurrentCardCollection.SelectedCardId == -2)
            {
                Card temp = new Card();

                temp.Regions.Add(new Card_Region());

                return temp;
            }
            return CurrentCardCollection.Find_Card_In_Collection(CurrentCardCollection.SelectedCardId);
        }

        private Tree_View_Card findTreeViewCardFromIndex(int cardId, IList<Tree_View_Card> cards)
        {
            Tree_View_Card result = null;
            foreach (Tree_View_Card i in cards)
            {
                if (i.Id == cardId)
                {
                    return i;
                }
                else if (i.Children.Count >= 1)
                {
                    result = findTreeViewCardFromIndex(cardId, i.Children);

                    if (result.Id == cardId)
                    {
                        return result;
                    }
                }
            }

            return null;
        }
        private Tree_View_Card FindSelectedTreeViewCard(IList<Tree_View_Card> cards)
        {
            Tree_View_Card result = null;
            foreach (Tree_View_Card i in cards)
            {
                if (i.IsSelected)
                {
                    return i;
                }
                else if (i.Children.Count >= 1)
                {
                    result = FindSelectedTreeViewCard(i.Children);

                    if (result != null)
                    {
                        return result;
                    }
                }
            }

            return  null;
        }

        private List<Card_Region> GetAllRelatedRegions(int regionId, int cardId, int parentId)
        {
            List<Card_Region> result = new List<Card_Region>();

            foreach(Card i in CurrentCardCollection.CardCollection)
            {
                if (i.Id == parentId || i.ParentCard == cardId)
                {
                    foreach(Card_Region j in i.Regions)
                    {
                        if (j.id == regionId)
                        {
                            result.Add(j);
                            result.AddRange(GetAllRelatedRegions(regionId, i.Id, i.ParentCard));
                            break;
                        }
                    }
                }
            }

            return result;
        }

        #endregion

        #region Public Functions
        public void AddNewDeck()
        {
            if (CurrentCardCollection != null)
            {
                CurrentCardCollection.Add_Deck(new Deck());
                CurrentlySelectedDeckId = CurrentDecks.Last().Id;
                OnPropertyChanged("CurrentDecks");
            }
        }

        public void Xml_Save(string file, bool only_templates = false)
        {
            XmlWriterSettings xmlWriterSettings = new XmlWriterSettings();

            xmlWriterSettings.ConformanceLevel = ConformanceLevel.Document;
            xmlWriterSettings.Indent = true;
            xmlWriterSettings.NewLineOnAttributes = true;

            Directory.CreateDirectory(file.Substring(0, Math.Max(file.LastIndexOf("\\"), file.LastIndexOf("/"))));

            string tmpSaveFile = file.Substring(0, file.LastIndexOf(".")) + "_write.xml";

            XmlWriter xmlWriter = XmlWriter.Create(tmpSaveFile, xmlWriterSettings);
            XmlSerializer xmlSerializer = new XmlSerializer(typeof(Card_Collection));

            xmlSerializer.Serialize(xmlWriter, _cardCollection);

            xmlWriter.Flush();
            xmlWriter.Close();

            if (File.Exists(file))
            {
                File.Delete(file);
            }
            File.Move(tmpSaveFile, file);
        }
        public void Xml_Load(string file, bool only_templates)
        {
            string tmpLoadFile = file.Substring(0, file.LastIndexOf(".")) + "_load.xml";

            if (File.Exists(tmpLoadFile))
            {
                File.Delete(tmpLoadFile);
            }
            if (File.Exists(file))
            {
                File.Copy(file, tmpLoadFile);

                XmlReader xmlReader = XmlReader.Create(tmpLoadFile);
                XmlSerializer xmlSerializer = new XmlSerializer(typeof(Card_Collection));

                SkipSave = true;

                Card_Collection oldCollection = _cardCollection;

                _cardCollection = null;

                try
                {
                    CurrentCardCollection = (Card_Collection)xmlSerializer.Deserialize(xmlReader);

                    if (CurrentCardCollection.SelectedCardId == -1)
                    {
                        CurrentCardCollection.SelectedCardId = CurrentCardCollection.CardCollection.First().Id;
                    }

                    OnPropertyChanged("CurrentDecks");
                }
                catch
                {
                    CurrentCardCollection = oldCollection;
                }

                xmlReader.Close();

                File.Delete(tmpLoadFile);

                SkipSave = false;
            }
        }
        #endregion
    }
}
