using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Threading;
using System.Text.RegularExpressions;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Data.OleDb;
using Microsoft.VisualBasic;
using System.Drawing.Drawing2D;
using System.Reflection;
using Microsoft.Win32;
using System.Windows.Forms.DataVisualization.Charting;
using System.Globalization;




namespace SPC_Prasy
{
    public partial class Form1 : Form
    {

        String readData = string.Empty;
        String firstLine;
        String secondLine;
        const String FILE_PATH = "C:\\param\\Laminacja_12NC.par";
        Object lockObject = new Object();
        float Efficiency;
        float czasObl;
        float wydajnosc;
        bool readDataBool = false; // if this boolean is False then we read both first line and 2nd line
        String buffor = String.Empty;
        List<RadioButton> radioButtons = new List<RadioButton>();
        List<TextBox> textBoxes = new List<TextBox>();
        float[] IlamAr =  { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 };
        float[] ElamAr = { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 };
        bool TrwaPomiar = false; //informacja ze zmiana wartosci radiobutton jest zainicjowana przez dane z COM1
        bool TabPomiary=true; //informacja potrzebna aby tylko na zakladce pomiary byly pobierane dane z miernika
        bool MeasureGo; //pobieraj dane z COM1 tylko gdy groupbox3 jest widoczny
        DateTime LastMeasured=Convert.ToDateTime("00:00:00"); //czas ostatniego pomiaru
        string[] ListaOperatorow;
        string AppPath;//sciezka exe z ktorego zostal uruchomiony program
        bool LiczPrzestoje = true, warning = false;
        

        public Form1()
        {
            
            InitializeComponent();
            //lame solution..
            textBoxes.Add(textBox2);
            textBoxes.Add(textBox3);
            textBoxes.Add(textBox8);
            textBoxes.Add(textBox7);
            textBoxes.Add(textBox12);
            textBoxes.Add(textBox11);
            textBoxes.Add(textBox10);
            textBoxes.Add(textBox9);
            textBoxes.Add(textBox16);
            textBoxes.Add(textBox15);
            textBoxes.Add(textBox14);
            textBoxes.Add(textBox13);
            radioButtons.Add(radioButton1);
            radioButtons.Add(radioButton2);
            radioButtons.Add(radioButton3);
            radioButtons.Add(radioButton4);
            radioButtons.Add(radioButton5);
            radioButtons.Add(radioButton6);
            radioButtons.Add(radioButton7);
            radioButtons.Add(radioButton8);
            radioButtons.Add(radioButton9);
            radioButtons.Add(radioButton10);
            radioButtons.Add(radioButton11);
            radioButtons.Add(radioButton12);
            radioButtons.Add(radioButton13);
            radioButtons.Add(radioButton14);
            radioButtons.Add(radioButton15);
            radioButtons.Add(radioButton16);
            radioButtons.Add(radioButton17);
            radioButtons.Add(radioButton18);
            radioButtons.Add(radioButton19);
            radioButtons.Add(radioButton20);
            foreach (var radio in radioButtons)
               radio.Text = "";
            AppPath = Application.ExecutablePath.Substring(0, Application.ExecutablePath.Length - 4) + ".exe";
            Registry.SetValue("HKEY_CURRENT_USER\\Software\\SPC_Prasy", "Path", AppPath);
            label114.Text = Assembly.GetEntryAssembly().GetName().Version.ToString();
            SynchroParam();
            
        }

    

        [DllImport("USER32.DLL")]
        public static extern bool SetForegroundWindow(IntPtr hWnd);

        private void SynchroParam()
        {
            //DateTime start = System.DateTime.Now;
           if (Directory.Exists("P:\\"));
            {
            Directory.CreateDirectory("C:\\param");
            Directory.CreateDirectory("C:\\log\\time");
            Directory.CreateDirectory("C:\\log\\spc");
            DirectoryInfo param = new DirectoryInfo("P:\\");
            DirectoryInfo time = new DirectoryInfo("C:\\log\\time");
            DirectoryInfo spc = new DirectoryInfo("C:\\log\\spc");

            foreach (var file in param.GetFiles("*.*"))
            {
                FileInfo destFile = new FileInfo(Path.Combine("C:\\param\\", file.Name));
                if (destFile.Exists)
                {
                    if (file.LastWriteTime > destFile.LastWriteTime)
                    {
                        file.CopyTo("C:\\param\\" + file.Name.ToString(), true);
                    }
                }
                else
                {
                    file.CopyTo("C:\\param\\" + file.Name.ToString(), true);
                }
                
            }

            foreach (var file in time.GetFiles("*.*"))
            {
                FileInfo destFile = new FileInfo(Path.Combine("S:\\time\\", file.Name));
                if (destFile.Exists)
                {
                    if (file.LastWriteTime > destFile.LastWriteTime)
                    {
                        file.CopyTo("S:\\time\\" + file.Name.ToString(), true);
                    }
                }
                else
                {
                    file.CopyTo("S:\\time\\" + file.Name.ToString(), true);
                }
            }

            foreach (var file in spc.GetFiles("*.*"))
            {

                FileInfo destFile = new FileInfo(Path.Combine("S:\\spc\\", file.Name));
                if (destFile.Exists)
                {
                    if (file.LastWriteTime > destFile.LastWriteTime)
                    {
                        file.CopyTo("S:\\spc\\" + spc.Name.ToString(), true);
                    }
                }
                else
                {
                    file.CopyTo("S:\\spc\\" + file.Name.ToString(), true);
                }
                
            }
            }
            
            //TimeSpan diff = System.DateTime.Now-start;
            //if (diff.TotalSeconds < 1.5) System.Threading.Thread.Sleep(1500);
            
            
        }




        private void LabelToRadio() //w zaleznosci od zaznaczonego radio wyswietla odpowienia linie odczytana z miernika
        {
            if (firstLine.Length > 3) { firstLine = firstLine.Substring(6); firstLine = firstLine.Replace(".", ","); } 
            if (secondLine.Length > 3) { secondLine = secondLine.Substring(6); secondLine = secondLine.Replace(".", ","); } 
            
            firstLine.TrimStart(new char[] { '0' });
            secondLine.TrimStart(new char[] { '0' });

           for (int i = 0; i <= radioButtons.Count; i++)
            {
               
               if (radioButtons[i].Checked == true)
                {
                    if (i < 10)
                    {
                        if (float.TryParse(firstLine, out IlamAr[i]))
                        radioButtons[i].Invoke((MethodInvoker)delegate { radioButtons[i].Text = Math.Round(IlamAr[i],2).ToString("f2"); });
                        break;
                        //listBox2.Items[i] = IlamAr[i].ToString();
                      }
                    else
                    {
                        if (float.TryParse(secondLine, out ElamAr[i-10]))
                        radioButtons[i].Invoke((MethodInvoker)delegate { radioButtons[i].Text = Math.Round(ElamAr[i-10], 2).ToString("f2"); });
                        break;
                        //listBox3.Items[i] = ElamAr[i].ToString();
                     }
                }
            }

        }
        public void RestoreCtrlAltDelete()
        {
            RegistryKey regkey;
            string keyValueInt = "0";
            string subKey = "Software\\Microsoft\\Windows\\CurrentVersion\\Policies\\System";

            try
            {
                regkey = Registry.CurrentUser.CreateSubKey(subKey);
                regkey.SetValue("DisableTaskMgr", keyValueInt);
                regkey.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }
        }

