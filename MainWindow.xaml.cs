using System;
using System.Collections.Generic;
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
using CommonMark;
using Microsoft.Win32;
using System.IO;

namespace SimpleMarkdownEditor
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private string currentFile;
        private bool isfirst = false;
        private bool issave = true;
        bool isinit = false;
        public MainWindow()
        {
            InitializeComponent();
            isfirst = false;
            issave = true;
            isinit = true;
            if (!File.Exists(".\\style.css"))
            {
                _ = File.Create(".\\style.css");
            }
        }

        private void Undo_Click(object sender, RoutedEventArgs e)
        {
            _ = textEditor.Undo();
        }

        private void Redo_Click(object sender, RoutedEventArgs e)
        {
            _ = textEditor.Redo();
        }

        private void Copy_Click(object sender, RoutedEventArgs e)
        {
            textEditor.Copy();
        }

        private void Cut_Click(object sender, RoutedEventArgs e)
        {
            textEditor.Cut();
        }

        private void Paste_Click(object sender, RoutedEventArgs e)
        {
            textEditor.Paste();
        }

        private void TextBox_GotFocus(object sender, RoutedEventArgs e)
        {
            TextBox senderObject = (TextBox)sender;
            if (senderObject.Text.Equals("Search..."))
            {
                senderObject.Text = "";
            }
        }

        private void TextBox_LostFocus(object sender, RoutedEventArgs e)
        {
            TextBox senderObject = (TextBox)sender;
            if (senderObject.Text.Equals(""))
            {
                senderObject.Text = "Search...";
            }
        }

        private void Find_Click(object sender, RoutedEventArgs e)
        {
            TextRange range = new TextRange(textEditor.Document.ContentStart, textEditor.Document.ContentEnd);
            string pattern = SearchQuery.Text;
            List<TextRange> list = new List<TextRange>();
            range.ApplyPropertyValue(TextElement.BackgroundProperty, null);

            foreach (Paragraph paragraph in textEditor.Document.Blocks)
            {
                foreach (Inline inline in paragraph.Inlines)
                {
                    TextPointer start = inline.ContentStart;
                    TextPointer end = inline.ContentEnd;
                    TextRange r = new TextRange(start, end);
                    string src = r.Text;
                    int startIndex = 0;
                    int index = src.IndexOf(pattern, startIndex);

                    while (index != -1 && !src.Equals("") && !pattern.Equals(""))
                    {
                        TextRange tmp = new TextRange(start.GetPositionAtOffset(index),
                            start.GetPositionAtOffset(index + pattern.Length));
                        startIndex = index + pattern.Length;
                        index = src.IndexOf(pattern, startIndex);
                        list.Add(tmp);
                    }
                }
            }
            range.ApplyPropertyValue(TextElement.BackgroundProperty, null);

            foreach (TextRange textRange in list)
            {
                textRange.ApplyPropertyValue(TextElement.BackgroundProperty, "#66FFFF00");
            }
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            SearchGrid.Visibility = Visibility.Collapsed;
            textEditor.Margin = new Thickness(0, 20, 0, 0);
            TextRange range = new TextRange(textEditor.Document.ContentStart, textEditor.Document.ContentEnd);
            range.ApplyPropertyValue(TextElement.BackgroundProperty, null);
        }

        private void MenuItem_Find_Click(object sender, RoutedEventArgs e)
        {
            SearchGrid.Visibility = Visibility.Visible;
            textEditor.Margin = new Thickness(0, 20, 0, 40);
            SearchQuery.Focus();
        }

        private void New_Click(object sender, RoutedEventArgs e)
        {
            MessageBoxResult result = MessageBoxResult.No;
            if (isfirst)
            {
                result = MessageBox.Show("Do you want to save the open file?", "",
                  MessageBoxButton.YesNoCancel, MessageBoxImage.Question);
            }
            if (result == MessageBoxResult.Yes) 
            {
                Save_Click(sender, e);
                textEditor.Document.Blocks.Clear();
                this.Title = "SimpleMarkdownEditor";
            }
            else if (result == MessageBoxResult.No)
            {
                textEditor.Document.Blocks.Clear();
                this.Title = "SimpleMarkdownEditor";
            }
        }

        private void Open_Click(object sender, RoutedEventArgs e)
        {
            New_Click(sender, e);
            OpenFileDialog openFileDialog = new OpenFileDialog
            {
                Filter = "Markdown files (*.md;*.markdown)|*.md;*.markdown|All files|*.*"
            };
            if (openFileDialog.ShowDialog() == true)
            {
                currentFile = openFileDialog.FileName;
                textEditor.Document.Blocks.Clear();
                textEditor.Document.Blocks.Add(new Paragraph(new Run(File.ReadAllText(openFileDialog.FileName))));
                this.Title = currentFile;
                issave = true;
            }

        }

        private void Save_Click(object sender, RoutedEventArgs e)
        {
            issave = true;
            if (currentFile == "" || currentFile == null)
            {
                SaveAs_Click(sender, e);
            }
            else
            {
                File.WriteAllText(currentFile,
                    new TextRange(textEditor.Document.ContentStart, textEditor.Document.ContentEnd).Text);
            }
        }

        private void Exit_Click(object sender, RoutedEventArgs e)
        {
            System.ComponentModel.CancelEventArgs ei = new System.ComponentModel.CancelEventArgs { Cancel = false };
            Window_Closing(sender, ei);
        }

        private void SaveAs_Click(object sender, RoutedEventArgs e)
        {
            issave = true;
            SaveFileDialog saveFileDialog = new SaveFileDialog
            {
                Filter = "Markdown files (*.md;*.markdown)|*.md;*.markdown|All files|*.*"
            };
            if (saveFileDialog.ShowDialog() == true)
            {
                File.WriteAllText(saveFileDialog.FileName,
                    new TextRange(textEditor.Document.ContentStart, textEditor.Document.ContentEnd).Text);
            }
        }

        private void textEditor_TextChanged(object sender, TextChangedEventArgs e)
        {
            TextRange text = new TextRange(textEditor.Document.ContentStart, textEditor.Document.ContentEnd);
            isfirst = true;
            issave = false;
            if (isinit)
            {
                Markdown.NavigateToString("<!DOCTYPE html>\r\n<html>\r\n<head>\r\n<meta charset=\"utf-8\">\r\n<title>"
                    + ((currentFile == "" || currentFile == null) ? "Untitled" : currentFile)
                    + "</title>\r\n</head>\r\n<body>\r\n<style>\r\n"
                    + File.ReadAllText(".\\style.css")
                    + "\r\n</style>\r\n"
                    + CommonMarkConverter.Convert(text.Text)
                    + "</body>\r\n</html>");
            }
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            MessageBoxResult result = MessageBoxResult.No;
            if (!issave)
            {
                result = MessageBox.Show("Do you want to save the open file?", "",
                MessageBoxButton.YesNoCancel, MessageBoxImage.Question);
            }
            if (result == MessageBoxResult.Yes)
            {
                RoutedEventArgs ei = new RoutedEventArgs();
                SaveAs_Click(sender, ei);
            }
            else if (result == MessageBoxResult.Cancel)
            {
                e.Cancel = true;
            }
        }

        private void HTML_Click(object sender, RoutedEventArgs e)
        {
            SaveFileDialog saveFileDialog = new SaveFileDialog
            {
                Filter = "HTML files (*.html;*.htm)|*.html;*.htm"
            };
            if (saveFileDialog.ShowDialog() == true)
            {
                File.WriteAllText(saveFileDialog.FileName,"<!DOCTYPE html>\r\n<html>\r\n<head>\r\n<meta charset=\"utf-8\">\r\n<title>"
                    + ((currentFile == "" || currentFile == null) ? "Untitled" : currentFile)
                    + "</title>\r\n</head>\r\n<body>\r\n<style>\r\n"
                    + File.ReadAllText(".\\style.css")
                    + "\r\n</style>\r\n"
                    + CommonMarkConverter.Convert(new TextRange(textEditor.Document.ContentStart, textEditor.Document.ContentEnd).Text)
                    + "</body>\r\n</html>");
            }
        }

        private void Markdown_Navigating(object sender, NavigatingCancelEventArgs e)
        {
            if (e.Uri != null)
            {
                MessageBoxResult result = MessageBox.Show("URL :  " + e.Uri.ToString().Replace("about:", "") + "\nDo you want to copy?", "",
                MessageBoxButton.OKCancel, MessageBoxImage.Question);
                e.Cancel = true;
                if (result == MessageBoxResult.OK)
                {
                Clipboard.SetText(e.Uri.ToString().Replace("about:", ""));
                }
            }
            
        }

        private void CSS_Click(object sender, RoutedEventArgs e)
        {
            System.Diagnostics.Process process = new System.Diagnostics.Process
            {
                StartInfo = new System.Diagnostics.ProcessStartInfo
                {
                    FileName = "notepad.exe",
                    Arguments = AppDomain.CurrentDomain.BaseDirectory + "style.css"
                }
            };
            process.Start();
        }
    }
}
