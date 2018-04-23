using DevExpress.Xpo;
using DevExpress.XtraReports.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace WpfApplication38
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            this.Loaded += MainWindow_Loaded;
        }

        void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            reportDesigner.ReportStorage = new DataSetFileStorage();

            //Uncomment this line to register a report storage that uses XPO.
            // string conn = DevExpress.Xpo.DB.MSSqlConnectionProvider.GetConnectionString(@"your server name", string.Empty);
            // XpoDefault.DataLayer = XpoDefault.GetDataLayer(conn, DevExpress.Xpo.DB.AutoCreateOption.DatabaseAndSchema);
            // reportDesigner.FileStorage = new XpoFileStorage(new UnitOfWork(XpoDefault.DataLayer));
    
            // Uncomment this line to register a report storage, which uses Zip file.
            // reportDesigner.FileStorage = new ZipFileStorage();
        }
    }

}
