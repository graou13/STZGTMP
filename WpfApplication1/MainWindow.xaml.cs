using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;

namespace                               WindowsMediaPlayer
{
public partial class                MainWindow : Window
    {
        private Playlist                playlist;
        private FFinder                 FFinder;
        private MWInterface             MWInterface;
        private DispatcherTimer         timer;

        private bool                    isFullscreen = false;

        public MainWindow()
        {
            InitializeComponent();
            playlist = new Playlist(ref MediaPlayer, ref ImagePlayer);
            FFinder = new FFinder();
            MWInterface = new MWInterface();
            timer = new System.Windows.Threading.DispatcherTimer();
            this.Height = 720;
            this.Width = 1280;

            this.setIcons();
            this.FFinder.onStartup();
            playlist.elems = this.FFinder.getDataTable();
            FileList.DataContext = playlist.elems.DefaultView;
            VolumeSlider.DataContext = playlist;
            timer.Tick += TimerTick;
        }

        private void                    setIcons()
        {
            StopImg.Source = this.MWInterface.getIcon(0);
            PlayImg.Source = this.MWInterface.getIcon(2);
            PrevChaptImg.Source = this.MWInterface.getIcon(3);
            NextChaptImg.Source = this.MWInterface.getIcon(4);
            PrevMediaImg.Source = this.MWInterface.getIcon(5);
            NextMediaImg.Source = this.MWInterface.getIcon(6);
            RepeatImg.Source = this.MWInterface.getIcon(7);
            ShuffleImg.Source = this.MWInterface.getIcon(10);
            PlaylistImg.Source = this.MWInterface.getIcon(12);
            VolumeImg.Source = this.MWInterface.getIcon(13);
            FullscreenImg.Source = this.MWInterface.getIcon(15);
        }
        
        private void Play_Click(object sender, RoutedEventArgs e)
        {
            if (playlist.elems.Rows.Count > 0)
            {
                timer.Stop();
                if (playlist.Play() == false)
                {
                    WindowState = System.Windows.WindowState.Normal;
                    WindowStyle = System.Windows.WindowStyle.SingleBorderWindow;
                    menu.Visibility = System.Windows.Visibility.Visible;
                    FullscreenImg.Source = this.MWInterface.getIcon(15);
                    return;
                }
                if (!isFullscreen)
                    FullscreenImg.Source = this.MWInterface.getIcon(16);
                else
                    FullscreenImg.Source = this.MWInterface.getIcon(17);
                switch ((string)playlist.CurrentMedia["Type identifié"])
                {
                    case "Audio":
                        VisualStateManager.GoToElementState(this, "ShowImage", false);
                        MediaTime.Maximum = DateTime.ParseExact((string)playlist.CurrentMedia["Longueur"], "hh:mm:ss", CultureInfo.InvariantCulture).TimeOfDay.TotalSeconds;
                        MediaTime.IsEnabled = true;
                        MediaPlayer.Play();
                        timer.Start();
                        break;
                    case "Vidéo":
                        VisualStateManager.GoToElementState(this, "ShowMedia", false);MediaTime.Maximum = DateTime.ParseExact((string)playlist.CurrentMedia["Longueur"], "hh:mm:ss", CultureInfo.InvariantCulture).TimeOfDay.TotalSeconds;
                        MediaTime.IsEnabled = true;
                        MediaPlayer.Play();
                        timer.Start();
                        break;
                    case "Image":
                        VisualStateManager.GoToElementState(this, "ShowImage", false);
                        break;
                }
                
                PlayImg.Source = this.MWInterface.getIcon(1);
                Play.Click += Pause_Click;
                Play.Click -= Play_Click;
                MenuPlay.Header = "Pause";
                MenuPlay.Click += Pause_Click;
                MenuPlay.Click -= Play_Click;
            }
            else
                Debug.WriteLine("browse for file to add /!\\ TO IMPLEMENT");
        }

        private void Stop_Click(object sender, RoutedEventArgs e)
        {
            playlist.Stop();
            timer.Stop();
            PlayImg.Source = this.MWInterface.getIcon(2);
            Play.Click -= Pause_Click;
            Play.Click += Play_Click;
            MenuPlay.Header = "Play";
            MenuPlay.Click -= Pause_Click;
            MenuPlay.Click += Play_Click;
            WindowState = System.Windows.WindowState.Normal;
            WindowStyle = System.Windows.WindowStyle.SingleBorderWindow;
            menu.Visibility = System.Windows.Visibility.Visible;
            isFullscreen = false;
        }

        private void Pause_Click(object sender, RoutedEventArgs e)
        {
            playlist.Pause();
            PlayImg.Source = this.MWInterface.getIcon(2);
            Play.Click -= Pause_Click;
            Play.Click += Play_Click;
            MenuPlay.Header = "Play";
            MenuPlay.Click -= Pause_Click;
            MenuPlay.Click += Play_Click;
        }

