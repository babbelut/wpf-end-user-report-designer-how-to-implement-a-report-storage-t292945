# WPF End-User Report Designer - How to Implement a Report Storage


<p>This example demonstrates how to implement report storage to persist <a href="http://documentation.devexpress.com/XtraReports/CustomDocument2592.aspx">report definitions</a> in a database or in any other custom location. This enables your end-users to create and customize reports using the <a href="https://documentation.devexpress.com/#XtraReports/CustomDocument114104">End-User Designer for WPF</a> and have a common target for saving and sharing all reports. This functionality is accomplished through the <strong>DevExpress.Xpf.Reports.UserDesigner.IReportStorage</strong> interface. The interface provides the following methods:</p>
<p>• bool CanCreateNew();</p>
<p>Indicates whether or not it is possible to create a new tab with a blank report in the designer.</p>
<p>• bool CanOpen();</p>
<p>Indicates where or not the "Open" command is available.</p>
<p>• XtraReport CreateNew();</p>
<p>Provides the capability to customize a new report template.</p>
<p>• XtraReport CreateNewSubreport();</p>
<p>Provides the capability to customize a new <a href="https://documentation.devexpress.com/#XtraReports/CustomDocument5175">subreport</a> report template (i.e., a new report opened by double-clicking a specific XRSubreport control).</p>
<p>• string GetErrorMessage(Exception exception);</p>
<p>Provides the capability to display an error message for any encountered exception (a general one or the exception message if you expect that the user can understand and react based on this information).</p>
<p>• string Open(IReportDesignerUI designer);</p>
<p>This method expects a unique ID of the report selected by an end-user via a custom dialog.</p>
<p>• XtraReport Load(string reportID, IReportSerializer designerReportSerializer);</p>
<p>This method passes the report ID that has been selected at the previous step and expects the actual report instance to be loaded and returned. You may or may not use the <strong>IReportSerializer</strong> functionality to save or load a given report from a stream.</p>
<p>• string Save(string reportID, IReportProvider reportProvider, bool saveAs, string reportTitle, IReportDesignerUI designer);</p>
<p>This method is intended to save the currently edited reports. The method's parameters are:</p>
<p>- <em>reportID</em> is a unique ID of the edited report (<strong>null</strong> if it is a new report with no ID specified);</p>
<p>- <em>reportProvider</em> allows you to access the actual report instance being edited and optionally rename it (a new name will be updated in the designer as well);</p>
<p>- <em>saveAs</em> indicates which particular command has been executed ("Save" or "Save As").</p>
<p>- <em>reportTitle</em> represents the actual report title (the <strong>XtraReport.DisplayName</strong> property value).</p>
<p>- <em>designer</em> is the actual report designer instance (a <strong>DevExpress.Xpf.Reports.UserDesigner.ReportDesigner</strong> object).</p>
<p> </p>
<p><strong>See also</strong>: <a href="https://www.devexpress.com/Support/Center/Example/Details/E2704">Report Storage for the WinForms End-User Report Designer</a>.</p>

<br/>


