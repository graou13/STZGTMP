using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Data.Objects;
using System.Drawing;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Xml;

namespace WindowsMediaPlayer
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private Playlist playlist;
        private DataTable files = new DataTable();
        private DataView filesOrder;
        private System.Drawing.Image IconMap;
        private bool isFullscreen = false;
        List<int> ColumnListPerID = new List<int> {21, 27, 2, 13, 16, 14, 26, 187, 19, 9}; // do not remove any of those (you can add some if you want, list of id in columns_list.txt)
        List<string> ColumnListPerName = new List<string> { "ID", "URI", "Jaquette"}; // nor these
        public MainWindow()
        {
            InitializeComponent();
            playlist = new Playlist(MediaPlayer, ImagePlayer);
            filesOrder = files.DefaultView;
            playlist.elemsOrder = playlist.elems.DefaultView;
            Debug.WriteLine("begin loading IconMap");
            System.Reflection.Assembly thisExe;
            thisExe = System.Reflection.Assembly.GetExecutingAssembly();
            System.IO.Stream file =
                thisExe.GetManifestResourceStream("WindowsMediaPlayer.Icons.png");
            IconMap = System.Drawing.Image.FromStream(file);
            Debug.WriteLine("finished loading IconMap");
            if (IconMap == null)
                Debug.WriteLine("IconMap is null");
            Debug.WriteLine("begin loading icons");
            StopImg.Source = getIcon(0);
            PlayImg.Source = getIcon(2);
            PrevChaptImg.Source = getIcon(3);
            NextChaptImg.Source = getIcon(4);
            PrevMediaImg.Source = getIcon(5);
            NextMediaImg.Source = getIcon(6);
            RepeatImg.Source = getIcon(7);
            ShuffleImg.Source = getIcon(10);
            PlaylistImg.Source = getIcon(12);
            VolumeImg.Source = getIcon(13);
            FullscreenImg.Source = getIcon(15);
            Debug.WriteLine("begin creating datatable columns");
            files.Clear();
            Shell32.Shell shell = new Shell32.Shell();
            Shell32.Folder objFolder;
            objFolder = shell.NameSpace(@"C:\Windows");
            foreach(int id in ColumnListPerID)
            {
                string header = objFolder.GetDetailsOf(null, id);
                if (String.IsNullOrEmpty(header))
                    break;
                while (files.Columns.Contains(header))
                    header += "_";
                header = header.Replace("'", "_").Replace("’", "_");
                Debug.WriteLine("creating column named " + header);
                files.Columns.Add(header);
            }
            foreach (string name in ColumnListPerName)
            {
                Debug.WriteLine("creating column named " + name);
                files.Columns.Add(name);
            }
            files.Columns["ID"].DataType = Type.GetType("System.Int32");
            files.Columns[objFolder.GetDetailsOf(null, 26).Replace("'", "_").Replace("’", "_")].DataType = Type.GetType("System.Int32");
            //files.Columns["Longueur"].DataType = Type.GetType("System.TimeSpan");
            files.Columns["URI"].DataType = typeof(System.Uri);
            playlist.elems = files.Clone();
            Debug.WriteLine("charging files");
            ProcessLibraries();
            files.AcceptChanges();
            Debug.WriteLine("setting dataContexts");
            FileList.DataContext = playlist.elemsOrder;
            VolumeSlider.DataContext = playlist;
            MediaTimeText.DataContext = playlist;
            MediaTime.DataContext = playlist;
            Debug.WriteLine("end of Constructor");
        }

        [DllImport("shell32.dll")]
        private static extern Int32 SHGetKnownFolderPath([MarshalAs(UnmanagedType.LPStruct)] Guid rfid, UInt32 dwFlags, IntPtr hToken, ref IntPtr ppszPath);

        private void ProcessLibraries()
        {
            IntPtr dirPtr = default(IntPtr);
            string LibDir = null;
            List<string> dirList = new List<string> { };
            List<string> libList = new List<string> { "2112AB0A-C86A-4FFE-A368-0DE96E47012E", "491E922F-5643-4AF4-A7EB-4E7A138D8174", "A990AE9F-A03B-4E80-94BC-9912D7504104" };
            for (int j = 0; j < libList.Count; j++)
            {
                SHGetKnownFolderPath(new Guid(libList[j]), 0, IntPtr.Zero, ref dirPtr);
                LibDir = System.Runtime.InteropServices.Marshal.PtrToStringUni(dirPtr);
                System.Runtime.InteropServices.Marshal.FreeCoTaskMem(dirPtr);
                Debug.WriteLine("[Processing Library " + LibDir + "]");
                using (XmlReader reader = XmlReader.Create(LibDir))
                {
                    while (reader.ReadToFollowing("simpleLocation"))
                    {
                        reader.ReadToFollowing("url");
                        dirList.Add(reader.ReadElementContentAsString());
                    }
                }
                for (int i = 0; i < dirList.Count; i++)
                {
                    if (dirList[i].Contains("knownfolder"))
                    {
                        dirList[i] = dirList[i].Replace("knownfolder:{", "");
                        dirList[i] = dirList[i].Replace("}", "");
                        SHGetKnownFolderPath(new Guid(dirList[i]), 0, IntPtr.Zero, ref dirPtr);
                        dirList[i] = System.Runtime.InteropServices.Marshal.PtrToStringUni(dirPtr);
                        System.Runtime.InteropServices.Marshal.FreeCoTaskMem(dirPtr);
                        ProcessFolder(dirList[i]);
                    }
                    dirList.Clear();
                }
            }
        }

        private void ProcessFolder(string path)
        {
            Debug.WriteLine("    [Processing Folder " + path + "]");
            string[] files = Directory.GetFiles(path);
            foreach (string file in files)
            {
                Shell32.Shell shell = new Shell32.Shell();
                Shell32.Folder objFolder;
                objFolder = shell.NameSpace(@path);
                Shell32.FolderItem item = objFolder.ParseName(System.IO.Path.GetFileName(file));
                if (System.IO.Path.GetFileName(file) != "desktop.ini")
                    ProcessFile(item, objFolder);
            }
            string[] subdirs = Directory.GetDirectories(path);
            foreach (string subdir in subdirs)
                ProcessFolder(subdir);
        }

        private void ProcessFile(Shell32.FolderItem file, Shell32.Folder objFolder)
        {
            Debug.WriteLine("        [Processing File " + file.Path + "]");
            DataRow data = files.NewRow();
            foreach (int id in ColumnListPerID)
            {
                if (id != 26)
                    data[objFolder.GetDetailsOf(null, id).Replace("'", "_").Replace("’", "_")] = objFolder.GetDetailsOf(file, id);
            }
            if (string.IsNullOrEmpty(objFolder.GetDetailsOf(file, 26)))
                data[objFolder.GetDetailsOf(null, 26).Replace("'", "_").Replace("’", "_")] = DBNull.Value;
            else
                data[objFolder.GetDetailsOf(null, 26).Replace("'", "_").Replace("’", "_")] = int.Parse(objFolder.GetDetailsOf(file, 26));
            if (data[objFolder.GetDetailsOf(null, 21).Replace("'", "_").Replace("’", "_")] == "")
                data[objFolder.GetDetailsOf(null, 21).Replace("'", "_").Replace("’", "_")] = System.IO.Path.GetFileName(file.Path);
            data["URI"] = new Uri(file.Path);
            data["ID"] = files.Rows.Count + 1;
            files.Rows.Add(data);
        }

        private BitmapSource getIcon(int Index)
        {
            if (IconMap != null)
            {
                int x = Convert.ToInt32(IconMap.Width) / 30;
                int y = Convert.ToInt32(IconMap.Height) / 30;
                int iconCount = x * y;
                if (Index < 0 || Index >= iconCount) return null;
                int offsetX = (Index % x) * 30;
                int offsetY = (Index / x) * 30;
                Debug.WriteLine("[creating bitmap from index " + Index + "(offsetX=" + offsetX + ", offsetY=" + offsetY + ")]");
                Bitmap bitmap = new Bitmap(30, 30);
                Graphics.FromImage(bitmap).DrawImage(IconMap,
                    new System.Drawing.Rectangle(0, 0, bitmap.Width, bitmap.Height),
                    new System.Drawing.Rectangle(offsetX, offsetY, bitmap.Width, bitmap.Height),
                    GraphicsUnit.Pixel);
                IntPtr hbitmap = bitmap.GetHbitmap();
                BitmapSource bimage;
                bimage = Imaging.CreateBitmapSourceFromHBitmap(
                        hbitmap,
                        IntPtr.Zero,
                        Int32Rect.Empty,
                        BitmapSizeOptions.FromEmptyOptions());
                return bimage;
            }
            return null;
        }

        private void Play_Click(object sender, RoutedEventArgs e)
        {
            if (playlist.elems.Rows.Count > 0)
            {
                playlist.Play();
                PlayImg.Source = getIcon(1);
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
            PlayImg.Source = getIcon(2);
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
            PlayImg.Source = getIcon(2);
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
            playlist.PreviousMedia();
        }

        private void NextMedia_Click(object sender, RoutedEventArgs e)
        {
            playlist.NextMedia();
        }

        private void Repeat_Click(object sender, RoutedEventArgs e)
        {
            switch (playlist.repeatState)
            {
                case 0:
                    playlist.repeatState = 1;
                    RepeatImg.Source = getIcon(8);
                    break;
                case 1:
                    playlist.repeatState = 2;
                    RepeatImg.Source = getIcon(9);
                    break;
                case 2:
                    playlist.repeatState = 0;
                    RepeatImg.Source = getIcon(7);
                    break;
            }
        }

        private void Shuffle_Click(object sender, RoutedEventArgs e)
        {
            if (playlist.isShuffled == false)
                ShuffleImg.Source = getIcon(11);
            else
                ShuffleImg.Source = getIcon(10);
            playlist.isShuffled = !playlist.isShuffled;
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
                VolumeImg.Source = getIcon(13);
            }
            else
            {
                VolumeSlider.IsEnabled = false;
                VolumeImg.Source = getIcon(14);
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
            FileList.DataContext = playlist.elemsOrder;
            FileList.CanUserDeleteRows = true;
            Location.Text = "Liste de lecture";
        }

        private void Display_SavedPlaylists(object sender, RoutedEventArgs e)
        {

        }

        private void Display_Audio(object sender, RoutedEventArgs e)
        {
            FileList.DataContext = filesOrder;
            FileList.CanUserDeleteRows = false;
            filesOrder.RowFilter = "[Type identifié] = 'Audio'";
            Location.Text = "Ma Musique";
        }

        private void Display_Video(object sender, RoutedEventArgs e)
        {
            FileList.DataContext = filesOrder;
            FileList.CanUserDeleteRows = false;
            filesOrder.RowFilter = "[Type identifié] = 'Vidéo'";
            Location.Text = "Mes Vidéos";
        }

        private void Display_Image(object sender, RoutedEventArgs e)
        {
            FileList.DataContext = filesOrder;
            FileList.CanUserDeleteRows = false;
            filesOrder.RowFilter = "[Type identifié] = 'Image'";
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
            //display the same kind of window that you have when you save a website's page or an image and then call playlist.saveplaylist with the path as argument
        }

        private void SortOrder(object sender, RoutedEventArgs e)
        {
            filesOrder.Sort = string.Empty;
            playlist.elemsOrder.Sort = string.Empty;
            MenuItem item = e.Source as MenuItem;
            string sens = item.Name.Substring(item.Name.LastIndexOf('_')).Replace('_', ' ');
            string columnSorted = item.Name.Substring(0, item.Name.LastIndexOf('_')).Replace('_', ' ');
            if (FileList.DataContext == filesOrder)
                filesOrder.Sort = "[" + columnSorted + "]" + sens;
            else if (FileList.DataContext == playlist.elemsOrder)
                playlist.elemsOrder.Sort = "[" + columnSorted + "]" + sens;
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
    }
}
