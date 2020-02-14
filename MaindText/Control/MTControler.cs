using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections.ObjectModel;
using System.Windows.Data;
using System.Collections;
using System.ComponentModel;
using System.Timers;
using System.Windows;
using MaindText.View;
using MaindText.Model;
using System.IO;
using System.Windows.Media;
using System.Windows.Documents;
using MaindText.Utils;

namespace MaindText.Control
{
    public class MTControler
    {
        #region Parameters

        private static MTTextMgrModel _textManager = null;

        private static BackgroundWorker _wAutoTestWorker;
        private static DateTime _dtTimeRemaining;
        private static MTMainView _dlgMainView;
        private static System.Windows.Forms.NotifyIcon _niIcon;
        private static ObservableCollection<MTTestModel> _lstTest;
        private static MTLibraryModel _objLibrary;

        private static string _LibraryPath;
        private static string _UserPath;        

        private static MTTestView _dlgTest;
        private static MTSettingsModel _Settings;

        private static DialogPositioner _dlgPositTest;

        #endregion

        #region Initialization

        static public void Initialize()
        {
            _textManager = new MTTextMgrModel();            

            _wAutoTestWorker = new BackgroundWorker();
            _wAutoTestWorker.WorkerSupportsCancellation = true;
            _lstTest = new ObservableCollection<MTTestModel>(); 
            string appPath = System.Reflection.Assembly.GetExecutingAssembly().Location;
            _LibraryPath = System.IO.Path.GetDirectoryName(appPath) + "\\Library\\";
            _UserPath = System.IO.Path.GetDirectoryName(appPath) + "\\Users\\";

            if (!Directory.Exists(_UserPath))
                Directory.CreateDirectory(_UserPath);

            InitializeLibrary();
            string sSettingsPath = _UserPath + "Settings.xml";
            MTParser.Read(sSettingsPath);
            MTParser.ReadAllInFolder(PatchPath);

            MTClassificator.UpdateAllPatches();

            _dlgPositTest = new DialogPositioner();

            //Show Main Dialog
            _dlgMainView = new MTMainView();
            _dlgMainView.Show();
            InitializeNotificationIcon();
            _dlgTest = new MTTestView();
        }

        private static void InitializeNotificationIcon()
        {
            _niIcon = new System.Windows.Forms.NotifyIcon();
            System.Drawing.Size siz = new System.Drawing.Size(16,16);
            _niIcon.Icon = new System.Drawing.Icon(MaindText.Properties.Resources.brain, siz);
            _niIcon.Visible = true;
            _niIcon.Click +=
                delegate(object sender, EventArgs args)
                {
                    _dlgMainView.Show();
                    _dlgMainView.WindowState = WindowState.Normal;
                    if (MTControler.LstTest.Count > 0)
                        MTControler.ShowLstDlgTest();
                };
            _niIcon.BalloonTipTitle = "Novo Teste";
            _niIcon.BalloonTipText = " ";
            MTControler.NiIcon.BalloonTipClicked +=
                delegate(object sender, EventArgs args)
                {
                    MTControler.ShowLstDlgTest();
                };
        }

        private static void InitializeLibrary()
        {
            SetBooks();

            if (Directory.Exists(_LibraryPath))
            {
                string[] arrBook = Directory.GetDirectories(_LibraryPath);
                foreach (string sBook in arrBook)
                {
                    var objvar = _objLibrary.LstBooks.Where(x => x.Abbrev == (new DirectoryInfo(sBook).Name));
                    MTBookModel objBook = objvar.First() as MTBookModel;                 
                    string[] arrChap = Directory.GetFiles(sBook);
                    foreach (string sChap in arrChap)
                    {
                        MTChapterModel objChapter = new MTChapterModel(Convert.ToInt32(System.IO.Path.GetFileNameWithoutExtension(sChap)));
                        objBook.LstChapter.Add(objChapter);
                        objChapter.Parent = objBook;

                        TextReader tReader = new StreamReader(sChap);
                        using (tReader)
                        {
                            string sText = tReader.ReadToEnd();
                            int i = 1;
                            while (true)
                            {
                                string[] arrSplit = new string[1] { "$" + i.ToString() + "$" };                                
                                string[] arrVerses = sText.Split(arrSplit, StringSplitOptions.None);
                                string sVers = "";
                                if (arrVerses.Count() == 1)
                                    break;
                                if (arrVerses.Count() == 2)
                                    sVers = arrVerses[0];
                                else if (arrVerses.Count() == 3)
                                    sVers = arrVerses[1];

                                MTVerseModel objVerse = new MTVerseModel(i);
                                objVerse.Text = sVers;
                                objChapter.LstVerses.Add(objVerse);
                                objVerse.Parent = objChapter;
                                i++;
                            }
                        }
                        //add back in chapters
                        objChapter.LstVerses.Add(new MTBackModel(objChapter));
                    }
                    //add back in chapters
                    objBook.LstChapter.Add(new MTBackModel(objBook));
                }
            }
        }

