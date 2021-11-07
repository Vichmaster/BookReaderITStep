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

namespace Project11
{
    public partial class MainWindow : Window
    {
        private bool _currStyle;
        SpeechSynthesizer speechSynthesizer = new SpeechSynthesizer();
        public List<Book> bookList { get; private set; } = new List<Book>(); //список книг

        public bool OpenList { get; set; }
        public MainWindow()
        {
            InitializeComponent();
            fromFile();//считываем сохранённые адресса в список
            bookListBox.MouseDoubleClick += new MouseButtonEventHandler(bookListBox_DoublClick);
            OpenList = false;
        }

        private void OpenCommand_Executed(object sender, ExecutedRoutedEventArgs e)//обработичк кнопки Открыть
        {
            OpenFileDialog openFileDialog = new OpenFileDialog
            {
                Filter = "TXT files(*.txt;*.fb2) | *.txt; *.fb2"
            };

            if (openFileDialog.ShowDialog() == true)
            {
                string path = openFileDialog.FileName;
                string _data = "Oops something wrong";


                decoderFiles(path,ref _data);


                Par.Inlines.Clear();
                Par.Inlines.Add(_data);

                FlowDocument document = new FlowDocument();
                document.Blocks.Add(Par);

                flowDocReader.Document = document;

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
                        sw.WriteLine(item._name+" "+item._path+" "+item._size+" "+item._favorites);
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
                           
                            string[] dataObj = fileLine.Split(' ');
                           
                            if (File.Exists(dataObj[1]))
                                bookList.Add(new Book(dataObj[0], Convert.ToBoolean(dataObj[3]), dataObj[1], Convert.ToDouble(dataObj[2])));
                        }
                    }
                    foreach (var item in bookList)
                    {

                        bookListBox.Items.Add(item._name);
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
            StopPlay();

            foreach (Book item in bookList)
            {
               
                if(bookListBox.SelectedItem.ToString() == item._name)
                {


                    string _data = "";


                    decoderFiles(item._path, ref _data);



                    Par.Inlines.Clear();
                    Par.Inlines.Add(_data);

                    FlowDocument document = new FlowDocument();
                    document.Blocks.Add(Par);

                    flowDocReader.Document = document;
                }
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
                string voice = new TextRange(Par.Inlines.FirstInline.ContentStart, Par.Inlines.LastInline.ElementEnd).Text;

                speechSynthesizer.SpeakAsync($"{voice}");

            }


        }

        private void StopPlay()//остановка звука
        {
            speechSynthesizer.Dispose();
            speechSynthesizer = new SpeechSynthesizer();
        }

        private void addFavorite(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Hello");
        }

        private void delFavorite(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Hello");
        }

        private void decoderFiles(string path, ref string _data)
        {

            try
            {
                switch (Path.GetExtension(path).ToLower())
                {
                    case ".txt":
                        _data = File.ReadAllText(path, Encoding.Default);
                        break;

                    case ".fb2":
                        XElement el = XElement.Load(path);
                        _data = el.Value;
                        break;
                    case ".epub": break;
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

        private void Font_Click(object sender, RoutedEventArgs e)
        {
            FontDialogBox dialogBox = new FontDialogBox(Par);
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
    }
}
