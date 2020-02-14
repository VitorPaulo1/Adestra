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
using System.Timers;
using System.ComponentModel;
using System.Threading;
using MaindText.Control;
using MaindText.Model;
using System.IO;
using System.Globalization;
using System.Windows.Controls.Primitives;

namespace MaindText.View
{
    public partial class MTMainView : Window
    {
        #region Parameters

        private DateTime _dtlimit;
        private Popup _popCurrent;

        #endregion

        #region Initialization

        public MTMainView()
        {
            InitializeComponent();            
            MainGrid.DataContext = MTControler.TextManager;
            _dtlimit = new DateTime();
            lbCount.Content = MTControler.GetWaitngList(false);
            InitializeTimer();
            lbPatchCount.Content = "Número de textos: " + MTControler.TextManager.LstPatch.Count().ToString();
        }

        public void InitializeTimer()
        {
            //------------------  BackgroundWorker Structure (Start) ---------------------
            //get our dispatcher
            System.Windows.Threading.Dispatcher pdDispatcher = this.Dispatcher;

            MTControler.AutoTestWorker.DoWork += delegate(object s, DoWorkEventArgs args)
            {
                //create a new delegate for call uor methode
                UpdateProgressDelegate update = new UpdateProgressDelegate(RunInLastThread);

                MTControler.TimeRemaining = new DateTime().AddMinutes(MTControler.Settings.AutoTestInterval);

                //While
                while (MTControler.Settings.AutoTestEnabled)
                {
                    Thread.Sleep(1000);

                    //invoke the dispatcher and pass the percentage and max record count
                    pdDispatcher.BeginInvoke(update, this);
                } 
            };

            //run the process then show the progress dialog
            MTControler.AutoTestWorker.RunWorkerAsync();
            //------------------  BackgroundWorker Structure (End) ---------------------
        }        

        #endregion

        #region Events