        public void KillCtrlAltDelete()
        {
            RegistryKey regkey;
            string keyValueInt = "1";
            string subKey = "Software\\Microsoft\\Windows\\CurrentVersion\\Policies\\System";

            try
            {
                regkey = Registry.CurrentUser.CreateSubKey(subKey);
                regkey.SetValue("DisableTaskMgr", keyValueInt);
                regkey.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            //KillCtrlAltDelete();
            serialPort1.Open();
            WagiZPliku();
            NormyZPliku();
            foreach (var item in comboBox7.Items)
               POdpadu.Items.Add(comboBox7.GetItemText(item));

            
            for (int i =1 ; i < 11; i++)
            {
                chart3.Series["Line1"].Points.AddXY(i, 1.58);
                chart3.Series["Line2"].Points.AddXY(i, 1.62);
            }
        }

        private void serialPort1_DataReceived_1(object sender, System.IO.Ports.SerialDataReceivedEventArgs e)
        {
            if (MeasureGo)
            {
                lock (lockObject)
                {

                    TrwaPomiar = true;//informacja ze radioButtons maja reagowac na zmiane wartosci
                    String readData = String.Empty;
                    //tutaj musi byc jakas metoda do sprawdzenia czy dany string juz jest wypelnhiony do konca wszystko odczytane...
                    //chwilowo zrobie tka lame.
                    while (buffor.Split(Environment.NewLine.ToArray(), 2).Length < 2) //keep looping untill we got new line separator, each output from serial port finishes with line separator
                    {
                        buffor += serialPort1.ReadExisting();
                        Thread.Sleep(10);
                    }
                    var splitBuff = buffor.Split(Environment.NewLine.ToArray(), 2); //ok we can go ahead and split buffor outcome is [0] - full line [1] - rest

                    readData = splitBuff[0]; //copy our wole line to readData
                    buffor = splitBuff[1]; // buffor now becames everything After new line

                    readDataBool = !readDataBool;             //change state of our lame named boolean    

                    if (readDataBool) // if its true then we just read FIRST line.
                    {
                        firstLine = readData;
                    }
                    else // ew are on 2nd line.
                    {
                        secondLine = readData;
                    }
                    if (readDataBool == false) // if bool is False then we just read pair first line/second line, so we can go ahead and update labels.
                    {
                        if (TabPomiary)
                        {
                            LabelToRadio();
                        }
                        else
                        {
                            if (checkBox1.Checked)
                            {
                                MessageBox.Show("Prawidłowy sposób podłączenia:" + Environment.NewLine + "Miernik I laminacji - " + firstLine.Replace("01A-", "") + Environment.NewLine + "Miernik E laminacji - " + secondLine.Replace("02A-", ""));
                                checkBox1.Invoke((MethodInvoker)delegate { checkBox1.Checked = false; });
                            }
                        }
                    }
                }
            }
            else
            {
                serialPort1.DiscardInBuffer();
            }
        }

        private void radioButton1_TextChanged(object sender, EventArgs e)
        {

        }

        private void radioButton2_TextChanged(object sender, EventArgs e)
        {

        }

        private void label1_TextChanged(object sender, EventArgs e)
        {
            //radioButton2.Checked=true;
        }

        private void label2_TextChanged(object sender, EventArgs e)
        {
            //radioButton3.Checked = true;
        }

        private void label3_TextChanged(object sender, EventArgs e)
        {
            //radioButton4.Checked = true;
        }

        private void label4_TextChanged(object sender, EventArgs e)
        {
            //radioButton5.Checked = true;
        }

        private void button1_Click(object sender, EventArgs e)
        {

        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            label118.Text=CultureInfo.CurrentCulture.DateTimeFormat.DayNames[(int)System.DateTime.Now.DayOfWeek];
            label17.Text = DateTime.Now.ToString("HH:mm:ss");
            label111.Text = DateTime.Now.ToString("HH:mm:ss");
            label18.Text = DateTime.Now.ToString("dd/MM/yyyy");
            label110.Text = DateTime.Now.ToString("dd/MM/yyyy");
            label17.Visible = true;
            label18.Visible = true;

            if (label20.Text == "Operator") SumyCzesciowe();

            if (LastMeasured != Convert.ToDateTime("00:00:00"))
            {
                System.Diagnostics.Debug.Write("licze");
                if (Math.Truncate(double.Parse(DateTime.Now.Subtract(LastMeasured).TotalMinutes.ToString()))<10)
                         label23.Text = "0"+Math.Truncate(double.Parse(DateTime.Now.Subtract(LastMeasured).TotalMinutes.ToString())).ToString();
                else
                    label23.Text = Math.Truncate(double.Parse(DateTime.Now.Subtract(LastMeasured).TotalMinutes.ToString())).ToString();
                if (Int32.Parse(label23.Text) > 60)
                    panel2.BackColor = Color.Red;
                else
                    panel2.BackColor = Color.Transparent;
             }

            int CzasOdPomiaru = 0;
            if (Int32.TryParse(label23.Text, out CzasOdPomiaru) && CzasOdPomiaru > 59)
            {
                label23.ForeColor = Color.Red;
            }
            else
            {
                label23.ForeColor = Color.White;
            }
        }

        private void NormyZPliku()  //laduje normy wydajnosci z pliku Normy.par do komponentu w zakldce narzedzia
        {
            if (File.Exists("C:\\param\\Normy.par") == false) MessageBox.Show(@"Błąd Pliku C:\param\Normy.par");
            else
            {
                string[] wierszePliku = System.IO.File.ReadAllLines("C:\\param\\Normy.par");
                listView2.Items.Clear();

                foreach (var line in wierszePliku)
                {
                    var split = Regex.Split(line, ",");
                    string[] row = { split[0].ToString(), split[1].ToString(), split[2].ToString() };
                    ListViewItem item = new ListViewItem(row);
                    listView2.Items.Add(item);
                }
            }
        }

        private float WagaTypu(string typ)
        {
            float waga = 0;
            if (File.Exists("C:\\param\\Waga_laminacji.par") == false) { MessageBox.Show(@"Błąd Pliku C:\param\Waga_laminacji.par");  }
            else
            {
                bool typOK = false;

                if (comboBox4.Text == "A10") waga = 8.36F;
                if (comboBox4.Text == "A12") waga = 12.65F;
                if (comboBox4.Text == "A16") waga = 13.2F;
                else
                {
                    string[] wierszePliku = System.IO.File.ReadAllLines("C:\\param\\Waga_laminacji.par");
                    foreach (var line in wierszePliku)
                    {
                        if (line != ";")
                            if (line.Substring(1, 3) == typ.Substring(0, 3)) typOK = true;

                        if (typOK)
                            if (line.Substring(1, 3) == label51.Text)
                            {
                                var split = Regex.Split(line, ";");
                                waga = float.Parse(split[4]);
                            }
                    }
                }

            }
                return waga;

            
        }



        private void WagiZPliku()
        {
            if (File.Exists("C:\\param\\Waga_laminacji.par") == false) { MessageBox.Show(@"Błąd Pliku C:\param\Waga_laminacji.par"); label120.Visible = true; }
            else
            {
                ListViewGroup A10 = new ListViewGroup("A10");
                ListViewGroup A12 = new ListViewGroup("A12.5");
                ListViewGroup A16 = new ListViewGroup("A16");
                ListViewGroup A18 = new ListViewGroup("A18");
                ListViewGroup A21 = new ListViewGroup("A21");
                string typ = "";
                string[] wierszePliku = System.IO.File.ReadAllLines("C:\\param\\Waga_laminacji.par");

                listView1.Items.Clear();


                foreach (var line in wierszePliku)
                {
                    System.Text.RegularExpressions.Match match = Regex.Match(line, "^[*]");
                    if (match.Success)
                    {
                        typ = line.Substring(1, 3);
                        //MessageBox.Show("typ: " + typ);
                        continue;
                    }
                    else
                    {
                        var split = Regex.Split(line, ";");
                        string[] row = { split[1].ToString(), split[2].ToString(), split[3].ToString(), split[4].ToString() };

                        if (typ == "A10")
                        {
                            listView1.Groups.Add(A10);

                            //ListViewItem item = new ListViewItem() { Text = row, Group = A10 };
                            ListViewItem item = new ListViewItem(row, A10);
                            listView1.Items.Add(item);
                        }

                        if (typ == "A12")
                        {
                            listView1.Groups.Add(A12);

                            //ListViewItem item = new ListViewItem() { Text = row, Group = A10 };
                            ListViewItem item = new ListViewItem(row, A12);
                            listView1.Items.Add(item);
                        }

                        if (typ == "A16")
                        {
                            listView1.Groups.Add(A16);

                            //ListViewItem item = new ListViewItem() { Text = row, Group = A10 };
                            ListViewItem item = new ListViewItem(row, A16);
                            listView1.Items.Add(item);
                        }

                        if (typ == "A18")
                        {
                            listView1.Groups.Add(A18);

                            //ListViewItem item = new ListViewItem() { Text = row, Group = A10 };
                            ListViewItem item = new ListViewItem(row, A18);
                            listView1.Items.Add(item);
                        }

                        if (typ == "A21")
                        {
                            listView1.Groups.Add(A21);

                            //ListViewItem item = new ListViewItem() { Text = row, Group = A10 };
                            ListViewItem item = new ListViewItem(row, A21);
                            listView1.Items.Add(item);
                        }




                    }
                }

            }
        }

        
        private List<String> parseFileForCode(string type)
        {
            List<String> codes = new List<string>();
            if (File.Exists(FILE_PATH))
            {
            string[] wierszePliku = System.IO.File.ReadAllLines(FILE_PATH);
            
            bool correctPartOfFile = false;
            //*/ //A10		szczelina	E-lam		I-lam
             foreach (var line in wierszePliku)
            {
                System.Text.RegularExpressions.Match match = Regex.Match(line, "^\\s*$");

                if (match.Success)
                    continue;
                match = System.Text.RegularExpressions.Regex.Match(line, "^[*][/]\\s+.*");
                if (match.Success)
                    correctPartOfFile = false;
                match = System.Text.RegularExpressions.Regex.Match(line, "^[*][/]\\s+" + type + ".*");
                if (match.Success)
                {
                    correctPartOfFile = true;
                    continue;
                }
                if (correctPartOfFile)
                {
                    //lame solution whatever
                    var split = Regex.Split(line, "\\s+");
                    codes.Add(split[0] + " " + split[1] + " " + split[2]);

                }
            }
             
            
            }
            else
            {
                MessageBox.Show("Błąd pliku "+FILE_PATH);
                
            }
            return codes;
        }

        private List<String> parseFileForPacket(string type)
        {
            string[] wierszePliku = System.IO.File.ReadAllLines(FILE_PATH);
            List<String> codes = new List<string>();
            bool correctPartOfFile = false;
            //*/ //A10		szczelina	E-lam		I-lam
            foreach (var line in wierszePliku)
            {
                System.Text.RegularExpressions.Match match = Regex.Match(line, "^\\s*$");

                if (match.Success)
                    continue;
                match = System.Text.RegularExpressions.Regex.Match(line, "^[*][/]\\s+.*");
                if (match.Success)
                    correctPartOfFile = false;
                match = System.Text.RegularExpressions.Regex.Match(line, "^[*][/]\\s+" + type + ".*");
                if (match.Success)
                {
                    correctPartOfFile = true;
                    continue;
                }
                if (correctPartOfFile)
                {
                    //lame solution whatever
                    var split = Regex.Split(line, "\\s+");
                    codes.Add(split[9]);
                    //MessageBox.Show(split[9].ToString());

                }
            }
                return codes;
            
        }

        private String parseFileForAirGap(string condition)
        {
            string[] wierszePliku = System.IO.File.ReadAllLines(FILE_PATH);
            foreach (var line in wierszePliku)
            {
                condition = condition.Replace(" ", "\\s+");
                condition += ".*";
                Match match = Regex.Match(line, condition);
                if (match.Success)
                {
                    String[] split = Regex.Split(line, "\\s+");

                    return split[6];
                }
            }
            return "";
        } 
        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (comboBox1.SelectedIndex >= 0)
            {
                if (comboBox1.SelectedItem.ToString() == "A10" || comboBox1.SelectedItem.ToString() == "A12" || comboBox1.SelectedItem.ToString() == "A16")
                {
                    label13.Text = "0,04";
                    label10.Visible = true;
                    label10.Text = "1,60";
                    label11.Visible = true;
                    label12.Visible = true;
                    label12.Text = "0,02";
                    radioButton1.Visible = true;
                    radioButton2.Visible = true;
                    radioButton3.Visible = true;
                    radioButton4.Visible = true;
                    radioButton5.Visible = true;
                    radioButton6.Visible = true;
                    radioButton7.Visible = true;
                    radioButton8.Visible = true;
                    radioButton9.Visible = true;
                    radioButton10.Visible = true;
                    radioButton1.Checked = true;
                    comboBox2.Text = "Wybierz";
                    chart3.Visible = true;
                }
                if (comboBox1.SelectedItem.ToString() == "A18" || comboBox1.SelectedItem.ToString() == "A21")
                {
                    label13.Text = "0,07";
                    label10.Visible = false;
                    label10.Text = "0";
                    label11.Visible = false;
                    label12.Visible = false;
                    label12.Text = "0";
                    radioButton1.Visible = false;
                    radioButton2.Visible = false;
                    radioButton3.Visible = false;
                    radioButton4.Visible = false;
                    radioButton5.Visible = false;
                    radioButton6.Visible = false;
                    radioButton7.Visible = false;
                    radioButton8.Visible = false;
                    radioButton9.Visible = false;
                    radioButton10.Visible = false;
                    radioButton11.Checked = true;
                    comboBox2.Text = "Wybierz";
                    chart3.Visible = false;
                }
                comboBox2.Items.Clear();
                var lines = parseFileForCode(comboBox1.SelectedItem.ToString());
                foreach (var line in lines)
                {
                    comboBox2.Items.Add(line);
                }
                return;
            }
        }

        private void comboBox2_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (comboBox2.SelectedIndex >= 0)
            {
                if (comboBox5.SelectedIndex > 0 && comboBox6.SelectedIndex > 0) groupBox3.Visible = true;
                string Airgap;
                string comboVal = comboBox2.SelectedItem.ToString();
                if (comboVal == "") return;
                Airgap = parseFileForAirGap(comboVal);
                label15.Text = Airgap.Replace(".", ",");

                double up = double.Parse(label15.Text) + double.Parse(label13.Text);
                double lower = double.Parse(label15.Text) - double.Parse(label13.Text);


                chart2.Series["Line1"].Points.Clear();
                chart2.Series["Line2"].Points.Clear();
                for (int i = 0; i < 10; i++)
                {
                    chart2.Series["Line1"].Points.AddXY(i + 1, lower);
                    chart2.Series["Line2"].Points.AddXY(i + 1, up);
                }
            }

        }

