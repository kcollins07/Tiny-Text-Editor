using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Text.RegularExpressions;
using System.Drawing;
using Splat;
using System.Windows.Media;
using System.Reflection;
using System.Windows.Forms;



namespace Text_Editor
{
    public partial class MainWindow : Window
    {

        public bool saved = false;

        public MainWindow()
        {
            InitializeComponent();
            cmbFontFamily.ItemsSource = Fonts.SystemFontFamilies.OrderBy(f => f.Source);
            cmbFontSize.ItemsSource = new List<Int32>() { 2, 4, 6, 8, 10, 12, 14, 16, 18, 20, 22, 24, 26, 28, 32, 48, 72 };
            rtbEditor.Focus();
        }

        private void CmbFontFamily_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            string fontName = cmbFontFamily.Text;
            var selection = rtbEditor.Selection;

            if (cmbFontFamily.SelectedItem != null)
                rtbEditor.Selection.ApplyPropertyValue(Inline.FontFamilyProperty, cmbFontFamily.SelectedItem);

            //selection.ApplyPropertyValue(TextBlock.FontFamilyProperty, new FontFamily(fontName));
            //TextInput 

            if (selection != null)
            {
                // Check whether there is text selected or just sitting at cursor
                if (selection.IsEmpty)
                {
                    // Get current position of cursor
                    TextPointer curCaret = rtbEditor.CaretPosition;
                    // Get the current block object that the cursor is in
                    Block curBlock = rtbEditor.Document.Blocks.Where
                        (x => x.ContentStart.CompareTo(curCaret) == -1 && x.ContentEnd.CompareTo(curCaret) == 1).FirstOrDefault();
                    if (curBlock != null)
                    {
                        Paragraph curParagraph = curBlock as Paragraph;
                        // Create a new run object with the fontsize, and add it to the current block
                        Run newRun = new Run();
                        newRun.FontFamily = new System.Windows.Media.FontFamily(cmbFontFamily.SelectedItem.ToString());
                        curParagraph.Inlines.Add(newRun);
                        // Reset the cursor into the new block. 
                        // If we don't do this, the font size will default again when you start typing.
                        rtbEditor.CaretPosition = newRun.ElementStart;
                    }
                }
            }

            // Reset the focus onto the richtextbox after selecting the font in a toolbar etc
            rtbEditor.Focus();
        }

        private void CmbFontSize_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

            var selection = rtbEditor.Selection;

            selection.ApplyPropertyValue(TextBlock.FontSizeProperty, (double)int.Parse(cmbFontSize.SelectedItem.ToString()));

            //selection.ApplyPropertyValue(TextBlock.FontFamilyProperty, new FontFamily(fontName));
            //TextInput 

            if (selection != null)
            {
                // Check whether there is text selected or just sitting at cursor
                if (selection.IsEmpty)
                {
                    // Get current position of cursor
                    TextPointer curCaret = rtbEditor.CaretPosition;
                    // Get the current block object that the cursor is in
                    Block curBlock = rtbEditor.Document.Blocks.Where
                        (x => x.ContentStart.CompareTo(curCaret) == -1 && x.ContentEnd.CompareTo(curCaret) == 1).FirstOrDefault();
                    if (curBlock != null)
                    {
                        Paragraph curParagraph = curBlock as Paragraph;
                        // Create a new run object with the fontsize, and add it to the current block
                        Run newRun = new Run();
                        newRun.FontSize = (double)int.Parse(cmbFontSize.SelectedItem.ToString());
                        curParagraph.Inlines.Add(newRun);
                        // Reset the cursor into the new block. 
                        // If we don't do this, the font size will default again when you start typing.
                        rtbEditor.CaretPosition = newRun.ElementStart;
                    }
                }
            }

