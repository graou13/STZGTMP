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
using System.Xml.Serialization;
using System.IO;
using System.Windows.Media.Imaging;

namespace                       WindowsMediaPlayer
{
    public class                Playlist
    {
        public DataTable        elems = new DataTable();
        private int             index = 0;
        public int              repeatState = 0;
        public bool             isShuffled = false;
        public bool             isMute = false;
        private bool            isPaused = false;
        public double           oldVolume = 0;
        public bool             allowFullscreen = false;
        private MediaElement    _media;
        private Image           _image;

        public double Volume
        {
            get { return _media.Volume * 100; }
            set { _media.Volume = value / 100; }
        }

        public string TimeText
        {
            get { if (_media.Position.TotalSeconds != 0) { return _media.Position.ToString(@"hh\:mm\:ss"); } else { return "--:--:--"; } }
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
            Volume = 100;
        }

        public bool Play()
        {
            if (this.isPaused)
            {
                this._media.Play();
                this.isPaused = false;
                return (true);
            }
            _image.Source = null;
            _media.Source = null;
            if (index >= elems.Rows.Count && repeatState == 1)
                index = 0;
            else if (index >= elems.Rows.Count)
            {
                allowFullscreen = false;
                return false;
            }
            switch ((string)CurrentMedia["Type identifié"])
            {
                case "Audio":
                    _media.Source = (Uri)CurrentMedia["URI"];
                    break;
                case "Vidéo":
                    _media.Source = (Uri)CurrentMedia["URI"];
                    break;
                case "Image":
                    _image.Source = new BitmapImage((Uri)CurrentMedia["URI"]);
                    break;
            }
            allowFullscreen = true;
            return true;
        }

        public void Pause()
        {
            _media.Pause();
            this.isPaused = true;
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
            if (index > 0)
                index = index - 1;
        }
        
        public void NextMedia(bool wasFail)
        {
            if (index < elems.Rows.Count && (repeatState != 2 || wasFail == true))
                index = index + 1;
        }
        
        public void Repeat()
        {
            repeatState = repeatState + 1;
            repeatState = repeatState % 3;
        }
        
        public void Shuffle()
        {
            this.isShuffled = !this.isShuffled;
        }

        public void LoadPlaylist(string filename)
        {
            DataTable myObject;
            XmlSerializer mySerializer = new XmlSerializer(typeof(DataTable));
            FileStream myFileStream = new FileStream(filename, FileMode.Open);
            myObject = (DataTable) mySerializer.Deserialize(myFileStream);
        }

        public void SavePlayList(string filename)
        {
            this.elems.TableName = filename;
            XmlSerializer ser = new XmlSerializer(typeof(DataTable));
            TextWriter writer = new StreamWriter(filename);
            ser.Serialize(writer, elems);
            writer.Close();
        }
    }
}
