using Microsoft.Toolkit.Uwp.Notifications;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;

namespace FuryMediaPlayer_framework.views.templates
{
    /// <summary>
    /// Логика взаимодействия для MediaPlayerWindow.xaml
    /// </summary>
    public partial class MediaPlayerWindow : Window
    {
        //список всех медиафайлов
        LinkedList<MediaContent> mediaContents = new LinkedList<MediaContent>();
        //Узел медиаэлемента
        LinkedListNode<MediaContent> mediaNode = null;
        //Если мы в паузе
        private bool isPause = false;
        private bool isDrag = false;

        public MediaPlayerWindow()
        {
            InitializeComponent();

            //Создаем диспетчер, для отслеживания времени медиафайла и привязки его к Slider
            DispatcherTimer timer = new DispatcherTimer();
            timer.Interval = TimeSpan.FromSeconds(1);
            timer.Tick += timer_Tick;
            timer.Start();
        }


        #region Эффекты и визуализация
        private void Grid_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
                DragMove();
        }

        //ищем ВСЕ элементы WPF по указанному типу в указанном элементе управления.
        public static IEnumerable<T> FindVisualChildren<T>(DependencyObject depObj) where T : DependencyObject
        {
            if (depObj != null)
            {
                for (int i = 0; i < VisualTreeHelper.GetChildrenCount(depObj); i++)
                {
                    DependencyObject child = VisualTreeHelper.GetChild(depObj, i);
                    if (child != null && child is T)
                    {
                        yield return (T)child;
                    }

                    foreach (T childOfChild in FindVisualChildren<T>(child))
                    {
                        yield return childOfChild;
                    }
                }
            }
        }

        //Адаптивность навигации
        private void Window_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            double screenHeight = e.NewSize.Height;
            double screenWidth = e.NewSize.Width;

