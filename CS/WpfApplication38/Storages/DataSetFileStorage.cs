
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

namespace WpfApplication38
{
    public class DataSetFileStorage : IReportFileStorage
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

        public DevExpress.XtraReports.UI.XtraReport Load(string filePath)
        {
            StorageDataSet.ReportStorageRow row = FindRow(filePath);
            if (row != null)
                return XtraReport.FromStream(new MemoryStream(row.Buffer), true);
            else
            {
                XtraReport report = new XtraReport();
                report.Bands.Add(new DetailBand());
                return report;
            }
        }

        StorageDataSet.ReportStorageRow FindRow(string url)
        {
            DataRow[] result = ReportStorage.Select(string.Format("Url = '{0}'", url));
            if (result.Length > 0)
                return result[0] as StorageDataSet.ReportStorageRow;
            return null;
        }

        public void Save(string filePath, DevExpress.XtraReports.UI.XtraReport report)
        {
            TypeDescriptor.GetProperties(typeof(XtraReport))["DisplayName"].SetValue(report, filePath);
            SetData(report, filePath);
        }

        public string ShowOpenDialog(DevExpress.Xpf.Reports.UserDesigner.Native.IReportDesignerUI designer)
        {
            StorageEditorForm form = CreateForm();
            form.textBox1.IsEnabled = false;
            bool? result = form.ShowDialog();
            if (result.HasValue && result.Value)
                return form.textBox1.Text;
            else return string.Empty;
        }


        StorageEditorForm CreateForm()
        {
            StorageEditorForm form = new StorageEditorForm();
            foreach (string item in GetUrls())
                form.listBox1.Items.Add(item);
            return form;
        }

        string[] GetUrls()
        {
            return GetUrlsCore(null).ToArray();
        }
        List<string> GetUrlsCore(Predicate<string> method)
        {
            List<string> list = new List<string>();
            foreach (StorageDataSet.ReportStorageRow row in ReportStorage.Rows)
                if (method == null || method(row.ID.ToString()))
                    list.Add(row.Url);
            return list;
        }

        public string ShowSaveAsDialog(string filePath, string reportTitle, DevExpress.Xpf.Reports.UserDesigner.Native.IReportDesignerUI designer)
        {
            StorageEditorForm form = CreateForm();
            form.textBox1.Text = reportTitle;
            form.listBox1.IsEnabled = false;
            // Show the save dialog to get a URL for a new report.
            bool? result = form.ShowDialog();
            if (result.HasValue && result.Value)
            {
                string url = form.textBox1.Text;
                if (!string.IsNullOrEmpty(url) && !form.listBox1.Items.Contains(url))
                {
                   
                    return url;
                }
                else
                {
                    MessageBox.Show("Incorrect report name", "Error", MessageBoxButton.OKCancel, MessageBoxImage.Error);
                }
            }
            return string.Empty;
        }

        public void SetData(XtraReport report, string url)
        {
            StorageDataSet.ReportStorageRow row = FindRow(url);
            // Write the report to a corresponding row in the dataset.
            // If a row with a specified URL field value does not exist, create a new one.
            if (row != null)
                row.Buffer = GetBuffer(report);
            else
            {
                int id = ReportStorage.Rows.Count;
                report.Extensions["StorageID"] = id.ToString();
                row = ReportStorage.AddReportStorageRow(id, url, GetBuffer(report));
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
                    // Populate a dataset from an XML file specified in fileName.
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
    }
}
