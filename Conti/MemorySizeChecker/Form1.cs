using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml.Linq;

namespace MemStackSizeChecker
{
    public partial class Form1 : Form
    {   
        string fileContent = string.Empty;
        
        Block[] bk = new Block[400];
        //String configfile = @"NvM.xdm";
        //String mapfile = @"AZ8.map";
        int counter = 0;

        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            //inputfile.Text = @"D:\WorKing\FeeDumpSoft\MemStackSizeChecker\FeeDumpAnalyzerInfo\dumpfee.txt";
            succes.Visible = false;
            error1.Visible = false;
            error2.Visible = false;
            output.Visible = false;
            CheckConfigFile();
        }

        public void CheckConfigFile()
        {
            if (File.Exists(configfile))
            {
                //string[] files = Directory.GetDirectories(@"Config\");               
            }
            else
                MessageBox.Show("Config file (.xdm) not found!!!");

            if (File.Exists(mapfile))
            {
                //string[] files = Directory.GetDirectories(@"Config\");
            }
            else
                MessageBox.Show("Map file (.map) not found !!!");

        }

        public void GetConfigFile()
        {
            
        }

        public void GetItems()
        {
                counter = 0;
                String BlockName = null;
                String RamBlockName = null;
                String BlockLength = null;
                String BlockLengthBuffer = null;
                
                var lines = File.ReadAllLines(configfile);
                for (int i = 0; i < lines.Length; i += 1)
                {
                    if (lines[i].Contains("NvMRamBlockDataAddress") && !lines[i].Contains(">"))
                    {
                        if (lines[i + 1].Contains("value=")) 
                        {
                            RamBlockName = lines[i + 1].Replace(" ", "").Replace("\"", "").Replace("value=", "").Replace("&amp;", "").Replace(">", "").Trim();
                        }
                        int pointup = i;
                        int check = 0;
                        BlockLength = null;
                        BlockName = null;
                        do
                        {
                            pointup--;                           
                            int pointdown = pointup;
                            if (lines[pointup].Contains("IDENTIFIABLE") && lines[pointup].Contains("<d:ctr name="))
                            {
                                BlockName = lines[pointup].Replace("<d:ctr name=", "").Replace("type=", "").Replace("IDENTIFIABLE", "").Replace("\"", "").Replace(">", "").Trim();
                                do
                                {
                                    pointdown++;
                                    if (lines[pointdown].Contains("NvMNvBlockLength"))
                                    {
                                        BlockLength = lines[pointdown].Replace("<d:var name=", "").Replace("NvMNvBlockLength", "").Replace("\"", "").Replace("type=", "").Replace("INTEGER", "").Replace("value=", "").Replace(">", "").Replace("/", "").Trim();
                                    }
                                } while (!lines[pointdown].Contains("NvMNameOfFeeBlock"));
                            }
                            else
                            { check = 1; }
                        } while (!lines[pointup].Contains("IDENTIFIABLE"));
                        if (check == 1) 
                        {
                            pointup--;
                            int pointdown = pointup;
                            if (lines[pointup].Contains("name="))
                            {
                                BlockName = lines[pointup].Replace("<d:ctr name=", "").Replace("type=", "").Replace("IDENTIFIABLE", "").Replace("\"", "").Replace(">", "").Trim();
                                do
                                {
                                    pointdown++;
                                    if (lines[pointdown].Contains("NvMNvBlockLength"))
                                    {
                                        BlockLength = lines[pointdown].Replace("<d:var name=", "").Replace("NvMNvBlockLength", "").Replace("\"", "").Replace("type=", "").Replace("INTEGER", "").Replace("value=", "").Replace(">", "").Replace("/", "").Trim();
                                    }
                                } while (!lines[pointdown].Contains("NvMNameOfFeeBlock"));
                            }
                        }
                        if (BlockLength == null)
                        {
                            BlockLength = "undefined! - pleasy verify manually by RamBlockName !!!";
                        }
                        else
                        {
                            BlockLength = GetHex(BlockLength);
                            //BlockLength = GetDecimal(BlockLength);
                        }
                        BlockLengthBuffer = "undefined!";
                        bk[counter] = new Block(BlockName, RamBlockName, BlockLength, BlockLengthBuffer);
                        counter++;
                    }
                }
          
                var lines2 = File.ReadAllLines(mapfile);
                for (int i = 0; i < lines2.Length; i += 1)
                {
                    for (int k = 0; k < bk.Length; k++) 
                    {
                        if (bk[k] != null)
                        {
                            if (lines2[i].Contains(bk[k].RamBlockName + " "))
                            {
                                string[] allId = lines2[i].Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                                if (allId.Length>1) 
                                {
                                    if (allId[1].Length>2) 
                                    {
                                        BlockLengthBuffer = System.Text.RegularExpressions.Regex.Split(allId[allId.Length - 1], "\t")[1];
                                    }                                   
                                }                                                            
                                bk[k].BlockLengthBuffer = BlockLengthBuffer;                                
                            }
                        }
                    }
                }          
                int l = counter;              
        }

        
        public string GetHex(string blockumber)
        {
            int intVal = Int32.Parse(blockumber);
            string hex = intVal.ToString("X8");
            return hex.ToLower();
        }