        private static void SetBooks()
        {
            string sEscritHeb = "Escrituras Hebraicas";
            string sEscritGre = "Escrituras Gregas";
            //Color col1 = Colors.LightGray;
            //Color col2 = Colors.Khaki;
            //Color col3 = Colors.LightPink;
            //Color col4 = Colors.Thistle;
            //Color col5 = Colors.Wheat;
            //Color col6 = Colors.PaleGreen;            
            //Color col7 = Colors.PowderBlue;
            Color col1 = Colors.PowderBlue;
            Color col2 = Colors.PowderBlue;
            Color col3 = Colors.PowderBlue;
            Color col4 = Colors.PowderBlue;
            Color col5 = Colors.PowderBlue;
            Color col6 = Colors.PowderBlue;
            Color col7 = Colors.PowderBlue;  

            _objLibrary = new MTLibraryModel();

            _objLibrary.AddBook(new MTBookModel("Gênesis", "Gên", sEscritHeb, col1));
            _objLibrary.AddBook(new MTBookModel("Êxodo", "Êx", sEscritHeb, col1));
            _objLibrary.AddBook(new MTBookModel("Levitico", "Le", sEscritHeb, col1));
            _objLibrary.AddBook(new MTBookModel("Números", "Núm", sEscritHeb, col1));
            _objLibrary.AddBook(new MTBookModel("Deuteronômio", "De", sEscritHeb, col1));
            _objLibrary.AddBook(new MTBookModel("Josué", "Jos", sEscritHeb, col2));
            _objLibrary.AddBook(new MTBookModel("Juízes", "Jz", sEscritHeb, col2));
            _objLibrary.AddBook(new MTBookModel("Rute", "Ru", sEscritHeb, col2));
            _objLibrary.AddBook(new MTBookModel("1 Samuel", "1Sa", sEscritHeb, col3));
            _objLibrary.AddBook(new MTBookModel("2 Samuel", "2Sa", sEscritHeb, col3));
            _objLibrary.AddBook(new MTBookModel("1 Reis", "1Rs", sEscritHeb, col3));
            _objLibrary.AddBook(new MTBookModel("2 Reis", "2Rs", sEscritHeb, col3));
            _objLibrary.AddBook(new MTBookModel("1 Crônicas", "1Cr", sEscritHeb, col3));
            _objLibrary.AddBook(new MTBookModel("2 Crônicas", "2Cr", sEscritHeb, col3));
            _objLibrary.AddBook(new MTBookModel("Esdras", "Esd", sEscritHeb, col4));
            _objLibrary.AddBook(new MTBookModel("Neemias", "Ne", sEscritHeb, col4));
            _objLibrary.AddBook(new MTBookModel("Ester", "Est", sEscritHeb, col4));
            _objLibrary.AddBook(new MTBookModel("Jó", "Jó", sEscritHeb, col5));
            _objLibrary.AddBook(new MTBookModel("Salmos", "Sal", sEscritHeb, col5));
            _objLibrary.AddBook(new MTBookModel("Provérbios", "Pr", sEscritHeb, col5));
            _objLibrary.AddBook(new MTBookModel("Eclesiastes", "Ec", sEscritHeb, col5));
            _objLibrary.AddBook(new MTBookModel("Cântico de Salomão", "Cân", sEscritHeb, col5));
            _objLibrary.AddBook(new MTBookModel("Isaías", "Is", sEscritHeb, col6));
            _objLibrary.AddBook(new MTBookModel("Jeremias", "Je", sEscritHeb, col6));
            _objLibrary.AddBook(new MTBookModel("Lamentações", "La", sEscritHeb, col6));
            _objLibrary.AddBook(new MTBookModel("Ezequiel", "Ez", sEscritHeb, col6));
            _objLibrary.AddBook(new MTBookModel("Daniel", "Da", sEscritHeb, col6));
            _objLibrary.AddBook(new MTBookModel("Oséias", "Os", sEscritHeb, col7));
            _objLibrary.AddBook(new MTBookModel("Joel", "Jl", sEscritHeb, col7));
            _objLibrary.AddBook(new MTBookModel("Amós", "Am", sEscritHeb, col7));
            _objLibrary.AddBook(new MTBookModel("Obadias", "Ob", sEscritHeb, col7));
            _objLibrary.AddBook(new MTBookModel("Jonas", "Jon", sEscritHeb, col7));
            _objLibrary.AddBook(new MTBookModel("Miquéias", "Miq", sEscritHeb, col7));
            _objLibrary.AddBook(new MTBookModel("Naum", "Na", sEscritHeb, col7));
            _objLibrary.AddBook(new MTBookModel("Habacuque", "Hab", sEscritHeb, col7));
            _objLibrary.AddBook(new MTBookModel("Sofonias", "Sof", sEscritHeb, col7));
            _objLibrary.AddBook(new MTBookModel("Ageu", "Ag", sEscritHeb, col7));
            _objLibrary.AddBook(new MTBookModel("Zacarias", "Za", sEscritHeb, col7));
            _objLibrary.AddBook(new MTBookModel("Malaquias", "Mal", sEscritHeb, col7));

            _objLibrary.AddBook(new MTBookModel("Mateus", "Mt", sEscritGre, col1));
            _objLibrary.AddBook(new MTBookModel("Marcos", "Mr", sEscritGre, col1));
            _objLibrary.AddBook(new MTBookModel("Lucas", "Lu", sEscritGre, col1));
            _objLibrary.AddBook(new MTBookModel("João", "Jo", sEscritGre, col1));
            _objLibrary.AddBook(new MTBookModel("Atos", "At", sEscritGre, col2));
            _objLibrary.AddBook(new MTBookModel("Romanos", "Ro", sEscritGre, col3));
            _objLibrary.AddBook(new MTBookModel("1 Coríntios", "1Co", sEscritGre, col3));
            _objLibrary.AddBook(new MTBookModel("2 Coríntios", "2Co", sEscritGre, col3));
            _objLibrary.AddBook(new MTBookModel("Gálatas", "Gál", sEscritGre, col3));
            _objLibrary.AddBook(new MTBookModel("Efésios", "Ef", sEscritGre, col3));
            _objLibrary.AddBook(new MTBookModel("Filipenses", "Fil", sEscritGre, col3));
            _objLibrary.AddBook(new MTBookModel("Colossenses", "Col", sEscritGre, col3));
            _objLibrary.AddBook(new MTBookModel("1 Tessalonicenses", "1Te", sEscritGre, col3));
            _objLibrary.AddBook(new MTBookModel("2 Tessalonicenses", "2Te", sEscritGre, col3));
            _objLibrary.AddBook(new MTBookModel("1 Timóteo", "1Ti", sEscritGre, col3));
            _objLibrary.AddBook(new MTBookModel("2 Timóteo", "2Ti", sEscritGre, col3));
            _objLibrary.AddBook(new MTBookModel("Tito", "Tit", sEscritGre, col3));
            _objLibrary.AddBook(new MTBookModel("Filêmon", "Flm", sEscritGre, col3));
            _objLibrary.AddBook(new MTBookModel("Hebreus", "He", sEscritGre, col3));
            _objLibrary.AddBook(new MTBookModel("Tiago", "Tg", sEscritGre, col4));
            _objLibrary.AddBook(new MTBookModel("1 Pedro", "1Pe", sEscritGre, col4));
            _objLibrary.AddBook(new MTBookModel("2 Pedro", "2Pe", sEscritGre, col4));
            _objLibrary.AddBook(new MTBookModel("1 João", "1Jo", sEscritGre, col4));
            _objLibrary.AddBook(new MTBookModel("2 João", "2Jo", sEscritGre, col4));
            _objLibrary.AddBook(new MTBookModel("3 João", "3Jo", sEscritGre, col4));
            _objLibrary.AddBook(new MTBookModel("Judas", "Ju", sEscritGre, col4));
            _objLibrary.AddBook(new MTBookModel("Revelação", "Re", sEscritGre, col5));
        }

