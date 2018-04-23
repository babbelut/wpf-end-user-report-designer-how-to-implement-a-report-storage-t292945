Imports Microsoft.VisualBasic
Imports DevExpress.Utils.Zip
Imports DevExpress.Xpf.Reports.UserDesigner
Imports DevExpress.Xpf.Reports.UserDesigner.Native
Imports DevExpress.XtraReports.UI
Imports System
Imports System.Collections.Generic
Imports System.ComponentModel
Imports System.IO
Imports System.Linq
Imports System.Text
Imports System.Windows

Namespace WpfApplication38
	Public Class ZipFileStorage
		Implements IReportFileStorage

			Private Class ZipFilesHelper
				Implements IDisposable
			Private stream As Stream
			Private zipFiles_Renamed As New InternalZipFileCollection()
			Public ReadOnly Property ZipFiles() As InternalZipFileCollection
				Get
					Return zipFiles_Renamed
				End Get
			End Property
			Public Sub New(ByVal path As String)
				If File.Exists(path) Then
					stream = File.OpenRead(path)
					zipFiles_Renamed = InternalZipArchive.Open(stream)
				End If
			End Sub
			Public Overridable Sub Dispose() Implements IDisposable.Dispose
				If stream IsNot Nothing Then
					stream.Dispose()
				End If
			End Sub
			End Class
		Private Const fileName As String = "ReportStorage.zip"
		Public Sub New()
		End Sub
        Public Function GetErrorMessage(ByVal exception As Exception) As String Implements IReportFileStorage.GetErrorMessage
            Return ExceptionHelper.GetInnerErrorMessage(exception)
        End Function

        Public Function Load(ByVal filePath As String) As DevExpress.XtraReports.UI.XtraReport Implements IReportFileStorage.Load
            ' Open ZIP archive.
            Using helper As New ZipFilesHelper(StoragePath)
                ' Read a file with a specified URL from the archive.
                Dim zipFile As InternalZipFile = GetZipFile(helper.ZipFiles, filePath)
                If zipFile IsNot Nothing Then
                    Return XtraReport.FromStream(New MemoryStream(GetBytes(zipFile)), True)
                Else
                    Dim report As New XtraReport()
                    report.Bands.Add(New DetailBand())
                    Return report
                End If
            End Using
        End Function

        Public Sub Save(ByVal filePath As String, ByVal report As DevExpress.XtraReports.UI.XtraReport) Implements IReportFileStorage.Save
            TypeDescriptor.GetProperties(GetType(XtraReport))("DisplayName").SetValue(report, filePath)
            SetData(report, filePath)
        End Sub

        Public Function ShowOpenDialog(ByVal designer As DevExpress.Xpf.Reports.UserDesigner.Native.IReportDesignerUI) As String Implements IReportFileStorage.ShowOpenDialog
            ' Show the report selection dialog and return a URL for a selected report.
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
			Using helper As New ZipFilesHelper(StoragePath)
				For Each item As InternalZipFile In helper.ZipFiles
					If method Is Nothing OrElse method(item.FileName) Then
						list.Add(item.FileName)
					End If
				Next item
				Return list
			End Using
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
		Private ReadOnly Property StoragePath() As String
			Get
				Dim dirName As String = Path.GetDirectoryName(System.AppDomain.CurrentDomain.BaseDirectory)
				Return Path.Combine(dirName, fileName)
			End Get
		End Property
		Public Sub SetData(ByVal report As XtraReport, ByVal url As String)
			report.Extensions("StorageID") = url
			SaveArchive(url, GetBuffer(report))
		End Sub

		Private Sub SaveArchive(ByVal url As String, ByVal buffer() As Byte)
			Dim tempPath As String = Path.ChangeExtension(StoragePath, "tmp")
			' Create a new ZIP archive.
			Using arch As New InternalZipArchive(tempPath)
				' Open a ZIP archive where report files are stored.
				Using helper As New ZipFilesHelper(StoragePath)
					Dim added As Boolean = False
					' Copy all report files to a new archive.
					' Update a file with a specified URL.
					' If the file does not exist, create it.
					For Each item As InternalZipFile In helper.ZipFiles
						If StringsEgual(item.FileName, url) Then
							arch.Add(item.FileName, DateTime.Now, buffer)
							added = True
						Else
							arch.Add(item.FileName, DateTime.Now, GetBytes(item))
						End If
					Next item
					If (Not added) Then
						arch.Add(url, DateTime.Now, buffer)
					End If
				End Using
			End Using
			' Replace the old ZIP archive with the new one.
			If File.Exists(StoragePath) Then
				File.Delete(StoragePath)
			End If
			File.Move(tempPath, StoragePath)
		End Sub

		Private Shared Function StringsEgual(ByVal a As String, ByVal b As String) As Boolean
			Return String.Equals(a, b, StringComparison.OrdinalIgnoreCase)
		End Function

		Private Shared Function GetBytes(ByVal zipFile As InternalZipFile) As Byte()
			Return GetBytes(zipFile.FileDataStream, CInt(Fix(zipFile.UncompressedSize)))
		End Function
		Private Shared Function GetBytes(ByVal stream As Stream, ByVal length As Integer) As Byte()
			Dim result(length - 1) As Byte
			stream.Read(result, 0, result.Length)
			Return result
		End Function

		Private Function GetBuffer(ByVal report As XtraReport) As Byte()
			Using stream As New MemoryStream()
				report.SaveLayout(stream)
				Return stream.ToArray()
			End Using
		End Function
		Private Shared Function GetZipFile(ByVal zipFiles As InternalZipFileCollection, ByVal url As String) As InternalZipFile
			For Each item As InternalZipFile In zipFiles
				If StringsEgual(item.FileName, url) Then
					Return item
				End If
			Next item
			Return Nothing
		End Function
	End Class
End Namespace
