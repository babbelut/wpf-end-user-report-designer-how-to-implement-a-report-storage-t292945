Imports Microsoft.VisualBasic
Imports DevExpress.Xpf.Reports.UserDesigner
Imports DevExpress.Xpf.Reports.UserDesigner.Native
Imports DevExpress.XtraReports.UI
Imports System
Imports System.Collections.Generic
Imports System.ComponentModel
Imports System.Data
Imports System.IO
Imports System.Linq
Imports System.Text
Imports System.Windows

Namespace WpfApplication38
	Public Class DataSetFileStorage
		Implements IReportFileStorage
		Public Sub New()
		End Sub

		Private Const fileName As String = "ReportStorage.xml"
		Private dataSet_Renamed As StorageDataSet

        Public Function GetErrorMessage(ByVal exception As Exception) As String Implements IReportFileStorage.GetErrorMessage
            Return ExceptionHelper.GetInnerErrorMessage(exception)
        End Function

        Public Function Load(ByVal filePath As String) As DevExpress.XtraReports.UI.XtraReport Implements IReportFileStorage.Load
            Dim row As StorageDataSet.ReportStorageRow = FindRow(filePath)
            If row IsNot Nothing Then
                Return XtraReport.FromStream(New MemoryStream(row.Buffer), True)
            Else
                Dim report As New XtraReport()
                report.Bands.Add(New DetailBand())
                Return report
            End If
        End Function

		Private Function FindRow(ByVal url As String) As StorageDataSet.ReportStorageRow
			Dim result() As DataRow = ReportStorage.Select(String.Format("Url = '{0}'", url))
			If result.Length > 0 Then
				Return TryCast(result(0), StorageDataSet.ReportStorageRow)
			End If
			Return Nothing
		End Function

        Public Sub Save(ByVal filePath As String, ByVal report As DevExpress.XtraReports.UI.XtraReport) Implements IReportFileStorage.Save
            TypeDescriptor.GetProperties(GetType(XtraReport))("DisplayName").SetValue(report, filePath)
            SetData(report, filePath)
        End Sub

        Public Function ShowOpenDialog(ByVal designer As DevExpress.Xpf.Reports.UserDesigner.Native.IReportDesignerUI) As String Implements IReportFileStorage.ShowOpenDialog
            Dim form As StorageEditorForm = CreateForm()
            form.textBox1.IsEnabled = False
            Dim result? As Boolean = form.ShowDialog()
            If result.HasValue AndAlso result.Value Then
                Return form.textBox1.Text
            Else
                Return String.Empty
            End If
        End Function


		Private Function CreateForm() As StorageEditorForm
			Dim form As New StorageEditorForm()
			For Each item As String In GetUrls()
				form.listBox1.Items.Add(item)
			Next item
			Return form
		End Function

		Private Function GetUrls() As String()
			Return GetUrlsCore(Nothing).ToArray()
		End Function
		Private Function GetUrlsCore(ByVal method As Predicate(Of String)) As List(Of String)
			Dim list As New List(Of String)()
			For Each row As StorageDataSet.ReportStorageRow In ReportStorage.Rows
				If method Is Nothing OrElse method(row.ID.ToString()) Then
					list.Add(row.Url)
				End If
			Next row
			Return list
		End Function

        Public Function ShowSaveAsDialog(ByVal filePath As String, ByVal reportTitle As String, ByVal designer As DevExpress.Xpf.Reports.UserDesigner.Native.IReportDesignerUI) As String Implements IReportFileStorage.ShowSaveAsDialog
            Dim form As StorageEditorForm = CreateForm()
            form.textBox1.Text = reportTitle
            form.listBox1.IsEnabled = False
            ' Show the save dialog to get a URL for a new report.
            Dim result? As Boolean = form.ShowDialog()
            If result.HasValue AndAlso result.Value Then
                Dim url As String = form.textBox1.Text
                If (Not String.IsNullOrEmpty(url)) AndAlso (Not form.listBox1.Items.Contains(url)) Then

                    Return url
                Else
                    MessageBox.Show("Incorrect report name", "Error", MessageBoxButton.OKCancel, MessageBoxImage.Error)
                End If
            End If
            Return String.Empty
        End Function

		Public Sub SetData(ByVal report As XtraReport, ByVal url As String)
			Dim row As StorageDataSet.ReportStorageRow = FindRow(url)
			' Write the report to a corresponding row in the dataset.
			' If a row with a specified URL field value does not exist, create a new one.
			If row IsNot Nothing Then
				row.Buffer = GetBuffer(report)
			Else
				Dim id As Integer = ReportStorage.Rows.Count
				report.Extensions("StorageID") = id.ToString()
				row = ReportStorage.AddReportStorageRow(id, url, GetBuffer(report))
			End If
			DataSet.WriteXml(StoragePath, XmlWriteMode.WriteSchema)
		End Sub

		Private Function GetBuffer(ByVal report As XtraReport) As Byte()
			Using stream As New MemoryStream()
				report.SaveLayout(stream)
				Return stream.ToArray()
			End Using
		End Function

		Private ReadOnly Property StoragePath() As String
			Get
				Dim dirName As String = Path.GetDirectoryName(System.AppDomain.CurrentDomain.BaseDirectory)
				Return Path.Combine(dirName, fileName)
			End Get
		End Property
		Private ReadOnly Property DataSet() As StorageDataSet
			Get
				If dataSet_Renamed Is Nothing Then
					dataSet_Renamed = New StorageDataSet()
					' Populate a dataset from an XML file specified in fileName.
					If File.Exists(StoragePath) Then
						dataSet_Renamed.ReadXml(StoragePath, XmlReadMode.ReadSchema)
					End If
				End If
				Return dataSet_Renamed
			End Get
		End Property
		Private ReadOnly Property ReportStorage() As StorageDataSet.ReportStorageDataTable
			Get
				Return DataSet.ReportStorage
			End Get
		End Property
	End Class
End Namespace
