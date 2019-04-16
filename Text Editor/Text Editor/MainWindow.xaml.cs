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
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Text_Editor
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            cmbFontFamily.ItemsSource = Fonts.SystemFontFamilies.OrderBy(f => f.Source);
            cmbFontSize.ItemsSource = new List <Int32>() { 2, 4, 6, 8, 10, 12, 14, 16, 18, 20, 22, 24, 26, 28, 32, 48, 72 };
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
                        newRun.FontFamily = new FontFamily(cmbFontFamily.SelectedItem.ToString());
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
                MessageBoxResult result = MessageBox.Show("Would you like to save your file?", "New File", MessageBoxButton.YesNoCancel);

                switch (result)
                {
                    case MessageBoxResult.Yes:
                        Save_Executed(null, null);
                        break;
                    case MessageBoxResult.No:
                        rtbEditor.SelectAll();
                        rtbEditor.Selection.Text = "";
                        break;
                    case MessageBoxResult.Cancel:
                        break;

                }
            }
        }

        private void Open_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            OpenFileDialog file = new OpenFileDialog();

            file.Filter = file.Filter = "Doc Files (*.doc)|*.doc|Rich Text Files (*.rtf)|*.rtf|Text Files (*.txt)|*.txt";
            file.InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);

            if (file.ShowDialog() == true)
            {
                FileStream stream = new FileStream(file.FileName, FileMode.Open);
                TextRange range = new TextRange(rtbEditor.Document.ContentStart, rtbEditor.Document.ContentEnd);
                range.Load(stream, DataFormats.Rtf);
            }
        }

        private void Save_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            SaveFileDialog file = new SaveFileDialog();

            file.Filter = "Doc Files (*.doc)|*.doc|Rich Text Files (*.rtf)|*.rtf|Text Files (*.txt)|*.txt";
            file.InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);

            if (file.ShowDialog() == true)
            {
                FileStream stream = new FileStream(file.FileName, FileMode.Create);
                TextRange range = new TextRange(rtbEditor.Document.ContentStart, rtbEditor.Document.ContentEnd);
                range.Save(stream, DataFormats.Rtf);
            }
        }

        private void Exit_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            string text = new TextRange(rtbEditor.Document.ContentStart, rtbEditor.Document.ContentEnd).Text;
            if (String.IsNullOrWhiteSpace(text))
            {
                rtbEditor.SelectAll();

                rtbEditor.Selection.Text = "";
            }
            else
            {
                MessageBoxResult result = MessageBox.Show("Would you like to save your file?", "Exit Tiny Text Editor", MessageBoxButton.YesNoCancel);

                switch (result)
                {
                    case MessageBoxResult.Yes:
                        Save_Executed(null, null);
                        Application.Current.Shutdown();
                        break;
                    case MessageBoxResult.No:
                        Application.Current.Shutdown();
                        break;
                    case MessageBoxResult.Cancel:
                        break;


                }
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
        }

        private void Toolbar_Loaded(object sender, RoutedEventArgs e)
        {
            ToolBar toolBar = sender as ToolBar;
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
            else
            {
                MessageBoxResult result = MessageBox.Show("Would you like to save your file?", "Exit Tiny Text Editor", MessageBoxButton.YesNoCancel);

                switch (result)
                {
                    case MessageBoxResult.Yes:
                        Save_Executed(null, null);
                        break;
                    case MessageBoxResult.No:
                        Application.Current.Shutdown();
                        break;
                    case MessageBoxResult.Cancel:
                        e.Cancel = true;
                        break;
                        

                }
            }
        }
    }
}
