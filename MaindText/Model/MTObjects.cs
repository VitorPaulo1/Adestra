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
    public class MTPatchModel : INotifyPropertyChanged
    {
        #region Parameters

        private List<MTMapModel> _LstAdress;
        private string _sText;
        private string _sShortAdrees;
        private string _sAdrees;

        private DateTime _dtCreated;
        private List<MTStructTest> _lstTest;
        private Double _dScore;
        private Double _dPosition;
        private string _sGUID;
        private bool _bTestIsEnabled;
        private bool _bIsMemorized;

        private MTApplyMgrModel _mgrApply;

        #endregion

        #region Initialization

        public MTPatchModel()
        {
            _LstAdress = new List<MTMapModel>();
            _lstTest = new List<MTStructTest>();
            _dtCreated = DateTime.Now;
            _dScore = 0;
            _dPosition = 0;
            _sGUID = Guid.NewGuid().ToString();
            _bTestIsEnabled = true;
            _mgrApply = new MTApplyMgrModel(this);
        }

        public MTPatchModel(string sAdress, DateTime dtCreated, string sGUID, bool bTestIsEnabled)
        {
            SetLstAdress(MTControler.GetListMap(sAdress));

            _lstTest = new List<MTStructTest>();
            _dtCreated = dtCreated;
            _dScore = 0;
            _dPosition = 0;
            _sGUID = sGUID;
            _bTestIsEnabled = bTestIsEnabled;
            _mgrApply = new MTApplyMgrModel(this);
        }

        #endregion

        #region Methodes

        public void SetLstAdress(List<MTMapModel> lstAdres)
        {
            if (_LstAdress != null)
            {
                foreach (MTMapModel objMap in _LstAdress)
                    objMap.RemoveOccurrence(1);
            }

            _LstAdress = lstAdres;
            _sText = MTControler.GetStringText(_LstAdress);
            _sAdrees = MTControler.GetAdrees(_LstAdress, false);
            _sShortAdrees = MTControler.GetAdrees(_LstAdress, true);
            FirePropertyChanged("Text");
            FirePropertyChanged("Adrees");
            FirePropertyChanged("AdreesAbrevied");
            foreach (MTMapModel objMap in _LstAdress)
                objMap.AddOccurrence(1);
        }

        #endregion

        #region Properties

        public List<MTMapModel> LstAdress
        {
            get { return _LstAdress; }
        }

        public string FilePatch
        {
            get
            {
                return MTControler.PatchPath + _sGUID + ".xml";
            }
        }

        public string Text
        {
            get { return _sText; }
        }

        public string LabelToTest
        {
            get
            {
                string sToReturn = "";
                switch (MTControler.Settings.TestTypeConfig)
                {
                    case TestTypeConfig.Adrees:
                        sToReturn = _sText;
                        break;
                    case TestTypeConfig.Text:
                        sToReturn = _sAdrees;
                        break;
                    case TestTypeConfig.Alternate:
                        switch (LastTypeTest)
                        {
                            case TestType.None:
                                sToReturn = _sAdrees;
                                break;
                            case TestType.AskAdrees:
                                if (LastAnsuer)
                                    sToReturn = _sAdrees;
                                else
                                    sToReturn = _sText;

                                break;
                            case TestType.AskText:
                                if (LastAnsuer)
                                    sToReturn = _sText;
                                else
                                    sToReturn = _sAdrees;
                                break;
                        }
                        break;
                }
                return sToReturn;
            }
        }

        public string Adrees
        {
            get { return _sAdrees; }
        }

        public string ShortAdrees
        {
            get { return _sShortAdrees; }
        }

        public List<MTStructTest> lstTest
        {
            get { return _lstTest; }
            set { _lstTest = value; }
        }

        public string Periode
        {
            get
            {
                DateTime dtNow = DateTime.Now;
                TimeSpan dtInterval = dtNow.Subtract(LastDateTest);
                //Get periode
                if (dtInterval.Days == 1)
                    return "1 dia e " + dtInterval.Hours.ToString() + "horas";
                else if (dtInterval.Days > 1)
                    return dtInterval.Days.ToString() + " dias e " + dtInterval.Hours.ToString() + "horas";
                else if (dtInterval.Hours == 1)
                    return "1 hora e " + dtInterval.Minutes.ToString() + "min.";
                else if (dtInterval.Hours > 1)
                    return dtInterval.Hours.ToString() + " horas e " + dtInterval.Minutes.ToString() + "min.";
                else if (dtInterval.Minutes == 1)
                    return "1 minuto";
                else if (dtInterval.Minutes > 1)
                    return dtInterval.Minutes.ToString() + " minutos";
                else if (dtInterval.Minutes < 1)
                    return "Menos de 1 min.";
                else
                    return "Indefinido";
            }
        }

        public DateTime LastDateTest
        {
            get { return _lstTest.Count == 0 ? _dtCreated : _lstTest.Last().Date; }
        }

        public TestType LastTypeTest
        {
            get { return _lstTest.Count == 0 ? TestType.None : _lstTest.Last().Type; }
        }

        public bool LastAnsuer
        {
            get { return _lstTest.Count == 0 ? true : _lstTest.Last().WasCorrect; }
        }

        public DateTime Created
        {
            get { return _dtCreated; }
            set { _dtCreated = value; }
        }

        public String CreatedToShow
        {
            get { return _dtCreated.ToShortDateString(); }
        }

        public Double Score
        {
            get { return _dScore; }
            set
            {
                _dScore = value;
                FirePropertyChanged("Color");
                FirePropertyChanged("ScoreShadle");
            }
        }

        public Double ScoreShadle
        {
            get { return _dScore + MTClassificator.GetPoints(LastDateTest, DateTime.Now, true); }
        }

        public SolidColorBrush Color
        {
            get
            {
                if(IsMemorized)
                    return new SolidColorBrush(Colors.Blue);
                else if (_dScore <= MTControler.Settings.Stg1to2)
                    return new SolidColorBrush(Colors.Red);
                else if (_dScore <= MTControler.Settings.Stg2to3)
                    return new SolidColorBrush(Colors.Yellow);
                else
                    return new SolidColorBrush(Colors.Green);
            }
        }

        public Double PositionBrain
        {
            get { return _dPosition; }
            set { _dPosition = value; }
        }

        public string PositToShow
        {
            get { return String.Format("{0:0}", _dPosition); }
        }

        public string GUID
        {
            get { return _sGUID; }
        }

        public string TestCountToShow
        {
            get { return String.Format("{0:0}", _lstTest.Count); }
        }

        public int AnsuersCorrects
        {
            get
            {
                int iNum = 0;
                foreach (MTStructTest objTest in lstTest)
                    if (objTest.WasCorrect == true)
                        iNum++;

                return iNum;
            }
        }

        public int AnsuersWrongs
        {
            get
            {
                int iNum = 0;
                foreach (MTStructTest objTest in lstTest)
                    if (objTest.WasCorrect == false)
                        iNum++;

                return iNum;
            }
        }

        public double PercentsWrongs
        {
            get
            {
                int ITotal = lstTest.Count;
                if (ITotal == 0)
                    return 0;
                int IWrong = AnsuersWrongs;

                return (IWrong * 100) / ITotal;
            }
        }

        public string PercentsWrongsToShow
        {
            get { return String.Format("{0:0}", PercentsWrongs) + "%"; }
        }

        public MTTestModel NextTest
        {
            get
            {
                switch (MTControler.Settings.TestTypeConfig)
                {
                    case TestTypeConfig.Adrees:
                        return new MTTestAskAdreesModel(this);
                    case TestTypeConfig.Text:
                        return new MTTestAskTextModel(this);
                    case TestTypeConfig.Alternate:
                        switch (LastTypeTest)
                        {
                            case TestType.None:
                                return new MTTestAskTextModel(this);
                            case TestType.AskAdrees:
                                if (LastAnsuer)
                                    return new MTTestAskTextModel(this);
                                else
                                    return new MTTestAskAdreesModel(this);

                            case TestType.AskText:
                                if (LastAnsuer)
                                    return new MTTestAskAdreesModel(this);
                                else
                                    return new MTTestAskTextModel(this);
                        }
                        break;
                }
                return new MTTestAskAdreesModel(this);
            }
        }

        public bool TestIsEnabled
        {
            get { return _bTestIsEnabled; }
            set { _bTestIsEnabled = value; }
        }

        public int PositionAdrees
        {
            get { return MTControler.GetPositionAdrees(_LstAdress); }
        }

        public string Group
        {
            get
            {
                return (_LstAdress[0].Parent.Parent as MTBookModel).Name;
            }
        }

        public MTApplyMgrModel MgrApply
        {
            get { return _mgrApply; }
            set { _mgrApply = value; }
        }

        public bool ApplyIsEnabled
        {
            get
            {
                switch (MTControler.Settings.AllowApply)
                {
                    case AllowApplyConfig.All:
                        return true;
                    case AllowApplyConfig.OnlyText:
                        return LastTypeTest == TestType.AskAdrees ? false : true;
                }
                return true;
            }
        }

        public int ApplyNumb
        {
            get { return MgrApply.LstApply.Count; }
        }


        public bool IsMemorized
        {
            get { return _bIsMemorized; }
            set { _bIsMemorized = value;
            FirePropertyChanged("Color");
            }
        }

        #endregion

        #region INotifyPropertyChanged Members

        public event PropertyChangedEventHandler PropertyChanged;

        protected void FirePropertyChanged(string sPropertyName)
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(sPropertyName));
        }

        #endregion
    }

    public enum TestTypeConfig
    {
        Alternate = 0,
        Adrees = 1,
        Text = 2
    }

    public enum AllowApplyConfig
    {
        All = 0,
        OnlyText = 1,
    }

    public enum SortOrder
    {
        Text = 0,
        Applycations = 1,
        PositionBrain = 2,
        Date = 3,
        Progress = 4,
        TestTotal = 5,
        TestCorrects = 6,
        TestWrongs = 7,
        TestWrongPercents = 8,
        Created = 9
    }

    public class MTSettingsModel : INotifyPropertyChanged
    {
        #region Parameters

        //Settings
        private bool _bAutoTestEnabled;
        private double _iAutoTestInterval;
        private TestTypeConfig _enTestType;
        private AllowApplyConfig _enAllowApply;
        private SortOrder _enSortOrder;
        private double _dStg1to2;
        private double _dStg2to3;

        #endregion

        #region Initialization

        public MTSettingsModel()
        {
            _enSortOrder = SortOrder.PositionBrain;
        }

        #endregion

        #region Methodes

        public MTSettingsModel Clone()
        {
            MTSettingsModel objClonedSettings = new MTSettingsModel();

            objClonedSettings.AllowApply = AllowApply;
            objClonedSettings.AutoTestEnabled = AutoTestEnabled;
            objClonedSettings.AutoTestInterval = AutoTestInterval;
            objClonedSettings.EnSortOrder = EnSortOrder;
            objClonedSettings.TestTypeConfig = TestTypeConfig;
            objClonedSettings.Stg1to2 = _dStg1to2;
            objClonedSettings.Stg2to3 = _dStg2to3;

            return objClonedSettings;
        }

        #endregion

        #region Properties

        public bool AutoTestEnabled
        {
            get { return _bAutoTestEnabled; }
            set { _bAutoTestEnabled = value; }
        }

        public double AutoTestInterval
        {
            get { return _iAutoTestInterval; }
            set { _iAutoTestInterval = value;
            FirePropertyChanged("AutoTestIntervaltoShow");
            }
        }

        public string AutoTestIntervaltoShow
        {
            get { return _iAutoTestInterval.ToString(); }
            set { 
                double dNum;
                bool isNum = double.TryParse(value, out dNum);
                if (isNum)
                    _iAutoTestInterval = dNum;
                else
                    FirePropertyChanged("AutoTestIntervaltoShow");
            }
        }

        public TestTypeConfig TestTypeConfig
        {
            get { return _enTestType; }
            set { _enTestType = value;
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs("TestTypeConfig"));
            }
        }

        public AllowApplyConfig AllowApply
        {
            get { return _enAllowApply; }
            set { _enAllowApply = value;
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs("AllowApply"));
            }
        }

        public SortOrder EnSortOrder
        {
            get { return _enSortOrder; }
            set { _enSortOrder = value; }
        }

        public double Stg1to2
        {
            get { return _dStg1to2; }
            set { _dStg1to2 = value;
            FirePropertyChanged("Stg1to2");
            }
        }

        public double Stg2to3
        {
            get { return _dStg2to3; }
            set { _dStg2to3 = value;
            FirePropertyChanged("Stg2to3");
            }
        }

        #endregion

        #region INotifyPropertyChanged Members

        public event PropertyChangedEventHandler PropertyChanged;

        protected void FirePropertyChanged(string sPropertyName)
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(sPropertyName));
        }

        #endregion
    }

    internal class CustomSort : IComparer
    {
        public CustomSort()
        {
        }

        int IComparer.Compare(object first, object second)
        {
            if (first == null & second == null)
            {
                return 0;
            }

            else if (first == null)
            {
                return -1;
            }

            else if (second == null)
            {
                return 1;
            }
            else
            {
                MTMapModel objFirst = first as MTMapModel;
                MTMapModel objSecond = second as MTMapModel;

                double dFirst = 0;
                double dSecond = 0;
                if (double.TryParse(objFirst.Label, out dFirst))
                    if (double.TryParse(objSecond.Label, out dSecond))
                        return dFirst >= dSecond ? 1 : -1;

                return objFirst.Label.CompareTo(objSecond.Label);
            }
        }
    }

    public class MTApplyMgrModel : INotifyPropertyChanged
    {
        #region Parameters

        private MTPatchModel _objPatch;
        private int _iSelIndex = -1;
        private ObservableCollection<String> _lstApplycations;

        private ICommand _icmdAdd;
        private ICommand _icmdRemove;
        private ICommand _icmdToNext;
        private ICommand _icmdToBack;

        #endregion

        #region Initialization

        public MTApplyMgrModel(MTPatchModel objPatch)
        {
            _objPatch = objPatch;
            _lstApplycations = new ObservableCollection<string>();
        }

        #endregion

        #region Properties

        public int SelIndex
        {
            get { return _iSelIndex; }
            set
            {
                if (value > -2)
                {
                    if (value <= (LstApply.Count() - 1))
                    {
                        _iSelIndex = value;
                        FirePropertyChanged("TextApplycation");
                        FirePropertyChanged("SelIndexToShow");
                        FirePropertyChanged("CountApplyToShow");
                    }
                }
            }
        }

        public int SelIndexToShow
        {
            get { return _iSelIndex + 1; }
        }

        public string CountApplyToShow
        {
            get { return _lstApplycations.Count.ToString() + " aplicações"; }
        }

        public String TextApplycation
        {
            get
            {
                if (_iSelIndex == -1)
                    return "";
                else
                    return LstApply[_iSelIndex];
            }
            set
            {
                if (_iSelIndex != -1 && _iSelIndex < _lstApplycations.Count)
                {
                    LstApply[_iSelIndex] = value;
                    FirePropertyChanged("TextApplycation");
                    MTParser.WritePatch(_objPatch);
                }
            }
        }

        public ObservableCollection<String> LstApply
        {
            get { return _lstApplycations; }
            set
            {
                _lstApplycations = value;
                FirePropertyChanged("LstApply");
                FirePropertyChanged("TextApplycation");
            }
        }

        public bool IsEnabled
        {
            get { return _objPatch.LastTypeTest == TestType.AskAdrees ? false : true; }
        }

        public bool DocBlank
        {
            get
            {
                if (_lstApplycations.Count == 0)
                    return false;
                else
                    return true;
            }
        }

        public bool TextIsEnabled
        {
            get
            {
                return LstApply.Count() > 0 ? true : false;
            }
        }

        #endregion

        #region Methodes

        public void AddApply()
        {
            LstApply.Add("Nova Aplicação.");
            SelIndex = LstApply.Count() - 1;
            FirePropertyChanged("DocBlank");
            FirePropertyChanged("TextIsEnabled");
            MTParser.WritePatch(_objPatch);
        }

        public void RemoveSelectedApply()
        {
            if (LstApply.Count() >= 1)
            {
                LstApply.RemoveAt(SelIndex);
                if (SelIndex > (LstApply.Count() - 1))
                    SelIndex = (SelIndex - 1);
                else
                    SelIndex = (LstApply.Count() - 1);

                FirePropertyChanged("DocBlank");
                FirePropertyChanged("TextIsEnabled");
                MTParser.WritePatch(_objPatch);
            }
        }

        public void SetNext()
        {
            SelIndex = SelIndex + 1;
        }

        public void SetBack()
        {
            if (SelIndex > 0)
                SelIndex = SelIndex - 1;
        }

        #endregion

        #region Events

        public ICommand AddCommand
        {
            get
            {
                if (_icmdAdd == null)
                    _icmdAdd = new RelayCommand(param => this.AddApply());
                return _icmdAdd;
            }
        }

        public ICommand RemoveCommand
        {
            get
            {
                if (_icmdRemove == null)
                    _icmdRemove = new RelayCommand(param => this.RemoveSelectedApply());
                return _icmdRemove;
            }
        }

        public ICommand ToNextCommand
        {
            get
            {
                if (_icmdToNext == null)
                    _icmdToNext = new RelayCommand(param => this.SetNext());
                return _icmdToNext;
            }
        }

        public ICommand ToBackCommand
        {
            get
            {
                if (_icmdToBack == null)
                    _icmdToBack = new RelayCommand(param => this.SetBack());
                return _icmdToBack;
            }
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
}
