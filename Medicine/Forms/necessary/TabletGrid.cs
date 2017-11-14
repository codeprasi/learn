using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MedicalCamp.Forms.necessary
{
    class TabletGrid
    {
        public TabletGrid(int id, string TabletName, string BeforeStock, string AfterStock,string Balance,string Expiry,string Tabid,string ChartShow,string Date)
        {
            this.Id = id;
            this.tabletName = TabletName;
            this.beforeStock = BeforeStock;
        }
        private int Id;
        public int ID
        {
            get { return Id; }
            set { Id = value; }
        }
        private string tabletName;
        public string TABLETNAME
        {
            get { return tabletName; }
            set { tabletName = value; }
        }
        private string beforeStock;
        public string BEFORESTOCK
        {
            get { return beforeStock; }
            set { beforeStock = value; }
        }
        private string afterStock;
        public string AFTERSTOCK
        {
            get { return afterStock; }
            set { afterStock = value; }
        }
        private string balance;
        public string BALANCE
        {
            get { return balance; }
            set { balance = value; }
        }
        private string expiry;
        public string EXPIRY
        {
            get { return expiry; }
            set { expiry = value; }
        }
        private string tabid;
        public string TABID
        {
            get { return tabid; }
            set { tabid = value; }
        }
        private string chartShow;
        public string CHARTSHOW
        {
            get { return chartShow; }
            set { chartShow = value; }
        }
        private string date;
        public string DATE
        {
            get { return date; }
            set { date = value; }
        }

    }
}
