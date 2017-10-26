using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;

namespace ViretTool.BasicClient.Controls {

    // TODO - rewrite to make simpler functions
    class Paging : Control {

        static Paging() {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(Paging), new FrameworkPropertyMetadata(typeof(Paging)));
        }

        #region Initialization


        public const string PartLeft = "PART_Left";
        public const string PartCurrentPage = "PART_CurrentPage";
        public const string PartRight = "PART_Right";

        private TextBox mTextBox;
        private Button BLeft;
        private Button BRight;

        /// <summary>
        /// Initialize the UI elements and register events
        /// </summary>
        public override void OnApplyTemplate() {
            base.OnApplyTemplate();
            
            BLeft = (Button)Template.FindName(PartLeft, this);
            BRight = (Button)Template.FindName(PartRight, this);
            mTextBox = (TextBox)Template.FindName(PartCurrentPage, this);

            BLeft.Click += BLeft_Click;
            BRight.Click += BRight_Click;
            mTextBox.KeyUp += MTextBox_KeyUp;

            if (CurrentPage == 1) BRight.IsEnabled = false;
            else BRight.IsEnabled = true;

            if (CurrentPage == NumberOfPages) BLeft.IsEnabled = false;
            else BLeft.IsEnabled = true;
        }

        private void MTextBox_KeyUp(object sender, KeyEventArgs e) {
            if (e.Key != Key.Enter) return;
            int result;
            if (int.TryParse(mTextBox.Text, out result) && !(NumberOfPages < result || result < 1)) {
                if (CurrentPage != result) {
                    CurrentPage = result;
                    CurrentPageChangedEvent?.Invoke(CurrentPage);
                }
            } else {
                mTextBox.Text = CurrentPage.ToString();
                mTextBox.SelectionStart = mTextBox.Text.Length;
            }
        }

        private void BRight_Click(object sender, RoutedEventArgs e) {
            if (CurrentPage - 1 > 0) {
                CurrentPage--;
                mTextBox.Text = CurrentPage.ToString();
                CurrentPageChangedEvent?.Invoke(CurrentPage);
            }
        }

        private void BLeft_Click(object sender, RoutedEventArgs e) {
            if (CurrentPage < NumberOfPages) {
                CurrentPage++;
                mTextBox.Text = CurrentPage.ToString();
                CurrentPageChangedEvent?.Invoke(CurrentPage);
            }
        }

        #endregion

        public delegate void CurrentPageChangedHandler(int page);
        public event CurrentPageChangedHandler CurrentPageChangedEvent;

        #region Properties

        public static readonly DependencyProperty CurrentPageProperty = DependencyProperty.Register("CurrentPage", typeof(int), typeof(Paging), new FrameworkPropertyMetadata(1));
        public static readonly DependencyProperty NumberOfPagesProperty = DependencyProperty.Register("NumberOfPages", typeof(int), typeof(Paging), new FrameworkPropertyMetadata(1));

        public int CurrentPage {
            get { return (int)GetValue(CurrentPageProperty); }
            private set {
                SetValue(CurrentPageProperty, value);

                if (IsLoaded) {
                    if (value == 1) BRight.IsEnabled = false;
                    else BRight.IsEnabled = true;

                    if (value == NumberOfPages) BLeft.IsEnabled = false;
                    else BLeft.IsEnabled = true;
                }
            }
        }

        public int NumberOfPages {
            get { return (int)GetValue(NumberOfPagesProperty); }
            private set { SetValue(NumberOfPagesProperty, value); }
        }

        #endregion

        public void SetCurrentPage(int page, int numberOfPages) {
            CurrentPage = page;
            NumberOfPages = numberOfPages;
            if (IsLoaded) mTextBox.Text = CurrentPage.ToString();
        }

    }

}
