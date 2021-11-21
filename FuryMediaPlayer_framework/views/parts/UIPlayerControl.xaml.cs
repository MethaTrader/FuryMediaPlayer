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
    /// Логика взаимодействия для UIPlayerControl.xaml
    /// </summary>
    public partial class UIPlayerControl : Page
    {
        public UIPlayerControl()
        {
            InitializeComponent();
        }

        private void Path_MouseEnter(object sender, MouseEventArgs e)
        {
            Path uiElement = sender as Path;
            uiElement.Fill = new SolidColorBrush(Color.FromArgb(255, 167, 44, 67));
        }

        private void Path_MouseLeave(object sender, MouseEventArgs e)
        {
            Path uiElement = sender as Path;
            uiElement.Fill = new SolidColorBrush(Color.FromArgb(255, 239, 63, 97));
        }
    }
}