        private void PrevChapt_Click(object sender, RoutedEventArgs e)
        {
            playlist.PreviousChapter();
        }

        private void NextChapt_Click(object sender, RoutedEventArgs e)
        {
            playlist.NextChapter();
        }

        private void PrevMedia_Click(object sender, RoutedEventArgs e)
        {
            Stop_Click(sender, e);
            playlist.PreviousMedia();
            Play_Click(sender, e);
        }

        private void NextMedia_Click(object sender, RoutedEventArgs e)
        {
            Stop_Click(sender, e);
            playlist.NextMedia(false);
            Play_Click(sender, e);
        }

        private void Repeat_Click(object sender, RoutedEventArgs e)
        {
            switch (playlist.repeatState)
            {
                case 0:
                    RepeatImg.Source = this.MWInterface.getIcon(8);
                    break;
                case 1:
                    RepeatImg.Source = this.MWInterface.getIcon(9);
                    break;
                case 2:
                    RepeatImg.Source = this.MWInterface.getIcon(7);
                    break;
            }
            playlist.Repeat();
        }

        private void Shuffle_Click(object sender, RoutedEventArgs e)
        {
            if (playlist.isShuffled == false)
                ShuffleImg.Source = this.MWInterface.getIcon(11);
            else
                ShuffleImg.Source = this.MWInterface.getIcon(10);
            playlist.Shuffle();
        }

        private void Fullscreen_Click(object sender, RoutedEventArgs e)
        {
            if (playlist.allowFullscreen && !isFullscreen)
            {
                WindowState = System.Windows.WindowState.Maximized;
                WindowStyle = System.Windows.WindowStyle.None;
                menu.Visibility = System.Windows.Visibility.Collapsed;
            }
            else if (playlist.allowFullscreen)
            {
                WindowState = System.Windows.WindowState.Normal;
                WindowStyle = System.Windows.WindowStyle.SingleBorderWindow;
                menu.Visibility = System.Windows.Visibility.Visible;
            }
            isFullscreen = !isFullscreen;
        }

        private void Volume_Click(object sender, RoutedEventArgs e)
        {
            if (playlist.isMute)
            {
                VolumeSlider.IsEnabled = true;
                VolumeImg.Source = this.MWInterface.getIcon(13);
            }
            else
            {
                VolumeSlider.IsEnabled = false;
                VolumeImg.Source = this.MWInterface.getIcon(14);
            }
            playlist.isMute = !playlist.isMute;
        }

        private void FileList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            FileList.IsReadOnly = false;
        }

        private void FileList_PreparingCellForEdit(object sender, DataGridPreparingCellForEditEventArgs e)
        {
            FileList.IsReadOnly = true;
        }

        private void Display_Playlist(object sender, RoutedEventArgs e)
        {
            VisualStateManager.GoToElementState(this, "ShowFiles", false);
            FileList.DataContext = playlist.elems.DefaultView;
            FileList.CanUserDeleteRows = true;
            Location.Text = "Liste de lecture";
        }

        private void Display_SavedPlaylists(object sender, RoutedEventArgs e)
        {

        }

        private void Display_Audio(object sender, RoutedEventArgs e)
        {
            FileList.DataContext = this.FFinder.getDataView();
            FileList.CanUserDeleteRows = false;
            this.FFinder.setRowFilter("[Type identifié] = 'Audio'");
            Location.Text = "Ma Musique";
        }

        private void Display_Video(object sender, RoutedEventArgs e)
        {
            FileList.DataContext = this.FFinder.getDataView();
            FileList.CanUserDeleteRows = false;
            this.FFinder.setRowFilter("[Type identifié] = 'Vidéo'");
            Location.Text = "Mes Vidéos";
        }

        private void Display_Image(object sender, RoutedEventArgs e)
        {
            FileList.DataContext = this.FFinder.getDataView();
            FileList.CanUserDeleteRows = false;
            this.FFinder.setRowFilter("[Type identifié] = 'Image'");
            Location.Text = "Mes Images";
        }