        public string GetDecimal(string blockumber)
        {
            int intVal = Convert.ToInt32(blockumber, 16);
            string dec = intVal.ToString();
            return dec.ToLower();
        }
        
        private void browse_Click(object sender, EventArgs e)
        {
            error1.Visible = false;
            using (OpenFileDialog openFileDialog = new OpenFileDialog())
            {
                openFileDialog.InitialDirectory = "";
                openFileDialog.Filter = "txt files (*.txt)|*.txt|All files (*.*)|*.*";
                openFileDialog.FilterIndex = 2;
                openFileDialog.RestoreDirectory = true;

                if (openFileDialog.ShowDialog() == DialogResult.OK)
                {
                    //Get the path of specified file
                    inputfile.Text = openFileDialog.FileName;

                    //Read the contents of the file into a stream
                    var fileStream = openFileDialog.OpenFile();

                    using (StreamReader reader = new StreamReader(fileStream))
                    {
                        fileContent = reader.ReadToEnd();
                    }
                }
            }
            string path = Path.GetExtension(inputfile.Text);
           
        }

        private void run_Click(object sender, EventArgs e)
        {
            var bgw = new BackgroundWorker();
            succes.Visible = true;
            succes.Text = "Please wait >< , we work in the background....";

            bgw.DoWork += (_, __) =>
            {
                
                GetItems();
            };
            bgw.RunWorkerCompleted += (_, __) =>
            {
                succes.Text = "It,s OK!";              
                output.Visible = true;
            };
            bgw.RunWorkerAsync();
            //GetConfigFile();
            //string e2 = Path.GetExtension(filedatabase);
            //if (e2 == ".xdm")
            //{
            
                //GetByte();
            //}
            //else
               // error1.Visible = true;
        }

        private void output_Click(object sender, EventArgs e)
        {
            Form form = new Form();
            form.Location = new Point(100, 200);
            form.Name = "Result";
            form.Width = 1100;
            form.Height = 600;
            form.BackColor = Color.DarkBlue;

            Label lb = new Label();
            lb.Text = "Block Name:";
            lb.ForeColor = Color.White;
            lb.Location = new Point(20, 10);
            form.Controls.Add(lb);

            Label lbinfo = new Label();
            lbinfo.Text = "Information:";
            lbinfo.ForeColor = Color.White;
            lbinfo.Location = new Point(400, 10);
            form.Controls.Add(lbinfo);

            ListBox list = new ListBox();
            list.Height = 500;
            list.Width = 350;
            list.HorizontalScrollbar = true;
            list.Location = new Point(20, 40);           
            form.Controls.Add(list);            

            TextBox tbox = new TextBox();
            tbox.Height = 500;
            tbox.Width = 660;
            tbox.ScrollBars = ScrollBars.Vertical;
            tbox.Multiline = Enabled;
            tbox.Font = new Font(tbox.Font.FontFamily, 10);
            tbox.Location = new Point(400, 40);
            form.Controls.Add(tbox);

            Button all = new Button();
            all.Height = 20;
            all.Width = 80;
            all.Text = "Show All";
            all.BackColor = Color.White;
            all.ForeColor = Color.Black;
            all.Location = new Point(980, 10);
            all.Click += (se, ev) => ShowAll(sender, e, tbox);
            form.Controls.Add(all);

            Button all2 = new Button();
            all2.Height = 20;
            all2.Width = 180;
            all2.Text = "Change value in NvM";
            all2.BackColor = Color.White;
            all2.ForeColor = Color.Red;
            all2.Location = new Point(800, 10);
            all2.Click += (se, ev) => ChangeValue(sender, e, tbox);
            form.Controls.Add(all2);           

            for (int i = 0; i < bk.Length; i++)
            {
                if (bk[i] != null && bk[i].BlockLength != bk[i].BlockLengthBuffer)
                { 
                    list.Items.Add(bk[i].BlockName);                    
                }
            }

            list.SelectedIndexChanged += (se, ev) => list_SelectedIndexChanged(sender, e, tbox, list.SelectedItem.ToString());
            form.Show();
            succes.Visible = false;
            output.Visible = false;
        }

