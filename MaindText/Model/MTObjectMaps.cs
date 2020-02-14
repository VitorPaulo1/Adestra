using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;
using System.Collections.ObjectModel;
using System.Collections;
using System.Windows.Data;
using MaindText.Control;
using MaindText.Model;
using System.Windows.Media;
using System.Windows.Documents;
using System.Windows.Controls;
using System.Windows;
using MaindText.View;
using System.Windows.Input;
using MaindText;
using System.IO;
using System.Windows.Media.Imaging;
using System.Windows.Controls.Primitives;
using MaindText.Utils;

namespace MaindText.Model
{
    public class MTLibraryModel : MTMapModel, INotifyPropertyChanged
    {
        #region Parameters

        private ObservableCollection<MTBookModel> _lstBooks;
        private int _iOccurrences;
        private static TestStage enStage;

        #endregion

        #region Initialization

        public MTLibraryModel()
        {
            _lstBooks = new ObservableCollection<MTBookModel>();
            _iOccurrences = 0;
        }

        #endregion

        #region Methodes

        public void AddOccurrence(int iNum)
        {
            _iOccurrences += iNum;
            if (Parent != null)
                Parent.AddOccurrence(iNum);
        }

        public void RemoveOccurrence(int iNum)
        {
            _iOccurrences -= iNum;
            if (Parent != null)
                Parent.RemoveOccurrence(iNum);
        }

        #endregion

        #region Properties

        public string Label
        {
            get { return null; }
        }

        public SolidColorBrush Color
        {
            get { return new SolidColorBrush(Colors.White); }
        }

        public ObservableCollection<MTBookModel> LstBooks
        {
            get { return _lstBooks; }
            set { _lstBooks = value; }
        }

        public ListCollectionView LstItens
        {
            get
            {
                ListCollectionView dgCollection = new ListCollectionView(_lstBooks);
                dgCollection.GroupDescriptions.Add(new PropertyGroupDescription("Group"));
                //dgCollection.CustomSort = new CustomSort();
                return dgCollection;
            }
        }

        public MTMapModel Parent
        {
            get { return null; }
            set { ; }
        }

        public string Group
        {
            get { return "Library"; }
        }

        public int Occurrences
        {
            get { return _iOccurrences; }
        }

        public void AddBook(MTBookModel mTBookModel)
        {
            mTBookModel.Parent = this;
            LstBooks.Add(mTBookModel);
        }

        public static TestStage Stage
        {
            get { return MTLibraryModel.enStage; }
            set { MTLibraryModel.enStage = value; }
        }

        #endregion

        #region INotifyPropertyChanged Members

        public event PropertyChangedEventHandler PropertyChanged;