        private void FileContext_Unchecked(object sender, RoutedEventArgs e)
        {
            MenuItem item = e.Source as MenuItem;
            switch (item.Name)
            {
                case "Durée":
                    DureeColumn.Visibility = System.Windows.Visibility.Collapsed;
                    break;
                case "Artiste":
                    ArtisteColumn.Visibility = System.Windows.Visibility.Collapsed;
                    break;
                case "Genre":
                    GenreColumn.Visibility = System.Windows.Visibility.Collapsed;
                    break;
                case "Album":
                    AlbumColumn.Visibility = System.Windows.Visibility.Collapsed;
                    break;
                case "Numéro_de_piste":
                    NumeroColumn.Visibility = System.Windows.Visibility.Collapsed;
                    break;
                case "Description":
                    DescriptionColumn.Visibility = System.Windows.Visibility.Collapsed;
                    break;
                case "Note":
                    NoteColumn.Visibility = System.Windows.Visibility.Collapsed;
                    break;
                case "ID":
                    IDColumn.Visibility = System.Windows.Visibility.Collapsed;
                    break;
                case "URI":
                    URIColumn.Visibility = System.Windows.Visibility.Collapsed;
                    break;
                case "Jaquette":
                    JaquetteColumn.Visibility = System.Windows.Visibility.Collapsed;
                    break;
            }
        }
        private void FileContext_Checked(object sender, RoutedEventArgs e)
        {
            MenuItem item = e.Source as MenuItem;
            switch (item.Name)
            {
                case "Durée":
                    DureeColumn.Visibility = System.Windows.Visibility.Visible;
                    break;
                case "Artiste":
                    ArtisteColumn.Visibility = System.Windows.Visibility.Visible;
                    break;
                case "Genre":
                    GenreColumn.Visibility = System.Windows.Visibility.Visible;
                    break;
                case "Album":
                    AlbumColumn.Visibility = System.Windows.Visibility.Visible;
                    break;
                case "Numéro de piste":
                    NumeroColumn.Visibility = System.Windows.Visibility.Visible;
                    break;
                case "Description":
                    DescriptionColumn.Visibility = System.Windows.Visibility.Visible;
                    break;
                case "Note":
                    NoteColumn.Visibility = System.Windows.Visibility.Visible;
                    break;
                case "ID":
                    IDColumn.Visibility = System.Windows.Visibility.Visible;
                    break;
                case "URI":
                    URIColumn.Visibility = System.Windows.Visibility.Visible;
                    break;
                case "Jaquette":
                    JaquetteColumn.Visibility = System.Windows.Visibility.Visible;
                    break;
            }
        }

        private void DisplayInfos(object sender, RoutedEventArgs e)
        {
            //display windows with various details that you can edit. two button ok save the new data and close the window; cancel close the window.
        }

        private void OpenFolder(object sender, RoutedEventArgs e)
        {
            List<DataRowView> rowsToAdd = FileList.SelectedItems.Cast<DataRowView>().ToList();
            List<string> paths = new List<string>();
            foreach (DataRowView row in rowsToAdd)
            {
                string path = new Uri(row.Row["URI"].ToString()).LocalPath;
                paths.Add(System.IO.Path.GetDirectoryName(path));
            }
            foreach (string path in paths.Distinct())
                Process.Start(@path);
        }

        private void AddFile(object sender, RoutedEventArgs e)
        {
            List<DataRowView> rowsToAdd = FileList.SelectedItems.Cast<DataRowView>().ToList();
            foreach (DataRowView row in rowsToAdd)
                playlist.elems.Rows.Add(row.Row.ItemArray);
            playlist.elems.AcceptChanges();
        }

        private void SavePlaylist(object sender, RoutedEventArgs e)
        {
            this.playlist.SavePlayList("test");
            //display the same kind of window that you have when you save a website's page or an image and then call playlist.saveplaylist with the path as argument
        }

        private void SortOrder(object sender, RoutedEventArgs e)
        {
            this.FFinder.setSort(string.Empty);
            playlist.elems.DefaultView.Sort = string.Empty;
            MenuItem item = e.Source as MenuItem;
            string sens = item.Name.Substring(item.Name.LastIndexOf('_')).Replace('_', ' ');
            string columnSorted = item.Name.Substring(0, item.Name.LastIndexOf('_')).Replace('_', ' ').Replace('à','°');
            if (FileList.DataContext == this.FFinder.getDataView())
                this.FFinder.setSort("[" + columnSorted + "]" + sens);
            else if (FileList.DataContext == playlist.elems.DefaultView)
                playlist.elems.DefaultView.Sort = "[" + columnSorted + "]" + sens;
        }

        private void IncreaseVolume(object sender, RoutedEventArgs e)
        {
            if (playlist.Volume <= 90)
                playlist.Volume += 10;
        }

        private void EditSpeed(object sender, RoutedEventArgs e)
        {
            MenuItem item = e.Source as MenuItem;
            switch (item.Name.Substring(0, item.Name.IndexOf('_')))
            {
                case "faster":
                    if (playlist.Speed < 16) { playlist.Speed *= 2; }
                    break;
                case "normal":
                    playlist.Speed = 1;
                    break;
                case "slower":
                    if (playlist.Speed > 0.0625) { playlist.Speed /= 2; }
                    break;
            }
        }

        private void MediaFailed(object sender, ExceptionRoutedEventArgs e)
        {
            playlist.NextMedia(true);
        }

        private void MediaTime_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            playlist.Time = MediaTime.Value;
        }

        private void TimerTick(object sender, EventArgs e)
        {
            MediaTime.Value = playlist.Time;
            MediaTimeText.Text = playlist.TimeText;
        }

        private void VolumeSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            playlist.Volume = VolumeSlider.Value;
        }
    }
}