        private void list_SelectedIndexChanged(object sender, EventArgs e, TextBox tb , String s)
        {
            tb.Clear();
            for (int i = 0; i < bk.Length; i++)
            {
                if (bk[i] != null)
                {
                    if (s == bk[i].BlockName)
                    {
                        //if(bk[i].BlockLength == bk[i].BlockLengthBuffer)
                        //    tb.Text = "BlockName : " + bk[i].BlockName + System.Environment.NewLine + "RamBlockName : " +
                        //    bk[i].RamBlockName + System.Environment.NewLine + "BlockLength : " + bk[i].BlockLength + System.Environment.NewLine +
                        //    "BlockLengthBuffer : " + bk[i].BlockLengthBuffer + System.Environment.NewLine + "It's Ok ./";
                        //else
                            tb.Text = "BlockName : " + bk[i].BlockName + System.Environment.NewLine + "RamBlockName : " +
                            bk[i].RamBlockName + System.Environment.NewLine + "BlockLength : " + bk[i].BlockLength + System.Environment.NewLine +
                            "BlockLengthBuffer : " + bk[i].BlockLengthBuffer + System.Environment.NewLine + "BlockLength must change (x).........";
                    }
                }
            }
            tb.Refresh();
        }

        private void ShowAll(object sender, EventArgs e, TextBox tb)
        {
            tb.Clear();
            for (int i = 0; i < bk.Length; i++)
            {
                if (bk[i] != null && bk[i].BlockLength != bk[i].BlockLengthBuffer)
                {
                    tb.Text = tb.Text + "BlockName : " + bk[i].BlockName + System.Environment.NewLine + "RamBlockName : " +
                        bk[i].RamBlockName + System.Environment.NewLine + "BlockLength : " + bk[i].BlockLength + System.Environment.NewLine +
                        "BlockLengthBuffer : " + bk[i].BlockLengthBuffer + System.Environment.NewLine + System.Environment.NewLine;
                }
            }
            tb.Refresh();
        }

        private void ChangeValue(object sender, EventArgs e, TextBox tb)
        {
            var bgw = new BackgroundWorker();
            tb.Clear();
            tb.Text = "Please wait....we work in the background >:) ";
            string box_content = "";
            bgw.DoWork += (_, __) =>
            {
                string localRamName = null;
                string content = null;
                var lines = File.ReadAllLines(configfile);
                for (int i = 0; i < lines.Length; i += 1)
                {
                    if (lines[i].Contains("NvMRamBlockDataAddress") && !lines[i].Contains(">"))
                    {
                        if (lines[i + 1].Contains("value="))
                        {
                            localRamName = lines[i + 1].Replace(" ", "").Replace("\"", "").Replace("value=", "").Replace("&amp;", "").Replace(">", "").Trim();
                        }
                        int pointup = i;
                        do
                        {
                            pointup--;
                            if (lines[pointup].Contains("NvMNvBlockLength"))
                            {
                                string localblocklength = lines[pointup].Replace("<d:var name=", "").Replace("NvMNvBlockLength", "").Replace("\"", "").Replace("type=", "").Replace("INTEGER", "").Replace("value=", "").Replace(">", "").Replace("/", "").Trim();
                                for (int k = 0; k < bk.Length; k += 1)
                                {
                                    if (bk[k] != null)
                                    {
                                        if (localRamName != null && localRamName == bk[k].RamBlockName)
                                        {
                                            if (localblocklength != GetDecimal(bk[k].BlockLengthBuffer))
                                            {
                                                lines[pointup] = lines[pointup].Replace(localblocklength, GetDecimal(bk[k].BlockLengthBuffer));
                                                box_content = box_content + " Ram Block Name : " + bk[k].RamBlockName + "  Block Length : " + localblocklength + " change with " + GetDecimal(bk[k].BlockLengthBuffer) + " !!! " + System.Environment.NewLine;
                                            }
                                        }
                                    }
                                }
                            }
                        } while (!lines[pointup].Contains("IDENTIFIABLE"));
                    }
                }
                for (int i = 0; i < lines.Length; i += 1)
                {
                    content = content + lines[i] + System.Environment.NewLine;
                }
                System.IO.File.WriteAllText(configfile, content);
                
            };
            bgw.RunWorkerCompleted += (_, __) =>
            {
                tb.Clear();
                tb.Text = box_content;
                tb.Refresh();
                MessageBox.Show("Finish! NvM was succesfully updated!");
            };
            bgw.RunWorkerAsync();

            
        }

        private void button1_Click(object sender, EventArgs e)
        {
            MessageBox.Show("Soon......");
        }
    }
}
