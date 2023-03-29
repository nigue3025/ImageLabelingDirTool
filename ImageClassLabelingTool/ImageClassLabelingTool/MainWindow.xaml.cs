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
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Drawing;

namespace ImageClassLabelingTool
{
    /// <summary>
    /// MainWindow.xaml 的互動邏輯
    /// </summary>
    /// 
    public class ImageFileData : INotifyPropertyChanged
    {
        string fullpath = null;
        string filename = null;
        string category = null;
        public event PropertyChangedEventHandler PropertyChanged;
        public ImageFileData(string path)
        {
            this.fullpath = path;
            this.filename = Fullpath.Split('\\').Last();

        }
        public string Fullpath
        {
            get
            {
                return fullpath;
            }
            set
            {
                fullpath = value;
                NotifyPropertyChanged();
            }
        }
        public string Filename
        {
            get
            {
                return filename;
            }
            set
            {
                filename = value;
                NotifyPropertyChanged();
            }
        }
        public string Category
        {
            get
            {
                return category;
            }
            set
            {
                category = value;
                NotifyPropertyChanged();
            }
        }
        private void NotifyPropertyChanged([CallerMemberName] String propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }



    public class CategorizedLabel:INotifyPropertyChanged
    {
        string className;
        public event PropertyChangedEventHandler PropertyChanged;

        public string ClassName
        {
            get
            {
                return className;
            }
            set
            {
                className=value;
                NotifyPropertyChanged();
            }
        }

        private void NotifyPropertyChanged([CallerMemberName] String propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }


    public partial class MainWindow : Window
    {
        List<ImageFileData> files = new List<ImageFileData>();
        List<CategorizedLabel> classLabels = new List<CategorizedLabel>();

        

        string FolderPath
        {
            get;set;
        }
        string FileExtension
        {
            get;set;
        }
        public MainWindow()
        {
            InitializeComponent();
        }
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            //lstView_class.ItemsSource = null;
            lstView_class.ItemsSource = classLabels; 
           // lstbx_fileLst.ItemsSource = null;
            lstbx_fileLst.ItemsSource = files;
            cmbx_imageClassConfirm.ItemsSource = classLabels;
        }
        private void btn_confirmFolder_Click(object sender, RoutedEventArgs e)
        {
            FolderPath = txtbx_folderpath.Text;
            FileExtension = txtbx_fileExtension.Text;

            var loaded = System.IO.Directory.GetFiles(FolderPath).ToList().Select(str => new ImageFileData(str)).Where(a => a.Filename.Split('.').Last() == txtbx_fileExtension.Text).ToList();
            files.Clear();
            files.AddRange(loaded);
            //files = System.IO.Directory.GetFiles(FolderPath).ToList().Select(str => new ImageFileData(str)).ToList();
            lstbx_fileLst.Items.Refresh();
            cmbx_imageClassConfirm.Items.Refresh();


        }

    

        private void lstbx_fileLst_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (e.AddedItems.Count == 0) return;

            var selected = e.AddedItems[0];
            if (selected is ImageFileData)
            {

               // BitmapImage Bimg = new BitmapImage(new Uri((selected as ImageFileData).Fullpath));
               //// Bimg.BeginInit();
               // //Bimg.UriSource = source;
               // Bimg.CacheOption = BitmapCacheOption.OnLoad;
               // //Bimg.EndInit();
               // img_show.Source = Bimg;


                BitmapImage bitmapImage = new BitmapImage();
                bitmapImage.BeginInit();
                bitmapImage.CacheOption = BitmapCacheOption.OnLoad;
                bitmapImage.UriSource = new Uri((selected as ImageFileData).Fullpath);//szPath为图片的全路径
                bitmapImage.EndInit();
                bitmapImage.Freeze();
                img_show.Source = bitmapImage;
            }
        }

        private void btn_addClass_Click(object sender, RoutedEventArgs e)
        {
            
            classLabels.Add(new CategorizedLabel() { ClassName = txtbx_className.Text });

            lstView_class.Items.Refresh();
            cmbx_imageClassConfirm.Items.Refresh();
        }

        private void btn_removeClass_Click(object sender, RoutedEventArgs e)
        {
            var selected = lstView_class.SelectedItem;
            if (selected is CategorizedLabel)
            {
                classLabels.Remove(selected as CategorizedLabel);
                lstView_class.Items.Refresh();
            }
        }

        private void StackPanel_KeyUp(object sender, KeyEventArgs e)
        {
            btn_confirmClass_Click(this, null);
        }

        private void btn_confirmClass_Click(object sender, RoutedEventArgs e)
        {
            if (lstbx_fileLst.SelectedItem is ImageFileData)
            {
                int lastIndex = lstbx_fileLst.SelectedIndex;
                img_show.Source = null;
                
                var selectedFile = (lstbx_fileLst.SelectedItem as ImageFileData);
                var flPathLst= selectedFile.Fullpath.Split('\\').ToList();
                flPathLst=flPathLst.Where((a, i) => i < flPathLst.Count - 1).Select(a=>a).ToList();
                var curDir = string.Join("\\", flPathLst);

                var newPath = $"{curDir}\\{ (cmbx_imageClassConfirm.SelectedItem as CategorizedLabel).ClassName}\\{selectedFile.Filename}";
                System.IO.File.Move(selectedFile.Fullpath,newPath );
                files.Remove(selectedFile);
                
                lstbx_fileLst.Items.Refresh();
                lstbx_fileLst.SelectedIndex = lastIndex;
                lstbx_fileLst.Focus();
            }

            
        }

        private void lstbx_fileLst_KeyDown(object sender, KeyEventArgs e)
        {
            //目前只能標1~9個類別 (可由鍵盤1~9直接標示對應的類別順序)
            int keyVal = (int)e.Key;
            int numKey = keyVal - 35;
            if (numKey >= 0 && numKey <= 8)
                if (numKey < lstView_class.Items.Count)
                {
                    lstView_class.SelectedIndex = numKey;
                    var selectedClass = lstView_class.Items[numKey] as CategorizedLabel;
                    (lstbx_fileLst.SelectedItem as ImageFileData).Category = selectedClass.ClassName;

                }


            //if(e.Key== Key.Enter)
            //{
            //    btn_confirmClass_Click(this, null);
            //}
        }

        private void lstbx_fileLst_KeyUp(object sender, KeyEventArgs e)
        {
           
        }

        private void btn_moveFile_Click(object sender, RoutedEventArgs e)
        {
            HashSet<ImageFileData> movedFileLst = new HashSet<ImageFileData>();
            foreach(var selectedFile in files)
            {
                if (selectedFile.Category == null) continue;

                var flPathLst = selectedFile.Fullpath.Split('\\').ToList();
                flPathLst = flPathLst.Where((a, i) => i < flPathLst.Count - 1).Select(a => a).ToList();
                var curDir = string.Join("\\", flPathLst);

                var newPath = $"{curDir}\\{ selectedFile.Category}\\{selectedFile.Filename}";
                System.IO.File.Move(selectedFile.Fullpath, newPath);
                movedFileLst.Add(selectedFile);
            }
            foreach (var fl in movedFileLst)
                files.Remove(fl);

            lstbx_fileLst.Items.Refresh();
            movedFileLst.Clear();

       
        }
    }
}