        private void radioButton1_TextChanged_1(object sender, EventArgs e)
        {
            if (TrwaPomiar)
            {
                float Pomiar = float.Parse(radioButton1.Text);
                float GTol = float.Parse(label10.Text) + float.Parse(label12.Text);
                float DTol = float.Parse(label10.Text) - float.Parse(label12.Text);
                if (Pomiar < DTol || Pomiar > GTol) { radioButton1.BackColor = Color.Red; }
                if (Pomiar >= DTol && Pomiar <= GTol) { radioButton1.BackColor = Color.Green; }
                radioButton2.Checked = true;
                PrzeliczStatystyczne();
                RysujWykres();
            }
            else radioButton1.BackColor = Color.DarkGray;
        }

        private void radioButton2_TextChanged_1(object sender, EventArgs e)
        {
            if (TrwaPomiar)
            {
                float Pomiar = float.Parse(radioButton2.Text);
                float GTol = float.Parse(label10.Text) + float.Parse(label12.Text);
                float DTol = float.Parse(label10.Text) - float.Parse(label12.Text);
                if (Pomiar < DTol || Pomiar > GTol) { radioButton2.BackColor = Color.Red; }
                if (Pomiar >= DTol && Pomiar <= GTol) { radioButton2.BackColor = Color.Green; }
                radioButton3.Checked = true;
                PrzeliczStatystyczne();
                RysujWykres();
            }
            else radioButton2.BackColor = Color.DarkGray;
        }

        private void radioButton3_TextChanged(object sender, EventArgs e)
        {
            if (TrwaPomiar)
            {
                float Pomiar = float.Parse(radioButton3.Text);
                float GTol = float.Parse(label10.Text) + float.Parse(label12.Text);
                float DTol = float.Parse(label10.Text) - float.Parse(label12.Text);
                if (Pomiar < DTol || Pomiar > GTol) { radioButton3.BackColor = Color.Red; }
                if (Pomiar >= DTol && Pomiar <= GTol) { radioButton3.BackColor = Color.Green; }
                radioButton4.Checked = true;
                PrzeliczStatystyczne();
                RysujWykres();
            }
            else radioButton3.BackColor = Color.DarkGray;
        }

        private void radioButton4_TextChanged(object sender, EventArgs e)
        {
            if (TrwaPomiar)
            {
                float Pomiar = float.Parse(radioButton4.Text);
                float GTol = float.Parse(label10.Text) + float.Parse(label12.Text);
                float DTol = float.Parse(label10.Text) - float.Parse(label12.Text);
                if (Pomiar < DTol || Pomiar > GTol) { radioButton4.BackColor = Color.Red; }
                if (Pomiar >= DTol && Pomiar <= GTol) { radioButton4.BackColor = Color.Green; }

                radioButton5.Checked = true;
                PrzeliczStatystyczne();
                RysujWykres();
            }
            else radioButton4.BackColor = Color.DarkGray;
        }

        private void radioButton5_TextChanged(object sender, EventArgs e)
        {
            if (TrwaPomiar)
            {
                float Pomiar = float.Parse(radioButton5.Text);
                float GTol = float.Parse(label10.Text) + float.Parse(label12.Text);
                float DTol = float.Parse(label10.Text) - float.Parse(label12.Text);
                if (Pomiar < DTol || Pomiar > GTol) { radioButton5.BackColor = Color.Red; }
                if (Pomiar >= DTol && Pomiar <= GTol) { radioButton5.BackColor = Color.Green; }
                radioButton6.Checked = true;
                PrzeliczStatystyczne();
                RysujWykres();
            }
            else radioButton5.BackColor = Color.DarkGray;
        }

        private void comboBox1_MouseClick(object sender, MouseEventArgs e)
        {
            if (warning == false)
            foreach (var radio in radioButtons)
            {
                
                    if (radio.Text.Length > 0)
                {
                    MessageBox.Show("Zmiana typu spowoduje utrate wprowadzonych pomiarów. Dokończ pomiary i zapisz je  " + radio.Text + " " + radio.Text.Length);
                    //tutaj user moze wybrac czy chce continue or not, jezeli chce to wyczysc wszystko, else return 
                    break;

                }
                
            }
            warning = true;
        }

        private void button2_Click(object sender, EventArgs e)
        {
              if (Directory.Exists("C:\\log\\time"))
              {
                  SynchroParam();  
                  ListaOperatorow = File.ReadAllLines("C:\\param\\OPERATOR.PAR");
                    bool OperatorOK = false;

                    foreach (var Item in ListaOperatorow)
                    {
                        if (Item == textBox1.Text.ToUpper()) OperatorOK = true;
                    }

                    if (OperatorOK)
                    {
                        panel3.Visible = false;
                        label20.Text = textBox1.Text.ToUpper();
                        label37.Text = textBox1.Text.ToUpper();
                        if (File.Exists("C:\\log\\time\\" + DateTime.Now.Year.ToString() + ".log"))
                        {
                            string[] WierszePliku = File.ReadAllLines("C:\\log\\time\\" + DateTime.Now.Year.ToString() + ".log");
                            float WydPrasy = 0, WydOperatora = 0;
                            int IleWydPrasy = 0, IleWydOper = 0;
                            foreach (var line in WierszePliku)
                            {
                                string[] split = Regex.Split(line, ";");

                                if (Int32.Parse(split[1]) >= (DateTime.Now.DayOfYear - 5))
                                {

                                    IleWydPrasy++;
                                    WydPrasy += float.Parse(split[10].Substring(0, split[10].Length - 1));
                                }
                                
                                if (Int32.Parse(split[1]) >= DateTime.Now.DayOfYear - 5 && label20.Text == split[3])
                                {

                                    IleWydOper++;
                                    WydOperatora += float.Parse(split[10].Substring(0, split[10].Length - 1));
                                }

                            }

                            if (WydPrasy > 0)
                                label103.Text = Math.Round(WydPrasy / IleWydPrasy, 1).ToString() + "%";
                            else
                                label103.Text = "Brak danych";

                            if (WydOperatora > 0)
                                label104.Text = Math.Round(WydOperatora / IleWydOper, 1).ToString() + "%";
                            else
                                label104.Text = "Brak danych";
                            chart1.Visible = false;
                        }
                    }
                    else
                    {
                        MessageBox.Show("Nieprawidłowa nazwa operatora," + Environment.NewLine + "Format: Imię Nazwisko" + Environment.NewLine + "Bez polskich znaków");
                    }
                    
                  
                    
              }
            }

        private void radioButton6_TextChanged(object sender, EventArgs e)
        {
            if (TrwaPomiar)
            {
                float Pomiar = float.Parse(radioButton6.Text);
                float GTol = float.Parse(label10.Text) + float.Parse(label12.Text);
                float DTol = float.Parse(label10.Text) - float.Parse(label12.Text);
                if (Pomiar < DTol || Pomiar > GTol) { radioButton6.BackColor = Color.Red; }
                if (Pomiar >= DTol && Pomiar <= GTol) { radioButton6.BackColor = Color.Green; }
                radioButton7.Checked = true;
                PrzeliczStatystyczne();
                RysujWykres();
            }
            else radioButton6.BackColor = Color.DarkGray;
        }

        private void radioButton7_TextChanged(object sender, EventArgs e)
        {
            if (TrwaPomiar)
            {
                float Pomiar = float.Parse(radioButton7.Text);
                float GTol = float.Parse(label10.Text) + float.Parse(label12.Text);
                float DTol = float.Parse(label10.Text) - float.Parse(label12.Text);
                if (Pomiar < DTol || Pomiar > GTol) { radioButton7.BackColor = Color.Red; }
                if (Pomiar >= DTol && Pomiar <= GTol) { radioButton7.BackColor = Color.Green; }
                radioButton8.Checked = true;
                PrzeliczStatystyczne();
                RysujWykres();
            }
            else radioButton7.BackColor = Color.DarkGray;
        }

        private void radioButton8_TextChanged(object sender, EventArgs e)
        {
            if (TrwaPomiar)
            {
                float Pomiar = float.Parse(radioButton8.Text);
                float GTol = float.Parse(label10.Text) + float.Parse(label12.Text);
                float DTol = float.Parse(label10.Text) - float.Parse(label12.Text);
                if (Pomiar < DTol || Pomiar > GTol) { radioButton8.BackColor = Color.Red; }
                if (Pomiar >= DTol && Pomiar <= GTol) { radioButton8.BackColor = Color.Green; }
                radioButton9.Checked = true;
                PrzeliczStatystyczne();
                RysujWykres();
            }
            else radioButton8.BackColor = Color.DarkGray;
        }

        private void radioButton9_TextChanged(object sender, EventArgs e)
        {
            if (TrwaPomiar)
            {
                float Pomiar = float.Parse(radioButton9.Text);
                float GTol = float.Parse(label10.Text) + float.Parse(label12.Text);
                float DTol = float.Parse(label10.Text) - float.Parse(label12.Text);
                if (Pomiar < DTol || Pomiar > GTol) { radioButton9.BackColor = Color.Red; }
                if (Pomiar >= DTol && Pomiar <= GTol) { radioButton9.BackColor = Color.Green; }
                radioButton10.Checked = true;
                PrzeliczStatystyczne();
                RysujWykres();
            }
            else radioButton9.BackColor = Color.DarkGray;
        }

        private void radioButton10_TextChanged(object sender, EventArgs e)
        {
            if (TrwaPomiar)
            {
                float Pomiar = float.Parse(radioButton10.Text);
                float GTol = float.Parse(label10.Text) + float.Parse(label12.Text);
                float DTol = float.Parse(label10.Text) - float.Parse(label12.Text);
                if (Pomiar < DTol || Pomiar > GTol) { radioButton10.BackColor = Color.Red; }
                if (Pomiar >= DTol && Pomiar <= GTol) { radioButton10.BackColor = Color.Green; }
                radioButton11.Checked = true;
                PrzeliczStatystyczne();
                RysujWykres();
            }
            else radioButton10.BackColor = Color.DarkGray;
        }

        private void radioButton11_TextChanged(object sender, EventArgs e)
        {
            if (TrwaPomiar)
            {
                float Pomiar = float.Parse(radioButton11.Text);
                float GTol = float.Parse(label15.Text) + float.Parse(label13.Text);
                float DTol = float.Parse(label15.Text) - float.Parse(label13.Text);
                if (Pomiar < DTol || Pomiar > GTol) { radioButton11.BackColor = Color.Red; }
                if (Pomiar >= DTol && Pomiar <= GTol) { radioButton11.BackColor = Color.Green; }
                
                radioButton12.Checked = true;
                PrzeliczStatystyczne();
                RysujWykres();
            }
            else radioButton11.BackColor = Color.DarkGray;

        }

        private void radioButton12_TextChanged(object sender, EventArgs e)
        {
            if (TrwaPomiar)
            {
                float Pomiar = float.Parse(radioButton12.Text);
                float GTol = float.Parse(label15.Text) + float.Parse(label13.Text);
                float DTol = float.Parse(label15.Text) - float.Parse(label13.Text);
                if (Pomiar < DTol || Pomiar > GTol) { radioButton12.BackColor = Color.Red; }
                if (Pomiar >= DTol && Pomiar <= GTol) { radioButton12.BackColor = Color.Green; }
                radioButton13.Checked = true;
                PrzeliczStatystyczne();
                RysujWykres();
            }
            else radioButton12.BackColor = Color.DarkGray;

        }

