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

namespace CanUpdaterGui
{
    public class CanFrame
    {
        public uint Id { get; set; }
        public byte[] Payload { get; set; } = null!;
        public DateTime Timestamp { get; set; }
    }

    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public CanFrame[] Frames { get; set; } =
        {
            new() {Id = 0x12341, Payload = new byte[] {1, 2, 3, 4, 5, 6, 7, 8}},
            new() {Id = 0x871, Payload = new byte[] {12}}
        };

        public MainWindow()
        {
            InitializeComponent();
        }
    }
}