        #endregion

        #region Methodes

        public static void ShowLstDlgTest()
        {
            if (_lstTest.Count == 0)
                LstTest.Add((_dlgMainView.lvTexts.Items[0] as MTPatchModel).NextTest);

            if (!_dlgTest.IsLoaded)
            {
                Size siz = _dlgTest.RenderSize;
                _dlgTest.Close();
                _dlgTest = new MTTestView();
                _dlgTest.RenderSize = siz;
                _dlgTest.LstTests = LstTest;
                DlgTest.Show();
            }            
        }

        public static int GetPositionAdrees(List<MTMapModel> lstMap)
        {
            if (lstMap.Count == 0)
                return 0;
            if (lstMap[0] as MTVerseModel == null)
                return 0;
            int iResult = lstMap.Count;
            MTVerseModel objFirstVerse = lstMap[0] as MTVerseModel;
            iResult += objFirstVerse.Verse * 10;
            MTChapterModel objChapter = objFirstVerse.Parent as MTChapterModel;
            iResult += objChapter.Chapter * 10000;
            MTBookModel objBook = objChapter.Parent as MTBookModel;
            iResult += (_objLibrary.LstBooks.IndexOf(objBook) + 1) * 10000000;
            return iResult;
        }

        //if sSeparator == $Verse$ write Verse number
        public static Paragraph GetText(List<MTMapModel> lstMap, string sSeparator)
        {
            if (lstMap.Count == 0)
                return null;
            if (lstMap[0] as MTVerseModel == null)
                return null;

            List<MTVerseModel> lstMTVers = new List<MTVerseModel>();
            foreach (MTMapModel objMap in lstMap)
                lstMTVers.Add(objMap as MTVerseModel);

            Paragraph paragText = new Paragraph();

            foreach (MTVerseModel objVer in lstMTVers)
            {
                Run rNumb = new Run();
                rNumb.FontWeight = FontWeights.Bold;
                rNumb.FontSize = 10;
                rNumb.BaselineAlignment = BaselineAlignment.TextTop;

                if (sSeparator == "$Verse$")
                    rNumb.Text = objVer.Label;
                else
                    rNumb.Text = sSeparator;

                paragText.Inlines.Add(rNumb);

                string sText = objVer.Text;

                if (sSeparator == "")
                    sText = sText.TrimEnd();
                paragText.Inlines.Add(new Run(sText));
            }
            return paragText;
        }

