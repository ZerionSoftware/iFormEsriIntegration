using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
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
using Excel;
using iFormBuilderAPI;
using iFormTools;

namespace iFormBuilder_Toolbox
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            this.txtFileToUpload_Copy.Text = Utilities.iFormConfigFile;
        }

        private void btnUploadFile_Click_1(object sender, RoutedEventArgs e)
        {
            Microsoft.Win32.OpenFileDialog dlg = new Microsoft.Win32.OpenFileDialog();
            dlg.FileName = "Document"; // Default file name 
            dlg.DefaultExt = ".xls"; // Default file extension 
            dlg.Filter = "Excel Worksheets (*.xls, *.xlsx)|*.xls;*.xlsx"; // Filter files by extension 
            // Show open file dialog box 
            Nullable<bool> result = dlg.ShowDialog();

            // Process open file dialog box results 
            if (result == true)
            {
                // Open document 
                this.txtFileToUpload.Text = dlg.FileName;

               // this.dgExcelFile.BeginInit();
                //this.dgExcelFile.ItemsSource = this.GridData;
                //this.dgExcelFile.Items.Refresh();
                //this.dgExcelFile.EndInit();
            }
        }

        private string iFormConfigFile
        {
            get { return this.txtFileToUpload_Copy.Text.Length == 0 ? Utilities.iFormConfigFile : this.txtFileToUpload_Copy.Text; }
        }

        private void btnConfigLoad_Click(object sender, RoutedEventArgs e)
        {
            Microsoft.Win32.OpenFileDialog dlg = new Microsoft.Win32.OpenFileDialog();
            dlg.FileName = "Document"; // Default file name 
            dlg.DefaultExt = ".xml"; // Default file extension 
            dlg.Filter = "Xml File (*.xml)|*.xml"; // Filter files by extension 
            // Show open file dialog box 
            Nullable<bool> result = dlg.ShowDialog();

            // Process open file dialog box results 
            if (result == true)
            {
                // Open document 
                this.txtFileToUpload_Copy.Text = dlg.FileName;
            }
        }

        public DataView GridData
        {
            get
            {
                if (!this.txtFileToUpload.Text.Contains("xls"))
                    return null;

                FileStream stream = File.Open(this.txtFileToUpload.Text, FileMode.Open, FileAccess.Read);

                //1. Reading from a binary Excel file ('97-2003 format; *.xls)
                //IExcelDataReader excelReader = ExcelReaderFactory.CreateBinaryReader(stream);
                //...
                //2. Reading from a OpenXml Excel file (2007 format; *.xlsx)
                IExcelDataReader excelReader = ExcelReaderFactory.CreateOpenXmlReader(stream);
                excelReader.IsFirstRowAsColumnNames = true;
                //...
                //3. DataSet - The result of each spreadsheet will be created in the result.Tables
                DataSet result = excelReader.AsDataSet();

                //...
                //4. DataSet - Create column names from first row
                excelReader.IsFirstRowAsColumnNames = true;
                //DataSet result = excelReader.AsDataSet();

                return result.Tables[0].DefaultView;
            }
        }

        private void btnUploadDoc_Click(object sender, RoutedEventArgs e)
        {
            iFormBuilder api = new iFormBuilder();
            api.ReadConfiguration(this.iFormConfigFile);
            UploadExcelFile uploadFile = new UploadExcelFile(api.iformconfig);
            List<OptionList> options = uploadFile.CreateOptionList(this.txtFileToUpload.Text);

            StackPanel stack = new StackPanel();
            stack.Orientation = Orientation.Horizontal;
            Grid optGrid = new Grid();
            // <Grid Margin="10">
            foreach (OptionList optList in options)
            {
                //DataGrid rootGrid = new DataGrid();

                //DataGridTextColumn c1 = new DataGridTextColumn();
                //c1.Header = "Key";
                //c1.Binding = new Binding("Key");
                //c1.Width = 110;
                //rootGrid.Columns.Add(c1);
                //DataGridTextColumn c2 = new DataGridTextColumn();
                //c2.Header = "Value";
                //c2.Width = 110;
                //c2.Binding = new Binding("Value");
                //rootGrid.Columns.Add(c2);
                //rootGrid.ItemsSource = optList.OptionsDictionary;
                //stack.Children.Add(rootGrid);
            }
            //stack.Children.Add(
            OptionListGrid.Children.Add(stack);
        }

        private TextBlock CreateTextBlock(string text, double height, Thickness margin, int row, int column)
        {
            TextBlock tb = new TextBlock() { Text = text, Height = height, Margin = margin };
            Grid.SetColumn(tb, column);
            Grid.SetRow(tb, row);

            return tb;
        }

        private TextBox CreateTextBox(Thickness margin, int row, int column)
        {
            TextBox tb = new TextBox() { Margin = margin };
            Grid.SetColumn(tb, column);
            Grid.SetRow(tb, row);

            return tb;
        }

        private void ProgressBar_ValueChanged_1(object sender, RoutedPropertyChangedEventArgs<double> e)
        {

        }
    }
}
