using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Markup;
using System.Windows.Media;

namespace RoRLauncher
{
    public partial class PopupWindow : Window
    {
        private static PopupWindow popup;

        public static PopupWindow GetPopup()
        {
            if (PopupWindow.popup == null)
            {
                PopupWindow.popup = new PopupWindow();
            }
            return PopupWindow.popup;
        }

        public PopupWindow()
        {
            this.InitializeComponent();
        }

        public void SetError(string message)
        {
            this.error_MSG.Text = message;
            this.CheckScroll();
        }

        private void Window_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (Mouse.LeftButton == MouseButtonState.Pressed)
            {
                base.DragMove();
            }
        }

        private void CLOSE_BUTTON_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            // Hide the form instead of close it due to how the MainWindow.Popup function works
            base.Hide();
        }

        private void ScrollUp_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (ScrollUp.Opacity == 1.0)
            {
                this.myScroll.LineUp();
                this.myScroll.LineUp();
                this.CheckScroll();
            }
        }

        private void ScrollDown_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (ScrollDown.Opacity == 1.0)
            {
                this.myScroll.LineDown();
                this.myScroll.LineDown();
                this.CheckScroll();
            }
        }

        private void CheckScroll()
        {
            bool flag = this.myScroll.ScrollableHeight > 0.0 && this.myScroll.VerticalOffset > 0.0;
            bool flag2 = this.myScroll.ScrollableHeight > 0.0 && this.myScroll.VerticalOffset + this.myScroll.ViewportHeight < this.myScroll.ExtentHeight;
            if (flag)
            {
                this.ScrollUp.Opacity = 1.0;
            }
            else
            {
                this.ScrollUp.Opacity = 0.302;
            }
            if (flag2)
            {
                this.ScrollDown.Opacity = 1.0;
                return;
            }
            this.ScrollDown.Opacity = 0.302;
        }

        private void myScroll_ScrollChanged(object sender, ScrollChangedEventArgs e)
        {
            this.CheckScroll();
        }

        private void CopyToClipboardCanvas_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            Clipboard.SetText(this.error_MSG.Text);
        }

        private void Window_Activated(object sender, EventArgs e)
        {
            Background_Highlighter.Visibility = Visibility.Hidden;
        }

        private void Window_Deactivated(object sender, EventArgs e)
        {
            Background_Highlighter.Visibility = Visibility.Visible;
        }

        private void CopyToClipboardCanvas_MouseEnter(object sender, MouseEventArgs e)
        {
            this.CopyToClipboardImage.Source = new ImageSourceConverter().ConvertFromString("pack://application:,,,/RoRLauncher;component/Images_Popup/MouseOver_Button_Red_Popup.png") as ImageSource;
        }

        private void CopyToClipboardCanvas_MouseLeave(object sender, MouseEventArgs e)
        {
            this.CopyToClipboardImage.Source = new ImageSourceConverter().ConvertFromString("pack://application:,,,/RoRLauncher;component/Images_Popup/Frame_Button_Red_Popup.png") as ImageSource;
        }
    }
}
