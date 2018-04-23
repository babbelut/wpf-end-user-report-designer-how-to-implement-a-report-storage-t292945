Imports Microsoft.VisualBasic
Imports DevExpress.Data.Filtering
Imports DevExpress.Xpf.Reports.UserDesigner
Imports DevExpress.Xpf.Reports.UserDesigner.Native
Imports DevExpress.Xpo
Imports DevExpress.XtraReports.UI
Imports System
Imports System.Collections.Generic
Imports System.ComponentModel
Imports System.IO
Imports System.Linq
Imports System.Text
Imports System.Windows

Namespace WpfApplication38
	Public Class XpoFileStorage
		Implements IReportFileStorage
		 Private items_Renamed As XPCollection(Of StorageItem)
		 Public Sub New(ByVal session As UnitOfWork)
			 MyBase.New()
			 items_Renamed = New XPCollection(Of StorageItem)(session)
		 End Sub
        Public Function GetErrorMessage(ByVal exception As Exception) As String Implements IReportFileStorage.GetErrorMessage
            Return ExceptionHelper.GetInnerErrorMessage(exception)
        End Function

        Public Function Load(ByVal filePath As String) As DevExpress.XtraReports.UI.XtraReport Implements IReportFileStorage.Load
            ' Get a StorageItem containing the report.
            Dim item As StorageItem = FindItem(filePath)
            If item IsNot Nothing Then
                Return XtraReport.FromStream(New MemoryStream(item.Layout), True)
            Else
                Dim report As New XtraReport()
                report.Bands.Add(New DetailBand())
                Return report
            End If
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
			' Write the report to a corresponding StorageItem.
			' If a StorageItem with a specified Url property value does not exist, create a new one.
			Dim item As StorageItem = FindItem(url)
			If item IsNot Nothing Then
				item.Layout = GetBuffer(report)
			Else
				item = New StorageItem(Session)
				item.Url = url
				Session.CommitChanges()

				report.Extensions("StorageID") = item.Oid.ToString()
				item.Layout = GetBuffer(report)
			End If
			Session.CommitChanges()
			items_Renamed.Reload()
		End Sub
		Private Function GetBuffer(ByVal report As XtraReport) As Byte()
			Using stream As New MemoryStream()
				report.SaveLayout(stream)
				Return stream.ToArray()
			End Using
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
			For Each item As StorageItem In Items
				If method Is Nothing OrElse method(item.Oid.ToString()) Then
					list.Add(item.Url)
				End If
			Next item
			Return list
		End Function
		Private ReadOnly Property Session() As UnitOfWork
			Get
				Return CType(items_Renamed.Session, UnitOfWork)
			End Get
		End Property
		Public Function FindItem(ByVal name As String) As StorageItem
			Return Session.FindObject(Of StorageItem)(New BinaryOperator("Url", name))
		End Function
		Public ReadOnly Property Items() As XPCollection(Of StorageItem)
			Get
				Return items_Renamed
			End Get
		End Property


	End Class

	Public Class StorageItem
		Inherits XPObject
		Private url_Renamed As String
		Private layout_Renamed() As Byte = Nothing
		Public Property Url() As String
			Get
				Return url_Renamed
			End Get
			Set(ByVal value As String)
				SetPropertyValue("Url", url_Renamed, value)
			End Set
		End Property
		Public Property Layout() As Byte()
			Get
				Return layout_Renamed
			End Get
			Set(ByVal value As Byte())
				SetPropertyValue("Layout", layout_Renamed, value)
			End Set
		End Property
		Public Sub New(ByVal session As Session)
			MyBase.New(session)
		End Sub
	End Class
End Namespace
