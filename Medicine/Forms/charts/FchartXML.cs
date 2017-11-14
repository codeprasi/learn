using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using System.Drawing;

namespace MedicalCamp.Forms
{
   public class FchartXML
    {
        
  string _caption = "";
        public string Caption {
	get { return _caption; }
	set { _caption = value; }
}


string _subCaption = "";
public string SubCaption {
	get { return _subCaption; }
	set { _subCaption = value; }
}

Color _bcolor = Color.White;
public Color BackColor {
	get { return _bcolor; }
	set { _bcolor = value; }
}

string _yaxisname = "";
public string YAxisName {
	get { return _yaxisname; }
	set { _yaxisname = value; }
}

static string _xaxisname = "";
public string XAxisName {
	get { return _xaxisname; }
	set { _xaxisname = value; }
}

static string _rotatelabel = "0";
public string RotateLabel
{
    get { return _rotatelabel; }
    set { _rotatelabel = value; }
}

DataTable _table;
string _other = "";

public string GetXML(DataTable Table,string Other = "")
{
    _table = Table;
	_other = Other;
	if ((_table.Rows.Count <= 0))
        return "<chart></chart>";
	if ((!_table.Columns.Contains("Value")))
        return "<chart></chart>";
	if ((!_table.Columns.Contains("Name")))
        return "<chart></chart>";

	int iBColor = Convert.ToInt32(_bcolor.ToArgb());

    string _xml = "<graph showColumnShadow='1' caption='" + _caption + "' ";
	_xml += "subCaption ='" + _subCaption + "' ";
    _xml += "bgColor ='" + (Convert.ToInt32(iBColor)).ToString("X") +"' ";
	_xml += "yaxisname ='" + _yaxisname + "' ";
	_xml += "xaxisname ='" + _xaxisname + "' ";
    _xml += "decimalPrecision='" + 0 + "' ";
    _xml += "rotateNames='" + _rotatelabel + "' ";
    _xml += "showvalues='" + 1 + "' ";   
	_xml += _other + ">";

	foreach (DataRow row in _table.Rows) {
		_xml += "<set value='" + row["Value"] + "' name='" + row["Name"] + "'";
		if ((_table.Columns.Contains("Color"))) {
			_xml += "color='" + row["Color"] + "'/>";
		}
		_xml += "/>";
	}

	_xml += "</graph>";

	return _xml;
}



internal string GetXMLStack(DataTable Table, string Other = "")
{
    _table = Table;
    _other = Other;
    if ((_table.Rows.Count <= 0))
        return "";
    if ((!_table.Columns.Contains("Value")))
        return "";
    if ((!_table.Columns.Contains("Name")))
        return "";

    int iBColor = Convert.ToInt32(_bcolor.ToArgb());

    string _xml = "<graph showColumnShadow='1' caption='" + _caption + "' ";
    _xml += "subCaption ='" + _subCaption + "' ";
    _xml += "bgColor ='" + (Convert.ToInt32(iBColor)).ToString("X") + "' ";
    _xml += "yaxisname ='" + _yaxisname + "' ";
    _xml += "xaxisname ='" + _xaxisname + "' ";
    _xml += "decimalPrecision='" + 0 + "' ";
    _xml += "rotateNames='" + _rotatelabel + "' ";
    _xml += "showvalues='" + 0 + "' ";
    _xml += _other + ">";

    foreach (DataRow row in _table.Rows)
    {
        _xml += "<set value='" + row["Value"] + "' name='" + row["Name"] + "'";
        if ((_table.Columns.Contains("Color")))
        {
            _xml += "color='" + row["Color"] + "'/>";
        }
        _xml += "/>";
    }

    _xml += "</graph>";

    return _xml;
}
internal string GetXMLMS(DataTable Table, string Other = "")
{
    _table = Table;
    _other = Other;
    if ((_table.Rows.Count <= 0))
        return "";
    if ((!_table.Columns.Contains("Value")))
        return "";
    if ((!_table.Columns.Contains("Name")))
        return "";

    int iBColor = Convert.ToInt32(_bcolor.ToArgb());

    string _xml = "<graph showColumnShadow='1' caption='" + _caption + "' ";
    _xml += "subCaption ='" + _subCaption + "' ";
    _xml += "bgColor ='" + (Convert.ToInt32(iBColor)).ToString("X") + "' ";
    _xml += "yaxisname ='" + _yaxisname + "' ";
    _xml += "xaxisname ='" + _xaxisname + "' ";
    _xml += "decimalPrecision='" + 0 + "' ";
    _xml += _other + ">";

    foreach (DataRow row in _table.Rows)
    {
        _xml += "<set value='" + row["Value"] + "' name='" + row["Name"] + "'";
        if ((_table.Columns.Contains("Color")))
        {
            _xml += "color='" + row["Color"] + "'/>";
        }
        _xml += "/>";
    }

    _xml += "</graph>";

    return _xml;
}
    }
}
