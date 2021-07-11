using Microsoft.Win32;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;


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

        void DataGrid_LoadingRow(object sender, DataGridRowEventArgs e)
        {
            e.Row.Header = (e.Row.GetIndex()+1).ToString();
        }

        private string GetPathFromDialog()
        {
            OpenFileDialog openDlg = new OpenFileDialog();
            openDlg.InitialDirectory = @"c:\";
            openDlg.Filter = "Файлы ass|*.ass|Файлы srt|*.srt|Файлы mkv|*.mkv";  
            return openDlg.ShowDialog().Value ? openDlg.FileName : string.Empty;
        }

        private void OpenButton_Click(object sender, RoutedEventArgs e)
        {
            txtPath = GetPathFromDialog();

            if (txtPath.Length > 0)
            {
                var extention = System.IO.Path.GetExtension(txtPath);
                if (extention == ".mkv")
                {
                    VideoControl.Source = new Uri(txtPath);
                    VideoControl.Play();
                    VideoControl.Pause();
                }
                subtitleManager = new SubtitleManager(txtPath);
                subs = new ObservableCollection<Subtitle>(subtitleManager.GetSubtitles());
                this.Datagrid.ItemsSource = subs;

                this.startTextBox.Text = "1";
                this.endTextBox.Text = subs.Count().ToString();
                this.shiftTextBox.Text = new TimeSpan(0,0,0,0).ToString();

                InitializeComponent();
            }
        }

        private void TranslateButton_Click(object sender, RoutedEventArgs e)
        {
            //try parse !!!
            int start = int.Parse(startTextBox.Text);
            int end = int.Parse(endTextBox.Text);
            subs = new ObservableCollection<Subtitle>(subtitleManager.TranslateSubtitles(start-1, end-1));

            Datagrid.ItemsSource = subs;

            InitializeComponent();
        }

        private void VideoButton_Click(object sender, RoutedEventArgs e)
        {
            if (txtPath is null || System.IO.Path.GetExtension(txtPath) != ".mkv")
            {
                OpenFileDialog openDlg = new OpenFileDialog();
                openDlg.InitialDirectory = @"c:\";
                if (openDlg.ShowDialog().Value)
                {
                    VideoControl.Source = new Uri(openDlg.FileName);
                    VideoControl.Play();
                    VideoControl.Pause();
                }
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
            InitializeComponent();
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
                        int id = e.Row.GetIndex();
                        var el = e.EditingElement as TextBox;
                        var text = el.Text;
                        subs = new ObservableCollection<Subtitle>(subtitleManager.EditTranslated(id, text));
                    }
                }
            }
        }

        private void Datagrid_SelectedCellsChanged(object sender, SelectedCellsChangedEventArgs e)
        {
            var subtitle = Datagrid.SelectedItem as Subtitle;
            if(subtitle != null)
            {
                TimeSpan t = subtitle.Start;
                VideoControl.Position = t;
                VideoControl.Play();
                VideoControl.Pause();
            }
        }
    }
}
