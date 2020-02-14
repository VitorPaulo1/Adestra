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
    public struct MTStructTest
    {
        public DateTime Date;
        public bool WasCorrect;
        public TestType Type;
    };

    public enum TestType
    {
        None = 0,
        AskAdrees = 1,
        AskText = 2
    }

    public enum TestStage
    {
        Stage_1 = 0,
        Stage_2 = 1,
        Stage_3 = 2
    }

    public class MTTestModel : NotifyPropertyChangedBase
    {
        #region Parameters

        protected DateTime _dtDate;
        protected bool? _bWasCorrect;
        protected MTPatchModel _objPatch;
        protected List<MTMapModel> _LstAnsuer;
        protected string _sAnsuer;
        protected TestType _enType;

        #endregion

        #region Initialization

        public MTTestModel(MTPatchModel objPatch)
        {
            _enType = TestType.None;
            _dtDate = DateTime.Now;
            _objPatch = objPatch;            
        }

        public MTTestModel(DateTime dtDate, bool bWasCorrect, TestType enType)
        {
            _enType = enType;
            _dtDate = dtDate;
            _bWasCorrect = bWasCorrect;
            _objPatch = null;
        }

        #endregion

        #region Methodes

        public virtual void InitializeRichTextBox()
        {
        }

        public virtual void Action(MTMapUser ansuerUser)
        {
        }

        public virtual void SetAnsuer()
        {
            string s_Ansuer = MTControler.GetAdrees(_LstAnsuer, false);
            string s_Response = _objPatch.Adrees;

            _dtDate = DateTime.Now;
            MTStructTest strTest;
            strTest.Date = this.Date;            
            strTest.Type = this.Type;

            if (s_Ansuer == s_Response)
            {
                WasCorrect = true;
                strTest.WasCorrect = true;
                _objPatch.lstTest.Add(strTest);
                MTClassificator.UpdatePatch(_objPatch);
            }
            else
            {
                WasCorrect = false;
                strTest.WasCorrect = false;
                _objPatch.lstTest.Add(strTest);
                MTClassificator.UpdateAllPositions();
            }
            MTParser.WritePatch(_objPatch);
            MTParser.WriteDataTest(_objPatch, this);
        }

        #endregion

        #region Properties

        public virtual bool? WasCorrect
        {
            get { return _bWasCorrect; }
            set
            {
                _bWasCorrect = value;
                FirePropertyChanged("WasCorrect");
                FirePropertyChanged("AnsuerToShow");
            }
        }

        public DateTime Date
        {
            get { return _dtDate; }
        }

        public MTPatchModel Patch
        {
            get { return _objPatch; }
        }

        public List<MTMapModel> LstAnsuer
        {
            get { return _LstAnsuer; }
            set
            {
                _LstAnsuer = value;
                FirePropertyChanged("AnsuerToShow");
            }
        }

        public TestType Type
        {
            get { return _enType; }
            set { _enType = value; }
        }

        public virtual string LastTestToShow
        {
            get { return _objPatch.Periode; }
        }

        public virtual string PointsToShow
        {
            get { return MTClassificator.GetPoints(_objPatch.LastDateTest, DateTime.Now, true).ToString(); }
        }

        public virtual string AnsuerToShow
        {
            get
            {
                string sAnsuer = MTControler.GetAdrees(_LstAnsuer, false);

                if (_bWasCorrect == null)
                    return sAnsuer;
                else
                {
                    if (_bWasCorrect == true)
                        return String.Format("{0} está correto!", sAnsuer);
                    else
                        return String.Format("{0} está incorreto!\nO correto é {1}", sAnsuer, _objPatch.Adrees);
                }
            }
            set
            {
                _sAnsuer = value;
                FirePropertyChanged("AnsuerToShow");
            }
        }

        public virtual bool ApplyIsEnabled
        {
            get { return true; }
        }

        #endregion
    }

    public class MTTestAskAdreesModel : MTTestModel
    {
        #region Parameters

        private string _sPreviosText;

        #endregion

        #region Initialization

        public MTTestAskAdreesModel(MTPatchModel objPatch)
            : base(objPatch)
        {
            _enType = TestType.AskAdrees;
        }

        #endregion

        #region Methodes

        public override void InitializeRichTextBox()
        {
            ListView lvTest = MTControler.DlgTest.lbTest;
            RichTextBox objRtbox = UtilsTools.GetChild(lvTest, lvTest.Items.IndexOf(this), "rtbText") as RichTextBox;
            if (objRtbox != null)
            {
                objRtbox.Document.Blocks.Clear();                
                if (Patch.Score < MTControler.Settings.Stg2to3)
                    objRtbox.Document.Blocks.Add(GetParagAsk("#"));
                else
                    objRtbox.Document.Blocks.Add(GetParagAsk(""));
            }
        }
        public Paragraph GetParagAsk(string sSeparator)
        {
            Run rAsk = new Run("Que texto é esse?\n");
            rAsk.FontWeight = FontWeights.Bold;
            Paragraph parag = new Paragraph();
            parag = MTControler.GetText(_objPatch.LstAdress, sSeparator);
            parag.Inlines.InsertBefore(parag.Inlines.FirstInline, rAsk);
            return parag;
        }

        public override void Action(MTMapUser ansuerUser)
        {
            _LstAnsuer = ansuerUser.SelectedPatchClone;
            _sPreviosText = ansuerUser.tbTextAdres.Text;
            string sText = ansuerUser.GetAdrees();
            AnsuerToShow = (sText == "" ? _sAnsuer : sText);
        }

        public Paragraph GetWrongAnsuer()
        {
            if (_LstAnsuer == null || _LstAnsuer.Count == 0 || (_LstAnsuer.First() as MTVerseModel) == null)
                return new Paragraph();

            Paragraph parag = MTControler.GetText(_LstAnsuer, "$Verse$");
            Run rIncorrect = new Run("Texto da sua resposta: ");
            rIncorrect.FontWeight = FontWeights.Bold;
            parag.Inlines.InsertBefore(parag.Inlines.FirstInline, rIncorrect);
            parag.LineHeight = 1;
            return parag;
        }

        #endregion

        #region Properties

        public override bool? WasCorrect
        {
            get { return _bWasCorrect; }
            set
            {
                _bWasCorrect = value;
                FirePropertyChanged("WasCorrect");
                FirePropertyChanged("AnsuerToShow");
            }
        }

        public override string AnsuerToShow
        {
            get
            {
                string sAnsuer = MTControler.GetAdrees(_LstAnsuer, false);
                string sTextsInChapter = MTControler.GetTextsInChapter((_objPatch.LstAdress.First().Parent as MTChapterModel), _objPatch.LstAdress).Text;

                if (_bWasCorrect == null)
                    return "";
                else
                {
                    if (_bWasCorrect == true)
                    {
                        ListView lvTest = MTControler.DlgTest.lbTest;
                        RichTextBox objRtbox = UtilsTools.GetChild(lvTest, lvTest.Items.IndexOf(this), "rtbText") as RichTextBox;
                        objRtbox.Document.Blocks.Clear();
                        objRtbox.Document.Blocks.Add(GetParagAsk("$Verse$"));
                        return String.Format("{0} está correto!\n{1}", sAnsuer, sTextsInChapter);
                    }
                    else
                    {
                        ListView lvTest = MTControler.DlgTest.lbTest;
                        RichTextBox objRtbox = UtilsTools.GetChild(lvTest, lvTest.Items.IndexOf(this), "rtbText") as RichTextBox;
                        objRtbox.Document.Blocks.Clear();
                        objRtbox.Document.Blocks.Add(GetParagAsk("$Verse$"));
                        objRtbox.Document.Blocks.Add(GetWrongAnsuer());
                        return String.Format("{0} está incorreto!\nO correto é {1} {2}", sAnsuer, _objPatch.Adrees, sTextsInChapter);
                    }
                }
            }
            set
            {
                _sAnsuer = value;
                FirePropertyChanged("AnsuerToShow");
            }
        }


        public string PreviosText
        {
            get
            {
                if (_sPreviosText == null)
                    _sPreviosText = "";
                return _sPreviosText;
            }
            set { _sPreviosText = value; }
        }

        #endregion
    }

    public class MTTestAskTextModel : MTTestModel
    {
        #region Parameters

        private string _sFragment;

        #endregion

        #region Initialization

        public MTTestAskTextModel(MTPatchModel objPatch)
            : base(objPatch)
        {
            _enType = TestType.AskText;            
        }

        #endregion

        #region Methodes

        public override void InitializeRichTextBox()
        {
            ListView lvTest = MTControler.DlgTest.lbTest;
            RichTextBox objRtbox = UtilsTools.GetChild(lvTest, lvTest.Items.IndexOf(this), "rtbText") as RichTextBox;
            if (objRtbox == null)
                return;

            objRtbox.Document.Blocks.Clear();
            objRtbox.Document.Blocks.Add(GetParagAsk());
        }

        public override void Action(MTMapUser ansuerUser)
        {
            _LstAnsuer = ansuerUser.SelectedPatchClone;
            _sFragment = ansuerUser.tbSearch.Text;
            string sText = MTControler.GetAdrees(ansuerUser.SelectedPatch, false);
            AnsuerToShow = (sText == "" ? _sAnsuer : sText);
            ListView lvTest = MTControler.DlgTest.lbTest;
            RichTextBox objRtbox = UtilsTools.GetChild(lvTest, lvTest.Items.IndexOf(this), "rtbText") as RichTextBox;
            objRtbox.Document.Blocks.Clear();
            objRtbox.Document.Blocks.Add(GetParagAsk());
            objRtbox.Document.Blocks.Add(GetSearchedText());
        }

        public Paragraph GetParagAsk()
        {
            Paragraph parag = new Paragraph();
            Run rAsk = new Run("O que está escrito em: ");
            Run rText = new Run(Patch.Adrees);
            rText.FontWeight = FontWeights.Bold;
            parag.Inlines.Add(rAsk);
            parag.Inlines.Add(rText);
            if (Patch.Score < MTControler.Settings.Stg1to2 || WasCorrect != null)
            {
                parag.Inlines.Add(MTControler.GetTextsInChapter((_objPatch.LstAdress.First().Parent as MTChapterModel), _objPatch.LstAdress));
            }
            parag.LineHeight = 1;
            return parag;
        }

        public Paragraph GetCorrectAnsuer()
        {
            Paragraph parag = MTControler.GetText(_objPatch.LstAdress, "$Verse$");
            Run rCorrect = new Run("Texto correto: ");
            rCorrect.FontWeight = FontWeights.Bold;
            parag.Inlines.InsertBefore(parag.Inlines.FirstInline, rCorrect);
            parag.LineHeight = 3;
            return parag;
        }

        private Paragraph GetSearchedText()
        {
            string sText = MTControler.GetStringText(_LstAnsuer);
            if (sText == null || _sFragment == null)
                return new Paragraph();

            sText = sText.Replace("[", "");
            sText = sText.Replace("]", "");
            sText = sText.Replace("'", "");
            sText = sText.Replace("\"", "");
            sText = sText.Replace(" ", "");

            int iStart = sText.ToLower().IndexOf(_sFragment.ToLower());
            int iEnd = iStart + _sFragment.Length;

            Run rStart = new Run(sText.Substring(0, iStart));
            Run rMid = new Run(sText.Substring(iStart, iEnd - iStart));
            Run rEnd = new Run(sText.Substring(iEnd));

            rMid.Background = new SolidColorBrush(Colors.Yellow);

            Paragraph paragText = new Paragraph();

            paragText.Inlines.Add(rStart);
            paragText.Inlines.Add(rMid);
            paragText.Inlines.Add(rEnd);
            paragText.LineHeight = 3;

            if (paragText != null)
            {
                return paragText;
            }
            else
                return new Paragraph();
        }

        #endregion

        #region Properties

        public override bool? WasCorrect
        {
            get { return _bWasCorrect; }
            set
            {
                _bWasCorrect = value;
                FirePropertyChanged("WasCorrect");
                FirePropertyChanged("AnsuerToShow");
                FirePropertyChanged("ApplyIsEnabled");
            }
        }

        public override string AnsuerToShow
        {
            get
            {
                string sAnsuer = MTControler.GetAdrees(_LstAnsuer, false);

                if (_bWasCorrect == null)
                    return "";
                else
                {
                    if (_bWasCorrect == true)
                    {
                        ListView lvTest = MTControler.DlgTest.lbTest;
                        RichTextBox objRtbox = UtilsTools.GetChild(lvTest, lvTest.Items.IndexOf(this), "rtbText") as RichTextBox;
                        objRtbox.Document.Blocks.Clear();
                        objRtbox.Document.Blocks.Add(GetParagAsk());
                        objRtbox.Document.Blocks.Add(GetSearchedText());
                        return String.Format("Correto!");
                    }
                    else
                    {
                        ListView lvTest = MTControler.DlgTest.lbTest;
                        RichTextBox objRtbox = UtilsTools.GetChild(lvTest, lvTest.Items.IndexOf(this), "rtbText") as RichTextBox;
                        objRtbox.Document.Blocks.Clear();
                        objRtbox.Document.Blocks.Add(GetParagAsk());
                        objRtbox.Document.Blocks.Add(GetCorrectAnsuer());
                        Paragraph parag = GetSearchedText();
                        if (parag.Inlines.FirstInline != null)
                        {
                            Run rIncorrect = new Run("Sua resposta: ");
                            rIncorrect.FontWeight = FontWeights.Bold;
                            parag.Inlines.InsertBefore(parag.Inlines.FirstInline, rIncorrect);
                            objRtbox.Document.Blocks.Add(parag);
                        }

                        return String.Format(sAnsuer + "  Incorreto!");
                    }
                }
            }
            set
            {
                _sAnsuer = value;
                FirePropertyChanged("AnsuerToShow");
            }
        }

        public override bool ApplyIsEnabled
        {
            get {
                if (WasCorrect == true || WasCorrect == false)
                    return true;
                else
                    return false;
            }
        }

        public string Fragment
        {
            get {
                if (_sFragment == null)
                    _sFragment = "";
                return _sFragment; 
            }
            set { _sFragment = value; }
        }

        #endregion
    }

    public class MTTextMgrModel : NotifyPropertyChangedBase
    {
        #region Parameters

        private List<MTPatchModel> _lstPatch;

        #endregion

        #region Initialization

        public MTTextMgrModel()
        {
            _lstPatch = new List<MTPatchModel>();
        }

        #endregion

        #region Properties

        public List<MTPatchModel> LstPatch
        {
            get { return _lstPatch; }
            set { _lstPatch = value;
            FirePropertyChanged("DgCollection");
            }
        }
        
        public ListCollectionView DgCollection
        {
            get
            {
                ListCollectionView dgCollection = new ListCollectionView(LstPatch);
                if(MTControler.MainView.tbtCategorized.IsChecked == true)
                    dgCollection.GroupDescriptions.Add(new PropertyGroupDescription("Group"));
                dgCollection.CustomSort = new CustomSort();
                return dgCollection;
            }
        }

        #endregion

        #region Methodes

        public void AddPatch(MTPatchModel objPatch)
        {
            _lstPatch.Add(objPatch);
            MTClassificator.UpdatePatch(objPatch);            
        }

        #endregion

        #region Events

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
                    MTPatchModel objFirst = first as MTPatchModel;
                    MTPatchModel objSecond = second as MTPatchModel;
                    //Check if Test is Enabled
                    if(!objFirst.TestIsEnabled && objSecond.TestIsEnabled)
                        return 1;
                    else if (objFirst.TestIsEnabled && !objSecond.TestIsEnabled)
                        return -1;

                    switch (MTControler.Settings.EnSortOrder)
                    {
                        case SortOrder.PositionBrain:
                            if ((objFirst.PositionBrain) == (objSecond.PositionBrain))
                                return objFirst.LastDateTest.CompareTo(objSecond.LastDateTest);
                            else if ((objFirst.PositionBrain) < (objSecond.PositionBrain))
                                return 1;
                            break;

                        case SortOrder.Applycations:
                            if ((objFirst.MgrApply.LstApply.Count) == (objSecond.MgrApply.LstApply.Count))
                                return objFirst.PositionAdrees.CompareTo(objSecond.PositionAdrees);
                            else if ((objFirst.MgrApply.LstApply.Count) < (objSecond.MgrApply.LstApply.Count))
                                return 1;
                            break;

                        case  SortOrder.Date:
                            if ((objFirst.LastDateTest) == (objSecond.LastDateTest))
                                return objFirst.ShortAdrees.CompareTo(objSecond.ShortAdrees);
                            else if ((objFirst.LastDateTest) > (objSecond.LastDateTest))
                                return 1;                                
                            break;

                        case  SortOrder.Text:
                            if ((objFirst.PositionAdrees) == (objSecond.PositionAdrees))
                                return objFirst.ShortAdrees.CompareTo(objSecond.ShortAdrees);
                            else if ((objFirst.PositionAdrees) > (objSecond.PositionAdrees))
                                return 1;
                            break;

                        case SortOrder.Progress:
                            if ((objFirst.Score) == (objSecond.Score))
                                return objFirst.LastDateTest.CompareTo(objSecond.LastDateTest);
                            else if ((objFirst.Score) > (objSecond.Score))
                                return 1;
                            break;

                        case SortOrder.TestTotal:
                            if ((objFirst.lstTest.Count) == (objSecond.lstTest.Count))
                                return objFirst.LastDateTest.CompareTo(objSecond.LastDateTest);
                            else if ((objFirst.lstTest.Count) > (objSecond.lstTest.Count))
                                return 1;
                            break;

                        case SortOrder.TestCorrects:
                            if ((objFirst.AnsuersCorrects) == (objSecond.AnsuersCorrects))
                                return objFirst.LastDateTest.CompareTo(objSecond.LastDateTest);
                            else if ((objFirst.AnsuersCorrects) > (objSecond.AnsuersCorrects))
                                return 1;
                            break;

                        case SortOrder.TestWrongs:
                            if ((objFirst.AnsuersWrongs) == (objSecond.AnsuersWrongs))
                                return objFirst.LastDateTest.CompareTo(objSecond.LastDateTest);
                            else if ((objFirst.AnsuersWrongs) < (objSecond.AnsuersWrongs))
                                return 1;
                            break;

                        case SortOrder.TestWrongPercents:
                            if ((objFirst.PercentsWrongs) == (objSecond.PercentsWrongs))
                                return objFirst.LastDateTest.CompareTo(objSecond.LastDateTest);
                            else if ((objFirst.PercentsWrongs) < (objSecond.PercentsWrongs))
                                return 1;
                            break;

                        case SortOrder.Created:
                            if ((objFirst.Created) == (objSecond.Created))
                                return objFirst.PositionAdrees.CompareTo(objSecond.PositionAdrees);
                            else if ((objFirst.Created) < (objSecond.Created))
                                return 1;
                            break;
                    }
                    return -1;
                }
            }
        }

        #endregion
    }
}