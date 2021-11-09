using System.Collections.Generic;
using System.Windows;
using System.IO;
using System.Windows.Input;
using Microsoft.Win32;
using System.Windows.Documents;
using System.Text;
using System;
using System.Speech.Synthesis;
using System.Xml.Linq;
using System.Xml;
using System.Windows.Media.Imaging;
using System.Windows.Controls;
using System.Windows.Media;
using System.Text.RegularExpressions;
using System.Linq;

namespace Project11
{
    public partial class MainWindow : Window
    {
        private bool _currStyle;
        SpeechSynthesizer speechSynthesizer = new SpeechSynthesizer();
        public List<Book> bookList { get; private set; } = new List<Book>(); //список книг
        public bool OpenList { get; set; }
        /*=============для формата epub====================================*/
        private string _tempPath;
        private string _baseMenuXmlDiretory;
        private List<string> _menuItems;
        private int _currentPage;
        /*=================================================================*/
        public MainWindow()
        {
            InitializeComponent();
            fromFile();//считываем сохранённые адресса в список
            bookListBox.MouseDoubleClick += new MouseButtonEventHandler(bookListBox_DoublClick);
            OpenList = false;
            _menuItems = new List<string>();
        }

        private void OpenCommand_Executed(object sender, ExecutedRoutedEventArgs e)//обработичк кнопки Открыть
        {
            OpenFileDialog openFileDialog = new OpenFileDialog
            {
                Filter = "TXT files(*.txt;*.fb2;*.epub) | *.txt; *.fb2; *.epub"
            };

            if (openFileDialog.ShowDialog() == true)
            {
                string path = openFileDialog.FileName;
                string _data = "Oops something wrong";

                flowDocReader.Document.Blocks.Clear();
                Par.Inlines.Clear();

                decoderFiles(path,ref _data);

                flowDocReader.Document = flowDoc;
                // что бы не перепрыгивало на последнюю страницу
                flowDocReader.ViewingMode = FlowDocumentReaderViewingMode.Scroll;
                flowDocReader.ViewingMode = FlowDocumentReaderViewingMode.Page;

               FileInfo file = new FileInfo(path);             

                Book newBook = new Book(Path.GetFileNameWithoutExtension(path), false, path, file.Length);

                if(!bookList.Contains(newBook))//проверяем есть ли эта книга в списке
                {
                    bookList.Add(new Book(Path.GetFileNameWithoutExtension(path), false, path, file.Length));//если нет, то добавляем в список

                    bookListBox.Items.Add(Path.GetFileNameWithoutExtension(path));//добавляем в список
                    toFile();//переписываем файл со списком
                }
            }
        }

        private void toFile()//метод записи в текстовый файл
        {
            try
            {
                using (StreamWriter sw = new StreamWriter("save.txt", false, System.Text.Encoding.Default))
                {
                    foreach (Book item in bookList)
                    {                      
                        sw.WriteLine(item._name+"#"+item._path+"#"+item._size+"#"+item._favorites);
                    }
                }
            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message, "Exeption");

            }

        }
        private void fromFile()//метод чтения из файла, проверки на существование адресса, и добавления в список
        {
            try
            {
                if (File.Exists("save.txt"))
                {
                    using (StreamReader sr = new StreamReader("save.txt", System.Text.Encoding.Default))
                    {                       

                        string fileLine;
                        while ((fileLine = sr.ReadLine()) != null)
                        {                          
                            string[] dataObj = fileLine.Split('#');
                           
                            if (File.Exists(dataObj[1]))
                                bookList.Add(new Book(dataObj[0], Convert.ToBoolean(dataObj[3]), dataObj[1], Convert.ToDouble(dataObj[2])));
                        }
                    }
                    readFavorites_fromfile();
                    foreach (var item in bookList)
                    {
                        if (item._favorites)
                        {
                            bookListBox.Items.Add(new ListBoxItem() { Content = item._name, Foreground = Brushes.Red });
                        }
                        else
                        {
                            bookListBox.Items.Add(new ListBoxItem() { Content = item._name });
                        }
                    }
                }
            }
            catch (Exception e)
            {

                MessageBox.Show(e.Message, "Exeption");
            }        
        }