            // Reset the focus onto the richtextbox after selecting the font in a toolbar etc
            rtbEditor.Focus();
        }

        private void New_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            string text = new TextRange(rtbEditor.Document.ContentStart, rtbEditor.Document.ContentEnd).Text;
            if (String.IsNullOrWhiteSpace(text))
            {
                rtbEditor.SelectAll();

                rtbEditor.Selection.Text = "";
            }
            else
            {
                MessageBoxResult result = System.Windows.MessageBox.Show("Would you like to save your file?", "New File", MessageBoxButton.YesNoCancel);

                switch (result)
                {
                    case MessageBoxResult.Yes:
                        Save_Executed(null, null);
                        saved = false;
                        break;
                    case MessageBoxResult.No:
                        rtbEditor.SelectAll();
                        rtbEditor.Selection.Text = "";
                        saved = false;
                        break;
                    case MessageBoxResult.Cancel:
                        saved = false;
                        break;

                }
            }

        }

        private void Open_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            Microsoft.Win32.OpenFileDialog file = new Microsoft.Win32.OpenFileDialog();

            file.Filter = file.Filter = "Doc Files (*.doc)|*.doc|Rich Text Files (*.rtf)|*.rtf|Text Files (*.txt)|*.txt";
            file.InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);

            if (file.ShowDialog() == true)
            {
                FileStream stream = new FileStream(file.FileName, FileMode.Open);
                TextRange range = new TextRange(rtbEditor.Document.ContentStart, rtbEditor.Document.ContentEnd);
                range.Load(stream, System.Windows.DataFormats.Rtf);
                Title = System.IO.Path.GetFileNameWithoutExtension(file.FileName);
                saved = false;
            }
        }

        private void Save_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            Microsoft.Win32.SaveFileDialog file = new Microsoft.Win32.SaveFileDialog();

            file.Filter = "Doc Files (*.doc)|*.doc|Rich Text Files (*.rtf)|*.rtf|Text Files (*.txt)|*.txt";
            file.InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);

            if (file.ShowDialog() == true)
            {
                FileStream stream = new FileStream(file.FileName, FileMode.Create);
                TextRange range = new TextRange(rtbEditor.Document.ContentStart, rtbEditor.Document.ContentEnd);
                range.Save(stream, System.Windows.DataFormats.Rtf);
                Title = System.IO.Path.GetFileNameWithoutExtension(file.FileName);
                saved = true;
            }
        }

        private void Exit_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            if (saved == false)
            {
                MessageBoxResult result = System.Windows.MessageBox.Show("Would you like to save your file?", "Exit Tiny Text Editor", MessageBoxButton.YesNoCancel);

                switch (result)
                {
                    case MessageBoxResult.Yes:
                        Save_Executed(null, null);
                        System.Windows.Application.Current.Shutdown();                        
                        break;
                    case MessageBoxResult.No:
                        System.Windows.Application.Current.Shutdown();
                        break;
                    case MessageBoxResult.Cancel:
                        break;
                }
            }
            else
            {
                System.Windows.Application.Current.Shutdown();
            }

        }

        private void rtbEditor_SelectionChanged(object sender, RoutedEventArgs e)
        {
            object temp = rtbEditor.Selection.GetPropertyValue(Inline.FontWeightProperty);
            btnBold.IsChecked = (temp != DependencyProperty.UnsetValue) && (temp.Equals(FontWeights.Bold));
            temp = rtbEditor.Selection.GetPropertyValue(Inline.FontStyleProperty);
            btnItalic.IsChecked = (temp != DependencyProperty.UnsetValue) && (temp.Equals(FontStyles.Italic));
            temp = rtbEditor.Selection.GetPropertyValue(Inline.TextDecorationsProperty);
            btnUnderline.IsChecked = (temp != DependencyProperty.UnsetValue) && (temp.Equals(TextDecorations.Underline));

            temp = rtbEditor.Selection.GetPropertyValue(Inline.FontFamilyProperty);
            cmbFontFamily.SelectedItem = temp;
            temp = rtbEditor.Selection.GetPropertyValue(Inline.FontSizeProperty);
            cmbFontSize.Text = temp.ToString();

            temp = rtbEditor.Selection.GetPropertyValue(FlowDocument.ForegroundProperty);
            temp = rtbEditor.Selection.GetPropertyValue(FlowDocument.BackgroundProperty);

            TextRange text = new TextRange(rtbEditor.Document.ContentStart, rtbEditor.Document.ContentEnd);
            string rtbContent = text.Text;
            text.Text = rtbContent.Replace("\r\n", " ");

        }

        private void Toolbar_Loaded(object sender, RoutedEventArgs e)
        {
            System.Windows.Controls.ToolBar toolBar = sender as System.Windows.Controls.ToolBar;
            var overflowGrid = toolBar.Template.FindName("OverflowGrid", toolBar) as FrameworkElement;
            if (overflowGrid != null)
            {
                overflowGrid.Visibility = Visibility.Collapsed;
            }
            var mainPanelBorder = toolBar.Template.FindName("MainPanelBorder", toolBar) as FrameworkElement;
            if (mainPanelBorder != null)
            {
                mainPanelBorder.Margin = new Thickness();
            }
        }

        private void Main_Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            string text = new TextRange(rtbEditor.Document.ContentStart, rtbEditor.Document.ContentEnd).Text;
            if (String.IsNullOrWhiteSpace(text))
            {
                rtbEditor.SelectAll();

                rtbEditor.Selection.Text = "";
            }
            else if (saved == false)
            {
                MessageBoxResult result = System.Windows.MessageBox.Show("Would you like to save your file?", "Exit Tiny Text Editor", MessageBoxButton.YesNoCancel);

                switch (result)
                {
                    case MessageBoxResult.Yes:
                        Save_Executed(null, null);
                        System.Windows.Application.Current.Shutdown();
                        break;
                    case MessageBoxResult.No:
                        System.Windows.Application.Current.Shutdown();
                        break;
                    case MessageBoxResult.Cancel:
                        break;
                }
            }
            else
            {
                System.Windows.Application.Current.Shutdown();
            }
        }

        private void RtbEditor_TextChanged(object sender, TextChangedEventArgs e)
        {
            saved = false;
        }

        private void BtnFontColor_Click(object sender, RoutedEventArgs e)
        {
            var colorDialog = new System.Windows.Forms.ColorDialog();
            if (colorDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                var wpfColor = System.Windows.Media.Color.FromArgb(colorDialog.Color.A, colorDialog.Color.R, colorDialog.Color.G, colorDialog.Color.B);
                TextRange range = new TextRange(rtbEditor.Selection.Start, rtbEditor.Selection.End);
                range.ApplyPropertyValue(FlowDocument.ForegroundProperty, new SolidColorBrush(wpfColor));

                if (range != null)
                {
                    // Check whether there is text selected or just sitting at cursor
                    if (range.IsEmpty)
                    {
                        // Get current position of cursor
                        TextPointer curCaret = rtbEditor.CaretPosition;
                        // Get the current block object that the cursor is in
                        Block curBlock = rtbEditor.Document.Blocks.Where
                            (x => x.ContentStart.CompareTo(curCaret) == -1 && x.ContentEnd.CompareTo(curCaret) == 1).FirstOrDefault();
                        if (curBlock != null)
                        {
                            Paragraph curParagraph = curBlock as Paragraph;
                            // Create a new run object with the fontsize, and add it to the current block
                            Run newRun = new Run();
                            newRun.Foreground = new SolidColorBrush(wpfColor);
                            curParagraph.Inlines.Add(newRun);
                            // Reset the cursor into the new block. 
                            // If we don't do this, the font size will default again when you start typing.
                            rtbEditor.CaretPosition = newRun.ElementStart;
                        }
                    }
                }

                rtbEditor.Focus();

                Color.Fill = new SolidColorBrush(wpfColor);
            }
        }

        private void BtnHighlightColor_Click(object sender, RoutedEventArgs e)
        {
            var colorDialog = new System.Windows.Forms.ColorDialog();
            if (colorDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                var wpfColor = System.Windows.Media.Color.FromArgb(colorDialog.Color.A, colorDialog.Color.R, colorDialog.Color.G, colorDialog.Color.B);
                TextRange range = new TextRange(rtbEditor.Selection.Start, rtbEditor.Selection.End);
                range.ApplyPropertyValue(FlowDocument.BackgroundProperty, new SolidColorBrush(wpfColor));

                if (range != null)
                {
                    // Check whether there is text selected or just sitting at cursor
                    if (range.IsEmpty)
                    {
                        // Get current position of cursor
                        TextPointer curCaret = rtbEditor.CaretPosition;
                        // Get the current block object that the cursor is in
                        Block curBlock = rtbEditor.Document.Blocks.Where
                            (x => x.ContentStart.CompareTo(curCaret) == -1 && x.ContentEnd.CompareTo(curCaret) == 1).FirstOrDefault();
                        if (curBlock != null)
                        {
                            Paragraph curParagraph = curBlock as Paragraph;
                            // Create a new run object with the fontsize, and add it to the current block
                            Run newRun = new Run();
                            newRun.Background = new SolidColorBrush(wpfColor);
                            curParagraph.Inlines.Add(newRun);
                            // Reset the cursor into the new block. 
                            // If we don't do this, the font size will default again when you start typing.
                            rtbEditor.CaretPosition = newRun.ElementStart;
                        }
                    }
                }

                rtbEditor.Focus();

                highlightColor.Fill = new SolidColorBrush(wpfColor);
            }
        }
    }
}
