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

            Xml_Load(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData)+"\\TCG_Creator\\autosave.xml", false);

            OnPropertyChanged("CurrentCardCollection");
        }

        #region Fields

        private Card_Collection _cardCollection = new Card_Collection();
        private ICommand _getCardCommand;
        private ICommand _saveTemplateCardCommand;
        private ICommand _deleteSelectedTemplateCard;

        public PercentageConverter percentConvertor = new PercentageConverter();

        private int _selectedRegionId = -1;
        private int _nextRegionId = 0;

        private Xceed.Wpf.Toolkit.RichTextBox _richTextBox = new Xceed.Wpf.Toolkit.RichTextBox();
        private TextBox _textBox = new TextBox();

        private bool _richTextBoxEditingTools = false;
        private bool _deleteNextSelectedRegion = false;
        private bool _hideTextEditBox = false;
        private bool _showAllRegions = false;

        private bool _currentlySaving = false;

        private Tree_View_Card _currentlySelectedTreeViewCard = null;
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
        public ICommand DeleteSelectedTemplateCardCommand
        {
            get
            {
                if (_deleteSelectedTemplateCard == null)
                {
                    _deleteSelectedTemplateCard = new RelayCommand(
                        param => SaveTemplateCard(),
                        param => (_deleteSelectedTemplateCard != null)
                    );
                }
                return _deleteSelectedTemplateCard;
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
                    if (SelectedRegionId == i.id)
                    {
                        if (HideTextEditBox)
                        {
                            frameworkElement = new Rectangle();

                            ((Rectangle)frameworkElement).Fill = Brushes.Transparent;
                            ((Rectangle)frameworkElement).Style = (Style)Application.Current.Resources["Selected_Card_Rectangle"];
                        }
                        else
                        {
                            frameworkElement = _textBox;
                            ((TextBox)frameworkElement).Text = i.DesiredInherittedProperties.StringContainer.GetAllStrings();
                        }
                    }
                    else
                    {
                        frameworkElement = new Rectangle();

                        ((Rectangle)frameworkElement).Fill = Brushes.Transparent;
                        ((Rectangle)frameworkElement).Style = (Style)Application.Current.Resources["Card_Rectangle"];
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

                    frameworkElement.ToolTip = RegionTitle(i.id);

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

                selectedRegion.DesiredInherittedProperties.ImageProperties.BackgroundImageLocationType = IMAGE_LOCATION_TYPE.Online;
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
            if (DeleteNextSelectedRegion)
            {
                int regionId = (int)((FrameworkElement)sender).DataContext;

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
                if (!HideTextEditBox)
                {
                    CurrentlySelectedRegion.DesiredInherittedProperties.StringContainer.strings = new List<String_Drawing>();
                    CurrentlySelectedRegion.DesiredInherittedProperties.StringContainer.strings.Add(new String_Drawing(_textBox.Text));
                }

                SelectedRegionId = -1;
            }
            else
            {
                DisplayRichTextBoxEditingTools = true;
                SelectedRegionId = (int)((FrameworkElement)sender).DataContext;
            }
            OnPropertyChanged("Drawing_Card_Elements");
        }

        public void AddNewCardRegion(Rect location)
        {
            Card selectedCard = Find_Selected_Card();

            selectedCard.Regions.Add(new Card_Region(ref _nextRegionId));

            selectedCard.Regions.Last().ideal_location = location;

            OnPropertyChanged("Drawing_Card_Elements");
        }

        
        [DependsUpon("CurrentCardCollection")]
        public IList<Tree_View_Card> TreeViewCards
        {
            get
            {
                return CurrentCardCollection.Get_Tree_View_Template_Cards(ref _cardCollection);
            }
        }
        [DependsUpon("CurrentCardCollection")]
        public Tree_View_Card CurrentlySelectedTreeViewCard
        {
            get
            {
                return _currentlySelectedTreeViewCard;
            }
            set
            {
                if (_currentlySelectedTreeViewCard != value)
                {
                    _currentlySelectedTreeViewCard = value;
                    OnPropertyChanged("CurrentlySelectedTreeViewCard");
                }
            }
        }

        [DependsUpon("CurrentlySelectedCard")]
        public int SelectedRegionId
        {
            get
            {
                return _selectedRegionId;
            }
            set
            {
                if (_selectedRegionId != value)
                {
                    _selectedRegionId = value;
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


        public double LevelOfDetail
        {
            get
            {
                return CurrentlySelectedCard.LevelOfDetail;
            }
            set
            {
                if (CurrentlySelectedCard.LevelOfDetail != value)
                {
                    CurrentlySelectedCard.LevelOfDetail = value;
                    OnPropertyChanged("LevelOfDetail");
                }
            }
        }
        [DependsUpon("CardPhysicalHeight")]
        [DependsUpon("LevelOfDetail")]
        public double CardRenderHeight
        {
            get
            {
                return CardPhysicalHeight * LevelOfDetail;
            }
        }
        [DependsUpon("CardPhysicalHeight")]
        [DependsUpon("LevelOfDetail")]
        public double CardRenderWidth
        {
            get
            {
                return CardPhysicalWidth * LevelOfDetail;
            }
        }

        #endregion

        protected override void Save(string file)
        {
            Xml_Save(file);
        }

        public void Xml_Save(string file, bool only_templates = false)
        {
            XmlWriterSettings xmlWriterSettings = new XmlWriterSettings();

            xmlWriterSettings.ConformanceLevel = ConformanceLevel.Auto;
            xmlWriterSettings.Indent = true;
            xmlWriterSettings.NewLineOnAttributes = true;
            Directory.CreateDirectory(file);
            _currentlySaving = true;
            XmlWriter xmlWriter = XmlWriter.Create(file+"autosave_write.xml", xmlWriterSettings);
            XmlSerializer xmlSerializer = new XmlSerializer(typeof(Card_Collection));
            
            xmlSerializer.Serialize(xmlWriter, _cardCollection);

            xmlWriter.Flush();
            xmlWriter.Close();

            if (File.Exists(file + "autosave.xml"))
            {
                File.Delete(file + "autosave.xml");
            }
            File.Move(file + "autosave_write.xml", file + "autosave.xml");

            _currentlySaving = false;
        }

        public void Xml_Load(string file, bool only_templates)
        {
            if (File.Exists(file + ".tmp"))
            {
                File.Delete(file + ".tmp");
            }
            if (File.Exists(file))
            {
                File.Copy(file, file + ".tmp");

                XmlReader xmlReader = XmlReader.Create(file + ".tmp");
                XmlSerializer xmlSerializer = new XmlSerializer(typeof(Card_Collection));

                SkipSave = true;

                CurrentCardCollection = null;
                CurrentlySelectedTreeViewCard = null;
                SelectedRegionId = -1;

                CurrentCardCollection = (Card_Collection)xmlSerializer.Deserialize(xmlReader);

                File.Delete(file + "autosave.xml.tmp");

                SkipSave = false;
            }
        }

        public void Tree_View_Selected_Item_Changed()
        {
            OnPropertyChanged("CurrentlySelectedCard");
        }

        #region Visibility Controls and Checkbox States
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
            get { return _showAllRegions; }
            set
            {
                if (_showAllRegions != value)
                {
                    _showAllRegions = value;
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
        public FontFamily SelectedFontFamily
        {
            get { return new FontFamily(CurrentlySelectedRegion.DesiredInherittedProperties.StringProperties.FontFamily); }
            set
            {
                if (CurrentlySelectedRegion.DesiredInherittedProperties.StringProperties.FontFamily != value.Source)
                {
                    CurrentlySelectedRegion.DesiredInherittedProperties.StringProperties.FontFamily = value.Source;

                    OnPropertyChanged("SelectedFontFamily");
                }
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
        public Color SelectedGradientFontBrush1
        {
            get
            {
                if (GradientBrushRequested)
                {
                    return CurrentlySelectedRegion.DesiredInherittedProperties.StringProperties.GradientTextBrushStops.First().Color;
                }
                else
                {
                    return Colors.Red;
                }
            }
            set
            {
                if (GradientBrushRequested)
                {
                    if (CurrentlySelectedRegion.DesiredInherittedProperties.StringProperties.GradientTextBrushStops.First().Color != value)
                    {
                        GradientStop tmp = new GradientStop(value, 0);

                        CurrentlySelectedRegion.DesiredInherittedProperties.StringProperties.GradientTextBrushStops.RemoveAt(0);
                        CurrentlySelectedRegion.DesiredInherittedProperties.StringProperties.GradientTextBrushStops.Insert(0, tmp);

                        OnPropertyChanged("SelectedFontBrush1");
                    }
                }
                else
                {
                    if (CurrentlySelectedRegion.DesiredInherittedProperties.StringProperties.SolidColorTextBrushColor != value)
                    {
                        return;
                    }
                }
            }
        }
        [DependsUpon("CurrentlySelectedRegion")]
        public Color SelectedGradientFontBrush2
        {
            get
            {
                if (GradientBrushRequested)
                {
                    return CurrentlySelectedRegion.DesiredInherittedProperties.StringProperties.GradientTextBrushStops.Last().Color;
                }
                else
                {
                    return Colors.Orange;
                }
            }
            set
            {
                if (GradientBrushRequested)
                {
                    if (CurrentlySelectedRegion.DesiredInherittedProperties.StringProperties.GradientTextBrushStops.Last().Color != value)
                    {
                        GradientStop tmp = new GradientStop(value, 1);

                        CurrentlySelectedRegion.DesiredInherittedProperties.StringProperties.GradientTextBrushStops.RemoveAt(1);
                        CurrentlySelectedRegion.DesiredInherittedProperties.StringProperties.GradientTextBrushStops.Add(tmp);

                        OnPropertyChanged("SelectedFontBrush2");
                    }
                }
                else
                {
                    return;
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

        [DependsUpon("SelectedFontFamily")]
        public IList<FontStyle> AllFontStyles
        {
            get
            {
                IList<FontStyle> result = new List<FontStyle>();

                foreach(FamilyTypeface i in SelectedFontFamily.FamilyTypefaces)
                {
                    result.Add(i.Style);
                }

                return result;
            }
        }
        [DependsUpon("SelectedFontFamily")]
        public IList<FontWeight> AllFontWeights
        {
            get
            {
                IList<FontWeight> result = new List<FontWeight>();

                foreach (FamilyTypeface i in SelectedFontFamily.FamilyTypefaces)
                {
                    result.Add(i.Weight);
                }

                return result;
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
        public IList<Color> GetUsedCardColors
        {
            get
            {
                return CurrentlySelectedCard.GetUsedColors;
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

        #region Private Helpers

        private void AddNewCard()
        {
            Card newCard = new Card();

            newCard.IsTemplateCard = true;
            if (CurrentlySelectedTreeViewCard != null)
            {
                Card parentCard = Find_Selected_Card();
                newCard = new Card(parentCard);
                
                newCard.ParentCard = parentCard.Id;
            }
            else
            {
                newCard.Regions.Add(new Card_Region(ref _nextRegionId));
                newCard.Regions[0].ideal_location = new Rect(0, 0, 1, 1);
            }

            CurrentCardCollection.Add_Card_To_Collection(newCard);
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

            return new Card_Region();
        }
        private Card Find_Selected_Card()
        {
            if (CurrentlySelectedTreeViewCard == null)
            {
                Card temp = new Card();

                temp.Regions.Add(new Card_Region());

                return temp;
            }
            return CurrentCardCollection.Find_Card_In_Collection(CurrentlySelectedTreeViewCard.Id);
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
