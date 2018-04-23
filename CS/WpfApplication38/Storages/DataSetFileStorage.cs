
using DevExpress.Xpf.Reports.UserDesigner;
using DevExpress.Xpf.Reports.UserDesigner.Native;
using DevExpress.XtraReports.UI;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows;
using WpfApplication38.Storages;

namespace WpfApplication38
{
    public class DataSetFileStorage : IReportStorage
    {
        public DataSetFileStorage()
        {
        }

        const string fileName = "ReportStorage.xml";
        StorageDataSet dataSet;

        public string GetErrorMessage(Exception exception)
        {
            return ExceptionHelper.GetInnerErrorMessage(exception);
        }


        StorageDataSet.ReportStorageRow FindRow(string id)
        {
            DataRow[] result = ReportStorage.Select(string.Format("ID = '{0}'", id));
            if (result.Length > 0)
                return result[0] as StorageDataSet.ReportStorageRow;
            return null;
        }


        public string Save(string reportID, IReportProvider reportProvider, bool saveAs, string reportTitle, IReportDesignerUI designer)
        {
            XtraReport report = reportProvider.GetReport();
            if (reportID == null)
            {
                reportID = Guid.NewGuid().ToString();
                saveAs = true;
            }
            if (!saveAs)
            {
                  SetData(reportID, reportTitle, report);
            }
            else
            {
                if (ShowSaveAsDialog(ref reportTitle, designer))
                {
                    SetData(reportID, reportTitle, report);
                }
                else return null;
            }

            return reportID;
        }

        public bool ShowSaveAsDialog(ref string recordName, IReportDesignerUI designer)
        {
            StorageEditorForm form = CreateForm();
            form.Owner = Window.GetWindow(designer as DependencyObject);
            form.listBox1.IsEnabled = true;
            form.textBox1.IsEnabled = true;
            bool? result = form.ShowDialog();
            recordName = form.textBox1.Text;
            return result.Value;
        }
      
        public string ShowSaveDialog(string filePath, string reportTitle, IReportDesignerUI designer)
        {
            StorageEditorForm form = CreateForm();
            form.textBox1.Text = reportTitle;
            form.listBox1.IsEnabled = false;
            bool? result = form.ShowDialog();
            if (result.HasValue && result.Value)
            {
                string title = form.textBox1.Text;
                if (!string.IsNullOrEmpty(title))
                {
                    return title;
                }
                else
                {
                    MessageBox.Show("Incorrect report name", "Error", MessageBoxButton.OKCancel, MessageBoxImage.Error);
                }
            }
            return string.Empty;
        }



        public string Open(IReportDesignerUI designer)
        {
            StorageEditorForm form = CreateForm();
            form.Owner = Window.GetWindow(designer as DependencyObject);
            form.textBox1.IsEnabled = false;
            bool? result = form.ShowDialog();
            if (result.HasValue && result.Value)
                return (string)form.textBox1.Tag;
            else return string.Empty;
        }
       

        StorageEditorForm CreateForm()
        {
            StorageEditorForm form = new StorageEditorForm();
            form.listBox1.ItemsSource = this.ReportStorage.DefaultView;
            return form;
        }

        public void SetData(string reportId, string title, XtraReport report)
        {
            StorageDataSet.ReportStorageRow row = FindRow(reportId);
            if (row != null)
                row.Buffer = GetBuffer(report);
            else
            {
                row = ReportStorage.AddReportStorageRow(reportId, title, GetBuffer(report));
            }
            DataSet.WriteXml(StoragePath, XmlWriteMode.WriteSchema);
        }

        byte[] GetBuffer(XtraReport report)
        {
            using (MemoryStream stream = new MemoryStream())
            {
                report.SaveLayout(stream);
                return stream.ToArray();
            }
        }

        string StoragePath {
            get {
                string dirName = Path.GetDirectoryName(System.AppDomain.CurrentDomain.BaseDirectory);
                return Path.Combine(dirName, fileName);
            }
        }
        StorageDataSet DataSet {
            get {
                if (dataSet == null) {
                    dataSet = new StorageDataSet();
                    if (File.Exists(StoragePath))
                        dataSet.ReadXml(StoragePath, XmlReadMode.ReadSchema);
                }
                return dataSet;
            }
        }
        StorageDataSet.ReportStorageDataTable ReportStorage {
            get {
                return DataSet.ReportStorage;
            }
        }

        public bool CanCreateNew()
        {
            return true;
        }

        public bool CanOpen()
        {
            return true;
        }

        public XtraReport CreateNew()
        {
            return new XtraReport1();
        }

        public XtraReport CreateNewSubreport()
        {
            return new XtraReport1();
        }

        public XtraReport Load(string reportID, IReportSerializer designerReportSerializer)
        {
            StorageDataSet.ReportStorageRow row = FindRow(reportID);
            using (MemoryStream ms = new MemoryStream(row.Buffer))
            {
                return XtraReport.FromStream(ms, true);
            }
        }
    }
}
