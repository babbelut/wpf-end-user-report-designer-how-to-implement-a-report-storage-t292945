Imports Microsoft.VisualBasic
Imports DevExpress.Xpo
Imports DevExpress.XtraReports.Extensions
Imports System
Imports System.Collections.Generic
Imports System.Linq
Imports System.Text
Imports System.Windows
Imports System.Windows.Controls
Imports System.Windows.Data
Imports System.Windows.Documents
Imports System.Windows.Input
Imports System.Windows.Media
Imports System.Windows.Media.Imaging
Imports System.Windows.Navigation
Imports System.Windows.Shapes

Namespace WpfApplication38
	''' <summary>
	''' Interaction logic for MainWindow.xaml
	''' </summary>
	Partial Public Class MainWindow
		Inherits Window
		Public Sub New()
			InitializeComponent()
			AddHandler Me.Loaded, AddressOf MainWindow_Loaded
		End Sub

		Private Sub MainWindow_Loaded(ByVal sender As Object, ByVal e As RoutedEventArgs)
			reportDesigner.FileStorage = New DataSetFileStorage()

			' Uncomment this line to register a report storage that uses XPO.
            'Dim conn As String = DevExpress.Xpo.DB.MSSqlConnectionProvider.GetConnectionString("your server name", String.Empty)
            'XpoDefault.DataLayer = XpoDefault.GetDataLayer(conn, DevExpress.Xpo.DB.AutoCreateOption.DatabaseAndSchema)
            'reportDesigner.FileStorage = New XpoFileStorage(New UnitOfWork(XpoDefault.DataLayer))

			' Uncomment this line to register a report storage, which uses Zip file.
            'reportDesigner.FileStorage = New ZipFileStorage()
		End Sub
	End Class

End Namespace
