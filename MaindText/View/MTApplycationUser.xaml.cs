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
using System.Collections.ObjectModel;
using System.Windows.Controls.Primitives;
using MaindText.Model;

namespace MaindText.View
{
    /// <summary>
    /// Interaction logic for MTApplycationUser.xaml
    /// </summary>
    public partial class MTApplycationUser : UserControl
    {
        public MTApplycationUser()
        {
            InitializeComponent();
        }

        private void Grid_KeyUp(object sender, KeyEventArgs e)
        {
            MTApplyMgrModel mgrApply = this.DataContext as MTApplyMgrModel;
            if (e.Key == Key.PageDown)
                mgrApply.SetBack();
            else if (e.Key == Key.PageUp)
                mgrApply.SetNext();            
        }

        private void btAdd_Click(object sender, RoutedEventArgs e)
        {
            tbSearch.SelectAll();
        }
    }
}
