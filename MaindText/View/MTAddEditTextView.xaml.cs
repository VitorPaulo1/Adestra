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

namespace MaindText.View
{
    /// <summary>
    /// Interaction logic for MTaddTextView.xaml
    /// </summary>
    public partial class MTAddEditTextView : Window
    {
        private MTPatchModel _objText = null;
        private bool _bIsAdd;

        #region Initialization

        public MTAddEditTextView()
        {
            InitializeComponent();
            _Map.MapMode = TestType.AskAdrees;
            _Map.Action = new DoSomeThing(Action);
            _objText = new MTPatchModel();
            _bIsAdd = true;
            btAdd.Content = "Adicionar";
            this.Title = "Adicionar Texto";
        }

        public MTAddEditTextView(ref MTPatchModel objText)
        {
            InitializeComponent();
            _Map.MapMode = TestType.AskAdrees;
            _Map.Action = new DoSomeThing(Action);
            _Map.SetSelectedVersesList(objText.LstAdress);
            _objText = objText;
            _bIsAdd = false;
            btAdd.Content = "Editar";
            this.Title = "Editar Texto";
        }

        #endregion

        #region Methodes

        public void Action(MTMapUser ansuerUser)
        {
            tbText.Document.Blocks.Clear();
            Paragraph parag = MTControler.GetText(_Map.SelectedPatch, "$Verse$");
            if(parag != null)
                tbText.Document.Blocks.Add(parag);
        }

        #endregion

        #region Events

        private void btAdd_Click(object sender, RoutedEventArgs e)
        {
            var obj = MTControler.TextManager.LstPatch.Where(x => x.Adrees == _Map.GetAdrees());
            if (obj.Count() != 0)
            {
                MessageBox.Show(string.Format("O texto {0} já existe.", _objText.Adrees), "Texto existente", MessageBoxButton.OK, MessageBoxImage.Exclamation);
                return;
            }

            if (!_bIsAdd)
                MTControler.TextManager.LstPatch.Remove(_objText);

            _objText.SetLstAdress(_Map.SelectedPatch);

            MTControler.TextManager.AddPatch(_objText);
            MTParser.WritePatch(_objText);
            this.Close();
        }
        
        private void btCancel_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        #endregion

        private void Window_Activated(object sender, EventArgs e)
        {
            MTLibraryModel.Stage = TestStage.Stage_1;
            foreach (MTMapModel objMap in _Map.lvMap.Items)
                objMap.FirePropertyChanged("Color");
        }
    }
}
