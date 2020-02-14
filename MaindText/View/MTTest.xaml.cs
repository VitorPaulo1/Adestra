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
using MaindText.Model;
using MaindText.Control;
using System.Collections.ObjectModel;
using System.Windows.Controls.Primitives;
using MaindText.Utils;

namespace MaindText.View
{
    public partial class MTTestView : Window
    {
        #region Parameters

        private ObservableCollection<MTTestModel> _lstTests;

        #endregion

        #region Initialization

        public MTTestView()
        {
            InitializeComponent();
            _lstTests = new ObservableCollection<MTTestModel>();
            lbTest.ItemsSource = _lstTests;
            MTControler.DlgPositTest.SetWindow(this);
        }

        #endregion

        #region Events

        private void btCancel_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void btAdd_Click(object sender, RoutedEventArgs e)
        {
            MTTestModel objTest = MTControler.GetNextTest(_lstTests);
            _lstTests.Add(objTest);
            InitializeTests(objTest);
            UpdateDisplay();
        }

        private void btask_Click(object sender, RoutedEventArgs e)
        {
            Button objButton = (Button)sender;
            MTTestModel objTest = objButton.Tag as MTTestModel;
            lbTest.SelectedItem = objTest;
            ShowAnsuerPopup(objTest);   
        }

        private void DocApply_Click(object sender, RoutedEventArgs e)
        {
            Button objButton = (Button)sender;
            MTApplyMgrModel mgrApply = objButton.Tag as MTApplyMgrModel;
            MTApplycationUser applyUser = CurrentPopup.FindName("userApplyForm") as MTApplycationUser;
            applyUser.DataContext = mgrApply;
            CurrentPopup.PlacementTarget = objButton.Parent as Grid;
            CurrentPopup.IsOpen = true;
            (applyUser.FindName("tbSearch") as TextBox).Focus();
        }

        private void btAnsuerClose_Click(object sender, RoutedEventArgs e)
        {
            CurrentAsuerPopup.IsOpen = false;
            ListViewItem item = lbTest.ItemContainerGenerator.ContainerFromItem(lbTest.SelectedItem) as ListViewItem;
            item.Focus();
        }

        private void btAnsuerCheck_Click(object sender, RoutedEventArgs e)
        {
            SetAnsuer();
            CurrentAsuerPopup.IsOpen = false;
            ListViewItem item = lbTest.ItemContainerGenerator.ContainerFromItem(lbTest.SelectedItem) as ListViewItem;
            item.Focus();
        }

        private void btClose_Click(object sender, RoutedEventArgs e)
        {
            CurrentPopup.IsOpen = false;            
        }  

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            UpdateDisplay();

            if (lbTest.Items.Count > 0)
            {
                lbTest.SelectedIndex = 0;
                ListViewItem item = lbTest.ItemContainerGenerator.ContainerFromItem(lbTest.SelectedItem) as ListViewItem;
                item.Focus();
            }
            InitializeAllTests();
        }

        private void Window_Unloaded(object sender, RoutedEventArgs e)
        {
            _lstTests.Clear();
            MTControler.MainView.lbCount.Content = MTControler.GetWaitngList(false);
            MTControler.DlgPositTest.GetPosition(this);
            GC.Collect();
            GC.WaitForPendingFinalizers();
            GC.Collect();
        }

        private void Window_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                if (CurrentAsuerPopup.IsOpen == false)
                    //In TestView
                    ShowAnsuerPopup(lbTest.SelectedItem as MTTestModel);
                else
                {
                    //In Popup
                    SetAnsuer();
                    CurrentAsuerPopup.IsOpen = false;
                    ListViewItem item = lbTest.ItemContainerGenerator.ContainerFromItem(lbTest.SelectedItem) as ListViewItem;
                    item.Focus();
                }

            }
            else if (e.Key == Key.Escape)
            {
                if (CurrentAsuerPopup.IsOpen == false)
                    //In TestView
                    this.Close();
                else
                {
                    CurrentAsuerPopup.IsOpen = false;
                    ListViewItem item = lbTest.ItemContainerGenerator.ContainerFromItem(lbTest.SelectedItem) as ListViewItem;
                    item.Focus();
                }

            }
        }

        #endregion

        #region Methodes

        public void SetAnsuer()
        {
            (lbTest.SelectedItem as MTTestModel).SetAnsuer();
            UpdateDisplay();
        }

        public void InitializeAllTests()
        {
            foreach (MTTestModel objTest in LstTests)
            {
                InitializeTests(objTest);
            }
        }

        public void InitializeTests(MTTestModel objTest)
        {
            if (LstTests.Contains(objTest))
            {
                lbTest.UpdateLayout();
                objTest.InitializeRichTextBox();
            }
        }

        public void ShowAnsuerPopup(MTTestModel objTest)
        {
            if (objTest.WasCorrect == null)
            {
                MTMapUser ansuerUser = CurrentAsuerPopup.FindName("_Map") as MTMapUser;

                //Set Map stage
                if (objTest.Patch.Score < MTControler.Settings.Stg1to2)
                    MTLibraryModel.Stage = TestStage.Stage_1;
                else if (objTest.Patch.Score < MTControler.Settings.Stg2to3)
                    MTLibraryModel.Stage = TestStage.Stage_2;
                else
                    MTLibraryModel.Stage = TestStage.Stage_3;

                ansuerUser.MapMode = objTest.Type;
                ansuerUser.Action = new DoSomeThing(objTest.Action);              

                Grid objGrid = UtilsTools.GetChild(lbTest, lbTest.SelectedIndex, "GridTest") as Grid;

                CurrentAsuerPopup.PlacementTarget = objGrid;
                foreach (MTMapModel objMap in ansuerUser.lvMap.Items)
                    objMap.FirePropertyChanged("Color");
                CurrentAsuerPopup.IsOpen = true;

                switch (ansuerUser.MapMode)
                {
                    case TestType.AskAdrees:                        
                        ansuerUser.SetSelectedVersesList(objTest.LstAnsuer);
                        ansuerUser.tbTextAdres.Text = (objTest as MTTestAskAdreesModel).PreviosText;
                        if (MTLibraryModel.Stage == TestStage.Stage_3)
                            ansuerUser.tbTextAdres.Focus();
                        else
                            ansuerUser.lvMap.Focus();
                        break;
                    case TestType.AskText:                        
                        ansuerUser.tbSearch.Text = (objTest as MTTestAskTextModel).Fragment;
                        ansuerUser.tbSearch.Focus();
                        break;
                }  
            } 
        }        

        public void UpdateDisplay()
        {
            double dUnansuered = 0;
            double dRight = 0;
            double dWrong = 0;
            double dPointsGave = 0;
            double dPointsLossed = 0;

            foreach (MTTestModel objTest in _lstTests)
            {
                if (objTest.WasCorrect == null)
                    dUnansuered++;
                else if (objTest.WasCorrect == true)
                {
                    dRight++;
                    dPointsGave += MTControler.LastPoints(objTest.Patch);
                }
                else
                {
                    dWrong++;
                    dPointsLossed += MTControler.LastPoints(objTest.Patch);
                }
            }

            lbUnansuered.Content = dUnansuered.ToString();
            lbRight.Content = dRight.ToString();
            lbWrong.Content = dWrong.ToString();
            lbPointsGave.Content = dPointsGave.ToString();
            lbPointsLossed.Content = dPointsLossed.ToString();
        }

        #endregion

        #region Properties

        public ObservableCollection<MTTestModel> LstTests
        {
            get { return _lstTests; }
            set { _lstTests = value;
            lbTest.ItemsSource = _lstTests;
            }
        }

        #endregion
    }
}
