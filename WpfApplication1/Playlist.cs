using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Data;
using System.Data.Objects;
using System.Globalization;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;

namespace WindowsMediaPlayer
{
    public class Playlist
    {
        public DataTable elems = new DataTable();
        public DataView elemsOrder;
        private int index = 0;
        public int repeatState = 0;
        public bool isShuffled = false;
        public bool isMute = false;
        public bool allowFullscreen = false;
        private MediaElement _media;
        private Image _image;
        private MainWindow _window;

        private void initVideo()
        {

        }

        private void initMusic()
        {

        }

        private void initImage()
        {

        }

        public double Volume
        {
            get { return _media.Volume; }
            set { _media.Volume = value; }
        }

        public string TimeText
        {
            get { if (_media.Position.TotalSeconds != 0) { return _media.Position.ToString("hh:mm:ss"); } else { return "--:--:--"; } }
            set { _media.Position = DateTime.ParseExact(value, "hh:mm:ss", CultureInfo.InvariantCulture).TimeOfDay; }
        }

        public double Time
        {
            get { return _media.Position.TotalSeconds; }
            set { _media.Position = TimeSpan.FromSeconds(value); }
        }

        public double Speed
        {
            get { return _media.SpeedRatio; }
            set { _media.SpeedRatio = value; }
        }

        public DataRow CurrentMedia
        {
            get { return elems.Rows[index]; }
        }

        public Playlist(ref MediaElement media, ref Image image)
        {
            _media = media;
            _image = image;
            _window = (MainWindow)Application.Current.MainWindow;
            Volume = 100;
        }

        public bool Play()
        {
            _media.Source = null;
            _image.Source = null;
            if (index >= elems.Rows.Count)
            {
                allowFullscreen = false;
                if (_window.isFullscreen == true)
                    _window.Fullscreen_Click(null, null);
                _window.VolumeImg.Source = _window.getIcon(13);
                return false;
            }
            switch ((string)elems.Rows[index]["Type identifié"])
            {
                case "Audio":
                    if (VisualStateManager.GoToState(_window, "ShowImage", false))
                        Debug.WriteLine("Switched to ShowImage");
                    _media.Source = (Uri)elems.Rows[index]["URI"];
                    _media.Play();
                    break;
                case "Vidéo":
                    if (VisualStateManager.GoToState(_window, "ShowMedia", false))
                        Debug.WriteLine("Switched to ShowMedia");
                    _media.Source = (Uri)elems.Rows[index]["URI"];
                    _media.Play();
                    break;
                case "Image":
                    if (VisualStateManager.GoToState(_window, "ShowImage", false))
                        Debug.WriteLine("Switched to ShowImage");
                    _image.Source = new BitmapImage((Uri)elems.Rows[index]["URI"]);
                    break;
                default:
                    return false;
            }
            _window.MediaTime.IsEnabled = true;
            allowFullscreen = true;
            if (_window.isFullscreen == false)
                _window.VolumeImg.Source = _window.getIcon(14);
            return true;
        }

        public void Pause()
        {
            _media.Pause();
        }
        
        public void Stop()
        {
            _media.Stop();
        }
        
        public void PreviousChapter()
        {
            
        }
        
        public void NextChapter()
        {

        }
        
        public void PreviousMedia()
        {
            Stop();
            if (index > 0)
                index--;
            Play();
        }
        
        public void NextMedia()
        {
            Stop();
            if (index < elems.Rows.Count)
                index++;
            Play();
        }
        
        public void Repeat()
        {

        }
        
        public void Shuffle()
        {

        }

        public void LoadPlaylist(string name)
        {

        }

        public void SavePlayList(string name)
        {

        }
    }
}
