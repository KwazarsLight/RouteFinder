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
    /// Interaction logic for SetCarsCountWindow.xaml
    /// </summary>
    public partial class SetCarsCountWindow : Window
    {
        public SetCarsCountWindow()
        {
            InitializeComponent();
        }

        private void SetCarsCountButton_Click(object sender, RoutedEventArgs e)
        {
            MainWindow.carsCount = Int32.Parse(CarsCountBox.Text);
            Close();
        }
    }
}
