using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
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


namespace TranslatorWPF
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        ObservableCollection<Subtitle> subs;
        string txtPath;
        SubtitleManager subtitleManager;
        public MainWindow()
        {
            string path = @"..\..\..\..\Translate-aa9400584f1b.json";
            Environment.SetEnvironmentVariable("GOOGLE_APPLICATION_CREDENTIALS", path);

            InitializeComponent();
        }

        private string GetPathFromDialog()
        {
            OpenFileDialog openDlg = new OpenFileDialog();
            openDlg.InitialDirectory = @"c:\";
            openDlg.Filter = "Файлы ass|*.ass|Файлы srt|*.srt|Файлы mkv|*.mkv";
            openDlg.ShowDialog();
            return openDlg.FileName;
        }

        private void OpenButton_Click(object sender, RoutedEventArgs e)
        {
            txtPath = GetPathFromDialog();

            if (txtPath.Length == 0)
            {
                OpenWindow openWindow = new OpenWindow();
                if (openWindow.ShowDialog() == true)
                {
                    if (openWindow.Path.Length > 0)
                    {
                        txtPath = openWindow.Path;
                    }
                    else
                        MessageBox.Show("No path");
                }
                else
                {
                    MessageBox.Show("No path");
                }
            }

            if (txtPath.Length > 0)
            {
                subtitleManager = new SubtitleManager(txtPath);
                subs = new ObservableCollection<Subtitle>(subtitleManager.GetSubtitles());
                this.Datagrid.ItemsSource = subs;

                this.startTextBox.Text = subs.Min(a => a.Id).ToString();
                this.endTextBox.Text = subs.Max(a => a.Id).ToString();
                this.shiftTextBox.Text = new TimeSpan(0,0,0,0).ToString();

                InitializeComponent();
            }
        }

        private void TranslateButton_Click(object sender, RoutedEventArgs e)
        {
            //try parse !!!
            int start = int.Parse(startTextBox.Text);
            int end = int.Parse(endTextBox.Text);
            subs = new ObservableCollection<Subtitle>(subtitleManager.TranslateSubtitles(start, end));

            this.Datagrid.ItemsSource = subs;

            InitializeComponent();
        }

        private void VideoButton_Click(object sender, RoutedEventArgs e)
        {
            if (txtPath is null || System.IO.Path.GetExtension(txtPath) != ".mkv")
            {
                OpenFileDialog openDlg = new OpenFileDialog();
                openDlg.InitialDirectory = @"c:\";
                openDlg.ShowDialog();
                VideoControl.Source = new Uri(openDlg.FileName);
                VideoControl.Play();
                VideoControl.Pause();
            }
            else
            {
                VideoControl.Source = new Uri(txtPath);
                VideoControl.Play();
                VideoControl.Pause();
            }
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            subtitleManager.SaveSubtitles();
        }

        private void StartButton_Click(object sender, RoutedEventArgs e)
        {
            VideoControl.Play();
        }

        private void PauseButton_Click(object sender, RoutedEventArgs e)
        {
            VideoControl.Pause();
        }

        private void StopButton_Click(object sender, RoutedEventArgs e)
        {
            VideoControl.Stop();
        }

        private void ShiftButton_Click(object sender, RoutedEventArgs e)
        {
            //try parse !!!
            TimeSpan shift = TimeSpan.Parse(shiftTextBox.Text);
            if (forwardButton.IsChecked == true)
            {
                subs = new ObservableCollection<Subtitle>(subtitleManager.ShiftTime(shift, true));
            }
            else if (backButton.IsChecked == true)
            {
                subs = new ObservableCollection<Subtitle>(subtitleManager.ShiftTime(shift, false));
            }
        }
        void DG_CellEditEnding(object sender, DataGridCellEditEndingEventArgs e)
        {
            if (e.EditAction == DataGridEditAction.Commit)
            {
                var column = e.Column as DataGridBoundColumn;
                if (column != null)
                {
                    var bindingPath = (column.Binding as Binding).Path.Path;
                    if (bindingPath == "Translated")
                    {
                        int id = e.Row.GetIndex() + 1;
                        var el = e.EditingElement as TextBox;
                        var text = el.Text;
                        subs = new ObservableCollection<Subtitle>(subtitleManager.EditTranslated(id, text));
                    }
                }
            }
        }
        void DG_BeginningEdit(object sender, DataGridBeginningEditEventArgs e)
        {
            var row = e.Row;
            int id = row.GetIndex();
            Subtitle s = row.DataContext as Subtitle;
            TimeSpan t = s.Start;
            VideoControl.Position = t;
            VideoControl.Play();
            VideoControl.Pause();
        }
    }
}
