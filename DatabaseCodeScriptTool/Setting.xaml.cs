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

namespace DatabaseCodeScriptTool
{
    /// <summary>
    /// Setting.xaml 的交互逻辑
    /// </summary>
    public partial class Setting : Window
    {
        public int inputType = 1;
        public Setting()
        {
            InitializeComponent();
        }

        public Setting(int type)
        {
            inputType = type;
            InitializeComponent();
            switch (type)
            {
                case 0:
                    this.inputTable.IsChecked = true;
                    this.inputView.IsChecked = true;
                    break;
                case 1:
                    this.inputTable.IsChecked = true;
                    this.inputView.IsChecked = false;
                    break;
                case 2:
                    this.inputTable.IsChecked = false;
                    this.inputView.IsChecked = true;
                    break;
                default:
                    this.inputTable.IsChecked = true;
                    this.inputView.IsChecked = false;
                    break;
            }
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            if ((bool)this.inputTable.IsChecked && (bool)this.inputView.IsChecked)
            {
                inputType = 0;
            }
            else if ((bool)this.inputTable.IsChecked && !((bool)this.inputView.IsChecked))
            { inputType = 1; }
            else if (!((bool)this.inputTable.IsChecked) && (bool)this.inputView.IsChecked)
            {
                inputType = 2;
            }
            else {
                inputType = -1;
            }
            this.Hide();
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {

        }
    }
}
