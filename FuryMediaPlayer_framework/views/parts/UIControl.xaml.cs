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

namespace FuryMediaPlayer_framework.views.parts
{
    /// <summary>
    /// Логика взаимодействия для UIControl.xaml
    /// </summary>
    public partial class UIControl : Page
    {
        public bool isPinned = false;

        public UIControl()
        {
            InitializeComponent();
        }

        public void closeButton_Click(object sender, RoutedEventArgs e)
        {
            Environment.Exit(0);
        }

        private void pinActionButton_Click(object sender, RoutedEventArgs e)
        {
            isPinned = !isPinned;
        }
    }
}