        private void radioButton13_TextChanged(object sender, EventArgs e)
        {
            if (TrwaPomiar)
            {
                float Pomiar = float.Parse(radioButton13.Text);
                float GTol = float.Parse(label15.Text) + float.Parse(label13.Text);
                float DTol = float.Parse(label15.Text) - float.Parse(label13.Text);
                if (Pomiar < DTol || Pomiar > GTol) { radioButton13.BackColor = Color.Red; }
                if (Pomiar >= DTol && Pomiar <= GTol) { radioButton13.BackColor = Color.Green; }
                radioButton14.Checked = true;
                PrzeliczStatystyczne();
                RysujWykres();
            }
            else radioButton13.BackColor = Color.DarkGray;

        }

        private void radioButton14_TextChanged(object sender, EventArgs e)
        {
            if (TrwaPomiar)
            {
                float Pomiar = float.Parse(radioButton14.Text);
                float GTol = float.Parse(label15.Text) + float.Parse(label13.Text);
                float DTol = float.Parse(label15.Text) - float.Parse(label13.Text);
                if (Pomiar < DTol || Pomiar > GTol) { radioButton14.BackColor = Color.Red; }
                if (Pomiar >= DTol && Pomiar <= GTol) { radioButton14.BackColor = Color.Green; }
                radioButton15.Checked = true;
                PrzeliczStatystyczne();
                RysujWykres();
            }
            else radioButton14.BackColor = Color.DarkGray;

        }

        private void radioButton15_TextChanged(object sender, EventArgs e)
        {
            if (TrwaPomiar)
            {
                float Pomiar = float.Parse(radioButton15.Text);
                float GTol = float.Parse(label15.Text) + float.Parse(label13.Text);
                float DTol = float.Parse(label15.Text) - float.Parse(label13.Text);
                if (Pomiar < DTol || Pomiar > GTol) { radioButton15.BackColor = Color.Red; }
                if (Pomiar >= DTol && Pomiar <= GTol) { radioButton15.BackColor = Color.Green; }
                radioButton16.Checked = true;
                PrzeliczStatystyczne();
                RysujWykres();
            }
            else radioButton15.BackColor = Color.DarkGray;

        }

        private void radioButton16_TextChanged(object sender, EventArgs e)
        {
            if (TrwaPomiar)
            {
                float Pomiar = float.Parse(radioButton16.Text);
                float GTol = float.Parse(label15.Text) + float.Parse(label13.Text);
                float DTol = float.Parse(label15.Text) - float.Parse(label13.Text);
                if (Pomiar < DTol || Pomiar > GTol) { radioButton16.BackColor = Color.Red; }
                if (Pomiar >= DTol && Pomiar <= GTol) { radioButton16.BackColor = Color.Green; }
                radioButton17.Checked = true;
                PrzeliczStatystyczne();
                RysujWykres();
            }
            else radioButton16.BackColor = Color.DarkGray;
        }

        private void radioButton17_TextChanged(object sender, EventArgs e)
        {
            if (TrwaPomiar)
            {
                float Pomiar = float.Parse(radioButton17.Text);
                float GTol = float.Parse(label15.Text) + float.Parse(label13.Text);
                float DTol = float.Parse(label15.Text) - float.Parse(label13.Text);
                if (Pomiar < DTol || Pomiar > GTol) { radioButton17.BackColor = Color.Red; }
                if (Pomiar >= DTol && Pomiar <= GTol) { radioButton17.BackColor = Color.Green; }
                radioButton18.Checked = true;
                PrzeliczStatystyczne();
                RysujWykres();
            }
            else radioButton17.BackColor = Color.DarkGray;
        }

        private void radioButton18_TextChanged(object sender, EventArgs e)
        {
            if (TrwaPomiar)
            {
                float Pomiar = float.Parse(radioButton18.Text);
                float GTol = float.Parse(label15.Text) + float.Parse(label13.Text);
                float DTol = float.Parse(label15.Text) - float.Parse(label13.Text);
                if (Pomiar < DTol || Pomiar > GTol) { radioButton18.BackColor = Color.Red; }
                if (Pomiar >= DTol && Pomiar <= GTol) { radioButton18.BackColor = Color.Green; }
                radioButton19.Checked = true;
                PrzeliczStatystyczne();
                RysujWykres();
            }
            else radioButton18.BackColor = Color.DarkGray;
        }

        private void radioButton19_TextChanged(object sender, EventArgs e)
        {
            if (TrwaPomiar)
            {
                float Pomiar = float.Parse(radioButton19.Text);
                float GTol = float.Parse(label15.Text) + float.Parse(label13.Text);
                float DTol = float.Parse(label15.Text) - float.Parse(label13.Text);
                if (Pomiar < DTol || Pomiar > GTol) { radioButton19.BackColor = Color.Red; }
                if (Pomiar >= DTol && Pomiar <= GTol) { radioButton19.BackColor = Color.Green; }
                radioButton20.Checked = true;
                PrzeliczStatystyczne();
                RysujWykres();
            }
            else radioButton19.BackColor = Color.DarkGray;
        }

        private void radioButton20_TextChanged(object sender, EventArgs e)
        {
            if (TrwaPomiar)
            {
                float Pomiar = float.Parse(radioButton20.Text);
                float GTol = float.Parse(label15.Text) + float.Parse(label13.Text);
                float DTol = float.Parse(label15.Text) - float.Parse(label13.Text);
                if (Pomiar < DTol || Pomiar > GTol) { radioButton20.BackColor = Color.Red; }
                if (Pomiar >= DTol && Pomiar <= GTol) { radioButton20.BackColor = Color.Green; }
                button1.Enabled = true;
                PrzeliczStatystyczne();
                RysujWykres();
            }
            else radioButton20.BackColor = Color.DarkGray;
        }

        private void label15_TextChanged(object sender, EventArgs e)
        {
            float Ilam = float.Parse(label10.Text);
            float Elam = float.Parse(label15.Text);

            label30.Text = (Elam - Ilam).ToString("0.00");
           // label30.Text = String.Format("{0,00}",(Elam - Ilam).ToString());

            float IlamTol = float.Parse(label12.Text);
            float ELamTol = float.Parse(label13.Text);
            label28.Text = (IlamTol + ELamTol).ToString();
        }
        private void bringCalcToFront()
        {
            Process[] processes = System.Diagnostics.Process.GetProcessesByName("calc");
            if (processes.Length > 0)
            {
                SetForegroundWindow(processes[0].MainWindowHandle);
            }
            else
            {
                Process calcProcess = new Process();
                calcProcess.StartInfo.FileName = "calc.exe";

                if (calcProcess.Start())
                {
                    bringCalcToFront();
                }
                else
                {
                    MessageBox.Show("Nie udalo sie wystartowac kalkulatora:((((((");
                }
            }
        }
        private void button3_Click(object sender, EventArgs e)
        {

        }

        private void ZapiszPomiar()
        {
            string NazwaPliku=comboBox2.Text+".log";
            string Operator=label20.Text;
            int Zmiana=1;

            int Godzina = Int32.Parse(DateTime.Now.Hour.ToString());
            if (Godzina < 7 && Godzina >= 22) Zmiana = 3;
            if (Godzina >= 7 && Godzina < 14) Zmiana = 1;
            if (Godzina >= 14 && Godzina < 22) Zmiana = 2;

            File.AppendAllText(NazwaPliku, "500;" + DateTime.Now.ToString() + ";\t" + DateTime.Now.ToString("HH:mm:ss") + ";\t" + Operator + ";\t" + Zmiana.ToString() + ";\t" + "1;\t" + radioButton1.Text + ";\t" + radioButton2.Text + ";\t" + radioButton3.Text + ";\t" + radioButton4.Text + ";\t" + radioButton5.Text + ";\t" + radioButton6.Text + ";\t" + radioButton7.Text + ";\t" + radioButton8.Text + ";\t" + radioButton9.Text + ";\t" + radioButton10.Text + ";\t" + radioButton11.Text + ";\t" + radioButton12.Text + ";\t" + radioButton13.Text + ";\t" + radioButton14.Text + ";\t" + radioButton15.Text + ";\t" + radioButton16.Text + ";\t" + radioButton17.Text + ";\t" + radioButton18.Text + ";\t" + radioButton19.Text + ";\t" + radioButton20.Text + ";\t" + Environment.NewLine);
        }

        private void comboBox4_SelectedIndexChanged(object sender, EventArgs e)
        {

            label65.Visible = false;
            label27.Visible = false;

            comboBox3.Items.Clear();
            var lines = parseFileForCode(comboBox4.SelectedItem.ToString());
            foreach (var line in lines)
            {
                comboBox3.Items.Add(line);
            }
            var pakiety=parseFileForPacket(comboBox4.SelectedItem.ToString());
            listBox1.Items.Clear();
            foreach (var line in pakiety)
            {
                listBox1.Items.Add(line.ToString());
            }
            return;
        }

        public static System.Boolean IsNumeric (System.Object Expression)
            {
                if(Expression == null || Expression is DateTime)
                    return false;

                if(Expression is Int16 || Expression is Int32 || Expression is Int64 || Expression is Decimal || Expression is Single || Expression is Double || Expression is Boolean)
                    return true;
   
                try 
                {
                    if(Expression is string)
                        Double.Parse(Expression as string);
                    else
                        Double.Parse(Expression.ToString());
                        return true;
                    } catch {} // just dismiss errors but return false
                    return false;
                }


