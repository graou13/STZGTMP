using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace                               WindowsMediaPlayer
{
    class                               FFinder
    {
        private DataTable               files = new DataTable();
        private DataView                filesOrder;

        private List<int>               ColumnListPerID = new List<int> { 21, 27, 2, 13, 16, 14, 26, 187, 19, 9 };
        private List<string>            ColumnListPerName = new List<string> { "ID", "URI", "Jaquette" };

        public                          FFinder()
        {
            this.filesOrder = files.DefaultView;
        }

        public DataTable                getDataTable()
        {
            return (this.files.Clone());
        }

        public DataView                 getDataView()
        {
            return (this.filesOrder);
        }

        public void                     setRowFilter(string filter)
        {
            this.filesOrder.RowFilter = filter;
        }

        public void                     setSort(string sort)
        {
            this.filesOrder.Sort = sort;
        }

        public void                     onStartup()
        {
            Shell32.Shell               shell = new Shell32.Shell();
            Shell32.Folder              objFolder = shell.NameSpace(@"C:\Windows");

            this.files.Clear();
            foreach (string name in ColumnListPerName)
                this.files.Columns.Add(name);
            foreach (int id in ColumnListPerID)
            {
                string header = objFolder.GetDetailsOf(null, id);
                if (String.IsNullOrEmpty(header))
                    break;
                while (this.files.Columns.Contains(header))
                    header += "_";
                header = header.Replace("'", "_").Replace("’", "_");
                Debug.WriteLine("creating column named " + header);
                this.files.Columns.Add(header);
            }

            this.files.Columns["ID"].DataType = Type.GetType("System.Int32");
            this.files.Columns[objFolder.GetDetailsOf(null, 26).Replace("'", "_").Replace("’", "_")].DataType = Type.GetType("System.Int32");
            //this.files.Columns["Longueur"].DataType = Type.GetType("System.TimeSpan");
            this.files.Columns["URI"].DataType = typeof(System.Uri);
            ProcessLibraries();
            this.files.AcceptChanges();
        }

        [DllImport("shell32.dll")]
        private static extern Int32     SHGetKnownFolderPath([MarshalAs(UnmanagedType.LPStruct)] Guid rfid, UInt32 dwFlags, IntPtr hToken, ref IntPtr ppszPath);

        public void                     ProcessLibraries()
        {
            IntPtr                      dirPtr = default(IntPtr);
            string                      LibDir = null;
            List<string>                dirList = new List<string> { };
            List<string>                libList = new List<string> { "2112AB0A-C86A-4FFE-A368-0DE96E47012E", "491E922F-5643-4AF4-A7EB-4E7A138D8174", "A990AE9F-A03B-4E80-94BC-9912D7504104" };

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

        public void                     ProcessFolder(string path)
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

        private void                    ProcessFile(Shell32.FolderItem file, Shell32.Folder objFolder)
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
            if ((string) data[objFolder.GetDetailsOf(null, 21).Replace("'", "_").Replace("’", "_")] == "")
                data[objFolder.GetDetailsOf(null, 21).Replace("'", "_").Replace("’", "_")] = System.IO.Path.GetFileName(file.Path);
            data["URI"] = new Uri(file.Path);
            data["ID"] = files.Rows.Count + 1;
            files.Rows.Add(data);
        }
    }
}
