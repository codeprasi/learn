using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace MedicalCamp
{
    public partial class Form3 : Form
    {
        public Form3()
        {
            InitializeComponent();
        }
        private void Form1_Load(object sender, EventArgs e)
        {
           /* Second  column grouping
            * this.dataGridView1.Columns.Add("JanWin", "Win");
            this.dataGridView1.Columns.Add("JanLoss", "Loss");
            this.dataGridView1.Columns.Add("FebWin", "Win");
            this.dataGridView1.Columns.Add("FebLoss", "Loss");
            this.dataGridView1.Columns.Add("MarWin", "Win");
            this.dataGridView1.Columns.Add("MarLoss", "Loss");

            for (int j = 0; j < this.dataGridView1.ColumnCount; j++)
            {
                this.dataGridView1.Columns[j].Width = 45;
            }
            this.dataGridView1.ColumnHeadersHeightSizeMode =
                 DataGridViewColumnHeadersHeightSizeMode.EnableResizing;
            this.dataGridView1.ColumnHeadersHeight =
                        this.dataGridView1.ColumnHeadersHeight * 2;
            this.dataGridView1.ColumnHeadersDefaultCellStyle.Alignment =
                 DataGridViewContentAlignment.BottomCenter;
            this.dataGridView1.CellPainting += new
                 DataGridViewCellPaintingEventHandler(dataGridView1_CellPainting2);
            this.dataGridView1.Paint += new PaintEventHandler(dataGridView1_Paint);*/
            //First
            //DataTable dt = new DataTable();
            //dt.Columns.Add("c1");
            //dt.Columns.Add("c2");
            //for (int j = 0; j < 10; j++)
            //{
            //    dt.Rows.Add("aaaaaaaaa", "bbbb");
            //}
            //this.dataGridView1.DataSource = dt;

            //for (int j = 0; j < 10; j++)
            //{
            //    int height = TextRenderer.MeasureText(
            //        this.dataGridView1[0, j].Value.ToString(),
            //        this.dataGridView1.DefaultCellStyle.Font).Width;
            //    this.dataGridView1.Rows[j].Height = height;
            //}
           // this.dataGridView1.CellPainting += new DataGridViewCellPaintingEventHandler(dataGridView1_CellPainting);

        }

        void dataGridView1_CellPainting(object sender, DataGridViewCellPaintingEventArgs e)//First vertical text
        {
            if (e.ColumnIndex == 0 && e.RowIndex > -1 && e.Value != null)
            {
                e.Paint(e.CellBounds, DataGridViewPaintParts.All
                     & ~DataGridViewPaintParts.ContentForeground);
                StringFormat sf = new StringFormat();
                sf.Alignment = StringAlignment.Center;
                sf.LineAlignment = StringAlignment.Center;
                sf.FormatFlags = StringFormatFlags.DirectionVertical;
                e.Graphics.DrawString(e.Value.ToString(), e.CellStyle.Font,
                    new SolidBrush(e.CellStyle.ForeColor), e.CellBounds, sf);
                e.Handled = true;
            }
        }
        void dataGridView1_Paint(object sender, PaintEventArgs e) //Second  column grouping
        {
            string[] monthes = { "January", "February", "March" };
            for (int j = 0; j < 6; )
            {
                //get the column header cell
                Rectangle r1 = this.dataGridView1.GetCellDisplayRectangle(j, -1, true);

                r1.X += 1;
                r1.Y += 1;
                r1.Width = r1.Width * 2 - 2;
                r1.Height = r1.Height / 2 - 2;
                e.Graphics.FillRectangle(new
                   SolidBrush(this.dataGridView1.ColumnHeadersDefaultCellStyle.BackColor), r1);
                StringFormat format = new StringFormat();
                format.Alignment = StringAlignment.Center;
                format.LineAlignment = StringAlignment.Center;
                e.Graphics.DrawString(monthes[j / 2],
                    this.dataGridView1.ColumnHeadersDefaultCellStyle.Font,
                    new SolidBrush(this.dataGridView1.ColumnHeadersDefaultCellStyle.ForeColor),
                    r1,
                    format);
                j += 2;
            }
        }
        void dataGridView1_CellPainting2(object sender, DataGridViewCellPaintingEventArgs e) //Second  column grouping
        {
            if (e.RowIndex == -1 && e.ColumnIndex > -1)
            {
                e.PaintBackground(e.CellBounds, false);

                Rectangle r2 = e.CellBounds;
                r2.Y += e.CellBounds.Height / 2;
                r2.Height = e.CellBounds.Height / 2;
                e.PaintContent(r2);
                e.Handled = true;
            }
        }

       
    }
    public class GroupByGrid : DataGridView
    {

        protected override void OnCellFormatting(
           DataGridViewCellFormattingEventArgs args)
        {
            // Call home to base
            base.OnCellFormatting(args);

            // First row always displays
            if (args.RowIndex == 0)
                return;


            if (IsRepeatedCellValue(args.RowIndex, args.ColumnIndex))
            {
                args.Value = string.Empty;
                args.FormattingApplied = true;
            }
        }

        private bool IsRepeatedCellValue(int rowIndex, int colIndex)
        {
            DataGridViewCell currCell =
               Rows[rowIndex].Cells[colIndex];
            DataGridViewCell prevCell =
               Rows[rowIndex - 1].Cells[colIndex];

            if ((currCell.Value == prevCell.Value) ||
               (currCell.Value != null && prevCell.Value != null &&
               currCell.Value.ToString() == prevCell.Value.ToString()))
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        protected override void OnCellPainting(
           DataGridViewCellPaintingEventArgs args)
        {
            base.OnCellPainting(args);

            args.AdvancedBorderStyle.Bottom =
               DataGridViewAdvancedCellBorderStyle.None;

            // Ignore column and row headers and first row
            if (args.RowIndex < 1 || args.ColumnIndex < 0)
                return;

            if (IsRepeatedCellValue(args.RowIndex, args.ColumnIndex))
            {
                args.AdvancedBorderStyle.Top =
                   DataGridViewAdvancedCellBorderStyle.None;
            }
            else
            {
                args.AdvancedBorderStyle.Top = AdvancedCellBorderStyle.Top;
            }
        }
    }
}