        private void CloseCommand_Executed(object sender, ExecutedRoutedEventArgs e)//закрываем программу
        {
            Close();
        }     

        void bookListBox_DoublClick(object sender, EventArgs e)//открытие файла при двойном клике в списке
        {
            try
            {
                StopPlay();
                ListBoxItem li = (ListBoxItem)bookListBox.SelectedItem;
                string selctedItem = li.Content.ToString();
                foreach (Book item in bookList)
                {

                    if (selctedItem == item._name)
                    {
                        string _data = "";
                        flowDoc.Blocks.Clear();
                        Par.Inlines.Clear();
                        decoderFiles(item._path, ref _data);
                    }
                }
                flowDocReader.Document = flowDoc;
                // что бы не перепрыгивало на последнюю страницу
                flowDocReader.ViewingMode = FlowDocumentReaderViewingMode.Scroll;
                flowDocReader.ViewingMode = FlowDocumentReaderViewingMode.Page;
            }
            catch (Exception) 
            {
                
            }
        }

        private void Pause_Click(object sender, RoutedEventArgs e)//пауза воспроизведения
        {
           
            speechSynthesizer.Pause();
        }

        private void Stop_Click(object sender, RoutedEventArgs e)//остановка воспроизведения
        {
            StopPlay();
        }

        private void Play_Click(object sender, RoutedEventArgs e)//воспроизведение звука
        {
            if (speechSynthesizer.State == SynthesizerState.Paused)
            {
                speechSynthesizer.Resume();
            }
            else
            {
                string voice = new TextRange(flowDoc.ContentStart, flowDoc.ContentEnd).Text;
                if (Regex.IsMatch(voice, "[а-яА-ЯеЁ]"))
                {
                    speechSynthesizer.SelectVoice("Microsoft Irina Desktop");
                }
                else
                {
                    speechSynthesizer.SelectVoice("Microsoft David Desktop");
                }
                speechSynthesizer.SpeakAsync($"{voice}");
            }
        }