        public static string GetStringText(List<MTMapModel> lstMap)
        {
            if (lstMap.Count == 0)
                return null;
            if (lstMap[0] as MTVerseModel == null)
                return null;

            List<MTVerseModel> lstMTVers = new List<MTVerseModel>();
            foreach (MTMapModel objMap in lstMap)
                lstMTVers.Add(objMap as MTVerseModel);

            string sText = "";

            foreach (MTVerseModel objVer in lstMTVers)            
                sText += objVer.Text;            
            
            return sText;
        }

        public static Run GetTextsInChapter(MTChapterModel objChapter, List<MTMapModel> lstExcluded)
        {
            if (objChapter == null)
                return null;

            List<MTVerseModel> lstVerses = new List<MTVerseModel>();
            foreach (MTMapModel objMap in objChapter.LstVerses)
            {
                if (objMap as MTVerseModel == null)
                    continue;
                if (lstExcluded.Contains(objMap))
                    continue;
                if (objMap.Occurrences > 0)
                    lstVerses.Add(objMap as MTVerseModel);
            }

            Run rVerses = new Run();
            if (lstVerses.Count() != 0)
            {
                rVerses.Text = " (";
                rVerses.Text += GetAdreesVerses(lstVerses);
                rVerses.Text += ")";
                rVerses.Foreground = Brushes.DarkGray;
            }
            return rVerses;
        }

        public static double LastPoints(MTPatchModel objPatch)
        {
            if (objPatch.lstTest.Count == 0)
                return 0;
            else if (objPatch.lstTest.Count == 1)
                return MTClassificator.GetPoints(objPatch.Created, objPatch.LastDateTest, true);
            else //if (_lstTest.Count > 1)
                return MTClassificator.GetPoints(objPatch.lstTest[objPatch.lstTest.Count - 2].Date, objPatch.LastDateTest, true);
        }

