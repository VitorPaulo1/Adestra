using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Input;
using System.Diagnostics;
using System.Windows.Media;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.ComponentModel;
using System.Reflection;

namespace MaindText.Utils
{
    public abstract class NotifyPropertyChangedBase : INotifyPropertyChanged
    {
        #region INotifyPropertyChanged Members

        public event PropertyChangedEventHandler PropertyChanged;

        #endregion

        #region Methods

        public void FirePropertyChanged(string propertyName)
        {
            if (this.PropertyChanged != null)
                this.PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }

        protected void FireAllPropertiesChanged()
        {
            PropertyInfo[] arrProperties = this.GetType().GetProperties();
            foreach (PropertyInfo objProperty in arrProperties)
                FirePropertyChanged(objProperty.Name);
        }

        #endregion
    }

    public class EnumToBoolConverter : IValueConverter
    {
        #region IValueConverter Members

        public object Convert(object value,
            Type targetType, object parameter,
            System.Globalization.CultureInfo culture)
        {
            if (parameter.Equals(value))
                return true;
            else
                return false;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            return parameter;

        }
        #endregion
    }  

    /// <summary>
    /// A command whose sole purpose is to 
    /// relay its functionality to other
    /// objects by invoking delegates. The
    /// default return value for the CanExecute
    /// method is 'true'.
    /// </summary>
    public class RelayCommand : ICommand
    {
        #region Fields

        readonly Action<object> _execute;
        readonly Predicate<object> _canExecute;

        #endregion // Fields

        #region Constructors

        /// <summary>
        /// Creates a new command that can always execute.
        /// </summary>
        /// <param name="execute">The execution logic.</param>
        public RelayCommand(Action<object> execute)
            : this(execute, null)
        {
        }

        /// <summary>
        /// Creates a new command.
        /// </summary>
        /// <param name="execute">The execution logic.</param>
        /// <param name="canExecute">The execution status logic.</param>
        public RelayCommand(Action<object> execute, Predicate<object> canExecute)
        {
            if (execute == null)
                throw new ArgumentNullException("execute");

            _execute = execute;
            _canExecute = canExecute;
        }

        #endregion // Constructors

        #region ICommand Members

        [DebuggerStepThrough]
        public bool CanExecute(object parameters)
        {
            return _canExecute == null ? true : _canExecute(parameters);
        }

        public event EventHandler CanExecuteChanged
        {
            add { CommandManager.RequerySuggested += value; }
            remove { CommandManager.RequerySuggested -= value; }
        }

        public void Execute(object parameters)
        {
            _execute(parameters);
        }

        #endregion // ICommand Members
    }

    public static class UtilsTools
    {
        public static object GetChild(Selector objSel, int iSelectedIndex, string sObjChildName)
        {
            // Getting the currently selected ListBoxItem
            // Note that the ListBox must have
            // IsSynchronizedWithCurrentItem set to True for this to work
            DependencyObject depObj = (objSel.ItemContainerGenerator.ContainerFromIndex(iSelectedIndex));

            //Getting the ContentPresenter of myListBoxItem
            ContentPresenter objContentPresenter = FindVisualChild<ContentPresenter>(depObj);
            if (objContentPresenter == null)
                return null;

            // Finding textBlock from the DataTemplate that is set on that ContentPresenter
            DataTemplate objDataTemplate = objContentPresenter.ContentTemplate;
            return objDataTemplate.FindName(sObjChildName, objContentPresenter);
        }

        private static childItem FindVisualChild<childItem>(DependencyObject obj)
        where childItem : DependencyObject
        {
            for (int i = 0; i < VisualTreeHelper.GetChildrenCount(obj); i++)
            {
                DependencyObject child = VisualTreeHelper.GetChild(obj, i);
                if (child != null && child is childItem)
                    return (childItem)child;
                else
                {
                    childItem childOfChild = FindVisualChild<childItem>(child);
                    if (childOfChild != null)
                        return childOfChild;
                }
            }
            return null;
        }
    }

    public class DialogPositioner
    {
        #region Parameters

        private bool bWithPosition;

        private double _dDlgHeight;
        private double _dDlgWidth;

        private double _dDlgTop;
        private double _dDlgLeft;

        private WindowState _winState;

        #endregion

        #region Initialize

        public DialogPositioner()
        {
            bWithPosition = false;
        }

        #endregion

        #region Methodes

        public void SetWindow(Window objWindow)
        {
            if (bWithPosition)
            {
                objWindow.WindowStartupLocation = WindowStartupLocation.Manual;

                objWindow.Height = _dDlgHeight;
                objWindow.Width = _dDlgWidth;

                objWindow.Top = _dDlgTop;
                objWindow.Left = _dDlgLeft;

                objWindow.WindowState = _winState;
            }
        }

        public void GetPosition(Window objWindow)
        {
            if (objWindow != null)
            {
                _dDlgHeight = objWindow.Height;
                _dDlgWidth = objWindow.Width;

                _dDlgTop = objWindow.Top;
                _dDlgLeft = objWindow.Left;

                _winState = objWindow.WindowState;

                bWithPosition = true;
            }
        }

        #endregion

        #region Properties

        //public double DlgHeight
        //{
        //    get { return _dDlgHeight; }
        //    set { _dDlgHeight = value; }
        //}

        //public double DlgWidth
        //{
        //    get { return _dDlgWidth; }
        //    set { _dDlgWidth = value; }
        //}

        //public double DlgTop
        //{
        //    get { return _dDlgTop; }
        //    set { _dDlgTop = value; }
        //}

        //public double DlgLeft
        //{
        //    get { return _dDlgLeft; }
        //    set { _dDlgLeft = value; }
        //}

        //public WindowState WinState
        //{
        //    get { return _winState; }
        //    set { _winState = value; }
        //}

        #endregion

    }

}
