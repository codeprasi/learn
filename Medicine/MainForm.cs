using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Data.SqlServerCe;
using System.IO;
using System.Configuration;
using System.Collections;
using System.Drawing.Printing;
using iTextSharp.text.pdf;
using iTextSharp.text;
using System.Text.RegularExpressions;
using MetroFramework.Forms;
using MetroFramework;
using MetroFramework.Controls;
using MedicalCamp.Forms;
using Microsoft.VisualBasic;
using MedicalCamp.Properties;
using AxShockwaveFlashObjects;
using System.Drawing.Imaging;
using System.Threading;
using System.Globalization;
using System.Net;
using System.Net.Mail;
//using XColor = Microsoft.Xna.Framework.Graphics.Color;
//using CColor = System.Drawing.Color;

namespace MedicalCamp
{
    public partial class MainForm : MetroForm
    {
        #region Member Variables
        public static SqlCeDataAdapter sda = null;
        public static SqlCeDataAdapter sdt = null;
        public static SqlCeDataAdapter sdb = null;
        public static SqlCeConnection con = null;
        private CheckBox HeaderCheckBox = null;
        private AxShockwaveFlash swfChart;
        DataTable bindingDataTable = new DataTable();
        StringFormat strFormat; //Used to format the grid rows.
        ArrayList arrColumnLefts = new ArrayList();//Used to save left coordinates of columns
        ArrayList arrColumnWidths = new ArrayList();//Used to save column widths
        AutoCompleteStringCollection namesCollection = new AutoCompleteStringCollection();
        AutoCompleteStringCollection groupCollection = new AutoCompleteStringCollection();
        IDictionary<string, string> collections = new Dictionary<string, string>();
        Thread th = null;
        string defLoc = AppDomain.CurrentDomain.BaseDirectory + @"\Export";
        List<string> screenImgLoc = new List<string>();
        List<Groups> Grouping = new List<Groups>();
        TreeNode[] gp;

        #endregion
        #region Constructor
        public MainForm()
        {
            InitializeComponent();
            CurdOperation();
            this.BindGrid(sda);
            reset_btn.Click += new EventHandler(Reset);
            creatChartControl();
            chartPageLoad();
            screenImgLoc.AddRange(Directory.GetFiles(defLoc, "*.jpg", SearchOption.AllDirectories).ToList());

            DataTable sample = new DataTable(); //Sample Data
            sample.Columns.Add("id", typeof(string));
            sample.Columns.Add("name", typeof(string));
            for (int i = 0; i < screenImgLoc.Count; i++)
            {
                sample.Rows.Add(i + 1, screenImgLoc[i]);
            }

            exportGV.DataSource = sample;
            exportGV.Columns[1].Width = 500;
            //exportGV.Columns.Add("id");
            //exportGV.Columns.Add("name");
            //
            //exportGV.Items.Clear();

            //exportGV.FullRowSelect = true;

            //foreach (DataRow row in sample.Rows)
            //{
            //    ListViewItem item = new ListViewItem(row["id"].ToString());
            //    item.SubItems.Add(row["name"].ToString());
            //    exportGV.Items.Add(item); //Add this row to the ListView
            //}
            //ListViewItem listitem = new ListViewItem("Images Location",200);
            //foreach (string subItems in screenImgLoc)
            //{
            //    listitem.SubItems.Add(subItems);
            //    listitem.Text = subItems;
            //}
            //exportGV.Items.Add(listitem);
        }

        private void chartPageLoad()
        {
            chartCombo.Items.Clear();
            files = null;
            files = System.IO.Directory.GetFiles(@System.IO.Directory.GetCurrentDirectory() + "\\Forms\\charts\\", "*.swf");
            string[] Tempfiles = files.Select(file => Path.GetFileNameWithoutExtension(file)).Where(s => !s.Contains("FCF_MS")).Where(s => !s.Contains("FCF_Stacked")).ToArray();
            Tempfiles = Tempfiles.Select(x => x.Replace("FCF_", "")).ToArray();
            this.chartCombo.Items.AddRange(Tempfiles);
            Tempfiles = null;
            Tempfiles = files.Select(x => x).Where(s => !s.Contains("FCF_MS")).Where(s => !s.Contains("FCF_Stacked")).ToArray();
            files = null;
            files = Tempfiles;
            chartCombo.SelectedIndex = chartOrder.SelectedIndex = chartMonth.SelectedIndex = exportMonth.SelectedIndex = 0;
        }

