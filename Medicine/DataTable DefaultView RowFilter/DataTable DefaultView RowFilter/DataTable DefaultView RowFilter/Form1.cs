using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Data.OleDb;
namespace DataTable_DefaultView_RowFilter
{
    public partial class Form1 : Form
    {
        string ConnectionString = System.Configuration.ConfigurationSettings.AppSettings["dsn"];
        OleDbCommand com;
        OleDbDataAdapter oledbda;
        DataSet ds;
        DataTable dt;
        string str;
        public Form1()
        {
            InitializeComponent();
        }

        private void btndisplayall_Click(object sender, EventArgs e)
        {
            bind();
        }

        void bind()
        {
            OleDbConnection con = new OleDbConnection(ConnectionString);
            con.Open();
            str = "select * from student";
            com = new OleDbCommand(str, con);
            oledbda = new OleDbDataAdapter(com);
            ds = new DataSet();
            dt = ds.Tables["student"];
            oledbda.Fill(ds, "student");
            dataGridView1.DataSource = ds;
            dataGridView1.DataMember = "student";
            con.Close();
        }

        private void btndisplay_Click(object sender, EventArgs e)
        {
            bind();
            dt = ds.Tables["student"];
            dt.DefaultView.RowFilter="saddress='" + textBox1.Text.Trim()+"'";
            dataGridView1.DataSource = dt.DefaultView;
            
		}
       
    }
}
