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
using System.Windows.Navigation;
using System.Windows.Shapes;
using MaindText.Control;
using MaindText.Model;
using System.Collections;
using System.Collections.ObjectModel;
using System.IO;
using System.Text.RegularExpressions;

namespace MaindText.View
{
    public class MapColorConverter : IValueConverter
    {
        #region IValueConverter Members

        public object Convert(object value,
            Type targetType, object parameter,
            System.Globalization.CultureInfo culture)
        {
            
            MTPatchModel objPatch = parameter as MTPatchModel;
            if (objPatch == null)
                return value;
            else
            {
                if(objPatch.Score < MTControler.Settings.Stg1to2)
                    if ((value as SolidColorBrush).Color.Equals(Colors.Khaki))
                        return value;
                    else
                        return new SolidColorBrush(Colors.PowderBlue);
                else
                    return new SolidColorBrush(Colors.PowderBlue);                
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            return null;
        }
        #endregion
    }  

    public delegate void DoSomeThing(MTMapUser ansuerUser);
    /// <summary>
    /// Interaction logic for MTPapView.xaml
    /// </summary>
    public partial class MTMapUser : UserControl
    {
        private List<MTMapModel> _selectedPatch;
        private Dictionary<MTPatchModel, string> _dicSearch;
        private DoSomeThing _deAction;
        private TestType _enTestType;

        public MTMapUser()
        {            
            InitializeComponent();
            _enTestType = TestType.AskAdrees;
            GridMap.DataContext = MTControler.Library;
            _dicSearch = new Dictionary<MTPatchModel, string>();            
        }

        public List<MTMapModel> SelectedPatch
        {
            get
            {
                if (_selectedPatch == null)
                    _selectedPatch = new List<MTMapModel>();
                return _selectedPatch;
            }
            set {
                List<MTMapModel> lstAdrees = new List<MTMapModel>();
                foreach (MTMapModel objVer in value)
                    lstAdrees.Add(objVer);
                _selectedPatch = lstAdrees; 
            }
        }

        public List<MTMapModel> SelectedPatchClone
        {
            get
            {
                List<MTMapModel> selectedPatchClone = new List<MTMapModel>();
                if (_selectedPatch == null)
                    _selectedPatch = new List<MTMapModel>();
                foreach (MTMapModel objMap in _selectedPatch)
                    selectedPatchClone.Add(objMap);
                return selectedPatchClone;
            }
        }

        public TestType MapMode
        {
            get
            {
                return _enTestType;
            }
            set
            {
                switch (value)
                {
                    case TestType.AskText:
                        GridMap.Visibility = Visibility.Collapsed;
                        GridSearch.Visibility = Visibility.Visible;
                        GridTextAdres.Visibility = Visibility.Collapsed;
                        break;
                    case TestType.AskAdrees:
                        if (MTLibraryModel.Stage == TestStage.Stage_3)
                        {
                            GridMap.Visibility = Visibility.Collapsed;
                            GridTextAdres.Visibility = Visibility.Visible;
                        }
                        else
                        {
                            GridMap.Visibility = Visibility.Visible;
                            GridTextAdres.Visibility = Visibility.Collapsed;
                        }
                        GridSearch.Visibility = Visibility.Collapsed;
                        
                        break;
                }
                _enTestType = value;
            }
        }

        public DoSomeThing Action
        {
            get { return _deAction; }
            set { _deAction = value; }
        }

        //################# Ask Adrees #####################
        private void lvMap_MouseUp(object sender, MouseButtonEventArgs e)
        {
            if ((lvMap.SelectedItem as MTMapModel) != null)
            {
                MTMapModel objMap = lvMap.SelectedItem as MTMapModel;
                if (objMap as MTBackModel != null)
                {
                    if (objMap.Parent != null)
                    {
                        GridMap.DataContext = objMap.Parent.Parent;
                        lvMap.SelectedItem = SelectedPatch[0].Parent;
                    }
                }
                else
                {
                    if (objMap.LstItens != null)
                        GridMap.DataContext = objMap;
                }
            }
        }

        private void lvMap_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {      
            MTBackModel objBack = null;
            foreach (object obj in lvMap.SelectedItems)
                if ((obj as MTBackModel) != null)
                    objBack = obj as MTBackModel;

            if (objBack != null)
            {
                if (objBack != null && lvMap.SelectedItems.Count > 1)
                    lvMap.SelectedItems.Remove(objBack);
                return;
            }
            

            if (lvMap.SelectedItems.Count != 0)
            {
                SetSelectedVersesList();
                tbTextMap.Text = GetAdrees();
                Action(this);
            }
        }

        public void SetSelectedVersesList()
        {
            SelectedPatch.Clear();
            foreach (object obj in lvMap.SelectedItems)
            {
                MTMapModel objMap = obj as MTMapModel;
                SelectedPatch.Add(objMap);
            }
        }

        public string GetAdrees()
        {
            string sText = "";
            if (lvMap.SelectedItems.Count > 0)
            {
                if ((lvMap.SelectedItems[0] as MTBookModel) != null)
                {
                    MTBookModel objBook = lvMap.SelectedItem as MTBookModel;
                    sText = MTControler.GetAdrees(objBook, false);
                }
                else if ((lvMap.SelectedItems[0] as MTChapterModel) != null)
                {
                    MTChapterModel objChapter = lvMap.SelectedItem as MTChapterModel;
                    sText = MTControler.GetAdrees(objChapter, false);
                }
                else if ((lvMap.SelectedItems[0] as MTVerseModel) != null)
                {
                    sText = MTControler.GetAdrees(SelectedPatch, false);
                }
            }
            return sText;
        }