        #endregion
        #region Create connection to the local database
        private static void CurdOperation()
        {
            try
            {
                con = new SqlCeConnection("Data Source=" + System.IO.Path.Combine(AssemblyPath(), "MyDB.sdf"));
                sda = new SqlCeDataAdapter(); sdt = new SqlCeDataAdapter(); sdb = new SqlCeDataAdapter(); SqlCeDataAdapter sdt2 = new SqlCeDataAdapter();
                SqlCeCommand cmd = con.CreateCommand();
                cmd.CommandText = ConfigurationManager.AppSettings["FirstQuery"];
                sda.SelectCommand = cmd;
                SqlCeCommand Table1 = con.CreateCommand();
                Table1.CommandText = ConfigurationManager.AppSettings["Table1"];
                sdt.SelectCommand = Table1;
                SqlCeCommand Table2 = con.CreateCommand();
                Table2.CommandText = ConfigurationManager.AppSettings["Table2"];
                sdb.SelectCommand = Table2;
                SqlCeCommand del = con.CreateCommand();
                del.CommandText = ConfigurationManager.AppSettings["unwantedentry"];
                sdt2.SelectCommand = del;
                DataSet dt = new DataSet();
                SqlCeCommandBuilder cbt = new SqlCeCommandBuilder(sdt);
                sdt.Fill(dt);
                if (dt.Tables[0].Rows.Count > 0)
                {
                    sdt.InsertCommand = cbt.GetInsertCommand();
                    sdt.UpdateCommand = cbt.GetUpdateCommand();
                    sdt.DeleteCommand = cbt.GetDeleteCommand();
                }
                dt.Clear();
                sdb.Fill(dt);
                if (dt.Tables[0].Rows.Count > 0)
                {
                    SqlCeCommandBuilder cbb = new SqlCeCommandBuilder(sdb);
                    sdb.InsertCommand = cbb.GetInsertCommand();
                    sdb.UpdateCommand = cbb.GetUpdateCommand();
                    sdb.DeleteCommand = cbb.GetDeleteCommand();
                }
                dt.Clear();
                sdt2.Fill(dt);
                if (dt.Tables[0].Rows.Count > 0)
                {
                    for (int intCount = 0; intCount < dt.Tables[0].Rows.Count; intCount++)
                    {
                        string txt = dt.Tables[0].Rows[intCount]["id"].ToString();
                        Delete(txt, true);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }
        }
        private static string AssemblyPath()
        {
            string dbBase = AppDomain.CurrentDomain.BaseDirectory;
            return Path.GetFullPath(Path.Combine(dbBase, @"Forms\necessary\"));
        }
        #endregion

        #region Event
        private void btnAdd_Click(object sender, EventArgs e)
        {
            try
            {
                if (VerificationFunction(e))
                {
                    Add();
                    Reset();
                    this.BindGrid(sda, false);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }
        }
        private void brnUpdate_Click(object sender, EventArgs e)
        {
            try
            {
                Update(lblId.Text);
                Reset();
                this.BindGrid(sda, false);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }

        }
        private void btnDelete_Click(object sender, EventArgs e)
        {
            try
            {
                if (MetroMessageBox.Show(this, "Do you want to delete " + txtName.Text + " Press `Yes` to delete", "MetroMessagebox", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == System.Windows.Forms.DialogResult.Yes)
                {
                    Delete(lblId.Text);
                    Reset();
                    this.BindGrid(sda, false);
                }
                return;

            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }
        }

        private void txtBSAS_TextChanged(object sender, EventArgs e)
        {
            valueAmt();
        }
        private void valueAmt()
        {
            if (!string.IsNullOrEmpty(txtBS.Text) && !string.IsNullOrEmpty(txtAS.Text))
            {
                int subValue = Convert.ToInt32(txtBS.Text.Trim()) - Convert.ToInt32(txtAS.Text.Trim());
                lblAmt.Text = subValue > 0 ? subValue.ToString() : "0";
            }
            else { lblAmt.Text = "0"; }
        }
        private void tbCbox_SelectionChangeCommitted(object sender, EventArgs e)
        {
            var result = tbCbox.SelectedValue;
            if (result.ToString() == "ALL")
            { this.BindGrid(sda, false); return; }
            if (result != null)
            {
                bindingDataTable.Clear();
                bindingDataTable = ((DataView)dgvOldData.DataSource).Table;
                this.BindGrid(sda, false);
            }
        }
        private void monthCbox_SelectionChangeCommitted(object sender, EventArgs e)
        {
            string result = monthCbox.SelectedValue.ToString();
            if (result != null)
            {
                bindingDataTable.Clear();
                bindingDataTable = ((DataView)dgvOldData.DataSource).Table;
                this.BindGrid(sda, false);
            }
        }
        private void Reset(object sender, EventArgs e)
        {
            Reset();
        }
        private bool VerificationFunction(EventArgs e)
        {
            txtName_Validated(this.txtName, e);
            txtBS_Validated(this.txtBS, e);
            txtBS_Validated(this.txtAS, e);
            bool ch = !string.IsNullOrEmpty(txtName.Text)
                && !string.IsNullOrEmpty(txtBS.Text) && !string.IsNullOrEmpty(txtAS.Text);
            if (string.IsNullOrEmpty(txtName.Text)) txtName.Select();
            if (string.IsNullOrEmpty(txtBS.Text)) txtBS.Select();
            if (string.IsNullOrEmpty(txtAS.Text)) txtAS.Select();
            return ch;
        }
        #endregion
        #region PageSepcific Method
        private void BindGrid(SqlCeDataAdapter sd, bool combo = true)
        {
            DataSet _ds = new DataSet();
            if (combo || (monthCbox.SelectedValue.ToString() == "ALL" && tbCbox.SelectedValue.ToString() == "ALL"))
            {
                sd.Fill(_ds);
                if (_ds.Tables.Count > 0)
                {
                    _ds.Tables[0].Columns.Add("DateNew", typeof(String));
                    //chartMap.Clear();
                    for (int intCount = 0; intCount < _ds.Tables[0].Rows.Count; intCount++)
                    {
                        if (firstLoad)
                        {
                            Grouping.Add(new Groups
                            {
                                GroupsNames = _ds.Tables[0].Rows[intCount]["Grouping"].ToString(),
                                TabletNames = _ds.Tables[0].Rows[intCount]["TabletName"].ToString()
                            });
                            if (_ds.Tables[0].Rows[intCount]["ChartShow"].ToString() == "True")
                            { chartMap.Add(new KeyValuePair<string, string>(_ds.Tables[0].Rows[intCount]["TabletName"].ToString(), _ds.Tables[0].Rows[intCount]["Balance"].ToString() + "@" + _ds.Tables[0].Rows[intCount]["BeforeStock"].ToString() + "@" + _ds.Tables[0].Rows[intCount]["AfterStock"].ToString() + "@" + months)); }

                        }
                        _ds.Tables[0].Rows[intCount]["DateNew"] = ((DateTime)_ds.Tables[0].Rows[intCount]["Date"]).ToString("d MMM yy", CultureInfo.CreateSpecificCulture("en-US"));
                        months = Convert.ToDateTime(_ds.Tables[0].Rows[intCount]["Date"].ToString()).ToString("MMM-yyyy");
                    }
                    if (firstLoad)
                    {
                        Grouping = Grouping.Select(o => new { o.GroupsNames, o.TabletNames }).Distinct().Select(o => new Groups() { GroupsNames = o.GroupsNames, TabletNames = o.TabletNames }).ToList();
                        gp = Grouping.Select(x => x.GroupsNames).Distinct().Select(x => new TreeNode(x)).ToArray();
                        myTreeView.Nodes.AddRange(gp);
                        foreach (Groups gt in Grouping)
                        {
                            TreeNode found = myTreeView.Nodes.Cast<TreeNode>().Where(r => r.Text == gt.GroupsNames).First();
                            found.Nodes.Add(gt.TabletNames);
                        }
                        myTreeView.ExpandAll();
                    }
                    _ds.Tables[0].Columns.Remove("Date");
                    _ds.Tables[0].Columns["DateNew"].ColumnName = "Date";
                    dgvOldData.AutoGenerateColumns = false;
                    dgvOldData.Columns["Balance"].ReadOnly = true;
                    DataView m_DataView = new DataView(_ds.Tables[0]);
                    dgvOldData.DataSource = m_DataView;
                    dgvOldData.Columns["ChartShow"].Width = 308;
                    //Add checkbox header
                    List<Control> c = dgvOldData.Controls.OfType<CheckBox>().Cast<Control>().ToList();
                    if (c.Count == 0)
                    {
                        HeaderCheckBox = new CheckBox();
                        HeaderCheckBox.Size = new System.Drawing.Size(50, 18);
                        HeaderCheckBox.BackColor = Color.Transparent;
                        HeaderCheckBox.ThreeState = true;
                        HeaderCheckBox.Checked = true;
                        HeaderCheckBox.Name = "HeaderCheckBox";
                        HeaderCheckBox.CheckStateChanged += new System.EventHandler(this.chk_CheckStateChanged);
                        dgvOldData.Controls.Add(HeaderCheckBox);
                        TotalCheckBoxes = TotalCheckedCheckBoxes = dgvOldData.RowCount;
                    }
                    dgvOldData.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.DisplayedCells;
                    dgvOldData.Columns[0].DefaultCellStyle.BackColor = Color.Red;
                }
            }
            else
            {
                DataTable dtCopy = bindingDataTable.Copy();
                _ds.Tables.Add(dtCopy);
            }
            if (monthCbox.SelectedValue != null && tbCbox.SelectedValue != null)
            {
                if (monthCbox.SelectedValue.ToString() != "ALL" && tbCbox.SelectedValue.ToString() != "ALL")
                {
                    int iMonthNo = Convert.ToDateTime("01-" + monthCbox.SelectedValue.ToString()).Month;
                    string iMonthYr = Regex.Match(monthCbox.SelectedValue.ToString().ToString(), @"\d+").Value;
                    _ds.Tables[0].DefaultView.RowFilter = " Date >= #" + Convert.ToDateTime(iMonthNo + "/01/" + iMonthYr).ToString("MM/dd/yyyy") + "# AND Date <= #" + Convert.ToDateTime(iMonthNo + "/28/" + iMonthYr).ToString("MM/dd/yyyy") + "#" + String.Format("AND TabletName LIKE '{0}'", tbCbox.Text.ToString());
                    dgvOldData.DataSource = _ds.Tables[0].DefaultView;
                }
                else if (monthCbox.SelectedValue.ToString() != "ALL")
                {
                    int iMonthNo = Convert.ToDateTime("01-" + monthCbox.SelectedValue.ToString()).Month;
                    string iMonthYr = Regex.Match(monthCbox.SelectedValue.ToString().ToString(), @"\d+").Value;
                    _ds.Tables[0].DefaultView.RowFilter = " Date >= #" + Convert.ToDateTime(iMonthNo + "/01/" + iMonthYr).ToString("MM/dd/yyyy") + "# AND Date <= #" + Convert.ToDateTime(iMonthNo + "/28/" + iMonthYr).ToString("MM/dd/yyyy") + "#";
                    dgvOldData.DataSource = _ds.Tables[0].DefaultView;
                }
                else if (tbCbox.SelectedValue.ToString() != "ALL")
                {
                    _ds.Tables[0].DefaultView.RowFilter = String.Format("TabletName LIKE '{0}'", tbCbox.Text.ToString());
                    dgvOldData.DataSource = _ds.Tables[0].DefaultView;
                }
            }


            if (combo)
            {
                DataSet _tabDs = new DataSet();
                sd.Fill(_tabDs);
                Dictionary<string, string> ComboDict = new Dictionary<string, string>();
                Dictionary<string, string> ComboMonthYear = new Dictionary<string, string>();
                ComboDict.Add("ALL", "ALL");
                ComboMonthYear.Add("ALL", "ALL");
                string txt = string.Empty;
                string val = string.Empty;
                for (int intCount = 0; intCount < _tabDs.Tables[0].Rows.Count; intCount++)
                {
                    txt = _tabDs.Tables[0].Rows[intCount]["TabletName"].ToString();

                    val = _tabDs.Tables[0].Rows[intCount]["Tabid"].ToString();
                    months = Convert.ToDateTime(_tabDs.Tables[0].Rows[intCount]["Date"].ToString()).ToString("MMM-yyyy");
                    //check if it already exists
                    if (!ComboDict.ContainsKey(txt))
                    {
                        namesCollection.Add(txt);
                        groupCollection.Add(_tabDs.Tables[0].Rows[intCount]["Grouping"].ToString());
                        collections.Add(new KeyValuePair<string, string>(txt, _tabDs.Tables[0].Rows[intCount]["Grouping"].ToString()));

                        ComboDict.Add(txt, val);
                    }
                    if (!ComboMonthYear.ContainsKey(months))
                    {
                        ComboMonthYear.Add(months, months);
                    }
                }
                tbCbox.DataSource = new BindingSource(ComboDict, null);
                tbCbox.DisplayMember = exportMonth.DisplayMember = "Key";
                tbCbox.ValueMember = exportMonth.ValueMember = "Value";
                chartMonth.DataSource = monthCbox.DataSource = exportMonth.DataSource = new BindingSource(ComboMonthYear, null); chartMonth.DisplayMember = monthCbox.DisplayMember = "Key";
                chartMonth.ValueMember = monthCbox.ValueMember = "Value";
                dateTimePicker1.Text = Settings.Default.datetimepicker;
                txtExpiry.Format = DateTimePickerFormat.Custom;
                txtExpiry.CustomFormat = "ddMMMyyyy";
                txtName.AutoCompleteMode = txtGroup.AutoCompleteMode = AutoCompleteMode.Append;
                txtName.AutoCompleteSource = txtGroup.AutoCompleteSource = AutoCompleteSource.CustomSource;
                txtName.AutoCompleteCustomSource = namesCollection;
                txtGroup.AutoCompleteCustomSource = groupCollection;
                this.StyleManager = msm;
                this.StyleManager.Theme = Settings.Default.Themes;
                this.StyleManager.Style = Settings.Default.Styles;
            }
        }
        private void btnGet_Click(object sender, EventArgs e)
        {
            string message = string.Empty;
            foreach (DataGridViewRow row in dgvOldData.Rows)
            {
                bool isSelected = Convert.ToBoolean(row.Cells["checkBoxColumn"].Value);
                if (isSelected)
                {
                    message += Environment.NewLine;
                    message += row.Cells["Name"].Value.ToString();
                }
            }
            MessageBox.Show("Selected Values" + message);
        }
        /// <summary>
        /// In this method write code for Inserting data in 
        /// this table MyDemoTable.
        /// </summary>
        private void Add()
        {
            for (int i = 0; i <= 1; i++)
            {
                if (i == 0)
                {
                    DataSet oldData = new DataSet();
                    DataRow dr;
                    sdt.Fill(oldData);
                    lblId.Text = (oldData.Tables[0].AsEnumerable().Select(x => Convert.ToInt32(x.Field<int>("id"))).DefaultIfEmpty(0).Max(x => x) + 1).ToString();
                    DataRow[] tempdata = oldData.Tables[0].AsEnumerable().Where(p => p["TabletName"].ToString().ToLower() == txtName.Text.Trim().ToLower()).ToArray();
                    if (tempdata.Length > 0)
                    {// MessageBox.Show("Tablet Already"); 
                        dr = tempdata[0];
                        lblId.Text = dr["id"].ToString();
                        continue;
                    }
                    dr = oldData.Tables[0].NewRow();
                    dr["TabletName"] = (txtName.Text.Trim()).ToUpper();
                    namesCollection.Add(dr["TabletName"].ToString());
                    dr["Grouping"] = (string.IsNullOrEmpty(txtName.Text.ToString()) ? txtGroup.Text.Trim() : "No Group").ToUpper();
                    groupCollection.Add(dr["Grouping"].ToString());
                    oldData.Tables[0].Rows.Add(dr); sdt.Update(oldData);
                }
                else
                {
                    DataSet oldData2 = new DataSet();
                    DataRow dr2;
                    sdb.Fill(oldData2);
                    dr2 = oldData2.Tables[0].NewRow();
                    dr2["Tabid"] = Convert.ToUInt32(lblId.Text.ToString());
                    dr2["BeforeStock"] = Convert.ToUInt32(txtBS.Text.Trim());
                    dr2["AfterStock"] = Convert.ToUInt32(txtAS.Text.Trim());
                    dr2["Balance"] = Convert.ToUInt32(lblAmt.Text);
                    dr2["Month"] = dateTimePicker1.Value.Month.ToString();
                    dr2["Year"] = Convert.ToUInt32(dateTimePicker1.Value.Year.ToString());
                    dr2["Expiry"] = txtExpiry.Text.ToString();
                    dr2["Date"] = dateTimePicker1.Value.ToString();
                    dr2["ChartShow"] = "True";
                    oldData2.Tables[0].Rows.Add(dr2); sdb.Update(oldData2);
                    saveSetting();
                }
            }

        }
        /// <summary>
        /// In this method write code for updating existing data.
        /// </summary>
        /// <param name="id"></param>
        private void Update(string id)
        {
            DataSet oldData = new DataSet();
            DataRow dr;
            //FillDataInDataset(out oldData, out dr);
            sdb.Fill(oldData);
            //Here get record of specified id.
            DataRow[] tempdata = oldData.Tables[0].AsEnumerable().Where(p => p["Tabid"].ToString() == id).Where(p => p["Date"].ToString() == dateTimePicker1.Value.ToString()).ToArray();
            if (tempdata.Length > 0)
            {
                dr = tempdata[0];
                dr["BeforeStock"] = Convert.ToUInt32(txtBS.Text.Trim());
                dr["AfterStock"] = Convert.ToUInt32(txtAS.Text.Trim());
                dr["Balance"] = Convert.ToUInt32(lblAmt.Text);
                dr["Month"] = dateTimePicker1.Value.Month.ToString();
                dr["Year"] = Convert.ToUInt32(dateTimePicker1.Value.Year.ToString());
                dr["Expiry"] = txtExpiry.Text.ToString();
                dr["Date"] = dateTimePicker1.Value.ToString();
                dr["ChartShow"] = (lblid2.Text.ToString() == "") ? "True" : lblid2.Text.ToString();
                saveSetting();
                sdb.Update(oldData);
            }

            DataSet oldData2 = new DataSet();
            DataRow dr2;
            //FillDataInDataset(out oldData, out dr);
            sdt.Fill(oldData2);
            //Here get record of specified id.
            tempdata = null;
            tempdata = oldData2.Tables[0].AsEnumerable().Where(p => p["id"].ToString() == id).ToArray();
            if (tempdata.Length > 0)
            {
                dr2 = tempdata[0];
                dr2["TABLETNAME"] = txtName.Text;
                dr2["Grouping"] = txtGroup.Text;
                sdt.Update(oldData2);
            }
        }
        /// <summary>
        ///In this method write code for Deleting existing data. 
        /// </summary>
        /// <param name="id"></param>
        private static void Delete(string id, bool tablets = false)
        {
            DataSet oldData = new DataSet();
            DataRow dr;
            DataRow[] tempdata = null;
            //FillDataInDataset(out oldData, out dr);
            if (tablets)
            {
                sdt.Fill(oldData);
                tempdata = oldData.Tables[0].AsEnumerable().Where(p => p["id"].ToString() == id).ToArray();
            }
            else
            {
                sdb.Fill(oldData);
                tempdata = oldData.Tables[0].AsEnumerable().Where(p => p["Tabid"].ToString() == id).ToArray();
            }
            //Here get record of specified id.

            if (tempdata.Length > 0)
            {
                dr = tempdata[0];
                dr.Delete();
            }
            if (tablets)
                sdt.Update(oldData);
            else
                sdb.Update(oldData);
        }
        /// <summary>
        /// Reset All control of Form.
        /// </summary>
        private void Reset()
        {
            formValidatorCheck = false;
            txtName.Text = string.Empty;
            txtGroup.Text = string.Empty;
            txtExpiry.Text = string.Empty;
            dateTimePicker1.Text = Settings.Default.datetimepicker;
            txtExpiry.Text = Settings.Default.Expiry;
            txtBS.Text = string.Empty;
            txtAS.Text = string.Empty;
            btnDelete.Enabled = false;
            brnUpdate.Enabled = false;
            btnAdd.Enabled = false;
            txtName.Enabled = true;
            txtGroup.Enabled = true;
            formValidatorCheck = true;
        }
        #endregion

        #region Print Button Click Event
        private void print_Click(object sender, EventArgs e)
        {
            //Open the print preview dialog
            PrintPreviewDialog objPPdialog = new PrintPreviewDialog();
            objPPdialog.Document = printDocument1;
            objPPdialog.ShowDialog();
        }
        private void printDocument1_BeginPrint(object sender, System.Drawing.Printing.PrintEventArgs e)
        {
            try
            {
                strFormat = new StringFormat();
                strFormat.Alignment = StringAlignment.Near;
                strFormat.LineAlignment = StringAlignment.Center;
                strFormat.Trimming = StringTrimming.EllipsisCharacter;

                arrColumnLefts.Clear();
                arrColumnWidths.Clear();
                iCellHeight = 0;
                iRow = 0;
                bFirstPage = true;
                bNewPage = true;

                // Calculating Total Widths
                iTotalWidth = 0;
                foreach (DataGridViewColumn dgvGridCol in dgvOldData.Columns)
                {
                    iTotalWidth += dgvGridCol.Width;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        private void printDocument1_PrintPage(object sender, System.Drawing.Printing.PrintPageEventArgs e)
        {
            try
            {
                //Set the left margin
                int iLeftMargin = e.MarginBounds.Left;
                //Set the top margin
                int iTopMargin = e.MarginBounds.Top;
                //Whether more pages have to print or not
                bool bMorePagesToPrint = false;
                int iTmpWidth = 0;
                //For the first page to print set the cell width and header height
                if (bFirstPage)
                {
                    foreach (DataGridViewColumn GridCol in dgvOldData.Columns)
                    {
                        if (GridCol.HeaderText != "Select" && ((System.Windows.Forms.DataGridViewBand)(GridCol)).Visible) //1
                        {
                            iTmpWidth = (int)(Math.Floor((double)((double)GridCol.Width /
                                           (double)iTotalWidth * (double)iTotalWidth *
                                           ((double)e.MarginBounds.Width / (double)iTotalWidth))));

                            iHeaderHeight = (int)(e.Graphics.MeasureString(GridCol.HeaderText,
                                        GridCol.InheritedStyle.Font, iTmpWidth).Height) + 11;

                            // Save width and height of headres
                            arrColumnLefts.Add(iLeftMargin);
                            arrColumnWidths.Add(iTmpWidth);
                            iLeftMargin += iTmpWidth;
                        }
                    }
                }
                //Loop till all the grid rows not get printed
                while (iRow <= dgvOldData.Rows.Count - 1)
                {
                    DataGridViewRow GridRow = dgvOldData.Rows[iRow];
                    //Set the cell height
                    iCellHeight = GridRow.Height + 5;
                    int iCount = 0;
                    System.Drawing.Font stringFont = new System.Drawing.Font(dgvOldData.Font, FontStyle.Bold);
                    //Check whether the current page settings allo more rows to print
                    if (iTopMargin + iCellHeight >= e.MarginBounds.Height + e.MarginBounds.Top)
                    {
                        bNewPage = true;
                        bFirstPage = false;
                        bMorePagesToPrint = true;
                        break;
                    }
                    else
                    {
                        if (bNewPage)
                        {
                            //Draw Header
                            e.Graphics.DrawString("YRSK MEDICAL CAMP", stringFont,
                                    Brushes.Black, e.MarginBounds.Left, e.MarginBounds.Top -
                                    e.Graphics.MeasureString("YRSK MEDICAL CAMP", stringFont, e.MarginBounds.Width).Height - 13);

                            String strDate = DateTime.Now.ToLongDateString() + " " + DateTime.Now.ToShortTimeString();
                            //Draw Date
                            e.Graphics.DrawString(strDate, stringFont,
                                    Brushes.Black, e.MarginBounds.Left + (e.MarginBounds.Width -
                                    e.Graphics.MeasureString(strDate, stringFont, e.MarginBounds.Width).Width), e.MarginBounds.Top -
                                    e.Graphics.MeasureString("YRSK MEDICAL CAMP", new System.Drawing.Font(stringFont, FontStyle.Bold), e.MarginBounds.Width).Height - 13);

                            //Draw Columns                 
                            iTopMargin = e.MarginBounds.Top;
                            foreach (DataGridViewColumn GridCol in dgvOldData.Columns)
                            {
                                if (GridCol.HeaderText != "Select" && GridCol.Visible)  //2
                                {
                                    e.Graphics.FillRectangle(new SolidBrush(Color.LightGray),
                                        new System.Drawing.Rectangle((int)arrColumnLefts[iCount], iTopMargin,
                                        (int)arrColumnWidths[iCount], iHeaderHeight));

                                    e.Graphics.DrawRectangle(Pens.Black,
                                        new System.Drawing.Rectangle((int)arrColumnLefts[iCount], iTopMargin,
                                        (int)arrColumnWidths[iCount], iHeaderHeight));

                                    e.Graphics.DrawString(GridCol.HeaderText, GridCol.InheritedStyle.Font,
                                        new SolidBrush(GridCol.InheritedStyle.ForeColor),
                                        new RectangleF((int)arrColumnLefts[iCount], iTopMargin,
                                        (int)arrColumnWidths[iCount], iHeaderHeight), strFormat);
                                    iCount++;
                                }
                            }
                            bNewPage = false;
                            iTopMargin += iHeaderHeight;
                        }
                        iCount = 0;
                        //Draw Columns Contents                
                        foreach (DataGridViewCell Cel in GridRow.Cells)
                        {
                            if (Cel.Value != null && Cel.Value.ToString() != "Select" && Cel.Visible == true)//3
                            {
                                e.Graphics.DrawString(Cel.Value.ToString(), Cel.InheritedStyle.Font,
                                            new SolidBrush(Cel.InheritedStyle.ForeColor),
                                            new RectangleF((int)arrColumnLefts[iCount], (float)iTopMargin,
                                            (int)arrColumnWidths[iCount], (float)iCellHeight), strFormat);

                                //Drawing Cells Borders 
                                e.Graphics.DrawRectangle(Pens.Black, new System.Drawing.Rectangle((int)arrColumnLefts[iCount],
                                        iTopMargin, (int)arrColumnWidths[iCount], iCellHeight));

                                iCount++;
                            }
                        }
                    }
                    iRow++;
                    iTopMargin += iCellHeight;
                }
                //If more lines exist, print another page.
                if (bMorePagesToPrint)
                    e.HasMorePages = true;
                else
                    e.HasMorePages = false;
            }
            catch (Exception exc)
            {
                MessageBox.Show(exc.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        private void exclbtn_Click(object sender, EventArgs e)
        {
            SaveFileDialog sfd = new SaveFileDialog();
            sfd.Filter = "Excel Documents (*.xls)|*.xls";
            sfd.FileName = "export.xls";
            //sfd.Filter = "Word Documents (*.doc)|*.doc";
            //sfd.FileName = "export.doc";
            sfd.FilterIndex = 0;
            sfd.RestoreDirectory = true;
            //sfd.CreatePrompt = true;
            sfd.Title = "Export YRSK Medical Camp Excel File To";
            if (sfd.ShowDialog() == DialogResult.OK)
            {
                string stOutput = "";
                // Export titles:
                string sHeaders = "";
                for (int j = 0; j < dgvOldData.Columns.Count; j++)
                {
                    if (dgvOldData.Columns[j].DataPropertyName == "" || !dgvOldData.Columns[j].Visible)
                    {
                        continue;
                    } sHeaders = sHeaders.ToString() + Convert.ToString(dgvOldData.Columns[j].HeaderText) + "\t";
                }
                stOutput += sHeaders + "\r\n";
                // Export data.
                for (int i = 0; i <= dgvOldData.RowCount - 1; i++)
                {
                    string stLine = "";
                    for (int j = 0; j < dgvOldData.Rows[i].Cells.Count; j++)
                    {
                        if (dgvOldData.Columns[j].DataPropertyName == "" || !dgvOldData.Columns[j].Visible)
                        {
                            continue;
                        } stLine = stLine.ToString() + Convert.ToString(dgvOldData.Rows[i].Cells[j].Value) + "\t";
                    }
                    stOutput += stLine + "\r\n";
                }
                Encoding utf16 = Encoding.GetEncoding(1254);
                byte[] output = utf16.GetBytes(stOutput);
                FileStream fs = new FileStream(sfd.FileName, FileMode.Create);
                BinaryWriter bw = new BinaryWriter(fs);
                bw.Write(output, 0, output.Length); //write the encoded file
                bw.Flush();
                bw.Close();
                fs.Close();
            }
        }
        private void pdfbtn_Click(object sender, EventArgs e)
        {
            SaveFileDialog sfd = new SaveFileDialog();
            sfd.Filter = "PDF Files|*.pdf";
            sfd.FileName = "export.pdf";
            sfd.DefaultExt = "*.pdf";
            sfd.FilterIndex = 0;
            sfd.RestoreDirectory = true;
            //sfd.CreatePrompt = true;
            sfd.Title = "Export YRSK Medical Camp PDF File To";
            if (sfd.ShowDialog() == DialogResult.OK) // Test result.
            {
                iTextSharp.text.Font fontTable = FontFactory.GetFont("Arial", 8, iTextSharp.text.Font.NORMAL, BaseColor.BLACK);
                PdfPTable pdfTable = new PdfPTable(dgvOldData.ColumnCount - 3);
                pdfTable.DefaultCell.Padding = 3;
                pdfTable.WidthPercentage = 67;//100
                pdfTable.HorizontalAlignment = Element.ALIGN_CENTER;
                pdfTable.DefaultCell.BorderWidth = 1;
                dgvOldData.Columns["ChartShow"].Visible = false;
                //Adding Header row
                for (int j = 0; j < dgvOldData.Columns.Count; j++)
                {
                    if (dgvOldData.Columns[j].DataPropertyName == "" || !dgvOldData.Columns[j].Visible)
                    {
                        continue;
                    }

                    pdfTable.AddCell(new Phrase(dgvOldData.Columns[j].HeaderText, fontTable));
                }
                pdfTable.HeaderRows = 1;
                for (int i = 0; i < dgvOldData.Rows.Count; i++)
                {
                    for (int k = 0; k < dgvOldData.Columns.Count; k++)
                    {
                        if (dgvOldData.Columns[k].DataPropertyName == "" || !dgvOldData.Columns[k].Visible)
                        {
                            continue;
                        }
                        if (dgvOldData[k, i].Value != null)
                        {
                            pdfTable.AddCell(new Phrase(dgvOldData[k, i].Value.ToString(), fontTable));
                        }
                    }
                }
                dgvOldData.Columns["ChartShow"].Visible = true;
                iTextSharp.text.Image jpg = null;
                if (chartImgLoc != string.Empty)
                {
                    jpg = iTextSharp.text.Image.GetInstance(chartImgLoc);
                    //Give space before image
                    //jpg.SpacingBefore = 10f;
                    //Give some space after the image
                    jpg.SpacingAfter = 1f;
                    jpg.Alignment = Element.ALIGN_LEFT;
                }

                using (FileStream stream = new FileStream(sfd.FileName, FileMode.Create))
                {
                    Document pdfDoc = new Document(PageSize.A4, 10f, 10f, 10f, 0f);
                    PdfWriter w = PdfWriter.GetInstance(pdfDoc, stream);
                    pdfDoc.Open();
                    iTextSharp.text.Paragraph titolo = new iTextSharp.text.Paragraph("YRSK MEDICAL CAMP\n\n");
                    titolo.Alignment = iTextSharp.text.Element.ALIGN_CENTER;
                    titolo.SpacingAfter = 20;
                    pdfDoc.Add(titolo);
                    //pdfTable.SpacingBefore = 20;
                    pdfDoc.Add(pdfTable);
                    if (jpg != null)
                    {
                        jpg.ScaleToFit(pdfDoc.PageSize);
                        //jpg.SetAbsolutePosition(0, 0);
                        pdfDoc.Add(jpg);
                    }
                    if (chartImgLoc == string.Empty)
                    {
                        if (screenImgLoc.Count > 0)
                        {
                            foreach (string locs in screenImgLoc)
                            {
                                jpg = iTextSharp.text.Image.GetInstance(locs);
                                //Give space before image
                                jpg.SpacingBefore = 10f;
                                //Give some space after the image
                                jpg.SpacingAfter = 10f;
                                jpg.Alignment = Element.ALIGN_LEFT;
                                jpg.ScaleToFit(pdfDoc.PageSize);
                                //jpg.SetAbsolutePosition(0, 0);
                                pdfDoc.Add(jpg);
                            }
                        }
                    }
                    PdfContentByte cb = w.DirectContent;
                    cb.BeginText();
                    BaseFont f_cn = BaseFont.CreateFont("c:\\windows\\fonts\\calibri.ttf", BaseFont.CP1252, BaseFont.NOT_EMBEDDED);
                    cb.SetFontAndSize(f_cn, 6);
                    cb.SetTextMatrix(475, 15);  //(xPos, yPos)
                    cb.ShowText("Date Created: " + DateTime.Now.ToShortDateString());
                    cb.EndText();
                    pdfDoc.Close();
                    stream.Close();
                }
            }
        }
        #endregion

        #region Validation&Setting
        private void gUserName_Leave(object sender, EventArgs e)
        {
            System.Text.RegularExpressions.Regex expr = new System.Text.RegularExpressions.Regex(@"^[a-zA-Z][\w\.-]{2,28}[a-zA-Z0-9]@[a-zA-Z0-9][\w\.-]*[a-zA-Z0-9]\.[a-zA-Z][a-zA-Z\.]*[a-zA-Z]$");
            string eMail = gUserName.Text.Trim();
            if (eMail.Contains('@'))
            {
             var eMails = eMail.Split('@');
             eMail = eMails[0];
             gUserName.Text = eMail;
            }
            if (!eMail.Contains("@GMAIL.COM") && !string.IsNullOrEmpty(eMail) && eMail.Length >= 6)
            {
                eMail = gUserName.Text = eMail + "@GMAIL.COM";
                //if (!expr.IsMatch(eMail))
                //{ this.errorProvider1.SetError(gUserName, "Email Id is not Valid"); gUserName.ForeColor = Color.Red; }
                //else
                {
                    this.errorProvider1.SetError(gUserName, "");
                    System.Drawing.Color myColor = System.Drawing.ColorTranslator.FromHtml( msm.Style.ToString());
                    gUserName.ForeColor = myColor;                   
                }
            }
            else { this.errorProvider1.SetError(gUserName, "Email Id is not Valid"); gUserName.ForeColor = Color.Red; }
           
        }
        private void txtName_Validated(object sender, EventArgs e)
        {
            if (!formValidatorCheck) return;
            MetroTextBox textBox = (MetroTextBox)sender;
            bool bTest = txtEmptyStringIsValid(textBox);
            if (bTest == true)
            {
                this.errorProvider1.SetError(textBox, "This field must contain text");
                this.btnAdd.Enabled = false;
            }
            else
            {
                this.errorProvider1.SetError(textBox, "");
                this.btnAdd.Enabled = true;
            }
            bool bTest2 = txtMinLengthTestIsValid(textBox,3);
            if (bTest2 == true)
            {
                this.errorProvider1.SetError(textBox,
                    "This field must contain at least 3 characters");
                this.btnAdd.Enabled = false;
            }
            else
            {
                this.errorProvider1.SetError(textBox, "");
                this.btnAdd.Enabled = true;
            }
            if (collections.ContainsKey(textBox.Text))
            {
                txtGroup.Enabled = false;
                txtGroup.Text = collections[textBox.Text];
            }
            else
            {
                txtGroup.Enabled = true;
                txtGroup.Text = "";
            }
        }
        private void txtBS_Validated(object sender, EventArgs e)
        {
            if (!formValidatorCheck) return;
            MetroTextBox textBox = (MetroTextBox)sender;
            bool bTest = txtNumericStringIsValid(textBox);
            if (bTest == true)
            {
                this.errorProvider1.SetError(textBox, "This field must contain only numbers");
                this.btnAdd.Enabled = false;
            }
            else
            {
                this.errorProvider1.SetError(textBox, "");
                this.btnAdd.Enabled = true;
            }
        }
        private bool txtMinLengthTestIsValid(MetroTextBox tb,int Countlen)
        {
            char[] testArr = tb.Text.ToCharArray();
            bool testBool = false;
            if (testArr.Length < Countlen) testBool = true;
            return testBool;
        }
        private bool txtEmptyStringIsValid(MetroTextBox tb)
        {
            if (tb.Text == string.Empty) return true;
            else return false;
        }
        private bool txtNumericStringIsValid(MetroTextBox textBox)
        {
            if (textBox.Text == string.Empty)
            {
                return true;
            }
            char[] testArr = textBox.Text.ToCharArray();
            bool testBool = false;
            for (int i = 0; i < testArr.Length; i++)
            {
                if (!char.IsNumber(testArr[i]))
                {
                    testBool = true;
                }
            }
            return testBool;
        }
        private void txtBS_KeyPress(object sender, KeyPressEventArgs e)
        {
            var textBox = (Control)sender;
            var countChar = textBox.Text;
            e.Handled = !char.IsDigit(e.KeyChar) && !char.IsControl(e.KeyChar);
            if (countChar.Length == 9)
            {
                e.Handled = e.KeyChar != (char)Keys.Back;
            }
        }
        private void btnAdd_MouseEnter(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(txtName.Text)
                || string.IsNullOrEmpty(txtBS.Text) || string.IsNullOrEmpty(txtAS.Text))
                metroToolTip1.Show(addTextToolTip, btnAdd);
            else metroToolTip1.Hide(btnAdd); ;
        }
        private void btnAdd_EnabledChanged(object sender, EventArgs e)
        {
            Button button = (Button)sender;
            MetroFramework.MetroTileTextSize t = MetroFramework.MetroTileTextSize.Small;
            MetroFramework.MetroTileTextWeight b = MetroFramework.MetroTileTextWeight.Light;
            addTextToolTip = "Please fill form";
            if (button.Enabled)
            {
                t = MetroFramework.MetroTileTextSize.Tall;
                b = MetroFramework.MetroTileTextWeight.Bold;
                addTextToolTip = "Add to Database";
            }
            switch (button.Text)
            {
                case "Add":
                    btnAdd.TileTextFontSize = t;
                    btnAdd.TileTextFontWeight = b;
                    break;

                case "Update":
                    brnUpdate.TileTextFontSize = t;
                    brnUpdate.TileTextFontWeight = b;
                    break;

                case "Delete":
                    btnDelete.TileTextFontSize = t;
                    btnDelete.TileTextFontWeight = b;
                    break;
                default:
                    reset_btn.TileTextFontSize = t;
                    reset_btn.TileTextFontWeight = b;
                    break;
            }
        }
        private void themeToggle_Click(object sender, EventArgs e)
        {
            if (themeToggle.Checked)
            {
                this.StyleManager.Theme = MetroThemeStyle.Light;

            }
            else this.StyleManager.Theme = MetroThemeStyle.Dark;
            saveSetting();
        }
        private void saveSetting()
        {
            Settings.Default.Expiry = this.txtExpiry.Text.ToString();
            Settings.Default.datetimepicker = this.dateTimePicker1.Text.ToString();
            Settings.Default.Themes = this.StyleManager.Theme;
            Settings.Default.Styles = this.StyleManager.Style;
            Settings.Default.Save();
        }
        private void metroTileColor_Click(object sender, EventArgs e)
        {
            msm.Style = (MetroColorStyle)(((MetroFramework.Controls.MetroTile)(sender)).Style);
            saveSetting();
        }
        private void lnlClose_Click(object sender, EventArgs e)
        {
            if (th != null)
                th.Abort();
            Application.Exit();
        }
        private void chartTabPage_Leave(object sender, EventArgs e)
        {
            chartTabPage.Controls.Remove(swfChart);
            // swfChart.Stop();                 
            swfChart.Dispose();
            swfChart = null;
            creatChartControl();
        }
        private void chartTabPage_Enter(object sender, EventArgs e)
        {
            firstChartLoad = true;
            chartCombo_SelectionChangeCommitted(sender, e);
            firstChartLoad = false;
        }
        private DataTable CreateTable()
        {
            DataSet dt = new DataSet();
            sda.Fill(dt);
            DataTable _table = new DataTable();
            _table.Columns.Add("Name");
            _table.Columns.Add("Value");
            _table.Columns.Add("Color");
            var random = new Random();
            foreach (KeyValuePair<string, string> acct in chartMap)
            {
                string[] rowsValue = acct.Value.Split('@');
                if (chartMonths == "ALL")
                    _table.Rows.Add(acct.Key, rowsValue[chartOrders], String.Format("#{0:X6}", random.Next(0x1000000)));

                else
                {
                    if (chartMonths == rowsValue[3])
                        _table.Rows.Add(acct.Key, rowsValue[chartOrders], String.Format("#{0:X6}", random.Next(0x1000000)));
                }
            }
            if (_table.Rows.Count <= 0)
                chartText.Text = "No Data to be loaded";
            else
                chartText.Text = "";
            return _table;
        }
        private void creatChartControl()
        {
            swfChart = new AxShockwaveFlash();
            swfChart.BeginInit();
            swfChart.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)
            | System.Windows.Forms.AnchorStyles.Right)));
            swfChart.Enabled = true;
            swfChart.Location = new System.Drawing.Point(0, 54);
            swfChart.Margin = new System.Windows.Forms.Padding(3, 3, 3, 30);
            swfChart.Name = "swfChart";
            swfChart.Size = new System.Drawing.Size(918, 452);
            swfChart.TabIndex = 1;
            chartTabPage.Controls.Add(swfChart);
            swfChart.CreateControl();
            swfChart.EndInit();
        }
        private void chartCombo_SelectionChangeCommitted(object sender, EventArgs e)
        {
            chartOrders = chartOrder.SelectedIndex;
            chartMonths = chartMonth.SelectedValue.ToString();
            List<Control> c = chartTabPage.Controls.OfType<AxShockwaveFlash>().Cast<Control>().ToList();
            if (c.Count >= 0 && !firstChartLoad) //secondload
            {
                chartTabPage.Controls.Remove(swfChart);
                // swfChart.Stop();                 
                swfChart.Dispose();
                swfChart = null;
                creatChartControl();
            }
            //swfChart.Width = 100 swfChart.CreateControl();.Height = 700;
            FchartXML _xml = new FchartXML();
            _xml.Caption = "Stock List";
            _xml.SubCaption = "Yogi Ram Surat Kumar";
            _xml.BackColor = Color.Pink;
            _xml.RotateLabel = "1";
            _xml.XAxisName = "Medicine Name";
            _xml.YAxisName = "Stock";

            swfChart.LoadMovie(0, files[chartCombo.SelectedIndex]);
            //swfChart.Movie = files[chartCombo.SelectedIndex];

            //swfChart.SetVariable("registerwithjs", "1");
            if (files[chartCombo.SelectedIndex].Contains("FCF_MS"))
                swfChart.SetVariable("dataXML", _xml.GetXMLMS(CreateTable(), ""));
            else if (files[chartCombo.SelectedIndex].Contains("FCF_Stacked"))
                swfChart.SetVariable("dataXML", _xml.GetXMLStack(CreateTable(), ""));
            else
                swfChart.SetVariable("dataXML", _xml.GetXML(CreateTable(), ""));
            swfChart.ScaleMode = 3;
            swfChart.AlignMode = 0;
            swfChart.SetVariable("chartHeight", "200");
            swfChart.SetVariable("chartWidth", "200");
            swfChart.Play();
            chartImgLoc = Path.GetFileNameWithoutExtension(files[chartCombo.SelectedIndex]).Replace("FCF_", string.Empty) + ".jpg";
            string filename = Path.Combine(defLoc, chartImgLoc);
            if (chartText.Text != "No Data to be loaded")
            {
                th = new Thread(new ThreadStart(WorkThread));
                th.Start();
            }
        }

        private void WorkThread()
        {
            ChartImgControls();
        }
        private void ChartImgControls()
        {
            if (InvokeRequired)
            {
                MethodInvoker method = new MethodInvoker(ChartImgControls);
                Thread.Sleep(10000);
                Invoke(method);
                return;
            }
            ScreenCapImg(Path.Combine(defLoc, chartImgLoc));
            chartText.Text = "Img is getting saved in " + Path.Combine(defLoc, chartImgLoc);
        }

        private void chartImgBtn_Click(object sender, EventArgs e)
        {
            SaveFileDialog sfd = new SaveFileDialog();
            sfd.Filter = "JPeg Image|*.jpg|Bitmap Image|*.bmp|Gif Image|*.gif";
            sfd.FileName = chartImgLoc;
            sfd.DefaultExt = "*.jpg";
            sfd.FilterIndex = 0;
            sfd.RestoreDirectory = true;
            sfd.Title = "Export YRSK Medical Camp Chart Image File To";
            if (sfd.ShowDialog() == DialogResult.OK) // Test result.
            {
                ScreenCapImg(sfd.FileName);
            }
        }
        private void ScreenCapImg(string FileName)
        {
            ScreenCapture sc = new ScreenCapture();
            System.Drawing.Image img = sc.CaptureScreen();
            sc.CaptureWindowToFile(swfChart.Handle, FileName, ImageFormat.Jpeg);
            chartImgLoc = FileName;
        }
        private static void GrantAccess(string file)
        {
            bool exists = System.IO.Directory.Exists(file);
            if (!exists)
            {
                DirectoryInfo di = System.IO.Directory.CreateDirectory(file);
                Console.WriteLine("The Folder is created Sucessfully");
            }
            else
            {
                Console.WriteLine("The Folder already exists");
            }
            System.Diagnostics.Process process = new System.Diagnostics.Process();
            System.Diagnostics.ProcessStartInfo startInfo = new System.Diagnostics.ProcessStartInfo();
            startInfo.WindowStyle = System.Diagnostics.ProcessWindowStyle.Hidden;
            startInfo.FileName = "cmd.exe";
            startInfo.Arguments = @"cacls " + file + " /t /e /g Everyone:f";
            process.StartInfo = startInfo;
            process.Start();

        }
        #endregion
        List<KeyValuePair<string, string>> chartMap = new List<KeyValuePair<string, string>>();
        private bool IsChanging = true;
        private void dgvOldData_CellContentClick(object sender, System.Windows.Forms.DataGridViewCellEventArgs e)
        {
            int columnIndex = dgvOldData.CurrentCell.ColumnIndex;
            if (dgvOldData.Rows.Count > 0 && e.RowIndex != -1)
            {       //&& dgvOldData.CurrentCell.OwningColumn.Name != "Balance"        
                txtName.Text = dgvOldData.Rows[e.RowIndex].Cells["TabletName"].Value.ToString();
                txtBS.Text = dgvOldData.Rows[e.RowIndex].Cells["BeforeStock"].Value.ToString();
                txtAS.Text = dgvOldData.Rows[e.RowIndex].Cells["AfterStock"].Value.ToString();
                valueAmt();
                dgvOldData.Rows[e.RowIndex].Cells["Balance"].Value = lblAmt.Text;
                lblAmt.Text = dgvOldData.Rows[e.RowIndex].Cells["Balance"].Value.ToString();
                txtExpiry.Text = dgvOldData.Rows[e.RowIndex].Cells["Expiry"].Value.ToString();
                dateTimePicker1.Text = dgvOldData.Rows[e.RowIndex].Cells["Date"].Value.ToString();
                lblId.Text = dgvOldData.Rows[e.RowIndex].Cells["Tabid"].Value.ToString();
                lblid2.Text = dgvOldData.Rows[e.RowIndex].Cells["ChartShow"].Value.ToString();
                txtGroup.Text = dgvOldData.Rows[e.RowIndex].Cells["Groups"].Value.ToString();

                saveSetting();
                btnDelete.Enabled = true;
                brnUpdate.Enabled = true;
                btnAdd.Enabled = false;
                txtName.Enabled = false;
                txtGroup.Enabled = false;
                //this.BindGrid(sda, false);                
            }
        }
        private void dgvOldData_CellValueChanged(object sender, DataGridViewCellEventArgs e)
        {
            if (dgvOldData.Rows.Count > 0 && e.RowIndex != -1 && dgvOldData.Columns[e.ColumnIndex].Name == "ChartShow" && IsChanging && HeaderCheckBox.CheckState == CheckState.Indeterminate)
            {
                bool checker = (bool)((DataGridViewCheckBoxCell)dgvOldData.Rows[e.RowIndex].Cells["ChartShow"]).Value;
                string sameValueUnCheck = ((DataGridViewTextBoxCell)dgvOldData.Rows[e.RowIndex].Cells[1]).Value.ToString();
                try
                {
                    this.IsChanging = false;
                    RowCheckBoxClick(checker);
                    foreach (DataGridViewRow row in dgvOldData.Rows)
                    {
                        if (row.Cells[1].Value.ToString() == sameValueUnCheck)
                        {
                            months = Convert.ToDateTime(row.Cells["Date"].Value).ToString("MMM-yyyy");
                            if (checker)
                            {
                                ((DataGridViewCheckBoxCell)row.Cells["ChartShow"]).Value = true;
                                chartMap.Add(new KeyValuePair<string, string>(row.Cells["TabletName"].Value.ToString(), row.Cells["Balance"].Value + "@" + row.Cells["BeforeStock"].Value + "@" + row.Cells["AfterStock"].Value + "@" + months));
                            }
                            else
                            {
                                ((DataGridViewCheckBoxCell)row.Cells["ChartShow"]).Value = false;
                                chartMap.Remove(new KeyValuePair<string, string>(row.Cells["TabletName"].Value.ToString(), row.Cells["Balance"].Value + "@" + row.Cells["BeforeStock"].Value + "@" + row.Cells["AfterStock"].Value + "@" + months));
                            }
                        }
                    }
                    dgvOldData.RefreshEdit();
                }
                finally
                {
                    IsChanging = true;
                    chartMap.OrderBy(x => x.Key);
                }
            }
            else if (dgvOldData.Rows.Count > 0 && e.RowIndex != -1 && dgvOldData.Columns[e.ColumnIndex].Name == "ChartShow" && IsChanging && HeaderCheckBox.CheckState == CheckState.Checked)
            {
                bool checker = (bool)((DataGridViewCheckBoxCell)dgvOldData.Rows[e.RowIndex].Cells["ChartShow"]).Value;
                months = Convert.ToDateTime(dgvOldData.Rows[e.RowIndex].Cells["Date"].Value).ToString("MMM-yyyy");
                if (checker)
                {
                    months = Convert.ToDateTime(dgvOldData.Rows[e.RowIndex].Cells["Date"].Value).ToString("MMM-yyyy");
                    chartMap.Add(new KeyValuePair<string, string>(dgvOldData.Rows[e.RowIndex].Cells["TabletName"].Value.ToString(), dgvOldData.Rows[e.RowIndex].Cells["Balance"].Value + "@" + dgvOldData.Rows[e.RowIndex].Cells["BeforeStock"].Value + "@" + dgvOldData.Rows[e.RowIndex].Cells["AfterStock"].Value + "@" + months));
                }
                else
                {
                    chartMap.Remove(new KeyValuePair<string, string>(dgvOldData.Rows[e.RowIndex].Cells["TabletName"].Value.ToString(), dgvOldData.Rows[e.RowIndex].Cells["Balance"].Value + "@" + dgvOldData.Rows[e.RowIndex].Cells["BeforeStock"].Value + "@" + dgvOldData.Rows[e.RowIndex].Cells["AfterStock"].Value + "@" + months));
                }
                chartMap.OrderBy(x => x.Key);
            }
            else if (dgvOldData.Rows.Count > 0 && e.RowIndex != -1 && (dgvOldData.Columns[e.ColumnIndex].Name == "BeforeStock" || dgvOldData.Columns[e.ColumnIndex].Name == "AfterStock"))
            {
                txtBS.Text = dgvOldData.Rows[e.RowIndex].Cells["BeforeStock"].Value.ToString();
                txtAS.Text = dgvOldData.Rows[e.RowIndex].Cells["AfterStock"].Value.ToString();
                valueAmt();
                dgvOldData[4, e.RowIndex].Value = lblAmt.Text;
                Update(lblId.Text);
            }


        }
        private void RowCheckBoxClick(bool RCheckBox)
        {
            if (RCheckBox && TotalCheckedCheckBoxes < TotalCheckBoxes)
                TotalCheckedCheckBoxes++;
            else if (TotalCheckedCheckBoxes > 0)
                TotalCheckedCheckBoxes--;
        }
        private void dgvOldData_CurrentCellDirtyStateChanged(object sender, EventArgs e)
        {
            if (dgvOldData.CurrentCell is DataGridViewCheckBoxCell && IsChanging)
                dgvOldData.CommitEdit(DataGridViewDataErrorContexts.Commit);



        }
        private void HeaderCheckBox_MouseClick(object sender, MouseEventArgs e)
        {
            HeaderCheckBoxClick((CheckBox)sender);
        }
        private void HeaderCheckBox_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Space)
                HeaderCheckBoxClick((CheckBox)sender);
        }
        private void ResetHeaderCheckBoxLocation(int ColumnIndex, int RowIndex)
        {
            //Get the column header cell bounds
            System.Drawing.Rectangle oRectangle = this.dgvOldData.GetCellDisplayRectangle(ColumnIndex, RowIndex, true);
            Point oPoint = new Point();
            oPoint.X = oRectangle.Location.X + (oRectangle.Width - 20);
            oPoint.Y = oRectangle.Location.Y;
            //Change the location of the CheckBox to make it stay on the header
            HeaderCheckBox.Location = oPoint;
        }
        private void dgvOldData_CellPainting(object sender, DataGridViewCellPaintingEventArgs e)
        {
            if (e.RowIndex == -1 && e.ColumnIndex == 8)
                ResetHeaderCheckBoxLocation(e.ColumnIndex, e.RowIndex);
        }

        private void HeaderCheckBoxClick(CheckBox HCheckBox)
        {
            if (IsChanging)
            {
                this.IsChanging = false;
                IsHeaderCheckBoxClicked = IsHeaderCheckBoxClicked == true ? false : true;
                DataSet oldData = new DataSet();
                sdb.Fill(oldData);
                List<string> tabids = new List<string>();
                for (int intCount = 0; intCount < dgvOldData.RowCount; intCount++)
                {
                    tabids.Add(dgvOldData.Rows[intCount].Cells["Tabid"].Value.ToString());
                    dgvOldData.Rows[intCount].Cells["ChartShow"].Value = IsHeaderCheckBoxClicked;
                }
                for (int intCount = 0; intCount < oldData.Tables[0].Rows.Count; intCount++)
                {
                    if (tabids.Contains(oldData.Tables[0].Rows[intCount]["Tabid"].ToString()))
                    {
                        oldData.Tables[0].Rows[intCount]["ChartShow"] = IsHeaderCheckBoxClicked;
                    }
                }
                sdb.Update(oldData);
                dgvOldData.RefreshEdit();
                TotalCheckedCheckBoxes = HCheckBox.Checked ? TotalCheckBoxes : 0;
                this.IsChanging = true;
            }
        }
        private void chk_CheckStateChanged(object sender, EventArgs e)
        {
            if (IsChanging)
            {
                CheckBox HCheckBox = sender as CheckBox;
                if (HCheckBox.Name == "HeaderCheckBox")
                {
                    this.IsChanging = true;
                    //MessageBox.Show(HCheckBox.CheckState.ToString());
                    if (HCheckBox.CheckState != CheckState.Indeterminate)
                    {
                        HeaderCheckBoxClick(HCheckBox);
                    }
                }
            }
        }

        public static bool CheckForInternetConnection()
        {
            try
            {
                using (var client = new WebClient())
                {
                    using (var stream = client.OpenRead("http://www.google.com"))
                    {
                        return true;
                    }
                }
            }
            catch
            {
                return false;
            }
        }
        public bool CheckForCredentials()
        {
            gUserName.Text = Settings.Default.GUserName;
            gPassword.Text = Settings.Default.GPwd;
            //Settings.Default.Save();
            if (string.IsNullOrEmpty(gUserName.Text) && string.IsNullOrEmpty(gUserName.Text))
                return true;
            return false;
        }
        private void Mailing_Enter(object sender, EventArgs e)
        {
            bool Icheck = CheckForInternetConnection();
            bool GCred = CheckForCredentials();
            if (Icheck && GCred)
            {
                ErrorMsgMail.Visible = false;
                MailPanel.Visible = true;
            }
            else if (Icheck && !GCred)
            {
                MailBasePanel.Visible = true;
            }
            else
                ErrorMsgMail.Text = "No Internet Connection";
        }

        private void sendMail_Click(object sender, EventArgs e)
        {
            try
            {
                MailMessage mail = new MailMessage();
                SmtpClient SmtpServer = new SmtpClient("smtp.gmail.com");
                mail.From = new MailAddress("your_email_address@gmail.com");
                mail.To.Add("to_address");
                mail.Subject = "Test Mail - 1";
                mail.Body = "mail with attachment";

                System.Net.Mail.Attachment attachment;
                attachment = new System.Net.Mail.Attachment("your attachment file");
                mail.Attachments.Add(attachment);

                SmtpServer.Port = 587;
                SmtpServer.Credentials = new System.Net.NetworkCredential("username", "password");
                SmtpServer.EnableSsl = true;

                SmtpServer.Send(mail);
                MessageBox.Show("mail Send");
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
        }

        private void clrLoc_Click(object sender, EventArgs e)
        {
            chartImgLoc = string.Empty;
        }

        private void clrLoc_MouseHover(object sender, EventArgs e)
        {
            if (chartImgLoc == string.Empty)
                this.clrLoc.BackColor = System.Drawing.Color.Black;
            else
                this.clrLoc.BackColor = System.Drawing.SystemColors.ActiveBorder;
        }

        private void dgvOldData_EditingControlShowing(object sender, DataGridViewEditingControlShowingEventArgs e)
        {
            if (this.dgvOldData.CurrentCell.OwningColumn.Name == "Balance" || this.dgvOldData.CurrentCell.OwningColumn.Name == "BeforeStock" || this.dgvOldData.CurrentCell.OwningColumn.Name == "AfterStock")
            {
                if (e.Control is TextBox)
                {
                    TextBox tb = e.Control as TextBox;
                    tb.KeyPress -= new KeyPressEventHandler(txtBS_KeyPress);
                    tb.KeyPress += new KeyPressEventHandler(txtBS_KeyPress);
                }
            }
        }

        private void themeTab_Enter(object sender, EventArgs e)
        {





        }

        private void myTreeView_DragDrop(object sender, DragEventArgs e)
        {
            Point targetPoint = myTreeView.PointToClient(new Point(e.X, e.Y));
            // Retrieve the node at the drop location.
            TreeNode targetNode = myTreeView.GetNodeAt(targetPoint);
            // Retrieve the node that was dragged.
            TreeNode draggedNode = (TreeNode)e.Data.GetData(typeof(TreeNode));
            // Confirm that the node at the drop location is not 
            // the dragged node and that target node isn't null
            // (for example if you drag outside the control)
            if (!draggedNode.Equals(targetNode) && targetNode != null && !gp.Contains(draggedNode))
            {
                // Remove the node from its current 
                // location and add it to the node at the drop location.
                draggedNode.Remove();
                targetNode.Nodes.Add(draggedNode);
                DataSet oldData = new DataSet();
                DataRow dr;
                sdt.Fill(oldData);
                dr = oldData.Tables[0].NewRow();
                dr["TabletName"] = draggedNode.Text;
                //namesCollection.Add(dr["TabletName"].ToString());
                dr["Grouping"] = targetNode.Text;
                oldData.Tables[0].Rows.Add(dr); sdt.Update(oldData);
                // Expand the node at the location 
                // to show the dropped node.
                targetNode.Expand();
            }
        }

        private void myTreeView_ItemDrag(object sender, ItemDragEventArgs e)
        {
            DoDragDrop(e.Item, DragDropEffects.Move);
        }

        private void myTreeView_DragEnter(object sender, DragEventArgs e)
        {
            e.Effect = DragDropEffects.Move;
        }

        private void exportMonth_SelectedIndexChanged(object sender, EventArgs e)
        {
            var lst = exportMonth.SelectedItems;//.Cast<DataRowView>();
            if (lst.Count == 0)
            {
                exportMonth.SetSelected(0, true);
            }
            else
            {
                foreach (KeyValuePair<string, string> item in lst)
                {
                    // MessageBox.Show(item.Value.ToString());// Or Row[1]...
                }
            }
        }

        private void gSign_Click(object sender, EventArgs e)
        {
            if (!string.IsNullOrEmpty(gUserName.Text))
            {
                this.errorProvider1.SetError(gUserName, "");
            }
            else
            {
                this.errorProvider1.SetError(gUserName, "Email Id is not Valid");
            }         
            if (txtMinLengthTestIsValid(gPassword,8))
            {
                this.errorProvider1.SetError(gPassword,
                    "This Password must contain at least 8 characters");
            }
            else
            {
                this.errorProvider1.SetError(gPassword, "");
            }
        }









    }

    class Groups
    {
        string _groupName = "";
        public string GroupsNames
        {
            get { return _groupName; }
            set { _groupName = value; }
        }
        string _tabletName = "";
        public string TabletNames
        {
            get { return _tabletName; }
            set { _tabletName = value; }
        }
    }
}