            //брейк-поинт для адаптивности
            if (screenHeight > 800 && screenWidth > 1200)
            {
                FindVisualChildren<System.Windows.Shapes.Path>(navigationGrid).ToList().ForEach(shape => { shape.Width = 50; shape.Height = 50; });
            }
            else if (screenHeight < 600 || screenWidth < 700)
            {
                FindVisualChildren<System.Windows.Shapes.Path>(navigationGrid).ToList().ForEach(shape => { shape.Width = 24; shape.Height = 24; });
            }
            else
            {
                FindVisualChildren<System.Windows.Shapes.Path>(navigationGrid).ToList().ForEach(shape => { shape.Width = 36; shape.Height = 36; });
            }
        }

        //всегда поверх других окон, если включена функция
        private void Window_Deactivated(object sender, EventArgs e)
        {
            Window window = (Window)sender;
            if (pinActionButton.IsChecked) window.Topmost = true;
            else window.Topmost = false;
        }

        //Стиль при наведении
        private void Path_MouseEnter(object sender, MouseEventArgs e)
        {
            System.Windows.Shapes.Path uiElement = sender as System.Windows.Shapes.Path;
            uiElement.Fill = new SolidColorBrush(Color.FromArgb(255, 167, 44, 67));
        }

        private void Path_MouseLeave(object sender, MouseEventArgs e)
        {
            System.Windows.Shapes.Path uiElement = sender as System.Windows.Shapes.Path;
            uiElement.Fill = new SolidColorBrush(Color.FromArgb(255, 239, 63, 97));
        }
        #endregion

        #region Навигационное меню приложения

        //Открываем медиафайл через меню
        private void openMediaFile_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog fileDialog = new OpenFileDialog();
            fileDialog.Filter = "All Media Files|*.wav;*.aac;*.wma;*.wmv;*.avi;*.mpg;*.mpeg;*.m1v;*.mp2;*.mp3;*.mpa;*.mpe;*.m3u;*.mp4;*.mov;*.3g2;*.3gp2;*.3gp;*.3gpp;*.m4a;*.cda;*.aif;*.aifc;*.aiff;*.mid;*.midi;*.rmi;*.mkv;*.WAV;*.AAC;*.WMA;*.WMV;*.AVI;*.MPG;*.MPEG;*.M1V;*.MP2;*.MP3;*.MPA;*.MPE;*.M3U;*.MP4;*.MOV;*.3G2;*.3GP2;*.3GP;*.3GPP;*.M4A;*.CDA;*.AIF;*.AIFC;*.AIFF;*.MID;*.MIDI;*.RMI;*.MKV";

            if (fileDialog.ShowDialog() == true)
            {
                addMediaContent(fileDialog);
            }
        }

        //Скрыть навигационное меню плеера
        private void hideNavigatationMedia_Click(object sender, RoutedEventArgs e)
        {
            if (hideNavigatationMedia.IsChecked)
            {
                navigationGrid.Visibility = Visibility.Collapsed;
                sliderNavigation.Visibility = Visibility.Collapsed;
                mediaColumn.Height = new GridLength(800, GridUnitType.Star);
                navigationMediaColumn.Height = new GridLength(0);
            }
            else
            {
                navigationGrid.Visibility = Visibility.Visible;
                sliderNavigation.Visibility = Visibility.Visible;
                mediaColumn.Height = new GridLength(640, GridUnitType.Star);
                navigationMediaColumn.Height = new GridLength(80, GridUnitType.Star);
            }
        }

        //Открыть папку с медиафайлами
        private void openMediaFolder_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new System.Windows.Forms.FolderBrowserDialog();

            if (dialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                addMediaFiles(dialog.SelectedPath);
            }
        }

        //закрыть плеер
        private void closeButton_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (mediaNode != null)
            {
                SaveData();
            }
            Thread.Sleep(500);
            Environment.Exit(0);
        }

        #endregion

        #region Навигационное меню плеера
        //проигрывание текущей
        private void playButton_MouseDown(object sender, MouseButtonEventArgs e)
        {
            isPause = !isPause;

            if (isPause)
            {
                playButton.Data = Geometry.Parse("M61.44 0c33.93 0 61.44 27.51 61.44 61.44s-27.51 61.44-61.44 61.44S0 95.37 0 61.44 27.51 0 61.44 0h0zm23.47 65.52c3.41-2.2 3.41-4.66 0-6.61L49.63 38.63c-2.78-1.75-5.69-.72-5.61 2.92l.11 40.98c.24 3.94 2.49 5.02 5.8 3.19l34.98-20.2h0z");
                mediaElement.Pause();
            }
            else
            {
                playButton.Data = Geometry.Parse("M61.44 0c33.93 0 61.44 27.51 61.44 61.44s-27.51 61.44-61.44 61.44S0 95.37 0 61.44 27.51 0 61.44 0h0zm6.72 33.88H84.1V89H68.16V33.88h0 0zm-29.38 0h15.94V89H38.78V33.88h0 0z");
                mediaElement.Play();
                mediaTitle.Content = mediaNode.Value.Title;
            }
        }

        //перемещения в начало
        private void startButton_MouseDown(object sender, MouseButtonEventArgs e)
        {
            mediaElement.Position = TimeSpan.Zero;
            mediaElement.Play();
        }

        //перемещение в конец
        private void endButton_MouseDown(object sender, MouseButtonEventArgs e)
        {
            mediaElement.Position = mediaNode.Value.Duraction - TimeSpan.FromSeconds(1);
            mediaElement.Play();
        }

        //изменение значение Slider
        private void duractionSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {

        }

        //промотать вперед
        private void nextButton_MouseDown(object sender, MouseButtonEventArgs e)
        {
            mediaElement.Position = mediaElement.Position + TimeSpan.FromSeconds(3);
        }

        //промотать назад
        private void backButton_MouseDown(object sender, MouseButtonEventArgs e)
        {
            mediaElement.Position = mediaElement.Position - TimeSpan.FromSeconds(3);
        }

        //включить или выключить звук
        private void muteButton_MouseDown(object sender, MouseButtonEventArgs e)
        {
            mediaElement.IsMuted = !mediaElement.IsMuted;
        }

        //Когда мы отпускаем Slider, медиа воспроизводится с места Value ползунка
        private void duractionSlider_DragCompleted(object sender, System.Windows.Controls.Primitives.DragCompletedEventArgs e)
        {
            isDrag = false;
            int SliderValue = (int)duractionSlider.Value;

            TimeSpan ts = new TimeSpan(0, 0, 0, SliderValue, 0);
            mediaElement.Position = ts;
        }

        //если мы начинаем двигать Slider
        private void duractionSlider_DragStarted(object sender, System.Windows.Controls.Primitives.DragStartedEventArgs e)
        {
            isDrag = true;
        }

        #endregion

        #region ControlEvents and Methods
        //Добавление медиафайла через fileDialog
        private void addMediaContent(OpenFileDialog fileDialog)
        {
            try
            {
                var metadataFile = TagLib.File.Create(fileDialog.FileName);
                string title = metadataFile.Tag.Title;
                string filename = metadataFile.Name;
                string type = checkFileFormat(metadataFile.MimeType.Split('/')[1]);
                TimeSpan duraction = metadataFile.Properties.Duration;

                MediaContent mediaFile = new MediaContent(filename, title, type, duraction);
                mediaNode = new LinkedListNode<MediaContent>(mediaFile);
                mediaContents.AddLast(mediaNode);

                mediaElement.Source = new Uri(mediaNode.Value.Filename);
                mediaTitle.Content = mediaNode.Value.Title;
            }
            catch (Exception)
            {
                // Библиотека Microsoft.Toolkit.Uwp.Notifications
                new ToastContentBuilder()
                    .AddText("Возникла ошибка")
                    .AddText("При добавлении файла возникла неизвестная ошибка")
                    .Show();
            }
        }

        //Добавление медиафайла через путь (Path)
        private void addMediaContentByPath(string path, double position = 0)
        {
            try
            {
                var metadataFile = TagLib.File.Create(path);
                string type = checkFileFormat(metadataFile.MimeType.Split('/')[1]);

                if (type != null)
                {
                    string title = metadataFile.Tag.Title;
                    string filename = metadataFile.Name;
                    TimeSpan duraction = metadataFile.Properties.Duration;
                    MediaContent mediaFile = new MediaContent(filename, title, type, duraction);
                    mediaNode = new LinkedListNode<MediaContent>(mediaFile);
                    mediaContents.AddLast(mediaNode);


                    mediaElement.Source = new Uri(mediaNode.Value.Filename);
                    mediaElement.Position = TimeSpan.FromSeconds(position);
                    mediaTitle.Content = mediaNode.Value.Title;
                    mediaElement.Play();
                }
            }
            catch (Exception)
            {
                // invalid format
            }
        }

        //Проверка на формат файла. Аудио или видео.
        private string checkFileFormat(string format)
        {
            string[] video = { "wmv", "mp4", "avi", "mkv", "flv" };
            string[] audio = { "mp3", "aac", "wav", "ogg", "m4a", "flac" };

            if (Array.IndexOf(video, format) >= 0)
            {
                return "video";
            }
            else if (Array.IndexOf(audio, format) >= 0)
            {
                return "audio";
            }
            else
            {
                return null;
            }
        }

        //добавление медиафайлов в LinkedList
        private void addMediaFiles(string selectedPath)
        {
            mediaContents.Clear();
            string[] files = Directory.GetFiles(selectedPath);

            foreach (string file in files) addMediaContentByPath(file);

            Thread.Sleep(1500);

            new ToastContentBuilder()
                .AddText("Медиафайлы успешно загружены")
                .AddText("Всего загружено файлов:" + mediaContents.Count)
                .Show(toast =>
                {
                    toast.ExpirationTime = DateTime.Now.AddSeconds(5);
                });
        }

        //Каждую секунду устанавливаем Slider как текущее время MediaElement
        private void timer_Tick(object sender, EventArgs e)
        {
            //если медиафайл загружен и мы не трогаем Slider
            if (mediaElement.Source != null && mediaElement.NaturalDuration.HasTimeSpan && isDrag == false)
            {
                duractionSlider.Minimum = 0;
                duractionSlider.Maximum = mediaElement.NaturalDuration.TimeSpan.TotalSeconds;
                duractionSlider.Value = mediaElement.Position.TotalSeconds;
            }
        }

        //Перемещение по связному списку - вперед
        private void nextMedia_Click(object sender, RoutedEventArgs e)
        {
            if (mediaNode != mediaContents.Last)
            {
                mediaElement.Stop();
                mediaNode = mediaNode.Next;
                mediaElement.Source = new Uri(mediaNode.Value.Filename);
                mediaTitle.Content = mediaNode.Value.Title;
                mediaElement.Play();
            }
        }

        //Перемещение по связному списку - назад
        private void previousMedia_Click(object sender, RoutedEventArgs e)
        {
            if (mediaNode != mediaContents.First)
            {
                mediaElement.Stop();
                mediaNode = mediaNode.Previous;
                mediaElement.Source = new Uri(mediaNode.Value.Filename);
                mediaTitle.Content = mediaNode.Value.Title;
                mediaElement.Play();
            }
        }

        //Автосохранение позиции при закрытии
        private void mediaplayerWindow_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (mediaNode != null)
            {
                e.Cancel = true;
                SaveData();
            }
        }

        //сохранение данных в файл
        public void SaveData()
        {
            string text = $"{mediaNode.Value.Filename}|{mediaElement.Position.TotalSeconds}";

            if (!Directory.Exists("saves"))
            {
                Directory.CreateDirectory("saves");
            }

            File.WriteAllText("saves/savedata.txt", text);
            Environment.Exit(0);
        }

        //При загрузки медиаплеера проверка на аргументы
        private async void mediaplayerWindow_Loaded(object sender, RoutedEventArgs e)
        {
            await Task.Delay(500);

            if (Environment.GetCommandLineArgs().Length <= 1)
            {
                if (File.Exists("saves/savedata.txt"))
                {
                    using (StreamReader readtext = new StreamReader("saves/savedata.txt"))
                    {
                        string dataText = readtext.ReadLine();

                        MessageBoxResult messageBoxResult = MessageBox.Show("Хотите продолжить с места где остановились?", "Автосохранение", MessageBoxButton.YesNo);
                        if (messageBoxResult == MessageBoxResult.Yes)
                            addMediaContentByPath(dataText.Split('|')[0], double.Parse(dataText.Split('|')[1]));
                    }
                }
            }
            else
            {
                addMediaContentByPath(Environment.GetCommandLineArgs()[1]);
            }
        }

        #endregion
    }
}