        public static List<MTMapModel> GetListMap(String sAdress)
        {
            try
            {
                List<MTMapModel> lstMap = new List<MTMapModel>();

                //Book
                string[] s1Split = sAdress.Split(' ');
                if (s1Split.Count() == 0)
                    return null;
                string sBook = s1Split[0];
                var varBook = _objLibrary.LstBooks.Where(x => x.Abbrev == sBook);
                if (varBook.Count() == 0)
                    return null;

                MTBookModel objBook = varBook.First() as MTBookModel;
                if (s1Split.Count() <= 1)
                {
                    lstMap.Add(objBook as MTMapModel);
                    return lstMap;
                }

                //Chapter
                string[] s2Split = s1Split[1].Split(':');
                string sChapter = s2Split[0];
                var varChapter = objBook.LstChapter.Where(x => x.Label == sChapter);
                if (varChapter.First() == null)
                    return null;

                MTChapterModel objChapter = varChapter.First() as MTChapterModel;
                if (s2Split.Count() <= 1)
                {
                    lstMap.Add(objChapter as MTMapModel);
                    return lstMap;
                }

                //Verses
                string[] s3Split = s2Split[1].Split(',');
                List<MTMapModel> lstVerses = new List<MTMapModel>();//MTVerseModel
                foreach (string sVers in s3Split)
                {
                    // "-"
                    string[] s4Split = sVers.Split('-');
                    if (s4Split.Count() > 1)
                    {
                        string sVer = s4Split[0];
                        while (Convert.ToInt32(sVer) <= Convert.ToInt32(s4Split[1]))
                        {
                            var varVer = objChapter.LstVerses.Where(x => x.Label == sVer);
                            if (varVer.First() == null)
                                return null;
                            lstVerses.Add(varVer.First() as MTMapModel);
                            sVer = (Convert.ToInt32(sVer) + 1).ToString();
                        }
                    }
                    // ","
                    else
                    {
                        var varVer = objChapter.LstVerses.Where(x => x.Label == s4Split[0]);
                        lstVerses.Add(varVer.First() as MTMapModel);
                    }
                }
                return lstVerses;
            }
            catch (Exception ex)
            {
                string e = ex.Message;
                return null;
            }
        }

        public static string GetAdrees(MTMapModel objMap, bool IsAbrevied)
        {
            List<MTMapModel> lstMap = new List<MTMapModel>();
            lstMap.Add(objMap);
            return GetAdrees(lstMap, IsAbrevied);
        }

        public static bool IsEqual(List<MTMapModel> lstMap1, List<MTMapModel> lstMap2)
        {
            return GetAdrees(lstMap1, true) == GetAdrees(lstMap2, true);
        }

        public static string GetAdrees(List<MTMapModel> lstMap, bool IsAbrevied)
        {
            if (lstMap == null || lstMap.Count == 0)
                return "";

            if (lstMap.First() as MTBookModel != null)
            {
                MTBookModel objBook = lstMap.First() as MTBookModel;
                string sBookName = IsAbrevied ? objBook.Abbrev : objBook.Name;
                return sBookName;
            }
            else if (lstMap.First() as MTChapterModel != null)
            {
                MTChapterModel objChapter = lstMap.First() as MTChapterModel;
                MTBookModel objBook = objChapter.Parent as MTBookModel;
                string sBookName = IsAbrevied ? objBook.Abbrev : objBook.Name;
                return sBookName + " " + objChapter.Label;
            }
            else if (lstMap.First() as MTVerseModel != null)
            {
                MTVerseModel objVerse = lstMap.First() as MTVerseModel;
                MTChapterModel objChapter = objVerse.Parent as MTChapterModel;
                MTBookModel objBook = objChapter.Parent as MTBookModel;
                string sBookName = IsAbrevied ? objBook.Abbrev : objBook.Name;
                string sReturn = sBookName + " " + objChapter.Label + ":";
                List<MTVerseModel> lstMTVers = new List<MTVerseModel>();
                foreach (MTMapModel objMap in lstMap)
                    lstMTVers.Add(objMap as MTVerseModel);
                sReturn += GetAdreesVerses(lstMTVers);
                return sReturn;
            }
            return null;            
        }

        private static string GetAdreesVerses(List<MTVerseModel> lstMTVers)
        {
            var lstMapOrdened = lstMTVers.OrderBy(c => (c as MTVerseModel).Verse);
            List<MTVerseModel> lstMTVersOrdened = new List<MTVerseModel>();
            foreach (MTMapModel objMap in lstMapOrdened)
                lstMTVersOrdened.Add(objMap as MTVerseModel);

            string sReturn = "";
            for (int i = 0; i < lstMTVersOrdened.Count; i++)
            {
                if (i == 0)
                    sReturn += lstMTVersOrdened[i].Label;
                else
                {
                    //Jump
                    int iJump = -1;
                    for (int ii = i; ii < lstMTVersOrdened.Count; ii++)
                        if (lstMTVersOrdened[ii].Verse == lstMTVersOrdened[ii - 1].Verse + 1)
                            iJump++;
                        else
                            break;

                    if (iJump > 0)
                    {
                        i += iJump;
                        sReturn += "-" + lstMTVersOrdened[i].Label;
                    }
                    else
                        sReturn += "," + lstMTVersOrdened[i].Label;
                }
            }
            return sReturn;
        }

