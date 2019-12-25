using PostCSP.eDocLib;
using PostCSP.Tsa;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading;
using System.Windows.Forms;

namespace eDocTest
{
    public partial class Form1 : Form
    {
        private List<TemplateProperty> CustomProperties;
        private BackgroundWorker _worker = new BackgroundWorker();
        private OpenFileDialog _openDlg;
        private SaveFileDialog _saveDlg;
        private List<string> files = new List<string>();

        public EdocTemplate Extended { get; }

        public string EDocFileName { get; set; }

        public Form1()
        {
            InitializeComponent();

            _openDlg = new OpenFileDialog
            {                
                RestoreDirectory = true,
                CheckFileExists = true
            };

            _saveDlg = new SaveFileDialog
            {
                AddExtension = true,
                SupportMultiDottedExtensions = false,
                RestoreDirectory = true,
                OverwritePrompt = true,
                Title = "Select file name to save eDoc",
                DefaultExt = ".edoc"
            };

            CustomProperties = new List<TemplateProperty>();

            Extended = new EdocTemplate()
            {
                sectionName = "Extended",
                url = "http://schemas.microsoft.com/edoc/2006/extended-section",
                id = "URN:MICROSOFT:LV:EDOCTEMPLATE:EXTENDED",
                info = new TemplateInfo() { description = "Default Template", organization = "AZ CSP", version = "1" }
            };

            _worker.DoWork += _worker_DoWork;
            _worker.RunWorkerCompleted += _worker_RunWorkerCompleted;
            _worker.WorkerReportsProgress = true;
            _worker.ProgressChanged += _worker_ProgressChanged;
        }

        private void _worker_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            progressBar1.Value = e.ProgressPercentage;
        }

        private void _worker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            bool result = (bool)e.Result;

