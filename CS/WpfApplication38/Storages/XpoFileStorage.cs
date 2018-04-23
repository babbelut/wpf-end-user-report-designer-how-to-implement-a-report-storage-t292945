using DevExpress.Data.Filtering;
using DevExpress.Xpf.Reports.UserDesigner;
using DevExpress.Xpf.Reports.UserDesigner.Native;
using DevExpress.Xpo;
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
    public class XpoFileStorage : IReportFileStorage
    {
         XPCollection<StorageItem> items;
         public XpoFileStorage(UnitOfWork session)
             : base()
         {
             items = new XPCollection<StorageItem>(session);
        }
        public string GetErrorMessage(Exception exception)
        {
            return ExceptionHelper.GetInnerErrorMessage(exception);
        }

        public DevExpress.XtraReports.UI.XtraReport Load(string filePath)
        {
            // Get a StorageItem containing the report.
            StorageItem item = FindItem(filePath);
            if (item != null)
                return XtraReport.FromStream(new MemoryStream(item.Layout), true);
            else
            {
                XtraReport report = new XtraReport();
                report.Bands.Add(new DetailBand());
                return report;
            }
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
            // Write the report to a corresponding StorageItem.
            // If a StorageItem with a specified Url property value does not exist, create a new one.
            StorageItem item = FindItem(url);
            if (item != null)
                item.Layout = GetBuffer(report);
            else
            {
                item = new StorageItem(Session);
                item.Url = url;
                Session.CommitChanges();

                report.Extensions["StorageID"] = item.Oid.ToString();
                item.Layout = GetBuffer(report);
            }
            Session.CommitChanges();
            items.Reload();
        }
        byte[] GetBuffer(XtraReport report)
        {
            using (MemoryStream stream = new MemoryStream())
            {
                report.SaveLayout(stream);
                return stream.ToArray();
            }
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
            foreach (StorageItem item in Items)
                if (method == null || method(item.Oid.ToString()))
                    list.Add(item.Url);
            return list;
        }
        UnitOfWork Session
        {
            get { return (UnitOfWork)items.Session; }
        }
        public StorageItem FindItem(string name)
        {
            return Session.FindObject<StorageItem>(new BinaryOperator("Url", name));
        }
        public XPCollection<StorageItem> Items
        {
            get { return items; }
        }


    }

    public class StorageItem : XPObject
    {
        string url;
        byte[] layout = null;
        public string Url
        {
            get { return url; }
            set { SetPropertyValue("Url", ref url, value); }
        }
        public byte[] Layout
        {
            get { return layout; }
            set { SetPropertyValue("Layout", ref layout, value); }
        }
        public StorageItem(Session session)
            : base(session)
        {
        }
    }
}
