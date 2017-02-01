using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using BackupUtility;
using System.IO;
using System.Xml;

namespace BackupGUI
{
    public partial class Form1 : Form
    {
        bool autoMode = false;
        public Form1()
        {
            InitializeComponent();
            if (File.Exists(Application.StartupPath + "\\lastfile.txt"))
                loadJobs(File.ReadAllText(Application.StartupPath + "\\lastfile.txt"));
        }

        public Form1(string[] args)
        {
            InitializeComponent();
            if (args.Length != 0)
            {
                string s = args[0];
                loadJobs(s);
                autoMode = true;
                button3_Click(null, null);
            }
            else
            {
                if (File.Exists(Application.StartupPath + "\\lastfile.txt"))
                    loadJobs(File.ReadAllText(Application.StartupPath + "\\lastfile.txt"));
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            string source, destination;
            folderBrowser.Description = "Choose the source folder";
            if (folderBrowser.ShowDialog() == DialogResult.OK)
            {
                source = folderBrowser.SelectedPath;

                folderBrowser.Description = "Choose the destination folder";
                if (folderBrowser.ShowDialog() == DialogResult.OK)
                    destination = folderBrowser.SelectedPath;
                else
                    return;
            }
            else
                return;

            ListViewObject lvo = new ListViewObject();
            lvo.Source = source;
            lvo.Destination = destination;
            listBox1.Items.Add(lvo);
        }

        private class ListViewObject
        {
            public string Source;
            public string Destination;
            public override string  ToString()
            {
 	             return Source + " -> " + Destination;
            }
        }
        private struct ProgressReport
        {
            public string msg;
            public int numChanges;
            public bool step;
        }

        private void button5_Click(object sender, EventArgs e)
        {
            ListViewObject[] selObjs = new ListViewObject[listBox1.SelectedItems.Count];
            listBox1.SelectedItems.CopyTo(selObjs, 0);
            foreach (ListViewObject selObj in selObjs)
                listBox1.Items.Remove(selObj);
            listBox1.SelectedItems.Clear();
        }

        private void button3_Click(object sender, EventArgs e)
        {
            if (button3.Text == "Go")
            {
                label1.Text = "";
                button3.Text = "Cancel";
                ListViewObject[] lvos = new ListViewObject[listBox1.Items.Count];
                listBox1.Items.CopyTo(lvos, 0);
                backgroundWorker1.RunWorkerAsync(lvos);
            }
            else
            {
                backgroundWorker1.CancelAsync();
            }
        }

        private void backgroundWorker1_DoWork(object sender, DoWorkEventArgs e)
        {
            ListViewObject[] lvos = (ListViewObject[])e.Argument;
            int job = 1;
            foreach (ListViewObject lvo in lvos)
            {
                ProgressReport report = new ProgressReport();
                report.msg = string.Format("Job {0}: Analysing...", job);
                report.numChanges = 0;
                report.step = false;
                backgroundWorker1.ReportProgress(0, report);
                

                BackupUtility.AnalysisResults ar = Utilities.Analyze(lvo.Source, lvo.Destination);

                report.msg = null;
                report.numChanges = ar.Count;
                backgroundWorker1.ReportProgress(0, report);
                report.step = true;
                report.numChanges = 0;
                for (int i = 0; i < ar.Count; i++)
                {
                    if (backgroundWorker1.CancellationPending)
                        return;
                    report.msg = "..." + ar.getRelativeFile(i) + " " + calcFileSize(ar.getOriginalFile(i));                    
                    backgroundWorker1.ReportProgress(0, report);
                    Utilities.BackupNext(ar);                    
                }
                job++;
            }
        }

        private string calcFileSize(string p)
        {
            FileInfo f = new FileInfo(p);
            long l = f.Length;
            string fmt;
            float fmtNmbr = 0.0F;

            if (l > 1073741824)
            {
                fmt = "GB";
                fmtNmbr = l / 1073741824.0F;
            }
            else if (l > 1048576)
            {
                fmt = "MB";
                fmtNmbr = l / 1048576.0F;
            }
            else if (l > 1024)
            {
                fmt = "KB";
                fmtNmbr = l / 1024.0F;
            }
            else
            {
                fmt = "Bytes";
                fmtNmbr = (float)l;
            }

            return string.Format("({0} {1})", Math.Round(fmtNmbr, 2), fmt);
                
        }

        private void backgroundWorker1_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            ProgressReport report = (ProgressReport)e.UserState;

            if (report.numChanges != 0)
            {
                progressBar1.Maximum = report.numChanges;
                progressBar1.Value = 0;
            }

            if (report.msg != null)
            {
                label1.Text = report.msg;
            }

            if (report.step)
                progressBar1.PerformStep();
        }

        private void backgroundWorker1_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            label1.Text = "Done!";
            button3.Text = "Go";
            progressBar1.Value = 0;
            if (autoMode)
                Close();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            SaveFileDialog dlg = new SaveFileDialog();
            dlg.Title = "Save Backup Configuration";
            dlg.DefaultExt = "job";
            dlg.Filter = "Backup Jobs | *.job";
            if (dlg.ShowDialog() == DialogResult.OK)
            {
                XmlTextWriter writer = new XmlTextWriter(dlg.FileName, Encoding.UTF8);
                writer.WriteStartDocument();
                writer.WriteStartElement("Jobs");
                
                foreach (ListViewObject lvo in listBox1.Items)
                {
                    writer.WriteStartElement("Job");
                    writer.WriteElementString("Source", lvo.Source);
                    writer.WriteElementString("Destination", lvo.Destination);
                    writer.WriteEndElement();
                }
                writer.WriteEndElement();
                writer.WriteEndDocument();
                writer.Close();

                File.WriteAllText(Application.StartupPath + "\\lastfile.txt", dlg.FileName);
            }

            
        }

        private void button4_Click(object sender, EventArgs e)
        {
            OpenFileDialog dlg = new OpenFileDialog();
            dlg.Filter = "Backup Jobs | *.job";
            if (dlg.ShowDialog() == DialogResult.OK)
            {
                listBox1.Items.Clear();
                loadJobs(dlg.FileName);
            }
        }

        private void loadJobs(string p)
        {
            XmlTextReader reader = new XmlTextReader(p);
            reader.Read();
            reader.Read();
            while (reader.Read())
            {
                ListViewObject lvo = new ListViewObject();
                if (reader.Name == "Job")
                {
                    reader.ReadStartElement("Job");
                    lvo.Source = reader.ReadElementString("Source");
                    lvo.Destination = reader.ReadElementString("Destination");
                    listBox1.Items.Add(lvo);
                }
                //reader.ReadEndElement();
            }

            File.WriteAllText(Application.StartupPath + "\\lastfile.txt", p);
        }
    }
}