        public void SetSelectedVersesList(List<MTMapModel> lstMap)
        {
            if (lstMap == null || lstMap.Count == 0)
            {
                GridMap.DataContext = MTControler.Library;
                return;
            }
            MTMapModel objMap = lstMap.First();
            if (objMap.Parent != null)
            {
                GridMap.DataContext = objMap.Parent;
            }
            foreach (MTMapModel objIt in lstMap)
            {
                lvMap.SelectedItems.Add(objIt);
            }
            lvMap.Focus();
        }

        //################# Ask Text #####################
        private void tbSearch_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (tbSearch.Text.Length > 3)
            {
                _dicSearch.Clear();
                foreach (MTPatchModel objPatch in MTControler.TextManager.LstPatch)
                {
                    string sText = objPatch.Text;

                    sText = sText.Replace("[", "");
                    sText = sText.Replace("]", "");
                    sText = sText.Replace("'", "");
                    sText = sText.Replace("\"", "");
                    sText = sText.Replace(" ", "");

                    if (sText.ToLower().Contains(tbSearch.Text.ToLower()))
                        _dicSearch.Add(objPatch, sText);
                }
                if (_dicSearch.Count > 0)
                {
                    lbNumb.Content = "1";
                    lbSearchDesc.Content = _dicSearch.Count.ToString() + " textos encontrados";
                    SelectedPatch = _dicSearch.First().Key.LstAdress;
                }
                else
                {
                    lbNumb.Content = "0";
                    lbSearchDesc.Content = "Nenhum texto encontrado";
                    SelectedPatch.Clear();
                }
            }
            else
            {
                SelectedPatch.Clear();
            }
            Action(this);
        }

        private void igFront_ImageFailed(object sender, MouseButtonEventArgs e)
        {
            SetNextPatch(true);
        }

        private void igBack_ImageFailed(object sender, MouseButtonEventArgs e)
        {
            SetNextPatch(false);
        }

        private void SetNextPatch(bool bToFront)
        {
            bool bSet = false;
            if (bToFront)
            {
                for (int i = 0; i < _dicSearch.Count(); i++)
                {
                    if (bSet)
                    {
                        SelectedPatch = _dicSearch.ElementAt(i).Key.LstAdress;
                        lbNumb.Content = (i + 1).ToString();
                        break;
                    }
                    if (MTControler.IsEqual(_dicSearch.ElementAt(i).Key.LstAdress, SelectedPatch))
                        bSet = true;
                }
            }
            else
            {
                for (int i = _dicSearch.Count() - 1; i >= 0; i--)
                {
                    if (bSet)
                    {
                        SelectedPatch = _dicSearch.ElementAt(i).Key.LstAdress;
                        lbNumb.Content = (i + 1).ToString();
                        break;
                    }
                    if (MTControler.IsEqual(_dicSearch.ElementAt(i).Key.LstAdress, SelectedPatch))
                        bSet = true;
                }
            }
            Action(this);
        }

        private void tbSearch_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Left)
                SetNextPatch(false);

            if (e.Key == Key.Right)
                SetNextPatch(true);
        }

        //################# Ask Adrees by Text #####################

        private void tbTextAdres_TextChanged(object sender, TextChangedEventArgs e)
        {
            string sText = tbTextAdres.Text.ToLower();
            sText = sText.Trim();

            if (sText == "")
            {
                tblockTextAdres.Text = "";
                SelectedPatch.Clear();
                return;
            }
            string sFirstchar = sText.Substring(0, 1);
            sText = sText.Substring(1);
            char[] arr = sText.ToCharArray();

            arr = Array.FindAll<char>(arr, (c => (char.IsLetter(c) || char.IsWhiteSpace(c))));
            sText = new string(arr);

            sText = sFirstchar + sText;
            sText = sText.Trim();

            if (sText == "")
            {
                tblockTextAdres.Text = "";
                Action(this);
                return;
            }

            MTBookModel objSelBook = null;
            foreach (MTBookModel objBook in MTControler.Library.LstBooks)
            {
                string sBookAbrev = objBook.Abbrev.ToLower();
                if(sText.Length <= sBookAbrev.Length)
                {
                if (sText.StartsWith(sBookAbrev.Substring(0,sText.Length)))
                    objSelBook = objBook;
                }

                string sBookFull = objBook.Name.ToLower();
                if(sText.Length <= sBookFull.Length)
                {
                if (sText.StartsWith(sBookFull.Substring(0,sText.Length)))
                    objSelBook = objBook;
                }                
            }
            if (objSelBook == null)
            {
                tblockTextAdres.Text = "Texto Inválido!";
                SelectedPatch.Clear();
                Action(this);
                return;
            }
            string sChapterVerses = tbTextAdres.Text.ToLower().Replace(sText, "");
            sChapterVerses = sChapterVerses.Replace(" ", "");
            tblockTextAdres.Text = objSelBook.Name + " " + sChapterVerses;

            List<MTMapModel> lstMap = MTControler.GetListMap(objSelBook.Abbrev + " " + sChapterVerses);
            if(lstMap != null)
                SelectedPatch = lstMap;
            else
                SelectedPatch.Clear();

            Action(this);
        }
    } 
}