        public static MTTestModel GetNextTest(ObservableCollection<MTTestModel> lstTest)
        {
            if (_dlgMainView.lvTexts.Items.Count == 0)
                return null;

            MTClassificator.UpdateAllPositions();
            foreach (MTPatchModel objPatch in _dlgMainView.lvTexts.Items)
            {
                var obj = lstTest.Where(x => x.Patch == objPatch);

                if (obj.Count() == 0)
                {
                    MTTestModel dlgTest = objPatch.NextTest;
                    return dlgTest;
                }
            }
            return null;
        }

        public static void AddTestInList(MTTestModel objTest)
        {
            _lstTest.Add(objTest);
            _dlgMainView.lbCount.Content = GetWaitngList(false);
            if (_dlgTest != null && _dlgTest.IsActive)
            {
                _dlgTest.lbTest.UpdateLayout();
                _dlgTest.InitializeTests(objTest);
                _dlgTest.lbTest.UpdateLayout();
                _dlgTest.UpdateDisplay();
            }
        }

        public static string GetWaitngList(Boolean IsAbbreviated)
        {
            string result = "";
            if (IsAbbreviated)
            {
                switch (_lstTest.Count)
                {
                    case 0:
                        result = "Nenhum teste em espera.";
                        break;
                    case 1:
                        result = "1 teste em espera.";
                        break;
                    default:
                        result = _lstTest.Count.ToString() + " testes em espera.";
                        break;
                }
            }
            else
            {
                switch (_lstTest.Count)
                {
                    case 0:
                        result = "Não ha teste em espera.";
                        break;
                    case 1:
                        result = "Hà um teste esperando para ser respondido.";
                        break;
                    default:
                        result = "Hà " + _lstTest.Count.ToString() + " testes esperando para serem respondidos.";
                        break;
                }
            }
            return result;
        }

        #endregion

        #region Properties

        public static MTTextMgrModel TextManager
        {
            get { return _textManager; }
            set { _textManager = value; }
        }

        public static BackgroundWorker AutoTestWorker
        {
            get { return MTControler._wAutoTestWorker; }
            set { MTControler._wAutoTestWorker = value; }
        }

        public static DateTime TimeRemaining
        {
            get { return _dtTimeRemaining; }
            set { _dtTimeRemaining = value; }
        }

        public static MTMainView MainView
        {
            get { return MTControler._dlgMainView; }
            set { MTControler._dlgMainView = value; }
        }

        public static System.Windows.Forms.NotifyIcon NiIcon
        {
            get { return MTControler._niIcon; }
            set { MTControler._niIcon = value; }
        }

        public static ObservableCollection<MTTestModel> LstTest
        {
            get { return MTControler._lstTest; }
            set { MTControler._lstTest = value; }
        }

        public static MTLibraryModel Library
        {
            get { return MTControler._objLibrary; }
            set { MTControler._objLibrary = value; }
        }

        public static string LibraryPath
        {
            get { return MTControler._LibraryPath; }
            set { MTControler._LibraryPath = value; }
        }

        public static string UserPath
        {
            get { return MTControler._UserPath; }
            set { MTControler._UserPath = value; }
        }
        
        public static string PatchPath
        {
            get 
            {
                string sPatchPath = MTControler._UserPath + "Patches\\";
                if (!Directory.Exists(sPatchPath))
                    Directory.CreateDirectory(sPatchPath);
                return sPatchPath; 
            }
        }

        public static string StudiesPath
        {
            get
            {
                string sEstudesPath = MTControler._UserPath + "Studies\\";
                if (!Directory.Exists(sEstudesPath))
                    Directory.CreateDirectory(sEstudesPath);
                return sEstudesPath;
            }
        }

        public static MTTestView DlgTest
        {
            get { return MTControler._dlgTest; }
            set { MTControler._dlgTest = value; }
        }

        public static MTSettingsModel Settings
        {
            get { return MTControler._Settings; }
            set { MTControler._Settings = value; }
        }

        public static DialogPositioner DlgPositTest
        {
            get { return MTControler._dlgPositTest; }
            set { MTControler._dlgPositTest = value; }
        }

        #endregion
    }
}