        private void btAdd_Click(object sender, RoutedEventArgs e)
        {
            CloseCurrentPopup();
            MTLibraryModel.Stage = TestStage.Stage_1;
            MTAddEditTextView dlgAdd = new MTAddEditTextView();
            dlgAdd.Owner = this;
            dlgAdd.ShowDialog();
            lbPatchCount.Content = "Número de textos: " + MTControler.TextManager.LstPatch.Count().ToString();
        }
         private void btRemove_Click(object sender, RoutedEventArgs e)
        {
            CloseCurrentPopup();
            if (this.lvTexts.SelectedIndex > -1)
            {
                MTPatchModel objText = this.lvTexts.SelectedItem as MTPatchModel;
                //Ask to user
                MessageBoxResult result = MessageBox.Show("Você tem certeza que deseja deletar este exto?\n\"" + objText.Adrees + "\"", "Deletar Texto", MessageBoxButton.YesNo, MessageBoxImage.Exclamation);
                if (result == MessageBoxResult.No)
                    return;
                
                MTControler.TextManager.LstPatch.Remove(objText);

                //Remove from Map Occurrence
                foreach (MTMapModel objMap in objText.LstAdress)
                    objMap.RemoveOccurrence(1);

                if(File.Exists(objText.FilePatch))
                    File.Delete(objText.FilePatch);

                lbPatchCount.Content = "Número de textos: " + MTControler.TextManager.LstPatch.Count().ToString();
            }
            else
            {
                MessageBox.Show("Selecione pelomenos im item na lista.","Selecione um item", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }


         private void btEdit_Click(object sender, RoutedEventArgs e)
         {
             CloseCurrentPopup();
             if (this.lvTexts.SelectedIndex > -1)
             {
                 MTPatchModel objText = this.lvTexts.SelectedItem as MTPatchModel;
                 MTAddEditTextView dlgAdd = new MTAddEditTextView(ref objText);
                 dlgAdd.Owner = this;
                 dlgAdd.Show();
             }
             else
             {
                 MessageBox.Show("Selecione pelomenos im item na lista.", "Selecione um item", MessageBoxButton.OK, MessageBoxImage.Information);
             }
         }

        private void btTest_Click(object sender, RoutedEventArgs e)
        {
            CloseCurrentPopup();
            RumNextTest();
        }

        private void btTestSelected_Click(object sender, RoutedEventArgs e)
        {
            CloseCurrentPopup();
            RumSelectedTest();
        }

        private void btAtualize_Click(object sender, RoutedEventArgs e)
        {
            CloseCurrentPopup();
            MTClassificator.UpdateAllPositions(); 
        }

        private void btSettings_Click(object sender, RoutedEventArgs e)
        {
            CloseCurrentPopup();
            MTSettingsView dlgSett = new MTSettingsView();
            dlgSett.Owner = this;
            dlgSett.Show();
        }

        private void header_Click(object sender, RoutedEventArgs e)
        {
            CloseCurrentPopup();
            GridViewColumnHeader header = sender as GridViewColumnHeader;

            if (header.Name == "heText")
                MTControler.Settings.EnSortOrder = SortOrder.Text;
            else if (header.Name == "heApply")
                MTControler.Settings.EnSortOrder = SortOrder.Applycations;
            else if (header.Name == "heDate")
                MTControler.Settings.EnSortOrder = SortOrder.Date;
            else if (header.Name == "hePosition")
                MTControler.Settings.EnSortOrder = SortOrder.PositionBrain;
            else if (header.Name == "heProgress")
                MTControler.Settings.EnSortOrder = SortOrder.Progress;
            else if (header.Name == "heNumTest")
                MTControler.Settings.EnSortOrder = SortOrder.TestTotal;
            else if (header.Name == "heAnsuersCorrects")
                MTControler.Settings.EnSortOrder = SortOrder.TestCorrects;
            else if (header.Name == "heAnsuersWrongs")
                MTControler.Settings.EnSortOrder = SortOrder.TestWrongs;
            else if (header.Name == "hePercentsWrongs")
                MTControler.Settings.EnSortOrder = SortOrder.TestWrongPercents;
            else if (header.Name == "heCreated")
                MTControler.Settings.EnSortOrder = SortOrder.Created;
            
            //header.Background = new SolidColorBrush(Colors.LightPink);
            MTClassificator.UpdateAllPositions();
            MTParser.WriteSettings();
        }

        protected override void OnStateChanged(EventArgs e)
        {
            CloseCurrentPopup();
            if (WindowState == WindowState.Minimized)
                this.Hide();

            base.OnStateChanged(e);
        }

        private void cbEnabled_Checked(object sender, RoutedEventArgs e)
        {
            CloseCurrentPopup();
            CheckBox objCb = sender as CheckBox;
            if (!lvTexts.SelectedItems.Contains(objCb.DataContext))
                MTParser.WritePatch(objCb.DataContext as MTPatchModel);
            else
            {
                foreach (MTPatchModel objPatch in lvTexts.SelectedItems)
                {
                    objPatch.TestIsEnabled = true;
                    MTParser.WritePatch(objPatch);                    
                }
            }
            MTControler.TextManager.FirePropertyChanged("DgCollection");
        }

        private void cbEnabled_Unchecked(object sender, RoutedEventArgs e)
        {
            CloseCurrentPopup();
            CheckBox objCb = sender as CheckBox;
            if (!lvTexts.SelectedItems.Contains(objCb.DataContext))
                MTParser.WritePatch(objCb.DataContext as MTPatchModel);
            else
            {
                foreach (MTPatchModel objPatch in lvTexts.SelectedItems)
                {
                    objPatch.TestIsEnabled = false;
                    MTParser.WritePatch(objPatch);                    
                }
            }
            MTControler.TextManager.FirePropertyChanged("DgCollection");
        }

        private void tbtCategorized_Checked(object sender, RoutedEventArgs e)
        {
            CloseCurrentPopup();
            lvTexts.GroupStyle.Add(Resources["styGroup"] as GroupStyle);
            ListCollectionView dgCollection = lvTexts.ItemsSource as ListCollectionView;
            dgCollection.GroupDescriptions.Add(new PropertyGroupDescription("Group"));            
        }

        private void tbtCategorized_Unchecked(object sender, RoutedEventArgs e)
        {
            CloseCurrentPopup();
            lvTexts.GroupStyle.Remove(Resources["styGroup"] as GroupStyle);
            ListCollectionView dgCollection = lvTexts.ItemsSource as ListCollectionView;
            dgCollection.GroupDescriptions.Clear();
        }

        private void btOpenEstude_Click(object sender, RoutedEventArgs e)
        {
            //Popup2.IsOpen = true;
            //if (MTControler.StudiesManager.DlgListStudy != null && MTControler.StudiesManager.DlgListStudy.IsLoaded)
            //    MTControler.StudiesManager.DlgListStudy.Activate();
            //else
            //{
            //    MTControler.StudiesManager.DlgListStudy = new MTListStudiesView();
            //    MTControler.StudiesManager.DlgListStudy.Show();
            //}
        }

        private void DocApply_Click(object sender, RoutedEventArgs e)
        {
            Button objButton = (Button)sender;
            MTApplyMgrModel mgrApply = objButton.Tag as MTApplyMgrModel;
            MTApplycationUser applyUser = CurrentPopup.FindName("userApplyForm") as MTApplycationUser;
            applyUser.DataContext = mgrApply;
            CurrentPopup.PlacementTarget = objButton;
            CurrentPopup.IsOpen = true;
            (applyUser.FindName("tbSearch") as TextBox).Focus();
        }   

        private void Popup_Opened(object sender, EventArgs e)
        {
            CloseCurrentPopup();
            _popCurrent = (Popup)sender;            
        }

        private void Window_KeyUp(object sender, KeyEventArgs e)
        {
            if (CurrentPopup.IsOpen == true)
            {
                if (e.Key == Key.Escape)                            
                    CurrentPopup.IsOpen = false;
            }            
        }

        private void btClose_Click(object sender, RoutedEventArgs e)
        {
            CurrentPopup.IsOpen = false;
        }  

        #endregion

        #region Methodes

        private void CloseCurrentPopup()
        {
            if (_popCurrent != null)
                _popCurrent.IsOpen = false;
        }

        public void RumNextTest()
        {
            MTParser.ReadNewBible(@"C:\Users\Vitor\OneDrive\Arsenal Technology\Projetos\Adestra\MaindText\bin\Release\Library\Mt\mat.xml");
            MTTestModel objTest = MTControler.GetNextTest(MTControler.LstTest);
            MTControler.AddTestInList(objTest);
            MTControler.DlgTest.InitializeTests(objTest);

            if (this.WindowState != WindowState.Minimized && MTControler.LstTest.Count == 1)
            {
                MTControler.ShowLstDlgTest();
            }
        }

        public void RumSelectedTest()
        {            
            if (lvTexts.Items.Count == 0)
                return;

            if (this.lvTexts.SelectedIndex > -1)
            {                
                foreach (MTPatchModel objPatch in lvTexts.SelectedItems)
                {
                    MTTestModel objTest = objPatch.NextTest;
                    MTControler.LstTest.Add(objTest);
                    MTControler.DlgTest.InitializeTests(objTest);
                }
                MTControler.ShowLstDlgTest();
            }
            else
            {
                MessageBox.Show("Selecione pelomenos im item na lista.", "Selecione um item", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        //------------------  BackgroundWorker Delegates (Start) ---------------------
        //our delegate used for updating the UI
        public delegate void UpdateProgressDelegate(MTMainView objMainView);

        //this is the method that the deleagte will execute
        public void RunInLastThread(MTMainView objMainView)
        {
            if (MTControler.TimeRemaining == _dtlimit)
            {
                MTControler.TimeRemaining = new DateTime().AddMinutes(MTControler.Settings.AutoTestInterval);                
                //Rum methode
                objMainView.RumNextTest();
                if (objMainView.WindowState == WindowState.Minimized)
                {
                    MTControler.NiIcon.BalloonTipText = MTControler.GetWaitngList(true);
                    MTControler.NiIcon.ShowBalloonTip(3000);
                }
            }
            else if (MTControler.Settings.AutoTestEnabled)
            {
                MTControler.TimeRemaining = MTControler.TimeRemaining.AddSeconds(-1);
                CultureInfo provider = new CultureInfo("pt-PT");
                objMainView.lbTimer.Content = "Próximo teste em " + MTControler.TimeRemaining.ToString("T", provider);
            }            
        }

        //------------------  BackgroundWorker Delegates (End) ---------------------
        #endregion

        private void Window_Closed(object sender, EventArgs e)
        {
            MTControler.Settings.AutoTestEnabled = false;
            MTControler.AutoTestWorker.CancelAsync();
        }
    }
}
