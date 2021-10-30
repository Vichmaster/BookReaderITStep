using System.Collections.Generic;
using System.Windows;
using System.IO;
using System.Windows.Input;
using Microsoft.Win32;
using System.Windows.Documents;
using System.Text;
using System;
using System.Speech.Synthesis;

namespace Project11
{
    public partial class MainWindow : Window
    {
        SpeechSynthesizer speechSynthesizer = new SpeechSynthesizer();
        public List<string> bookList { get; private set; } = new List<string>(); //список всех локальных аадрессов книг
        public MainWindow()
        {
            InitializeComponent();
            fromFile();//считываем сохранённые адресса в список
            bookListBox.MouseDoubleClick += new MouseButtonEventHandler(bookListBox_DoublClick);
        }

        private void OpenCommand_Executed(object sender, ExecutedRoutedEventArgs e)//обработичк кнопки Открыть
        {
            OpenFileDialog openFileDialog = new OpenFileDialog
            {
                Filter = "TXT files(*.txt) | *.txt"
            };

            if (openFileDialog.ShowDialog() == true)
            {
                string path = openFileDialog.FileName;
               
                string _data = File.ReadAllText(path, Encoding.Default);


                Par.Inlines.Clear();
                Par.Inlines.Add(_data);

                FlowDocument document = new FlowDocument();
                document.Blocks.Add(Par);

                flowDocReader.Document = document;

                if(!bookList.Contains(path))//проверяем есть ли эта книга в списке
                {
                    bookList.Add(path);//если нет, то добавляем в список

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
                    foreach (var item in bookList)
                    {
                        sw.WriteLine(item.ToString());
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
            if (File.Exists("save.txt"))
            {
                using (StreamReader sr = new StreamReader("save.txt", System.Text.Encoding.Default))
                {
                    string path;
                    while ((path = sr.ReadLine()) != null)
                    {
                        if (File.Exists(path))
                            bookList.Add(path);
                    }
                }
                foreach (var item in bookList)
                {
                    string fileName = Path.GetFileNameWithoutExtension((string)item);//получаем имя файла по адрессу и без расширения
                 
                    bookListBox.Items.Add(fileName);
                }
            }

        }

        private void CloseCommand_Executed(object sender, ExecutedRoutedEventArgs e)//закрываем программу
        {
            Close();
        }     

        void bookListBox_DoublClick(object sender, EventArgs e)//открытие файла при двойном клике в списке
        {
            StopPlay();

            foreach (var item in bookList)
            {
                string fileName = Path.GetFileNameWithoutExtension((string)item);
                if(bookListBox.SelectedItem.ToString() == fileName)
                {
                    

                    string _data = File.ReadAllText((string)item,Encoding.Default);



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
    }
}
