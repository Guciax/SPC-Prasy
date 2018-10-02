using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace SPC_Prasy
{
    public partial class Form2 : Form
    {
        Form opener;


        public Form2(Form parentForm)
        {
            InitializeComponent();
            opener = parentForm;
        }

        private void Form2_Load(object sender, EventArgs e)
        {
            
            dataGridView1.Rows.Clear();
            string fileRow;
            string[] fileDataField;
            
 
                
                    string csvPath = "P:\\Normy.par";

                    if (System.IO.File.Exists(csvPath))
                    {
                        System.IO.StreamReader fileReader = new System.IO.StreamReader(csvPath, false);


                        //Reading Data
                        while (fileReader.Peek() != -1)
                        {
                            fileRow = fileReader.ReadLine();
                            fileDataField = fileRow.Split(',');
                            dataGridView1.Rows.Add(fileDataField);
                        }
                        fileReader.Dispose();
                        fileReader.Close();

            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            //opener.Close();
            

            string CsvFpath = "P:\\Normy.par";
            System.IO.StreamWriter csvFileWriter = new System.IO.StreamWriter(CsvFpath, false);

            int countColumn = dataGridView1.ColumnCount - 1;

            int iColCount = dataGridView1.Columns.Count;

            foreach (DataGridViewRow dataRowObject in dataGridView1.Rows)
            {
                //Checking for New Row in DataGridView
                if (!dataRowObject.IsNewRow)
                {
                    string dataFromGrid = "";

                    dataFromGrid = dataRowObject.Cells[0].Value.ToString();
                    

                    for (int i = 1; i <= countColumn; i++)
                    {
                        dataFromGrid = dataFromGrid + ',' + dataRowObject.Cells[i].Value.ToString();
                        //MessageBox.Show(dataFromGrid);
                        csvFileWriter.WriteLine(dataFromGrid);
                    }

                    //Writing Data Rows in File
                    
                }
            }
            csvFileWriter.Close();
            this.Close();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            if (textBox1.Text == "typefoud") panel1.Visible = false;
        }
    }
}
