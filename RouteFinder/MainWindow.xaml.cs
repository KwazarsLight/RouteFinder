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

namespace RouteFinder
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    /// 

        class Item
    {
        public string routeName { get; set; }
        public string time { get; set; }
    }

    public partial class MainWindow : Window
    {

        private bool isFileChosen = false;
        static public string pathToFile = " ";
        static public int carsCount = 0;

        public MainWindow()
        {
            InitializeComponent();
        }

        private void ChooseFileButton_Click(object sender, RoutedEventArgs e)
        {
            System.Windows.Forms.OpenFileDialog dialog = new System.Windows.Forms.OpenFileDialog();
            dialog.Filter = "txt files (*.txt)|*.txt";
            System.Windows.Forms.DialogResult result = dialog.ShowDialog();
            if (result == System.Windows.Forms.DialogResult.OK)
                if (!String.IsNullOrEmpty(dialog.FileName))
                {
                    isFileChosen = true;
                    pathToFile = dialog.FileName; 
                }
        }

        private void BrowserCall(string Url)
        { System.Diagnostics.Process.Start(Url); }

        private void RoutelistView_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            ClarkRightAlgorithm clr = new ClarkRightAlgorithm();
            BrowserCall(clr.RouteRequest(RoutelistView.SelectedIndex));
        }

        private void SetRouteButton_Click(object sender, RoutedEventArgs e)
        {
            RoutelistView.Items.Clear();
            if (!isFileChosen)
            {
                MessageBox.Show("Файл содержащий адреса отсутствует");
            }
            else if (carsCount == 0)
            {
                MessageBox.Show("Количество доступных машин не задано");
            }
            else
            {
                TransportationMap map = new TransportationMap(pathToFile);
                map.FillDeliveryPointsList(pathToFile);
                ClarkRightAlgorithm clr = new ClarkRightAlgorithm(carsCount);
                for (int i = 1; i < ClarkRightAlgorithm.RoutesList.Count + 1; i++)
                {
                    RoutelistView.Items.Add(new Item
                    {
                        routeName = "Маршрут №" + i,
                        time = "Время выполнения" + map.CalcTimeOfRoute(i - 1, ClarkRightAlgorithm.RoutesList) + " часов"
                    });
                }
                RoutelistView.Background.Opacity = 0.8;
            }
        }

        private void AddRequestButton_Click(object sender, RoutedEventArgs e)
        {
            AddRequestWindow wnd = new AddRequestWindow();
            wnd.Show();
        }

        private void CallInfoButton_Click(object sender, RoutedEventArgs e)
        {
            AppInfoWindow wnd = new AppInfoWindow();
            wnd.Show();
        }

        private void MenuItem_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void SetCarsCount_Click(object sender, RoutedEventArgs e)
        {
            SetCarsCountWindow wnd = new SetCarsCountWindow();
            wnd.Show();
        }
    }
}
