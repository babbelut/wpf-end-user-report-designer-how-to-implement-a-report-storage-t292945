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
Imports WpfApplication38.Storages

Namespace WpfApplication38
	Public Class DataSetFileStorage
		Implements IReportStorage

		Public Sub New()
		End Sub

		Private Const fileName As String = "ReportStorage.xml"
        Private storageDS As StorageDataSet

        Public Function GetErrorMessage(ByVal exception As Exception) As String Implements IReportStorage.GetErrorMessage
            Return ExceptionHelper.GetInnerErrorMessage(exception)
        End Function


        Private Function FindRow(ByVal id As String) As StorageDataSet.ReportStorageRow
            Dim result() As DataRow = ReportStorage.Select(String.Format("ID = '{0}'", id))
            If result.Length > 0 Then
                Return TryCast(result(0), StorageDataSet.ReportStorageRow)
            End If
            Return Nothing
        End Function


        Public Function Save(ByVal reportID As String, ByVal reportProvider As IReportProvider, ByVal saveAs As Boolean, ByVal reportTitle As String, ByVal designer As IReportDesignerUI) As String Implements IReportStorage.Save
            Dim report As XtraReport = reportProvider.GetReport()
            If reportID Is Nothing Then
                reportID = Guid.NewGuid().ToString()
                saveAs = True
            End If
            If Not saveAs Then
                SetData(reportID, reportTitle, report)
            Else
                If ShowSaveAsDialog(reportTitle, designer) Then
                    SetData(reportID, reportTitle, report)
                Else
                    Return Nothing
                End If
            End If

            Return reportID
        End Function

        Public Function ShowSaveAsDialog(ByRef recordName As String, ByVal designer As IReportDesignerUI) As Boolean
            Dim form As StorageEditorForm = CreateForm()
            form.Owner = Window.GetWindow(TryCast(designer, DependencyObject))
            form.listBox1.IsEnabled = True
            form.textBox1.IsEnabled = True
            Dim result? As Boolean = form.ShowDialog()
            recordName = form.textBox1.Text
            Return result.Value
        End Function

        Public Function ShowSaveDialog(ByVal filePath As String, ByVal reportTitle As String, ByVal designer As IReportDesignerUI) As String
            Dim form As StorageEditorForm = CreateForm()
            form.textBox1.Text = reportTitle
            form.listBox1.IsEnabled = False
            Dim result? As Boolean = form.ShowDialog()
            If result.HasValue AndAlso result.Value Then
                Dim title As String = form.textBox1.Text
                If Not String.IsNullOrEmpty(title) Then
                    Return title
                Else
                    MessageBox.Show("Incorrect report name", "Error", MessageBoxButton.OKCancel, MessageBoxImage.Error)
                End If
            End If
            Return String.Empty
        End Function



        Public Function Open(ByVal designer As IReportDesignerUI) As String Implements IReportStorage.Open
            Dim form As StorageEditorForm = CreateForm()
            form.Owner = Window.GetWindow(TryCast(designer, DependencyObject))
            form.textBox1.IsEnabled = False
            Dim result? As Boolean = form.ShowDialog()
            If result.HasValue AndAlso result.Value Then
                Return CStr(form.textBox1.Tag)
            Else
                Return String.Empty
            End If
        End Function


        Private Function CreateForm() As StorageEditorForm
            Dim form As New StorageEditorForm()
            form.listBox1.ItemsSource = Me.ReportStorage.DefaultView
            Return form
        End Function

        Public Sub SetData(ByVal reportId As String, ByVal title As String, ByVal report As XtraReport)
            Dim row As StorageDataSet.ReportStorageRow = FindRow(reportId)
            If row IsNot Nothing Then
                row.Buffer = GetBuffer(report)
            Else
                row = ReportStorage.AddReportStorageRow(reportId, title, GetBuffer(report))
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
                If storageDS Is Nothing Then
                    storageDS = New StorageDataSet()
                    If File.Exists(StoragePath) Then
                        storageDS.ReadXml(StoragePath, XmlReadMode.ReadSchema)
                    End If
                End If
                Return storageDS
            End Get
        End Property
		Private ReadOnly Property ReportStorage() As StorageDataSet.ReportStorageDataTable
			Get
                Return DataSet.ReportStorage
			End Get
		End Property

        Public Function CanCreateNew() As Boolean Implements IReportStorage.CanCreateNew
            Return True
        End Function

        Public Function CanOpen() As Boolean Implements IReportStorage.CanOpen
            Return True
        End Function

        Public Function CreateNew() As XtraReport Implements IReportStorage.CreateNew
            Return New XtraReport1()
        End Function

        Public Function CreateNewSubreport() As XtraReport Implements IReportStorage.CreateNewSubreport
            Return New XtraReport1()
        End Function

        Public Function Load(ByVal reportID As String, ByVal designerReportSerializer As IReportSerializer) As XtraReport Implements IReportStorage.Load
            Dim row As StorageDataSet.ReportStorageRow = FindRow(reportID)
            Using ms As New MemoryStream(row.Buffer)
                Return XtraReport.FromStream(ms, True)
            End Using
        End Function
	End Class
End Namespace