        private void comboBox7_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (comboBox7.Text == "Inne: (wpisz)") textBox6.Visible = true; else textBox6.Visible = false;
        }

        private void label26_Click(object sender, EventArgs e)
        {

        }

        private void button6_Click(object sender, EventArgs e)
        {
            SynchroParam();
            WagiZPliku();
        }

        private void comboBox8_SelectedIndexChanged(object sender, EventArgs e)
        {
            
        }

        private void listView1_SelectedIndexChanged(object sender, EventArgs e)
        {
            

        if ( IsNumeric(textBox17.Text))
        {
            float WagaE;
            float WagaI;
            float WagaEI;
            float WynikE;
            float WynikI;
            float WynikEI;

            if (listView1.SelectedItems.Count > 0)
            {
                WagaE = float.Parse(listView1.SelectedItems[0].SubItems[1].Text);
                WagaI = float.Parse(listView1.SelectedItems[0].SubItems[2].Text);
                WagaEI = float.Parse(listView1.SelectedItems[0].SubItems[3].Text);
                WynikE=WagaE*float.Parse(textBox17.Text);
                WynikI=WagaI*float.Parse(textBox17.Text);
                WynikEI=WagaEI*float.Parse(textBox17.Text);

                label56.Text = WynikE.ToString();
                label59.Text = WynikI.ToString();
                label55.Text = WynikEI.ToString();
            }

        }
  
        
        }

        private void button7_Click(object sender, EventArgs e)
        {
            //if (Microsoft.VisualBasic.Interaction.InputBox("Prompt", "Title", "Default", -1, -1) == "1") { }

            

            Form2 frm = new Form2(this);
            frm.Show();


        }


        public bool IsProcessOpen(string name)
        {
            foreach (Process clsProcess in Process.GetProcesses())
            {
                if (clsProcess.ProcessName.Contains(name))
                {
                    return true;
                }
            }
            return false;
        }
        private void button8_Click_1(object sender, EventArgs e)
        {
            bringCalcToFront();
           
        }
        
        private bool DanePoprawne()
        {
            bool OK=true;
            if (comboBox3.Text == "" || comboBox3.Text == "Wybierz") {MessageBox.Show("Nie wybrano typu laminacji");OK=false;}
            if (textBox4.Text == "") { MessageBox.Show("Nie podano ilości"); OK = false; }
            if(textBox5.Text != "0" && comboBox7.Text == "") {MessageBox.Show("Podaj przyczynę odpadu");OK=false;}
            if (label117.BackColor==Color.Red) 
            {
                MessageBox.Show("Niska wydajność, podaj przyczyny przestoju"); 
                OK=false;
            }

            if (float.Parse(label64.Text.Remove(label64.Text.Length - 1, 1)) > 150)
            {
                MessageBox.Show("Błąd wpisanych danych! Za duża wydajność!");
                OK = false;
            }

            if (OK==true) return true;
            else return false;


        }

        private float AllButNumberToZero(string TestObj) //wszystko poza liczba zamieni na zero.
        {
            float NumberObj;
            if (float.TryParse(TestObj, out NumberObj))
            {
                return NumberObj;
            }
            else
                return 0; ;
        }

        private void CheckEmpty()
        {
            textBox5.Text=AllButNumberToZero(textBox5.Text).ToString(); //odpad
            textBox2.Text = AllButNumberToZero(textBox2.Text).ToString(); // ten i pozostale to czasy przestoju
            textBox3.Text = AllButNumberToZero(textBox3.Text).ToString();
            textBox8.Text = AllButNumberToZero(textBox8.Text).ToString();
            textBox7.Text = AllButNumberToZero(textBox7.Text).ToString();
            textBox12.Text = AllButNumberToZero(textBox12.Text).ToString();
            textBox11.Text = AllButNumberToZero(textBox11.Text).ToString();
            textBox10.Text = AllButNumberToZero(textBox10.Text).ToString();
            textBox9.Text = AllButNumberToZero(textBox9.Text).ToString();
            textBox16.Text = AllButNumberToZero(textBox16.Text).ToString();
            textBox15.Text = AllButNumberToZero(textBox15.Text).ToString();
            textBox14.Text = AllButNumberToZero(textBox14.Text).ToString();
            textBox13.Text = AllButNumberToZero(textBox13.Text).ToString();
        }

        private void button4_Click(object sender, EventArgs e)
        {
            LiczPrzestoje = false;
            CheckEmpty();
            if (DanePoprawne())
            {
                DataGridViewRow row = new DataGridViewRow();
                row.CreateCells(dataGridView1);
                DataGridViewComboBoxCell comboCell = row.Cells[9] as DataGridViewComboBoxCell;

                row.Cells[0].Value = comboBox3.Text; 
                row.Cells[1].Value = dateTimePicker1.Text;
                row.Cells[2].Value = dateTimePicker2.Text;
                row.Cells[3].Value = Math.Round(czasObl,2).ToString();
                row.Cells[4].Value = textBox4.Text;
                row.Cells[5].Value = label46.Text.Remove(label46.Text.Length-2, 2);
                row.Cells[11].Value = Efficiency.ToString();
                row.Cells[12].Value = WagaTypu(comboBox4.Text + " " + label51.Text);
                
                row.Cells[6].Value = Math.Round(wydajnosc * 100, 1).ToString()+"%";
                if (wydajnosc < 0.87) row.Cells[6].Style.ForeColor = Color.Red;
                if (wydajnosc >= 0.87) row.Cells[6].Style.ForeColor = Color.Yellow;
                if (wydajnosc > 0.9) row.Cells[6].Style.ForeColor = Color.Green;

                if (IsFloat(textBox5.Text))
                    row.Cells[7].Value=textBox5.Text;

                
                row.Cells[8].Value = label47.Text;

                
                comboCell.Value = comboBox7.SelectedItem;

                row.Cells[10].Value = textBox2.Text + ";" + textBox3.Text + ";" + textBox8.Text + ";" + textBox7.Text + ";" + textBox12.Text + ";" + textBox11.Text + ";" + textBox10.Text + ";" + textBox9.Text + ";" + textBox16.Text + ";" + textBox15.Text + ";" + textBox14.Text + ";" + textBox13.Text;
                dataGridView1.Rows.Add(row);    //w koncu dodaj nowy rekord

                //przepisywanie danych do ostatniej zakladki
                PrzeliczPodsumowanie();
                
                //czyszczenie formularza
                comboBox3.Text = "";
                dateTimePicker1.Text = dateTimePicker2.Text;
                dateTimePicker2.Text=dateTimePicker2.Value.AddHours(1).ToString();
                textBox2.Text = "";
                textBox3.Text = "";
                textBox8.Text = "";
                textBox7.Text = "";
                textBox12.Text = "";
                textBox11.Text = "";
                textBox10.Text = "";
                textBox9.Text = "";
                textBox16.Text = "";
                textBox15.Text = "";
                textBox14.Text = "";
                textBox13.Text = "";
                textBox4.Text = "";
                textBox5.Text = "";
                comboBox7.Text = "";
                textBox6.Text = "";
                label64.Width = 1;
                label47.ForeColor = Color.Black;
                label47.Text = "0";
                label117.Text = "";
                label117.BackColor = Color.Green;

                label44.Visible = false;
                label45.Visible = false;
                label41.Visible = false;
                label42.Visible = false;
                label43.Visible = false;
                label46.Visible = false;
                label47.Visible = false;
                label16.Visible = false;
                label64.Visible = false;
                label39.Visible = false;
                label40.Visible = false;
                label50.Visible = false;
                label51.Visible = false;
                label52.Visible = false;
                label107.Visible = true;
                label106.Visible = true;
                dateTimePicker1.Visible = false;
                dateTimePicker2.Visible = false;
                textBox4.Visible = false;
                textBox5.Visible = false;
                groupBox6.Visible = false;
                groupBox1.Visible = false;
                comboBox7.Visible = false;
                comboBox7.SelectedIndex = -1;
                comboBox3.SelectedIndex = -1;
                label119.Visible = false;


            }
            LiczPrzestoje = true;
        }

        private void PrzeliczPodsumowanie()
        {
            float CalaProdukcjaKG = 0, CalaProdukcjaSzt = 0, CalyOdpad = 0, CalyCzas = 0, CalyCzasPrzestojow = 0;

            for (int i = 0; i < dataGridView1.Rows.Count; i++)
            {
                CalaProdukcjaKG += float.Parse(dataGridView1[5, i].Value.ToString());
                CalaProdukcjaSzt += float.Parse(dataGridView1[4, i].Value.ToString());
                CalyOdpad += float.Parse(dataGridView1[7, i].Value.ToString());
                CalyCzas += float.Parse(dataGridView1[3, i].Value.ToString());
                string[] przestoje = Regex.Split(dataGridView1[10, i].Value.ToString(), ";");
                foreach (var item in przestoje)
                {
                    CalyCzasPrzestojow += float.Parse(item);
                }

            }
            float CalaWydajnosc = 0;
            for (int i = 0; i < dataGridView1.Rows.Count; i++)
            {
                CalaWydajnosc += float.Parse(dataGridView1[5, i].Value.ToString()) / CalaProdukcjaKG * float.Parse(dataGridView1[6, i].Value.ToString().Substring(0, dataGridView1[6, i].Value.ToString().Length - 1));
            }

            label102.Text = (Math.Round(CalaWydajnosc, 1)).ToString() + "%";
            if (CalaWydajnosc <= 87) label102.ForeColor = Color.Red;
            if (CalaWydajnosc > 87 && CalaWydajnosc < 90) label102.ForeColor = Color.Yellow;
            if (CalaWydajnosc >= 90) label102.ForeColor = Color.Green;
            

            label94.Text = Math.Round(CalaProdukcjaKG, 0).ToString() + "kg";
            label99.Text = CalaProdukcjaSzt.ToString() + "szt,";
            label95.Text = Math.Round(CalyOdpad, 2).ToString() + "kg";
            label100.Text = Math.Round(CalyOdpad * 100 / CalaProdukcjaKG, 2).ToString() + "%";
            label96.Text = CalyCzas.ToString();
            label98.Text = CalyCzasPrzestojow.ToString() + "min.";
        }

        private void textBox4_TextChanged(object sender, EventArgs e)
        {
            PrzeliczWydajnosc();
            if (textBox5.Text.Length > 0 && IsFloat(textBox5.Text))
            {
                if (label46.Text != "0kg")
                {
                    float ProcOdpad = float.Parse(textBox5.Text) / float.Parse(label46.Text.Remove(label46.Text.Length - 2, 2)) * 100;
                    label47.Text = Math.Round(ProcOdpad, 2).ToString() + "%";
                    if (ProcOdpad > 0.7 && ProcOdpad <= 0.9) label47.ForeColor = Color.Yellow;
                    if (ProcOdpad > 0.9) label47.ForeColor = Color.Red;
                    if (ProcOdpad <= 0.7) label47.ForeColor = Color.Green;
                }
            }
            RealWydajnosc();
        }

        private void PrzeliczWydajnosc()
        {

                DateTime start = DateTime.Parse(dateTimePicker1.Text);
                DateTime end = DateTime.Parse(dateTimePicker2.Text);
                String czas = (end - start).TotalHours.ToString();



                ParseFileForEfficiency();
                int Ilosc;
                bool number = Int32.TryParse(textBox4.Text.Trim(), out Ilosc);

                if (textBox4.Text.Length > 0 && number)
                {

                    if (float.Parse(czas) < 0)
                        czasObl = (24 + float.Parse(czas));
                    else
                        czasObl = float.Parse(czas);

                    wydajnosc = (Ilosc / czasObl) / (Efficiency / 8);

                    float costam = 190 * wydajnosc;
                    int szerokosc = (int)Math.Round(costam);
                    if (szerokosc > 190) szerokosc = 190;



                    if (wydajnosc < 0.87)
                    {
                        label64.BackColor = Color.Red;
                        label119.Visible = true;
                    }
                    if (wydajnosc >= 0.87)
                    {
                        label64.BackColor = Color.Yellow;
                        label119.Visible = false;
                    }
                    if (wydajnosc >= 0.9)
                    {
                        label64.BackColor = Color.Green;
                        label119.Visible = false;
                    }

                    label64.Width = szerokosc;
                    label64.Refresh();
                    label64.Text = Math.Round(wydajnosc * 100, 1).ToString() + "%";
                    label46.Text = (WagaTypu(comboBox4.Text + " " + label51.Text) * float.Parse(textBox4.Text)).ToString() + "kg";
                }
                if (textBox4.Text == "") label46.Text = "0kg";
            
        }

        private void RealWydajnosc()
        {
            if (LiczPrzestoje == true)
            {
                DateTime start = DateTime.Parse(dateTimePicker1.Text);
                DateTime end = DateTime.Parse(dateTimePicker2.Text);
                String czas = (end - start).TotalHours.ToString();
                float Przestoje = 0;

                foreach (var box in textBoxes)
                {
                    if (box.Text != "")
                        Przestoje += float.Parse(box.Text);
                }


                //float Przestoje = float.Parse(textBox2.Text) + float.Parse(textBox3.Text) + float.Parse(textBox8.Text) + float.Parse(textBox7.Text) + float.Parse(textBox12.Text) + float.Parse(textBox11.Text) + float.Parse(textBox10.Text) + float.Parse(textBox9.Text) + float.Parse(textBox16.Text) + float.Parse(textBox15.Text) + float.Parse(textBox14.Text) + float.Parse(textBox13.Text);


                Przestoje = Przestoje / 60;
                ParseFileForEfficiency();
                int Ilosc;
                bool number = Int32.TryParse(textBox4.Text.Trim(), out Ilosc);
                //MessageBox.Show(number.ToString());
                float czasCal;

                if (textBox4.Text.Length > 0 && number)
                {

                    if (float.Parse(czas) < 0)
                        czasCal = (24 + float.Parse(czas));
                    else
                        czasCal = float.Parse(czas);
                    czasCal = czasCal - Przestoje;
                    if (czasCal < 0)
                    {
                        MessageBox.Show("Czas przestoju jest dłuższy od czasu pracy!");
                        return;
                    }
                    float wydajnoscReal;
                    wydajnoscReal = (Ilosc / czasCal) / (Efficiency / 8);





                    if (wydajnoscReal < 0.87)
                        label117.BackColor = Color.Red;
                    else
                    {
                        if (wydajnoscReal < 0.9)
                            label117.BackColor = Color.Yellow;
                        else
                            label117.BackColor = Color.Green;
                    }


                    label117.Text = Math.Round(wydajnoscReal * 100, 1).ToString() + "%";
                }
            }
        }
        private void ParseFileForEfficiency() //wyszukuje norme wydajnosci dla aktualnie wybranego typu laminacji i zapisuje ja do zmiennej Efficiency.
        {
            if (File.Exists("C:\\param\\Normy.par") == false) { MessageBox.Show(@"Błąd pliku C:\param\Normy.par");  }
            else
            {
                if (comboBox4.Text == "A10" || comboBox4.Text == "A12" || comboBox4.Text == "A16")
                    Efficiency = 340;
                else
                {
                    string[] wierszePliku = System.IO.File.ReadAllLines("C:\\param\\Normy.par");
                    string typ;

                    if (label51.Visible)
                        typ = comboBox4.Text + " " + label51.Text;
                    else
                        typ = comboBox4.Text;

                    foreach (var line in wierszePliku)
                    {
                        string[] split = Regex.Split(line, ",");
                        if (split[0] == typ)
                        {
                            Efficiency = int.Parse(split[1]);
                            return;
                        }
                        else
                            continue;
                    }
                }
            }
        }

        private void comboBox3_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (comboBox3.SelectedIndex == -1) return;
            else
            {
                label107.Visible = false;
                label106.Visible = false;
            }

            if (listBox1.Items[comboBox3.SelectedIndex].ToString().Length < 3)
                label51.Text = listBox1.Items[comboBox3.SelectedIndex].ToString() + " ";
            else
                label51.Text = listBox1.Items[comboBox3.SelectedIndex].ToString();

            if (comboBox3.Text != "")
            {
                label44.Visible = true;
                label45.Visible = true;
                label41.Visible = true;
                label42.Visible = true;
                label43.Visible = true;
                label46.Visible = true;
                label47.Visible = true;
                label16.Visible = true;
                label64.Visible = true;
                label39.Visible = true;
                label40.Visible = true;
                label107.Visible = false;
                label106.Visible = false;
                dateTimePicker1.Visible = true;
                dateTimePicker2.Visible = true;
                textBox4.Visible = true;
                textBox5.Visible = true;
                groupBox6.Visible = true;
                comboBox7.Visible = true;
                groupBox1.Visible = true;
            }
            else
            {
                label44.Visible = false;
                label45.Visible = false;
                label41.Visible = false;
                label42.Visible = false;
                label43.Visible = false;
                label46.Visible = false;
                label47.Visible = false;
                label16.Visible = false;
                label64.Visible = false;
                label39.Visible = false;
                label39.Visible = false;
                label107.Visible = true;
                label106.Visible = true;
                dateTimePicker1.Visible = false;
                dateTimePicker2.Visible = false;
                textBox4.Visible = false;
                textBox5.Visible = false;
                groupBox6.Visible = false;
                comboBox7.Visible = false;
                groupBox1.Visible = false;
            }

            if (comboBox4.SelectedItem == "A18" || comboBox4.SelectedItem == "A21")
            {
                label50.Visible = true;
                label51.Visible = true;
                label52.Visible = true;
            }
            else
            {
                label50.Visible = false;
                label51.Visible = false;
                label52.Visible = false;
            }
            
        }

        private void PrzeliczStatystyczne()
        {

            float sredniaI = 0, sredniaE = 0, sumaI = 0, sumaE = 0, maxI = IlamAr[0], minI = IlamAr[0], maxE = ElamAr[0], minE = ElamAr[0];
            int countI=0, countE=0;


            for (int i=0;i < 10;i++)
            {
                    sumaI += IlamAr[i];
                    if (IlamAr[i] > 0) countI++;

                    sumaE += ElamAr[i];
                    if (ElamAr[i] > 0) countE++;
            }
            
            sredniaI = sumaI/countI;
            sredniaE = sumaE/countE;
            label66.Text = Math.Round(sredniaI, 2).ToString("f2");
            label75.Text = Math.Round(sredniaE, 2).ToString("f2");

            label67.Text = Math.Round(IlamAr.Max(), 2).ToString("f2");
            label74.Text = Math.Round(ElamAr.Max(), 2).ToString("f2");
            label68.Text = Math.Round(IlamAr.Min(), 2).ToString("f2");
            label73.Text = Math.Round(ElamAr.Min(),2).ToString("f2");

            //odchylenie standardowe
            float sumyKwRozI = IlamAr.Where(f => f > 0).Select(val => (val - sredniaI) * (val - sredniaI)).Sum();
            float sumyKwRozE = ElamAr.Where(f => f > 0).Select(val => (val - sredniaE) * (val - sredniaE)).Sum();
            double sdI = Math.Round(Math.Sqrt(sumyKwRozI / IlamAr.Count(f => f > 0)), 2);
            double sdE = Math.Round(Math.Sqrt(sumyKwRozE / ElamAr.Count(f => f > 0)), 2);
            label69.Text = Math.Round(sdI,4).ToString("f4");
            label72.Text = Math.Round(sdE,4).ToString("f4");

            if (IlamAr.Count(f => f > 0) > 0) label68.Text = IlamAr.Where(f => f > 0).Min().ToString();
            if (ElamAr.Count(f => f > 0) > 0) label73.Text = ElamAr.Where(f => f > 0).Min().ToString();
            if (IlamAr.Count(f => f > 0) > 0) label67.Text = IlamAr.Where(f => f > 0).Max().ToString();
            if (ElamAr.Count(f => f > 0) > 0) label74.Text = ElamAr.Where(f => f > 0).Max().ToString();

            label77.Text = Math.Round(ElamAr.Min() - IlamAr.Max(),2).ToString("f2");
            label76.Text = Math.Round(ElamAr.Max() - IlamAr.Min(),2).ToString("f2");
        }

        private bool IsInt(string obiekt)
        {
            int bla = 0;
            if (Int32.TryParse(obiekt, out bla))
                return true;
            else return false;
        }

        private bool IsFloat(string obiekt)
        {
            float bla=0;
            if (float.TryParse(obiekt,out bla))
                return true;
            else return false;
        }

        private void textBox5_TextChanged(object sender, EventArgs e)
        {
            if (textBox5.Text.Length > 0 && IsFloat(textBox5.Text))
            {
                if (label46.Text != "0kg")
                {
                    float ProcOdpad=float.Parse(textBox5.Text) / float.Parse(label46.Text.Remove(label46.Text.Length-2,2)) * 100;
                    label47.Text = Math.Round(ProcOdpad,2).ToString() + "%";
                    if (ProcOdpad > 0.7 && ProcOdpad <= 0.9) label47.ForeColor = Color.Yellow;
                    if (ProcOdpad > 0.9) label47.ForeColor = Color.Red;
                    if (ProcOdpad <=0.7) label47.ForeColor = Color.Green;
                }
            }
        }

        private void button3_Click_1(object sender, EventArgs e)
        {
            string NazwaPliku;
            if (Directory.Exists("C:\\log\\time\\"))
                NazwaPliku = "C:\\log\\time\\" + DateTime.Now.Year.ToString() + ".log";
            else
            {
                if (Directory.Exists("C:\\Log\\time\\") == false) Directory.CreateDirectory("C:\\Log\\time\\");
                NazwaPliku = "C:\\Log\\time\\" + DateTime.Now.Year.ToString() + ".log";
                MessageBox.Show("Błąd dostępu do sieci, dane zostaną zapisane lokalnie");
            }
                //string rekord;

            System.Globalization.CultureInfo cul = System.Globalization.CultureInfo.CurrentCulture; //weekNum - numertygodnia
            int weekNum = cul.Calendar.GetWeekOfYear(
                DateTime.Now,
                System.Globalization.CalendarWeekRule.FirstFourDayWeek,
                DayOfWeek.Monday);

           // string combo;

            using (StreamWriter NewFileLine = new StreamWriter(NazwaPliku, true))
            {
                for (int i = 0; i < dataGridView1.RowCount; i++)
                {
                    String record = weekNum.ToString() + ";" +
                        DateTime.Today.ToString("dd/MM/yyyy") + ";" +
                        label37.Text + ";";
                    for (int columnIndex = 0; columnIndex < dataGridView1.ColumnCount-2; ++columnIndex)
                    {
                        DataGridViewComboBoxCell comboCell = dataGridView1[columnIndex,i] as DataGridViewComboBoxCell;
                        if (comboCell != null)
                        {
                            if (comboCell.Value != null) // ?
                                record += comboCell.Value.ToString() + ";";
                            else record += " ;";
                        }
                        else
                        {
                            record += dataGridView1[columnIndex, i].Value.ToString() + ";";
                        }
                    }
                    NewFileLine.WriteLine(record);

                }
                NewFileLine.Close();
                dataGridView1.Rows.Clear();
                dataGridView1.Refresh();
            }
        }

        private void dataGridView1_CellValueChanged(object sender, DataGridViewCellEventArgs e)
        {
            
            for (int i = 0; i < dataGridView1.RowCount; i++)
            {
                DateTime start;
                DateTime end;
                if (DateTime.TryParse(dataGridView1[1, i].Value.ToString(), out start)) { }
                else
                {
                    MessageBox.Show("Nieprawidłowy format godziny!" + Environment.NewLine+"Powinno być: GG:MM, na przykład: 06:00");
                    break; 
                }

                if (DateTime.TryParse(dataGridView1[2, i].Value.ToString(), out end)) { }
                else
                {
                    MessageBox.Show("Nieprawidłowy format godziny!" + Environment.NewLine + "Powinno być: GG:MM, na przykład: 06:00");
                    break;
                }
                
                float ile=float.Parse(dataGridView1[4, i].Value.ToString());
                float czas = float.Parse((end - start).TotalHours.ToString());
                float norma = float.Parse(dataGridView1[11, i].Value.ToString());
                float waga = float.Parse(dataGridView1[12, i].Value.ToString());
                float odpad = float.Parse(dataGridView1[7, i].Value.ToString());

                dataGridView1[3, i].Value = Math.Round(czas,2).ToString();                                      //odswiez czas
                dataGridView1[6, i].Value = Math.Round(((ile / czas) / (norma / 8)*100),1).ToString()+"%";      //odswiez wydajnosc
                dataGridView1[5, i].Value = (ile * waga).ToString();                                            //odswiez wage
                dataGridView1[8, i].Value = Math.Round((odpad / ile) * 100, 2).ToString() + "%";                //odswiez odpad %

                if (float.Parse(dataGridView1[6, i].Value.ToString().Remove(dataGridView1[6, i].Value.ToString().Length - 1, 1)) <= 87) dataGridView1[6, i].Style.ForeColor = Color.Red;
                if (float.Parse(dataGridView1[6, i].Value.ToString().Remove(dataGridView1[6, i].Value.ToString().Length - 1, 1)) > 87) dataGridView1[6, i].Style.ForeColor = Color.Yellow;
                if (float.Parse(dataGridView1[6, i].Value.ToString().Remove(dataGridView1[6, i].Value.ToString().Length - 1, 1)) >= 90) dataGridView1[6, i].Style.ForeColor = Color.Green;
            }
        }

        private void button5_Click(object sender, EventArgs e)
        {

            if (dataGridView1.SelectedRows.Count > 0)
                dataGridView1.Rows.RemoveAt(dataGridView1.SelectedRows[0].Index);
            else
                MessageBox.Show("Zaznacz rząd do skasowania");
            PrzeliczPodsumowanie();
        }

        private void RysujWykres()
        {
            chart3.Series["Ilam"].Points.Clear();
            for (int i = 1; i < 11; i++)
            {
                if (IlamAr[i - 1] > 0) chart3.Series["Ilam"].Points.AddXY(i, IlamAr[i - 1]);
            }

            chart2.Series["Elam"].Points.Clear();
            for (int i = 1; i <= 10; i++)
            {
                if (ElamAr[i-1]>0) chart2.Series["Elam"].Points.AddXY(i, ElamAr[i-1]);
            }
            
        }

        private void button11_Click(object sender, EventArgs e)
        {
           
            


        }

        private void button10_Click(object sender, EventArgs e)
        {

        }

        private void button1_Click_1(object sender, EventArgs e)
        {
            bool DataComplete = true;

            foreach (var radio in radioButtons)
            {
                if (radio.Text == "") DataComplete = false;
            }
            if (DataComplete)
            {
                string NazwaPliku;
                if (Directory.Exists("C:\\log\\spc\\"))
                    NazwaPliku="C:\\log\\spc\\" + comboBox2.Text + ".log";
                else
                    NazwaPliku="C:\\spc\\" + comboBox2.Text + ".log";
                
                string rekord = "500;" + DateTime.Today.ToString("dd/MM/yyyy") + ";" + DateTime.Now.ToString("HH:mm:ss") + ";" + label20.Text + ";";
                int shiftNumber = 0;

                if (DateTime.Now > Convert.ToDateTime("14:00:00") && DateTime.Now < Convert.ToDateTime("22:00:00")) shiftNumber = 2;
                if (DateTime.Now < Convert.ToDateTime("14:00:00") && DateTime.Now > Convert.ToDateTime("06:00:00")) shiftNumber = 1;
                if (DateTime.Now > Convert.ToDateTime("22:00:00") && DateTime.Now < Convert.ToDateTime("06:00:00")) shiftNumber = 3;

                rekord += shiftNumber.ToString() + ";" + comboBox5.Text + ";" + comboBox6.Text + ";";

                foreach (var radio in radioButtons)
                {
                    rekord += radio.Text + ";";
                    radio.Enabled = false;
                    radio.ForeColor = Color.DimGray;
                }

                using (StreamWriter NewFileLine = new StreamWriter(NazwaPliku, true))
                {
                    NewFileLine.WriteLine(rekord);
                }
                //button9.Enabled = true;
                LastMeasured = DateTime.Now;
                panel2.Visible = true;
                label23.Text = "00";
                label91.Text = (Int32.Parse(label91.Text) + 1).ToString();
            }
            else MessageBox.Show("Uzupełnij pomiary");
        }

        private void groupBox3_Enter(object sender, EventArgs e)
        {

        }

        private void comboBox5_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (comboBox2.SelectedIndex >= 0 && comboBox6.SelectedIndex >= 0) groupBox3.Visible = true;
        }

        private void comboBox6_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (comboBox2.SelectedIndex >= 0 && comboBox5.SelectedIndex >= 0) groupBox3.Visible = true;
        }

        private void button9_Click(object sender, EventArgs e)
        {
            TrwaPomiar = false;
            foreach (var radio in radioButtons)
            {
                radio.Text = "";
                radio.Enabled = true;
                radio.ForeColor = Color.Black;
                radio.BackColor = Color.Transparent;
            }
            comboBox2.SelectedIndex = -1;
            comboBox6.SelectedIndex = -1;
            comboBox1.SelectedIndex = -1;
            chart3.Series["Ilam"].Points.Clear();
            chart2.Series["Elam"].Points.Clear();
            //button9.Enabled = false;
            groupBox3.Visible = false;
            label10.Text = "0,00";
            label12.Text = "0,00";
            label13.Text = "0,00";
            label15.Text = "0,00";
            label30.Text = "0,00";
            label28.Text = "0,00";
            chart2.Series["Line1"].Points.Clear();
            chart2.Series["Line2"].Points.Clear();
            Array.Clear(IlamAr, 0, IlamAr.Length);
            Array.Clear(ElamAr, 0, ElamAr.Length);
            warning = false;
        }

        private void tabControl1_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (tabControl1.SelectedTab == tabControl1.TabPages["tabPage1"])
            {
                TabPomiary = true;
                }
            else
            {
                TabPomiary = false;
                }
        }

        private void button11_Click_1(object sender, EventArgs e)
        {

                
        }

        private void groupBox3_VisibleChanged(object sender, EventArgs e)
        {
            if (groupBox3.Visible == true || checkBox1.Checked==true)
                MeasureGo = true;
            else
                MeasureGo = false;
        }

        private void dateTimePicker1_ValueChanged(object sender, EventArgs e)
        {
            PrzeliczWydajnosc();
        }

        private void dateTimePicker2_ValueChanged(object sender, EventArgs e)
        {
            PrzeliczWydajnosc();
        }

        private void tabPage1_Paint(object sender, PaintEventArgs e)
        {
            using (LinearGradientBrush brush = new LinearGradientBrush(this.ClientRectangle,
                                                               Color.WhiteSmoke,
                                                               Color.SteelBlue,
                                                               90F))
            {
                e.Graphics.FillRectangle(brush, this.ClientRectangle);
            }
        }

        private void tabPage2_Paint(object sender, PaintEventArgs e)
        {
            using (LinearGradientBrush brush = new LinearGradientBrush(this.ClientRectangle,
                                                               Color.WhiteSmoke,
                                                               Color.SteelBlue,
                                                               90F))
            {
                e.Graphics.FillRectangle(brush, this.ClientRectangle);
            }
        }

        private void tabPage3_Paint(object sender, PaintEventArgs e)
        {
            using (LinearGradientBrush brush = new LinearGradientBrush(this.ClientRectangle,
                                                   Color.WhiteSmoke,
                                                   Color.DarkSeaGreen,
                                                   90F))
            {
                e.Graphics.FillRectangle(brush, this.ClientRectangle);
            }
        }

        private void tabPage4_Paint(object sender, PaintEventArgs e)
        {
            using (LinearGradientBrush brush = new LinearGradientBrush(this.ClientRectangle,
                                                   Color.WhiteSmoke,
                                                   Color.DimGray,
                                                   90F))
            {
                e.Graphics.FillRectangle(brush, this.ClientRectangle);
            }
        }

        private void label87_Click(object sender, EventArgs e)
        {

        }

        private void button3_Click_2(object sender, EventArgs e)
        {
            bool DaneZapisane=true;
            foreach (var radio in radioButtons)
            {
                if (radio.Text != "") DaneZapisane = false;
            }

            if (DaneZapisane)
            {
                //zapisanie do pliku, nazwa pliku to akutalny rok

                string NazwaPliku = "C:\\log\\time\\" + DateTime.Now.Year.ToString() + ".log";
                if (Directory.Exists("C:\\log\\\time\\"))
                    NazwaPliku = "C:\\log\\time\\" + DateTime.Now.Year.ToString() + ".log";
                else
                    if (Directory.Exists("C:\\Log\\time\\") == false)
                    {
                        if (Directory.Exists("C:\\Log\\time\\")==false) Directory.CreateDirectory("C:\\Log\\time\\");
                        NazwaPliku = "C:\\time\\" + DateTime.Now.Year.ToString() + ".log";
                    }


                System.Globalization.CultureInfo cul = System.Globalization.CultureInfo.CurrentCulture; //weekNum - numertygodnia
                int weekNum = cul.Calendar.GetWeekOfYear(
                    DateTime.Now,
                    System.Globalization.CalendarWeekRule.FirstFourDayWeek,
                    DayOfWeek.Monday);

                using (StreamWriter NewFileLine = new StreamWriter(NazwaPliku, true))
                {
                    for (int i = 0; i < dataGridView1.RowCount; i++)
                    {
                        String record = weekNum.ToString() + ";" +
                            DateTime.Now.DayOfYear.ToString() + ";" +
                            DateTime.Today.ToString("dd/MM/yyyy") + ";" +
                            label37.Text + ";";
                        for (int columnIndex = 0; columnIndex < dataGridView1.ColumnCount - 2; ++columnIndex)
                        {
                            DataGridViewComboBoxCell comboCell = dataGridView1[columnIndex, i] as DataGridViewComboBoxCell;
                            if (comboCell != null)
                            {
                                if (comboCell.Value != null) // ?
                                    record += comboCell.Value.ToString() + ";";
                                else record += " ;";
                            }
                            else
                            {
                                record += dataGridView1[columnIndex, i].Value.ToString() + ";";
                            }
                        }
                        NewFileLine.WriteLine(record);
                    }
                    NewFileLine.Close();

                    dataGridView1.Rows.Clear();
                    dataGridView1.Refresh();
                    panel3.Visible = true;
                    textBox1.Text = "";

                    //czyszczenie ostatniej zakladki
                    label91.Text = "0";
                    label94.Text = "0kg";
                    label99.Text = "0szt.";
                    label95.Text = "0kg";
                    label100.Text = "0%";
                    label96.Text = "0h";
                    label98.Text = "0min";
                    label102.Text = "0%";
                    panel2.Visible = false;
                    //czyszczenie zakladki pomiary
                    label66.Text = "0";
                    label67.Text = "0";
                    label68.Text = "0";
                    label69.Text = "0";
                    label72.Text = "0";
                    label73.Text = "0";
                    label74.Text = "0";
                    label75.Text = "0";
                    label76.Text = "0";
                    label23.Text = "0";

                    label10.Text = "0,00";
                    label12.Text = "0,00";
                    label13.Text = "0,00";
                    label15.Text = "0,00";
                    label30.Text = "0,00";
                    //label25.Text = "0,00";
                    CheckUpdate();
                    label20.Text = "Operator";
                    SynchroParam();
                }
            }
            else MessageBox.Show(@"Zapisz dane pomiarów lub usuń je przyciskiem 'Nowy pomiar'.");
        }

        private void CheckUpdate()
        {
            string AppAssembly = Assembly.GetEntryAssembly().GetName().Version.ToString();
            if (File.Exists("P:\\Update\\SPC Prasy.exe"))
            {

                string NewFileAssembly = FileVersionInfo.GetVersionInfo("P:\\Update\\SPC Prasy.exe").FileVersion.ToString();
                //MessageBox.Show(AppAssembly+ "//"+ NewFileAssembly);
                if (NewFileAssembly != AppAssembly)
                {
                    if (File.Exists("P:\\Update\\Updater.exe"))
                        System.Diagnostics.Process.Start("P:\\Update\\Updater.exe");
                    else MessageBox.Show("Błąd aktualizacji!");
                    //MessageBox.Show("GOGOGOG");
                }
            }



        }

        private void panel3_Paint(object sender, PaintEventArgs e)
        {
            using (LinearGradientBrush brush = new LinearGradientBrush(this.ClientRectangle,
                                                   Color.DarkGray,
                                                   Color.Black,
                                                   90F))
            {
                e.Graphics.FillRectangle(brush, this.ClientRectangle);
            }
        }

        private void label65_VisibleChanged(object sender, EventArgs e)
        {
            if (label65.Visible == false)
            {
                label107.Visible = true;
                label106.Visible = true;
            }
        }

        
        private void SumyCzesciowe()
        {
            List<float> ListaProdukcjaTygKG = new List<float>();
            List<float> ListaProdukcjaTygSzt = new List<float>();
            List<float> ListaOdpadTyg = new List<float>();
            List<float> ListaCzasTyg = new List<float>();
            List<float> ListaWydajnoscTyg = new List<float>();
            List<int> ListaTygodni = new List<int>();
            List<float> ListaOdpadProc = new List<float>();


                if (Directory.Exists("C:\\log\\time\\"))
                {


                    string NazwaPliku = "C:\\log\\time\\" + DateTime.Now.Year.ToString() + ".log";
                    string[] Plik = File.ReadAllLines(NazwaPliku);


                    List<List<string>> Plik2D = new List<List<string>>();
                    foreach (var Line in Plik)
                    {
                        string[] Linia = Regex.Split(Line, ";");

                        List<string> ListaSplit = new List<string>();
                        foreach (var item in Linia)
                        {
                            ListaSplit.Add(item);
                        }
                        Plik2D.Add(ListaSplit);

                    }

                    float ProdukcjaKGL = 0, ProdukcjaSztL = 0, OdpadL = 0, CzasL = 0, WydajnoscL = 0, ProdukcjaKGT = 0, ProdukcjaSztT = 0, OdpadT = 0, CzasT = 0, WydajnoscT = 0, SumaProdWyd = 0;

                    string StartTyg = Plik2D[0][0];

                    List<int> granice = new List<int>();

                    for (int x = 0; x < Plik2D.Count; x++)
                    {
                        if (x == 0)
                        {
                            StartTyg = Plik2D[x][0];
                            ListaTygodni.Add(Int32.Parse(Plik2D[x][0]));

                        }
                        else
                        {
                            if (x == Plik2D.Count - 1) granice.Add(x);
                            if (Plik2D[x][0] != StartTyg)
                            {

                                granice.Add(x - 1);
                                StartTyg = Plik2D[x][0];
                                ListaTygodni.Add(Int32.Parse(Plik2D[x][0]));

                            }
                        }
                    }

                    for (int i = 0; i < granice.Count; i++)
                    {
                        //MessageBox.Show(i.ToString() + "/" + granice.Count.ToString() + ":" + granice[i].ToString());
                    }

                    int poprzedni = 0;
                    for (int i = 0; i < granice.Count; i = i + 1)
                    {
                        //MessageBox.Show("zewn");
                        for (int j = poprzedni; j <= granice[i]; j++)
                        {
                            float.TryParse(Plik2D[j][9], out ProdukcjaKGL);
                            float.TryParse(Plik2D[j][8], out ProdukcjaSztL);
                            float.TryParse(Plik2D[j][11], out OdpadL);
                            float.TryParse(Plik2D[j][7], out CzasL);
                            ProdukcjaKGT += ProdukcjaKGL;
                            ProdukcjaSztT += ProdukcjaSztL;
                            OdpadT += OdpadL;
                            CzasT += CzasL;
                            poprzedni = granice[i] + 1;

                        }

                        ListaProdukcjaTygKG.Add(ProdukcjaKGT);
                        ListaProdukcjaTygSzt.Add(ProdukcjaSztT);
                        ListaOdpadTyg.Add(OdpadT);
                        ListaCzasTyg.Add(CzasT);
                        ProdukcjaKGT = 0;
                        ProdukcjaSztT = 0;
                        OdpadT = 0;
                        CzasT = 0;
                    }

                    poprzedni = 0;
                    for (int i = 0; i < granice.Count; i = i + 1)
                    {

                        for (int j = poprzedni; j <= granice[i]; j++)
                        {
                            float.TryParse(Plik2D[j][9], out ProdukcjaKGL);
                            
                            WydajnoscT += ProdukcjaKGL / ListaProdukcjaTygKG[i] * float.Parse(Plik2D[j][10].Substring(0, Plik2D[j][10].Length - 1));
                            
                            //MessageBox.Show(ProdukcjaKGL.ToString()+ "/" + ListaProdukcjaTygKG[i].ToString()+ "*" + Plik2D[j][10]);
                            poprzedni = granice[i] + 1;
                        }

                        ListaWydajnoscTyg.Add(WydajnoscT);
                        WydajnoscT = 0;
                        //MessageBox.Show("Tyg: "+ListaWydajnoscTyg[i].ToString());
                    }




                    chart1.Visible = true;
                    label115.Visible = true;
            }

           // MessageBox.Show(label111.Text.Substring(6, 2));
            if (Int32.Parse(label111.Text.Substring(6, 2)) <= 12)
            {
                chart1.Series["Series1"].Points.Clear();
                chart1.Series["Series2"].Points.Clear();
                label115.Text = "Produkcja tygodniowa [kg]";
                for (int i = 0; i < ListaTygodni.Count; i++)
                {
                    
                    chart1.Series["Series1"].Points.AddXY(ListaTygodni[i], ListaProdukcjaTygKG[i]);
                }

            }

            if (Int32.Parse(label111.Text.Substring(6, 2)) > 12 && Int32.Parse(label111.Text.Substring(6, 2)) <= 24)
            {
                chart1.Series["Series1"].Points.Clear();
                chart1.Series["Series2"].Points.Clear();
                label115.Text = "Wydajność tygodniowa [%]";
                for (int i = 0; i < ListaTygodni.Count; i++)
                {
                    chart1.Series["Series2"].Points.AddXY(ListaTygodni[i], 90);
                    chart1.Series["Series1"].Points.AddXY(ListaTygodni[i], ListaWydajnoscTyg[i]);
                }

            }

            if (Int32.Parse(label111.Text.Substring(6, 2)) > 24 && Int32.Parse(label111.Text.Substring(6, 2)) <= 36)
            {
                chart1.Series["Series1"].Points.Clear();
                chart1.Series["Series2"].Points.Clear();
                label115.Text = "Odpad tygodniowy [kg]";
                for (int i = 0; i < ListaTygodni.Count; i++)
                {
                    
                    chart1.Series["Series1"].Points.AddXY(ListaTygodni[i], ListaOdpadTyg[i]);
                   
                }

            }

            if (Int32.Parse(label111.Text.Substring(6, 2)) > 36 && Int32.Parse(label111.Text.Substring(6, 2)) <= 48)
            {
                chart1.Series["Series1"].Points.Clear();
                chart1.Series["Series2"].Points.Clear();
                label115.Text = "Tygodniowy czas pracy prasy [h]";
                for (int i = 0; i < ListaTygodni.Count; i++)
                {
                    
                    chart1.Series["Series1"].Points.AddXY(ListaTygodni[i], ListaCzasTyg[i]);
                }

            }

            if (Int32.Parse(label111.Text.Substring(6, 2)) > 48)
            {
                chart1.Series["Series1"].Points.Clear();
                chart1.Series["Series2"].Points.Clear();
                label115.Text = "Tygodniowy odpad [%]";
                for (int i = 0; i < ListaTygodni.Count; i++)
                {
                    chart1.Series["Series2"].Points.AddXY(ListaTygodni[i], 0.7);
                    chart1.Series["Series1"].Points.AddXY(ListaTygodni[i], ListaOdpadTyg[i]/ListaProdukcjaTygKG[i]*100);
                }

            }
        
        }

        public string[] GetDayNames()
        {
            if (CultureInfo.CurrentCulture.Name.StartsWith("en-"))
            {
                return new[] { "Monday", "Tuesday", "Wednesday", "Thursday",
                        "Friday", "Saturday", "Sunday" };
            }
            else
            {
                return CultureInfo.CurrentCulture.DateTimeFormat.DayNames;
            }
        }

            

        private void panel3_DoubleClick(object sender, EventArgs e)
        {
         
        }

        private void button10_Click_1(object sender, EventArgs e)
        {
      
        }

        private void textBox2_TextChanged(object sender, EventArgs e)
        {
            RealWydajnosc();
        }

        private void textBox3_TextChanged(object sender, EventArgs e)
        {
            RealWydajnosc();
        }

        private void textBox8_TextChanged(object sender, EventArgs e)
        {
            RealWydajnosc();
        }

        private void textBox7_TextChanged(object sender, EventArgs e)
        {
            RealWydajnosc();
        }

        private void textBox12_TextChanged(object sender, EventArgs e)
        {
            RealWydajnosc();
        }

        private void textBox11_TextChanged(object sender, EventArgs e)
        {
            RealWydajnosc();
        }

        private void textBox10_TextChanged(object sender, EventArgs e)
        {
            RealWydajnosc();
        }

        private void textBox9_TextChanged(object sender, EventArgs e)
        {
            RealWydajnosc();
        }

        private void textBox16_TextChanged(object sender, EventArgs e)
        {
            RealWydajnosc();
        }

        private void textBox15_TextChanged(object sender, EventArgs e)
        {
            RealWydajnosc();
        }

        private void textBox14_TextChanged(object sender, EventArgs e)
        {
            RealWydajnosc();
        }

        private void textBox13_TextChanged(object sender, EventArgs e)
        {
            RealWydajnosc();
        }

        private void checkBox2_CheckedChanged(object sender, EventArgs e)
        {
            if (checkBox2.Checked == true) comboBox8.Visible = false;
            else
                comboBox8.Visible = true;
        }

        private void button10_Click_2(object sender, EventArgs e)
        {
            RestoreCtrlAltDelete();
        }

        private void radioButton1_CheckedChanged(object sender, EventArgs e)
        {

        }

        private void comboBox7_DropDown(object sender, EventArgs e)
        {
            comboBox7.DropDownWidth = 250;
        }

        private void button11_Click_2(object sender, EventArgs e)
        {
            ListaOperatorow = File.ReadAllLines("C:\\param\\OPERATOR.PAR");
            
            string Lista="";
            foreach (var Item in ListaOperatorow)
            {
                Lista += Item + System.Environment.NewLine;
            }
            MessageBox.Show("Lista uprawnionych operatorów:" + System.Environment.NewLine+Lista);
        }

        private void label108_DoubleClick(object sender, EventArgs e)
        {
            SynchroParam();
        }


  }
}
