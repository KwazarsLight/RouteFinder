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
using System.Windows.Shapes;

namespace RouteFinder
{
    /// <summary>
    /// Interaction logic for AddRequestWindow.xaml
    /// </summary>
    public partial class AddRequestWindow : Window
    {
        public AddRequestWindow()
        {
            InitializeComponent();
            TypeOfRequestBox.Items.Add("Проверка счетчика");
            TypeOfRequestBox.Items.Add("Замена счетчика");
            TypeOfRequestBox.Items.Add("Пломбирование");
            TypeOfRequestBox.Items.Add("Сан. Ремонт");
            TypeOfStreetBox.Items.Add("Улица");
            TypeOfStreetBox.Items.Add("Проспект");
            TypeOfStreetBox.Items.Add("Переулок");
            TypeOfStreetBox.Items.Add("Проезд");
            TypeOfStreetBox.Items.Add("Бульвар");
        }

        private void AddRequestButton_Click(object sender, RoutedEventArgs e)
        {
            MainWindow wnd = new MainWindow();
            TransportationMap route = new TransportationMap();
            route.AddRequest(MainWindow.pathToFile, this);
            Close();
        }
    }
}
