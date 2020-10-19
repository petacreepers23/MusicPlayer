using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.IO;
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

namespace Reproductor
{
    /// <summary>
    /// Lógica de interacción para MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        //Variables de objetos del reproductor
        private readonly MediaPlayer mediaPlayer = new MediaPlayer();
        private readonly MouseButtonEventHandler cambiarCancion;

        //Variables de estado del reproductor
        private bool isPlaying = false;
        private bool songsAvailable = false;
        private bool looping = false;
        private int currentIndex = 0;


        public MainWindow(StartupEventArgs e)
        {
            InitializeComponent();
            cambiarCancion = new MouseButtonEventHandler(lvi_MouseDoubleClick);
            mediaPlayer.Volume = 1;
            mediaPlayer.MediaEnded += new EventHandler(cancionAcabada);
            mediaPlayer.MediaOpened += new EventHandler(cancionCargada);


            foreach(string arg in e.Args)
            {
                addSongs(arg);
            }

            if (songsAvailable)
            {
                cargarCancion();
                mediaPlayer.Play();
                isPlaying = true;
                img_play.Source = new BitmapImage(new Uri("Recursos/pause.png", UriKind.Relative));
            }
        }

        private void addSongs(string directoryName)
        {
            FileAttributes attr = File.GetAttributes(directoryName);

            //NO DIR
            if (!attr.HasFlag(FileAttributes.Directory))
            {
                if (System.IO.Path.GetExtension(directoryName) == ".mp3")
                {
                    songsAvailable = true;
                    ListViewItem lv = new ListViewItem
                    {
                        Content = directoryName
                    };
                    lv.MouseDoubleClick += cambiarCancion;
                    lst_listaCanciones.Items.Add(lv);
                }
                return;
            }

            //ERA DIR
            string[] files = Directory.GetFiles(directoryName);

            foreach (string file in files)
            {
                if (System.IO.Path.GetExtension(file) == ".mp3")
                {
                    songsAvailable = true;
                    ListViewItem lv = new ListViewItem
                    {
                        Content = file
                    };
                    lv.MouseDoubleClick += cambiarCancion;
                    lst_listaCanciones.Items.Add(lv);
                }
            }
        }

        private void cancionAcabada(object sender, EventArgs e)
        {
            if (looping)
            {
                mediaPlayer.Stop();
                mediaPlayer.Play();
            }
            else
            {
                currentIndex++;
                cargarCancion();
            }
        }

        private void cargarCancion()
        {
            if (!songsAvailable)
            {
                return;
            }

            if (currentIndex == lst_listaCanciones.Items.Count)
            {
                currentIndex = 0;
            }
            leerNuevaCancion(new Uri((string)((ListViewItem)lst_listaCanciones.Items[currentIndex]).Content));
        }

        private void cancionCargada(object sender, EventArgs e)
        {
            tb_duracionCancion.Text = mediaPlayer.NaturalDuration.TimeSpan.ToString(@"mm\:ss");
            ((ListViewItem)lst_listaCanciones.Items[currentIndex]).IsSelected = true;
        }

        private void leerNuevaCancion(Uri u)
        {
            mediaPlayer.Stop();
            mediaPlayer.Open(u);
            tb_duracionCancion.Text = "Cargando...";
            if (isPlaying)
            {
                mediaPlayer.Play();
            }
        }

        private void btn_play_MouseUp(object sender, MouseButtonEventArgs e)
        {
            if (isPlaying)
            {
                mediaPlayer.Pause();
                isPlaying = false;
                img_play.Source = new BitmapImage(new Uri("Recursos/play.png", UriKind.Relative));
            }
            else if(songsAvailable)
            {
                cargarCancion();
                mediaPlayer.Play();
                isPlaying = true;
                img_play.Source = new BitmapImage(new Uri("Recursos/pause.png", UriKind.Relative));
            }
        }

        private void btn_prev_MouseDown(object sender, MouseButtonEventArgs e)
        {
            currentIndex--;
            if (currentIndex < 0)
            {
                currentIndex = 0;
            }
            cargarCancion();
        }

        private void btn_next_MouseDown(object sender, MouseButtonEventArgs e)
        {
            currentIndex++;
            cargarCancion();
        }

        private void sld_volume_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            mediaPlayer.Volume = sld_volume.Value / 100;
        }



        private void lst_listaCanciones_DragEnter(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                lst_listaCanciones.BorderBrush = new SolidColorBrush(Color.FromRgb(0, 240, 240));
            }
        }

        private void lst_listaCanciones_DragLeave(object sender, DragEventArgs e)
        {
            lst_listaCanciones.BorderBrush = new SolidColorBrush(Color.FromRgb(255, 255, 255));
        }


        private void lvi_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            ListViewItem cancion = (ListViewItem)sender;
            for (int i = 0; i < ((ListView)cancion.Parent).Items.Count; i++)
            {
                if (((ListView)cancion.Parent).Items[i].GetHashCode() == cancion.GetHashCode()) { 
                    currentIndex = i;
                }
            } 
            leerNuevaCancion(new Uri((string)((ListViewItem)sender).Content));
        }

        private void lst_listaCanciones_Drop(object sender, DragEventArgs e)
        {
            lst_listaCanciones.BorderBrush = new SolidColorBrush(Color.FromRgb(255, 255, 255));

            string[] directoryName = (string[])e.Data.GetData(DataFormats.FileDrop);
            for (int i = 0; i < directoryName.Length; i++)
            {
                addSongs(directoryName[i]);
            }
        }

        private void scv_scrollLista_PreviewMouseWheel(object sender, MouseWheelEventArgs e)
        {
            ScrollViewer scv = (ScrollViewer)sender;
            scv.ScrollToVerticalOffset(scv.VerticalOffset - e.Delta);
            e.Handled = true;
        }

        private void btn_loop_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (looping)
            {
                img_loop.Source = new BitmapImage(new Uri("Recursos/no_loop.png", UriKind.Relative));
                looping = false;
            }
            else
            {
                img_loop.Source = new BitmapImage(new Uri("Recursos/loop.png", UriKind.Relative));
                looping = true;
            }
        }

        private void lst_listaCanciones_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (!songsAvailable) {
                OpenFileDialog openFileDialog = new OpenFileDialog
                {
                    Filter = "Musica (*.mp3)|*.mp3|Todos (*.*)|*.*",
                    Multiselect = true
                };
                if (openFileDialog.ShowDialog() == true)
                {
                    foreach (var item in openFileDialog.FileNames)
                    {
                        addSongs(item);
                    }
                }
            }
            
        }

    }
}