        public void FirePropertyChanged(string sPropertyName)
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(sPropertyName));
        }

        #endregion
    }

    public class MTBookModel : MTMapModel, INotifyPropertyChanged
    {
        #region Parameters

        private string _sName;
        private string _sAbbrev;
        private string _sTestament;

        private ObservableCollection<MTMapModel> _lstChapter;
        private MTMapModel _objParent;
        private int _iOccurrences;

        #endregion

        #region Initialization

        public MTBookModel(string sName, string sAbbrev, string sTestament, Color color)
        {
            //Color is obsolet
            _sName = sName;
            _sAbbrev = sAbbrev;
            _sTestament = sTestament;
            _lstChapter = new ObservableCollection<MTMapModel>();
        }

        #endregion

        #region Methodes

        public void AddOccurrence(int iNum)
        {
            _iOccurrences += iNum;
            if (Parent != null)
                Parent.AddOccurrence(iNum);
        }

        public void RemoveOccurrence(int iNum)
        {
            _iOccurrences -= iNum;
            if (Parent != null)
                Parent.RemoveOccurrence(iNum);
        }

        #endregion

        #region Properties

        public string Label
        {
            get { return _sAbbrev; }
        }

        public string ToolTip
        {
            get { return _sName; }
        }

        public string Name
        {
            get { return _sName; }
        }

        public string Abbrev
        {
            get { return _sAbbrev; }
        }

        public SolidColorBrush Color
        {
            get { return new SolidColorBrush(PrimaryColor); }
        }

        public Color PrimaryColor
        {
            get
            {
                if (_iOccurrences != 0 && MTLibraryModel.Stage == TestStage.Stage_1)
                    return Colors.Khaki;
                else
                    return Colors.PowderBlue;
            }
        }

        public string Group
        {
            get { return _sTestament; }
        }

        public ObservableCollection<MTMapModel> LstChapter
        {
            get { return _lstChapter; }
            set { _lstChapter = value; }
        }

        public ListCollectionView LstItens
        {
            get
            {
                ListCollectionView dgCollection = new ListCollectionView(_lstChapter);
                dgCollection.GroupDescriptions.Add(new PropertyGroupDescription("Group"));
                dgCollection.CustomSort = new CustomSort();
                return dgCollection;
            }
        }

        public MTMapModel Parent
        {
            get { return _objParent; }
            set { _objParent = value; }
        }

        public int Occurrences
        {
            get { return _iOccurrences; }
        }

        #endregion

        #region INotifyPropertyChanged Members

        public event PropertyChangedEventHandler PropertyChanged;

        public void FirePropertyChanged(string sPropertyName)
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(sPropertyName));
        }

        #endregion
    }

    public class MTChapterModel : MTMapModel, INotifyPropertyChanged
    {
        #region Parameters

        private int _iChapter;
        private ObservableCollection<MTMapModel> _lstVerses;
        private MTMapModel _objParent;
        private int _iOccurrences;

        #endregion

        #region Initialization

        public MTChapterModel()
        {
            _iChapter = 1;
            _lstVerses = new ObservableCollection<MTMapModel>();
        }

        public MTChapterModel(int iChapter)
        {
            _iChapter = iChapter;
            _lstVerses = new ObservableCollection<MTMapModel>();
        }

        #endregion

        #region Methodes

        public void AddOccurrence(int iNum)
        {
            _iOccurrences += iNum;
            if (Parent != null)
                Parent.AddOccurrence(iNum);
        }

        public void RemoveOccurrence(int iNum)
        {
            _iOccurrences -= iNum;
            if (Parent != null)
                Parent.RemoveOccurrence(iNum);
        }

        #endregion

        #region Properties

        public string Label
        {
            get { return _iChapter.ToString(); }
        }

        public string ToolTip
        {
            get { return _iChapter.ToString(); }
        }

        public int Chapter
        {
            get { return _iChapter; }
            set { _iChapter = value; }
        }

        public SolidColorBrush Color
        {
            get { return new SolidColorBrush(PrimaryColor); }
        }

        public Color PrimaryColor
        {
            get
            {
                if (_iOccurrences != 0 && MTLibraryModel.Stage == TestStage.Stage_1)
                    return Colors.Khaki;
                else
                    return Colors.PowderBlue;
            }
        }

        public ObservableCollection<MTMapModel> LstVerses
        {
            get { return _lstVerses; }
            set { _lstVerses = value; }
        }

        public string Group
        {
            get { return "Capítulos"; }
        }

        public ListCollectionView LstItens
        {
            get
            {
                ListCollectionView dgCollection = new ListCollectionView(_lstVerses);
                dgCollection.CustomSort = new CustomSort();
                dgCollection.GroupDescriptions.Add(new PropertyGroupDescription("Group"));
                return dgCollection;
            }
        }

        public MTMapModel Parent
        {
            get { return _objParent; }
            set { _objParent = value; }
        }

        public int Occurrences
        {
            get { return _iOccurrences; }
        }

        #endregion

        #region INotifyPropertyChanged Members

        public event PropertyChangedEventHandler PropertyChanged;

        public void FirePropertyChanged(string sPropertyName)
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(sPropertyName));
        }

        #endregion
    }

    public class MTVerseModel : MTMapModel, INotifyPropertyChanged
    {
        #region Parameters

        private int _iVerse;
        private string _sText;
        private MTMapModel _objParent;
        private int _iOccurrences;

        #endregion

        #region Initialization

        public MTVerseModel(int iVerse)
        {
            _iVerse = iVerse;
        }

        #endregion

        #region Methodes

        public void AddOccurrence(int iNum)
        {
            _iOccurrences += iNum;
            if (Parent != null)
                Parent.AddOccurrence(iNum);
        }

        public void RemoveOccurrence(int iNum)
        {
            _iOccurrences -= iNum;
            if (Parent != null)
                Parent.RemoveOccurrence(iNum);
        }

        #endregion

        #region Properties

        public string Label
        {
            get { return _iVerse.ToString(); }
        }

        public string ToolTip
        {
            get { return _iVerse.ToString(); }
        }

        public int Verse
        {
            get { return _iVerse; }
            set { _iVerse = value; }
        }

        public string Text
        {
            get { return _sText; }
            set { _sText = value; }
        }

        public SolidColorBrush Color
        {
            get { return new SolidColorBrush(PrimaryColor); }
        }

        public Color PrimaryColor
        {
            get
            {
                if (_iOccurrences != 0 && MTLibraryModel.Stage == TestStage.Stage_1)
                    return Colors.Khaki;
                else
                    return Colors.PowderBlue;
            }
        }

        public ListCollectionView LstItens
        {
            get { return null; }
        }

        public string Group
        {
            get { return "Vercículos"; }
        }

        public MTMapModel Parent
        {
            get { return _objParent; }
            set { _objParent = value; }
        }

        public int Occurrences
        {
            get { return _iOccurrences; }
        }

        #endregion

        #region INotifyPropertyChanged Members

        public event PropertyChangedEventHandler PropertyChanged;

        public void FirePropertyChanged(string sPropertyName)
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(sPropertyName));
        }

        #endregion
    }

    public class MTBackModel : MTMapModel, INotifyPropertyChanged
    {
        #region Parameters

        private MTMapModel _objParent;

        #endregion

        #region Initialization

        public MTBackModel(MTMapModel objParent)
        {
            _objParent = objParent;
        }

        #endregion

        #region Methodes

        public void AddOccurrence(int iNum)
        {
        }

        public void RemoveOccurrence(int iNum)
        {
        }

        #endregion

        #region Properties

        public string Label
        {
            get { return "<<"; }
        }

        public string ToolTip
        {
            get
            {
                if (_objParent as MTBookModel != null)
                    return "Voltar a seleção de livros";
                else if (_objParent as MTChapterModel != null)
                    return "Voltar a seleção de capítulos";
                else
                    return "";
            }
        }

        public SolidColorBrush Color
        {
            get { return new SolidColorBrush(Colors.LightPink); }
        }

        public ListCollectionView LstItens
        {
            get { return null; }
        }

        public string Group
        {
            get
            {
                if (_objParent as MTBookModel != null)
                    return "Capítulos";
                else if (_objParent as MTChapterModel != null)
                    return "Vercículos";
                else
                    return "";
            }
        }

        public MTMapModel Parent
        {
            get { return _objParent; }
            set { _objParent = value; }
        }

        public int Occurrences
        {
            get { return 0; }
        }

        #endregion

        #region INotifyPropertyChanged Members

        public event PropertyChangedEventHandler PropertyChanged;

        public void FirePropertyChanged(string sPropertyName)
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(sPropertyName));
        }

        #endregion
    }

    public interface MTMapModel : INotifyPropertyChanged
    {
        void AddOccurrence(int iNum);
        void RemoveOccurrence(int iNum);
        string Label { get; }
        string Group { get; }
        MTMapModel Parent { get; }
        ListCollectionView LstItens { get; }
        SolidColorBrush Color { get; }
        int Occurrences { get; }
        void FirePropertyChanged(string sPropertyName);
    }
}