            if (InvokeRequired)
            {
                btnTest1.Invoke(new MethodInvoker(() => { btnTest1.Enabled = true; btnTest1.Invalidate(); }));
                btnTest1.Invalidate();
            }
            else
            {
                btnTest1.Enabled = true;
                btnTest1.Invalidate();
            }
        }

        private void _worker_DoWork(object sender, DoWorkEventArgs e)
        {
            _worker.ReportProgress(10);

            AddCustomMetadata("PIN", "1QWERTY");
            AddCustomMetadata("Debt", "10.0");

            Extended.properties = CustomProperties.ToArray();

            OpenXMLStorageProvider storageProvider = new OpenXMLStorageProvider();
            EDocument edoc = new EDocument(storageProvider, Extended);

            foreach (string file in files)
                edoc.AddFile(file);

            // 1st option is a basic
            // 2nd timestamped 
            // 3rd verified

            SignatureType sigType = new SignatureType();
            sigType.name = "Gücləndirilmiş elektron imza";

            //List<X509Certificate2> certs = new List<X509Certificate2>();
            //LoadNamedCerts(certs);
            //SignatureOptions sigOptions = new SignatureOptions();
            //sigOptions.Level = SignatureLevel.Basic;
            //sigOptions.SigningCertificate = certs.First();

            _worker.ReportProgress(40);

            List<X509Certificate2> certs = new List<X509Certificate2>();
            LoadTsaCerts(certs);
            SignatureOptions sigOptions = new SignatureOptions
            {
                Level = SignatureLevel.Timestamped,
                SigningCertificate = certs.First(),
                TsaCertificate = certs.First()
            };

            _worker.ReportProgress(60);

            SignatureProperties sigProperties = new SignatureProperties(sigType);
            edoc.Sign(sigOptions, sigProperties);

            CustomProperties.Clear();
            Extended.properties = null;

            edoc.Save(EDocFileName);
            edoc.Dispose();

            _worker.ReportProgress(100);
            Thread.Sleep(500);
            _worker.ReportProgress(0);

            e.Result = true;
        }

        private void AddCustomMetadata(string name, string value)
        {
            TemplateProperty tp = new TemplateProperty()
            {
                name = name,
                defaultValue = value,
                dataType = AllowedTypes.text,
                description = value
            };
            CustomProperties.Add(tp);
        }

        private void btnTest1_Click(object sender, EventArgs e)
        {
            files.Clear();

            _openDlg.Multiselect = true;
            _openDlg.Filter = @"Files (*.doc, *.docx, *.xls, *.xlsx, *.pdf) | *.doc; *.docx; *.xls; *.xlsx; *.pdf";
            _openDlg.Title = "Select files to be added to eDoc";

            var result = _openDlg.ShowDialog();

            if (result == DialogResult.OK)
                if (_openDlg.FileNames?.Length > 0)
                    files.AddRange(_openDlg.FileNames);

            if (files.Count > 0)
            {
                result = _saveDlg.ShowDialog();

                if (result == DialogResult.OK)
                {
                    if (_saveDlg.ValidateNames && _saveDlg.FileName != null)
                    {
                        EDocFileName = _saveDlg.FileName;

                        btnTest1.Enabled = false;

                        if (!_worker.IsBusy)
                            _worker.RunWorkerAsync();
                    }
                }
            }
        }

        private static void LoadNamedCerts(List<X509Certificate2> namedCerts)
        {
            X509Store x509Store = new X509Store();
            x509Store.Open(OpenFlags.OpenExistingOnly);
            X509Certificate2Collection x509Certificate2Collection = x509Store.Certificates.Find(X509FindType.FindByTimeValid, DateTime.Now, true);

            foreach (X509Certificate2 cert in x509Certificate2Collection)
            {
                if (SignatureHelper2.IsRightUsageSigning(cert))
                {
                    cert.SetPinForPrivateKey("85586433");
                    namedCerts.Add(cert);
                }
            }
        }

        private static void LoadTsaCerts(List<X509Certificate2> tsaCerts)
        {
            X509Store x509Store = new X509Store();
            x509Store.Open(OpenFlags.OpenExistingOnly);
            X509Certificate2Collection x509Certificate2Collection = x509Store.Certificates.Find(X509FindType.FindByTimeValid, DateTime.Now, true);

            foreach (X509Certificate2 cert in x509Certificate2Collection)
            {
                if (SignatureHelper2.IsRightUsageAuthentication(cert))
                {
                    cert.SetPinForPrivateKey("85586433");
                    tsaCerts.Add(cert);
                }
            }
        }

        private void btnLoadDoc_Click(object sender, EventArgs e)
        {
            _openDlg.Multiselect = false;
            _openDlg.Filter = @"eDocument (*.edoc) | *.edoc;";
            _openDlg.Title = "Select eDoc file to open";

            var result = _openDlg.ShowDialog();

            if (result == DialogResult.OK && 
                _openDlg.FileName != null)
            {
                #region extended values

                EdocTemplate extended = new EdocTemplate();
                extended.sectionName = "Extended";
                extended.url = "http://schemas.microsoft.com/edoc/2006/extended-section";
                extended.id = "URN:MICROSOFT:LV:EDOCTEMPLATE:EXTENDED";
                extended.info = new TemplateInfo() { description = "Default Template", organization = "AZ CSP", version = "1" };

                extended.properties = new TemplateProperty[] { new TemplateProperty(), new TemplateProperty() };

                extended.properties[0].name = "PIN";
                extended.properties[0].dataType = AllowedTypes.text;

                extended.properties[1].name = "Debt";
                extended.properties[1].dataType = AllowedTypes.text;

                #endregion

                EDocument edoc = new EDocument(new OpenXMLStorageProvider(), extended);
                edoc.Open(_openDlg.FileName);

                #region getting files from edoc

                AddRow("Files", string.Empty);

                foreach (FileSection sect in edoc.Files)
                    AddRow(sect.Name, $"Size {sect.Size.ToString()}");

                #endregion

                #region getting signatures from edoc

                AddRow("Signatures", string.Empty);

                foreach (SignatureSection sect in edoc.Signatures)
                {
                    ActionStatus actionStatus = ActionStatus.Unknown;

                    if (sect.VerificationReport != null)
                        actionStatus = sect.VerificationReport.Status;

                    var sigName = sect.Name;
                    var sigTag = sect.Id;

                    string timestamp = null;
                    if (sect.TimeStamp != null)
                        timestamp = sect.TimeStamp.Time.ToLocalTime().ToString();

                    var actStatus = actionStatus.ToString();

                    AddRow("Name", sigName);
                    AddRow("Tag", sigTag);
                    AddRow("Timestamp", timestamp);
                    AddRow("ActionStatus", actStatus);
                }

                #endregion

                #region getting metadata from edoc

                EdocTemplate customTemplate = edoc.CustomTemplate;
                bool flag2 = string.Compare(customTemplate.id, EDocument.DefaultTemplate.id, true) == 0;
                AddRow("Description", customTemplate.info.description);

                if (!flag2)
                {
                    AddRow("Id", customTemplate.id);
                    AddRow("Url", customTemplate.url);
                    AddRow("Orgnization", customTemplate.info.organization);
                }

                AddRow("Version", customTemplate.info.version);

                foreach (EdocTemplate edocTemplate in edoc.Templates)
                {
                    if ("core".Equals(edocTemplate.sectionName.ToLower()) || "base".Equals(edocTemplate.sectionName.ToLower()))
                        AddRow(edocTemplate.sectionName, string.Empty);
                    else
                        AddRow(edocTemplate.sectionName, string.Empty);

                    foreach (TemplateProperty templateProperty in edocTemplate.properties)
                    {
                        //AddRow("T.Description", templateProperty.description);

                        //if ("core".Equals(edocTemplate.sectionName.ToLower()) || "base".Equals(edocTemplate.sectionName.ToLower()))
                        //AddRow("T.SecName", string.Format("{0}Property_{1}", edocTemplate.sectionName, templateProperty.name.Replace(' ', '_')));

                        string text2 = edoc.GetMetadataValue(templateProperty.name);

                        DateTime dateTime;

                        if (DateTime.TryParseExact(text2, "yyyy-MM-dd\\THH-mm-sszz", DateTimeFormatInfo.InvariantInfo, DateTimeStyles.None, out dateTime))
                            text2 = dateTime.ToString();
                        if (templateProperty.@readonly)
                            text2 = $"{text2} (read-only)";
                        else
                        {
                            if (null != templateProperty.choice)
                                text2 = string.Join(",", templateProperty.choice.value);

                        }

                        if (!templateProperty.IsValid(text2))
                            text2 = $"{text2} (invalid)";

                        AddRow(templateProperty.description, text2);
                    }
                }

                var extData = edoc.Metadata.Where(x => "Extended".Equals(x.Name)).FirstOrDefault();

                foreach (KeyValuePair<string, PropertyData> kvp in extData.Properties)
                    AddRow(kvp.Key, kvp.Value?.ReadOnlyValue);

                #endregion

            }
        }

        private void AddRow(string propertyText, string valueText)
        {
            var index = dgvProperties.Rows.Add();
            dgvProperties.Rows[index].Cells["Property"].Value = propertyText;
            dgvProperties.Rows[index].Cells["Value"].Value = valueText;
        }
    }
}
