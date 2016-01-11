using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Data;
using System.Data.Objects;
using System.Globalization;
using System.Threading.Tasks;
using System.Windows.Controls;

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

        public Playlist(MediaElement media, Image image)
        {
            _media = media;
            _image = image;
            Volume = 100;
        }

        public void Play()
        {
            foreach (DataRow row in elems.Rows)
            {
                Debug.WriteLine("displaying playlist row");
                foreach (DataColumn column in elems.Columns)
                    Debug.WriteLine("    " + column.ColumnName + "=" + row[column]);
            }
        }

        public void Pause()
        {
            
        }
        
        public void Stop()
        {

        }
        
        public void PreviousChapter()
        {

        }
        
        public void NextChapter()
        {

        }
        
        public void PreviousMedia()
        {

        }
        
        public void NextMedia()
        {

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
