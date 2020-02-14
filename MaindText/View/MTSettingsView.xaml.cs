using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using MaindText.Control;
using MaindText.Model;
using System.IO;

namespace MaindText.View
{
    public partial class MTSettingsView : Window
    {
        #region Initialization

        private MTSettingsModel _objNewSettings;

        #endregion

        #region Initialization

        public MTSettingsView()
        {
            InitializeComponent();
            _objNewSettings = MTControler.Settings.Clone();
            GridMain.DataContext = _objNewSettings;
        }

        #endregion

        #region Events

        private void btCancel_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void btOk_Click(object sender, RoutedEventArgs e)
        {
            MTSettingsModel objOldSettings = MTControler.Settings;
            MTControler.Settings = _objNewSettings;

            if (objOldSettings.AutoTestEnabled != _objNewSettings.AutoTestEnabled)
            {
                if (_objNewSettings.AutoTestEnabled)
                    MTControler.AutoTestWorker.RunWorkerAsync();
                else
                    MTControler.MainView.lbTimer.Content = "Auto test desabled";
            }
            if (objOldSettings.AutoTestInterval != _objNewSettings.AutoTestInterval)            
                MTControler.TimeRemaining = new DateTime().AddMinutes(_objNewSettings.AutoTestInterval);                

            //Update
            MTClassificator.UpdateAllPositions(); 
            //Write XML
            MTParser.WriteSettings();
            this.Close();
        }

        #endregion

        //################# Obsolet ######################
        private void btBuild_Click(object sender, RoutedEventArgs e)
        {
            string sName = tbName.Text;
            string sAbbrev = tbAbbr.Text;
            TextRange allTextRange = new TextRange(rtData.Document.ContentStart, rtData.Document.ContentEnd);
            string sData = allTextRange.Text;
            List<int> lstChapter = new List<int>();
            List<string> lstText = new List<string>();
            int iPos = 0;
            double dCurrNumb = 0;
            double dOldNumb = 0;
            int iLastPos = 0;

            while (iPos < sData.Length)
            {
                int iLenghtPos = 1;
                double dCheck;
                if (double.TryParse(sData.Substring(iPos, 1), out dCheck))
                {                                        
                    while (double.TryParse(sData.Substring(iPos, iLenghtPos), out dCheck))
                    {
                        dCurrNumb = dCheck;
                        iLenghtPos++;
                    }
                    iLenghtPos--;
                    if (dOldNumb == 0)
                        dOldNumb = dCurrNumb;
                    else
                    {
                        string sText = sData.Substring(iLastPos, (iPos - iLastPos));
                        lstChapter.Add(Convert.ToInt32(dOldNumb));
                        lstText.Add(sText);
                        iLastPos = iPos + iLenghtPos;
                        dOldNumb = dCurrNumb;
                    }
                }
                iPos += iLenghtPos;
                if (iPos == sData.Length)
                {
                    string sText = sData.Substring(iLastPos);
                    lstChapter.Add(Convert.ToInt32(dCurrNumb));
                    lstText.Add(sText);
                }
            }

            List<MTChapterModel> lstChapters = new List<MTChapterModel>();
            int iChapter = 1;
            for (int i = 0; i < lstChapter.Count(); i++)
            {
                MTVerseModel objVerse = new MTVerseModel(lstChapter[i]);
                objVerse.Text = lstText[i];

                if (i == 0)
                {
                    lstChapters.Add(new MTChapterModel(iChapter++));
                    objVerse.Text = lstText[i].Substring(1);
                }
                //if (i < lstChapter.Count && lstChapter[i] == 2)
                //{
                //    if (lstChapter[i + 1] == 2)
                //    {
                //        lstChapters.Add(new MTChapterModel(lstChapter[iChapter++]));
                //        objVerse.Verse = 1;
                //    }
                //}
                else if (i < (lstChapter.Count - 1) && lstChapter[i + 1] == 2)
                {
                    if (lstChapter[i + 2] != 2)
                    {
                        lstChapters.Add(new MTChapterModel(iChapter++));
                        objVerse.Verse = 1;
                    }
                }

                lstChapters.Last().LstVerses.Add(objVerse);
            }

            string appPath = System.Reflection.Assembly.GetExecutingAssembly().Location;
            string DataPath = System.IO.Path.GetDirectoryName(appPath) + "\\Library\\" + sAbbrev;

            if (!Directory.Exists(DataPath)) 
                Directory.CreateDirectory(DataPath);

            foreach (MTChapterModel objChap in lstChapters)
            {
                string sChapterPath = DataPath + "\\" + objChap.Label;

                if (!Directory.Exists(sChapterPath))
                    Directory.CreateDirectory(sChapterPath);

                foreach (MTVerseModel objVerse in objChap.LstVerses)
                {
                    string sVersePath = sChapterPath + "\\" + objVerse.Label + ".txt";
                    if (File.Exists(sVersePath))
                    {
                        using (System.IO.StreamWriter file = new System.IO.StreamWriter(sVersePath, false))
                        {
                            file.Write(objVerse.Text);
                        }
                    }
                    else
                    {
                        using (System.IO.StreamWriter file = new System.IO.StreamWriter(sVersePath))
                        {
                            file.Write(objVerse.Text);
                        }
                    }
                }
            }
        }
    }
}