        private void StopPlay()//остановка звука
        {
            speechSynthesizer.Dispose();
            speechSynthesizer = new SpeechSynthesizer();
        }
        private void AddFavoriteToFile()
        {
            using (StreamWriter stream = new StreamWriter("favorites.txt", false, System.Text.Encoding.Default))
            {
                foreach (Book item in bookList)
                {
                    if (item._favorites)
                    {
                        stream.WriteLine(item._name);
                    }
                }
            }
        }
        void readFavorites_fromfile()
        {
            try
            {
                if (File.Exists("favorites.txt"))
                {
                    string text;

                    using (StreamReader fs = new StreamReader("favorites.txt", Encoding.GetEncoding(1251)))
                    {
                        while (true)
                        {
                            string temp = fs.ReadLine();
                            if (temp == null) break;
                            text = temp;
                            foreach (Book item in bookList)
                            {
                                if (item._name == text)
                                    item._favorites = true;
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }
        private void addFavorite(object sender, RoutedEventArgs e)
        {
            var menuItem = (MenuItem)sender;
            var contextMenu = (ContextMenu)menuItem.Parent;
            var target = (ListBoxItem)contextMenu.PlacementTarget;
            target.Foreground = Brushes.Red;
            foreach (Book item in bookList)
            {
                if (item._name == target.Content.ToString())
                {
                    item._favorites = true;
                }
            }
            AddFavoriteToFile();
            Fav_Show();
        }

        private void delFavorite(object sender, RoutedEventArgs e)
        {
            var menuItem = (MenuItem)sender;
            var contextMenu = (ContextMenu)menuItem.Parent;
            var target = (ListBoxItem)contextMenu.PlacementTarget;
            target.Foreground = Brushes.Black;
            foreach (Book item in bookList)
            {
                if (item._name == target.Content.ToString())
                {
                    item._favorites = false;
                }
            }
            AddFavoriteToFile();
            Fav_Show();
        }
        // обход xml дерева (для чтения fb2)
        private static void MovesNodes(XmlNodeList nodes, FlowDocument flowDoc)
        {
            Paragraph p = new Paragraph();
            foreach (XmlNode node in nodes)
            {
                if (node.NodeType == XmlNodeType.Text)
                {
                    p.Inlines.Add(node.InnerText);
                    flowDoc.Blocks.Add(p);
                }
                MovesNodes(node.ChildNodes, flowDoc);
            }
        }
        // декодер для разных форматов
        private void decoderFiles(string path, ref string _data)
        {
            try
            {
                switch (Path.GetExtension(path).ToLower())
                {
                    case ".txt":
                        epubHidden();
                        _data = File.ReadAllText(path, Encoding.Default);
                        Par.Inlines.Add(_data);
                        flowDoc.Blocks.Add(Par);
                        break;
                    case ".fb2":
                        epubHidden();
                        XmlDocument doc = new XmlDocument();
                        doc.Load(path);
                        XmlElement root = doc.DocumentElement;
                        foreach (XmlNode node in root)
                        {
                            if (node.Name == "body")
                            {
                                MovesNodes(node.ChildNodes, flowDoc);
                            }
                        }
                        break;
                    case ".epub":
                        if (!Directory.Exists("Library"))
                        {
                            Directory.CreateDirectory("Library");
                        }
                        string fileName = Path.GetFileNameWithoutExtension(path);
                        File.Copy(path, Path.Combine("Library", fileName + ".zip"), true);
                        _tempPath = Path.Combine("Library", fileName);
                        if (Directory.Exists(_tempPath))
                        {
                            FileUtility.DeleteDirectory(_tempPath);
                        }
                        FileUtility.UnZIPFiles(Path.Combine("Library", fileName + ".zip"), Path.Combine("Library", fileName));

                        var containerReader = XDocument.Load(ConvertToMemmoryStream(Path.Combine("Library", fileName, "META-INF", "container.xml")));
                        var baseMenuXmlPath = containerReader.Root.Descendants(containerReader.Root.GetDefaultNamespace() + "rootfile").First().Attribute("full-path").Value;
                        XDocument menuReader = XDocument.Load(Path.Combine(_tempPath, baseMenuXmlPath));
                        _baseMenuXmlDiretory = Path.GetDirectoryName(baseMenuXmlPath);
                        var menuItemsIds = menuReader.Root.Element(menuReader.Root.GetDefaultNamespace() + "spine").Descendants()
                            .Select(a => a.Attribute("idref").Value).ToList();
                        _menuItems = menuReader.Root.Element(menuReader.Root.GetDefaultNamespace() + "manifest").Descendants()
                            .Where(mn => menuItemsIds.Contains(mn.Attribute("id").Value)).Select(mn => mn.Attribute("href").Value).ToList();
                        _currentPage = 0;
                        string uri = GetPath(0);
                        web.Visibility = Visibility.Visible;
                        flowDocReader.Visibility = Visibility.Collapsed;
                        NextButton.Visibility = Visibility.Visible;
                        PreviousButton.Visibility = Visibility.Visible;
                        audio.Visibility = Visibility.Collapsed;
                        web.Navigate(uri);                        
                        break;
                    case ".rtf": break;
                    case ".pdf": break;
                    default:
                        MessageBox.Show("Невозможно открыть данный файл");
                        break;
                }
            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message, "Exeption");
            }
        }
        // для формата epub
        public string GetPath(int index)
        {
            return String.Format("file:///{0}", Path.GetFullPath(Path.Combine(_tempPath, _baseMenuXmlDiretory, _menuItems[index])));
        }
        // для формата epub
        public MemoryStream ConvertToMemmoryStream(string fillPath)
        {
            var xml = File.ReadAllText(fillPath);
            byte[] encodedString = Encoding.UTF8.GetBytes(xml);
            MemoryStream ms = new MemoryStream(encodedString);
            ms.Flush();
            ms.Position = 0;
            return ms;
        }
        // для формата epub
        public void epubHidden()
        {
            flowDocReader.Visibility = Visibility.Visible;
            audio.Visibility = Visibility.Visible;
            NextButton.Visibility = Visibility.Collapsed;
            PreviousButton.Visibility = Visibility.Collapsed;
            web.Visibility = Visibility.Collapsed;
        }
        private void Font_Click(object sender, RoutedEventArgs e)
        {
            FontDialogBox dialogBox = new FontDialogBox(flowDoc);
            dialogBox.Show();
        }

        private void openList(object sender, RoutedEventArgs e)
        {
            if (OpenList)
            {
                bookListBox.Width = 0;
                OpenList = false;
                ListBoxImg.Source = new BitmapImage(new Uri($"../../Icons/openArrows.png", UriKind.Relative));
            }
            else
            {
                bookListBox.Width = 200;
                OpenList = true;
                ListBoxImg.Source = new BitmapImage(new Uri($"../../Icons/closeArrows.png", UriKind.Relative));
            }
        }

        int clickFav = 0;
        private void Fav_Show()
        {
            if (clickFav % 2 != 0)
            {
                bookListBox.Items.Clear();
                foreach (Book item in bookList)
                {
                    if (item._favorites)
                    {
                        bookListBox.Items.Add(new ListBoxItem() { Content = item._name, Foreground = Brushes.Red });
                    }
                }
            }
            else
            {
                bookListBox.Items.Clear();
                foreach (var item in bookList)
                {
                    if (item._favorites)
                    {
                        bookListBox.Items.Add(new ListBoxItem() { Content = item._name, Foreground = Brushes.Red });
                    }
                    else
                    {
                        bookListBox.Items.Add(new ListBoxItem() { Content = item._name });
                    }
                }
            }
        }
        private void Button_Fav_Click(object sender, RoutedEventArgs e)
        {
            clickFav++;
            Fav_Show();
        }

        private void styleBtn_Click(object sender, RoutedEventArgs e)
        {
            _currStyle = !_currStyle;
            string style;
            if (_currStyle)
            {
                style = "dark";

            }
            else
            {
                style = "light";
            }
            // определяем путь к файлу ресурсов
            var uri = new Uri("Styles/" + style + ".xaml", UriKind.Relative);
            // загружаем словарь ресурсов
            ResourceDictionary resourceDict = Application.LoadComponent(uri) as ResourceDictionary;
            // очищаем коллекцию ресурсов приложения
            Application.Current.Resources.Clear();
            // добавляем загруженный словарь ресурсов
            Application.Current.Resources.MergedDictionaries.Add(resourceDict);
        }
        // листание назад для epub
        private void PreviousButton_Click(object sender, RoutedEventArgs e)
        {
            if (_currentPage >= 1)
            {
                _currentPage--;
            }
            else
            {

                PreviousButton.Visibility = Visibility.Hidden;
            }
            if (_currentPage == 1)
            {
                PreviousButton.Visibility = Visibility.Hidden;
            }
            if (_currentPage <= _menuItems.Count - 1)
            {
                NextButton.Visibility = Visibility.Visible;
            }
            string uri = GetPath(_currentPage);
            web.Navigate(uri);
        }
        // листание вперед для epub
        private void NextButton_Click(object sender, RoutedEventArgs e)
        {
            if (_currentPage < _menuItems.Count - 1)
            {
                _currentPage++;
            }
            else
            {
                NextButton.Visibility = Visibility.Hidden;
            }
            if (_currentPage == _menuItems.Count - 1)
            {
                NextButton.Visibility = Visibility.Hidden;
            }
            if (_currentPage > 0)
            {

                PreviousButton.Visibility = Visibility.Visible;
            }
            string uri = GetPath(_currentPage);
            web.Navigate(uri);
        }
    }
}
