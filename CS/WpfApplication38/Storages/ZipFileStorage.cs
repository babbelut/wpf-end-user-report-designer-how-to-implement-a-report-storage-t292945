using DevExpress.Utils.Zip;
using DevExpress.Xpf.Reports.UserDesigner;
using DevExpress.Xpf.Reports.UserDesigner.Native;
using DevExpress.XtraReports.UI;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows;

namespace WpfApplication38
{
    public class ZipFileStorage : IReportFileStorage
    {

            class ZipFilesHelper : IDisposable {
            Stream stream;
            InternalZipFileCollection zipFiles = new InternalZipFileCollection();
            public InternalZipFileCollection ZipFiles {
                get {
                    return zipFiles;
                }
            }
            public ZipFilesHelper(string path) {
                if (File.Exists(path)) {
                    stream = File.OpenRead(path);
                    zipFiles = InternalZipArchive.Open(stream);
                }
            }
            public virtual void Dispose() {
                if (stream != null)
                    stream.Dispose();
            }
        }
        const string fileName = "ReportStorage.zip";
        public ZipFileStorage()
        {
        }
        public string GetErrorMessage(Exception exception)
        {
            return ExceptionHelper.GetInnerErrorMessage(exception);
        }

        public DevExpress.XtraReports.UI.XtraReport Load(string filePath)
        {
            // Open ZIP archive.
            using (ZipFilesHelper helper = new ZipFilesHelper(StoragePath))
            {
                // Read a file with a specified URL from the archive.
                InternalZipFile zipFile = GetZipFile(helper.ZipFiles, filePath);
                if (zipFile != null)
                       return XtraReport.FromStream(new MemoryStream(GetBytes(zipFile)), true);
                else
                {
                    XtraReport report = new XtraReport();
                    report.Bands.Add(new DetailBand());
                    return report;
                }
            }
        }

        public void Save(string filePath, DevExpress.XtraReports.UI.XtraReport report)
        {
            TypeDescriptor.GetProperties(typeof(XtraReport))["DisplayName"].SetValue(report, filePath);
            SetData(report, filePath);
        }

        public string ShowOpenDialog(DevExpress.Xpf.Reports.UserDesigner.Native.IReportDesignerUI designer)
        {
            // Show the report selection dialog and return a URL for a selected report.
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
            using (ZipFilesHelper helper = new ZipFilesHelper(StoragePath))
            {
                foreach (InternalZipFile item in helper.ZipFiles)
                    if (method == null || method(item.FileName))
                        list.Add(item.FileName);
                return list;
            }
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
        string StoragePath
        {
            get
            {
                string dirName = Path.GetDirectoryName(System.AppDomain.CurrentDomain.BaseDirectory);
                return Path.Combine(dirName, fileName);
            }
        }
        public void SetData(XtraReport report, string url)
        {
            report.Extensions["StorageID"] = url;
            SaveArchive(url, GetBuffer(report));
        }

        void SaveArchive(string url, byte[] buffer)
        {
            string tempPath = Path.ChangeExtension(StoragePath, "tmp");
            // Create a new ZIP archive.
            using (InternalZipArchive arch = new InternalZipArchive(tempPath))
            {
                // Open a ZIP archive where report files are stored.
                using (ZipFilesHelper helper = new ZipFilesHelper(StoragePath))
                {
                    bool added = false;
                    // Copy all report files to a new archive.
                    // Update a file with a specified URL.
                    // If the file does not exist, create it.
                    foreach (InternalZipFile item in helper.ZipFiles)
                    {
                        if (StringsEgual(item.FileName, url))
                        {
                            arch.Add(item.FileName, DateTime.Now, buffer);
                            added = true;
                        }
                        else
                            arch.Add(item.FileName, DateTime.Now, GetBytes(item));
                    }
                    if (!added)
                        arch.Add(url, DateTime.Now, buffer);
                }
            }
            // Replace the old ZIP archive with the new one.
            if (File.Exists(StoragePath))
                File.Delete(StoragePath);
            File.Move(tempPath, StoragePath);
        }

        static bool StringsEgual(string a, string b)
        {
            return string.Equals(a, b, StringComparison.OrdinalIgnoreCase);
        }

        static byte[] GetBytes(InternalZipFile zipFile)
        {
            return GetBytes(zipFile.FileDataStream, (int)zipFile.UncompressedSize);
        }
        static byte[] GetBytes(Stream stream, int length)
        {
            byte[] result = new byte[length];
            stream.Read(result, 0, result.Length);
            return result;
        }

        byte[] GetBuffer(XtraReport report)
        {
            using (MemoryStream stream = new MemoryStream())
            {
                report.SaveLayout(stream);
                return stream.ToArray();
            }
        }
        static InternalZipFile GetZipFile(InternalZipFileCollection zipFiles, string url)
        {
            foreach (InternalZipFile item in zipFiles)
            {
                if (StringsEgual(item.FileName, url))
                    return item;
            }
            return null;
        }
    }
}
