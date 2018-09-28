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
using Microsoft.Msagl.Drawing;
using Microsoft.Msagl.WpfGraphControl;

namespace GraphDisplay
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        DockPanel Panel = new DockPanel();

        public MainWindow()
        {
            InitializeComponent();
            this.Content = Panel;
            Loaded += MainWindow_Loaded;
        }

        private void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            if (Environment.GetCommandLineArgs().Length >= 1)
            {
                string fileName = Environment.GetCommandLineArgs()[1];
                GraphViewer graphViewer = new GraphViewer();

                if (File.Exists(fileName))
                {
                    graphViewer.BindToPanel(Panel);
                    Graph graph = new Graph();

                    graph = Graph.Read(fileName);
                    graph.Attr.LayerDirection = LayerDirection.TB;
                    graphViewer.Graph = graph;
                }
            }
        }
    }
}
