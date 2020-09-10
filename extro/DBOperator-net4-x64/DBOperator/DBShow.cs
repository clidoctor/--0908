using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Data.OleDb;
using System.IO;
using System.Text.RegularExpressions;
using ADODB;

namespace DBOperator
{
    public partial class DBShow : UserControl
    {
        public DBShow()
        {
            InitializeComponent();

            if (accConn != null)
            {
                ShowTableNames(combox_table_names);
                Console.Write("");
            }
        }



        private OleDbConnection accConn;
        private OleDbCommand myCommand;
        private OleDbDataAdapter da;

        #region  建立数据库连接
        /// <summary>
        /// 建立数据库连接.
        /// </summary>
        /// <returns></returns>
        public string ConnectMdb(string dbPath)
        {
            try
            {
                ADOX.Catalog cataLog = new ADOX.Catalog();
                bool a = File.Exists(dbPath);
                if (!File.Exists(dbPath))
                {
                    cataLog.Create("Provider=Microsoft.ACE.OLEDB.12.0;Data Source=" + dbPath + ";" + "Jet OLEDB:Engine Type=5");
                }
                string accConnStr = "Provider=Microsoft.ACE.OLEDB.12.0;Data Source= " + dbPath;
                accConn = new OleDbConnection(accConnStr);
                accConn.Open();
                ShowTableNames(combox_table_names);
            }
            catch (Exception ex)
            {
                return ex.Message + "  -->ConnectMdb";
            }
            return "OK";
        }

        public string DisconnectMdb()
        {
            try
            {
                if (accConn!=null)
                {
                    accConn.Dispose();
                }
            }
            catch (Exception ex)
            {
                return ex.Message + "  -->ConnectMdb";
            }
            return "OK";
        }
        #endregion
        #region  添加表
        /// <summary>
        /// 添加表.
        /// </summary>
        /// <returns></returns>
        public string CreateTable(string dbPath, string tableName, string[] fieldNames, string[] fieldTypes)
        {
            if (fieldNames.Length != fieldTypes.Length)
            {
                return "field name count discordance with type count  -->CreateTable";
            }
            string names = GetTableName();
            if (names.Contains(tableName))
            {
                return "this data base already exist the table  -->CreateTable";
            }
            ADOX.DataTypeEnum[] ADOXFieldTypes = new ADOX.DataTypeEnum[fieldTypes.Length];
            for (int i = 0; i < fieldTypes.Length; i++)
            {
                switch (fieldTypes[i])
                {
                    case "int":
                        ADOXFieldTypes[i] = ADOX.DataTypeEnum.adInteger;
                        break;
                    case "string":
                        ADOXFieldTypes[i] = ADOX.DataTypeEnum.adVarWChar;
                        break;
                    case "double":
                        ADOXFieldTypes[i] = ADOX.DataTypeEnum.adDouble;
                        break;
                    case "bool":
                        ADOXFieldTypes[i] = ADOX.DataTypeEnum.adBoolean;
                        break;
                    default:
                        return "nonsupport the data type  -->CreateTable";
                }
            }
            try
            {
                ADOX.Catalog cataLog = new ADOX.Catalog();
                ADODB.Connection cn = new ADODB.Connection();
                cn.Open("Provider=Microsoft.ACE.OLEDB.12.0;Data Source=" + dbPath + ";" + "Jet OLEDB:Engine Type=5");
                cataLog.ActiveConnection = cn;

                ADOX.Table table = new ADOX.Table();
                table.ParentCatalog = cataLog;
                table.Name = tableName;


                for (int i = 0; i < fieldNames.Length; i++)
                {
                    ADOX.Column col = new ADOX.Column();
                    col.ParentCatalog = cataLog;
                    col.Type = ADOXFieldTypes[i];
                    col.Name = fieldNames[i];

                    col.Properties["Jet OLEDB:Allow Zero Length"].Value = true;
                    col.Properties["AutoIncrement"].Value = false;   //自动编号,注意此处不允许自动编号
                    table.Columns.Append(col, ADOX.DataTypeEnum.adDouble, 50);
                }
                cataLog.Tables.Append(table);
                ShowTableNames(combox_table_names);
                return "OK";
            }
            catch (Exception ex)
            {
                return ex.Message + "  -->CreateTable";
            }

        }

        #endregion


        #region  增删改查
        /// <summary>
        /// 增加数据
        /// </summary>
        /// <returns></returns>
        public string InsertData(string tableName, string[] fieldNames, string[] fieldValues, string[] fieldTypes)
        {
            if (fieldNames.Length != fieldValues.Length || fieldValues.Length != fieldTypes.Length)
            {
                return "field name count discordance with value count or type count  -->InsertData";
            }
            string names = "";
            string values = "";
            transArr(fieldValues, fieldTypes);
            for (int i = 0; i < fieldNames.Length; i++)
            {
                names += "[" + fieldNames[i] + "]";
                values += fieldValues[i];
                if (i < fieldNames.Length - 1)
                {
                    names += ",";
                    values += ",";
                }
            }
            try
            {
                string sql = "insert into[" + tableName + "](" + names + ") values(" + values + ")";
                myCommand = new OleDbCommand(sql, accConn);
                int a = myCommand.ExecuteNonQuery();
                return a == 1 ? "OK" : "fail";
            }
            catch (Exception ex)
            {
                return ex.Message + "  -->InsertData";
            }
        }

        /// <summary>
        /// 修改数据
        /// </summary>
        /// <returns></returns>
        public string UpdateData(string tableName, string[] fieldNames, string[] fieldValues, string[] fieldTypes, string oldName, string oldValue, string oldType)
        {
            if (fieldNames.Length != fieldValues.Length || fieldValues.Length != fieldTypes.Length)
            {
                return "field name count discordance with value count or type count  -->UpdateData";
            }
            string str = "";
            transArr(fieldValues, fieldTypes);
            for (int i = 0; i < fieldNames.Length; i++)
            {
                str += "[" + fieldNames[i] + "]=" + fieldValues[i];
                if (i < fieldNames.Length - 1)
                {
                    str += ",";
                }
            }
            transStr(ref oldValue, oldType);
            string sql = "update [" + tableName + "] set  " + str + " where [" + oldName + "]=" + oldValue + "";
            try
            {
                myCommand = new OleDbCommand(sql, accConn);
                int a = myCommand.ExecuteNonQuery();
                return a == 1 ? "OK" : "fail";
            }
            catch (Exception ex)
            {
                return ex.Message + "  -->UpdateData";
            }

        }

        /// <summary>
        /// 查询.
        /// </summary>
        /// <returns></returns>
        public string Query(string tableName)
        {
            string names;
            try
            {
                names = GetTableName();
            }
            catch (Exception)
            {
                return "Get data base table fail  -->Query";
            }

            if (!names.Contains(tableName))
            {
                return "this data base don't contain the table  -->Query";
            }
            string sql = "select * from [" + tableName + "]";
            try
            {
                da = new OleDbDataAdapter(sql, accConn);
                ShowData(da);
                return "OK";
            }
            catch (Exception ex)
            {
                return ex.Message + "  -->Query";
            }
        }

        public string Query(string tableName, out DataTable dt)
        {
            dt = new DataTable();
            string names;
            try
            {
                names = GetTableName();
            }
            catch (Exception)
            {
                return "Get data base table fail  -->Query";
            }

            if (!names.Contains(tableName))
            {
                return "this data base don't contain the table  -->Query";
            }
            string sql = "select * from [" + tableName + "]";
            try
            {
                da = new OleDbDataAdapter(sql, accConn);
                da.Fill(dt);
                return "OK";
            }
            catch (Exception ex)
            {
                return ex.Message + "  -->Query";
            }
        }

        public string Query(string tableName, string fieldName, string fieldValue, string fieldType)
        {
            string names;
            try
            {
                names = GetTableName();
            }
            catch (Exception)
            {
                return "Get data base table fail  -->Query";
            }
            if (!names.Contains(tableName))
            {
                return "this data base don't contain the table  -->Query";
            }
            transStr(ref fieldValue, fieldType);
            try
            {
                string sql = "select * from [" + tableName + "] where " + fieldName + "=" + fieldValue;
                da = new OleDbDataAdapter(sql, accConn);
                ShowData(da);
                return "OK";
            }
            catch (Exception ex)
            {
                return ex.Message + "  -->Query";
            }
        }

        /// <summary>
        /// 删除表.
        /// </summary>
        /// <returns></returns>
        public string RemoveTable(string tableName)
        {
            string sql = "Drop table [" + tableName + "]";
            try
            {
                myCommand = new OleDbCommand(sql, accConn);
                int a = myCommand.ExecuteNonQuery();
                return a == 0 ? "OK" : "fail";
            }
            catch (Exception ex)
            {
                return ex.Message + "  -->RemoveTable";
            }
        }
        /// <summary>
        /// 删除表.
        /// </summary>
        /// <returns></returns>
        public string RemoveData(string tableName, string[] fieldNames, string[] fieldValues, string[] fieldTypes)
        {
            if (fieldNames.Length != fieldValues.Length)
            {
                return "field name count discordance with value count or type count  -->RemoveData";
            }
            StringBuilder sb = new StringBuilder("delete * from " + tableName + " where ");
            for (int i = 0; i < fieldNames.Length; i++)
            {
                transStr(ref fieldValues[i], fieldTypes[i]);
                if (string.IsNullOrEmpty(fieldValues[i].ToString()))
                {
                    continue;
                }
                if (i < fieldNames.Length - 1)
                {
                    sb.Append("[" + fieldNames[i] + "] = " + fieldValues[i] + " and ");
                }
                else
                {
                    sb.Append("[" + fieldNames[i] + "] = " + fieldValues[i]);
                }
            }
            string sql = sb.ToString();
            try
            {
                myCommand = new OleDbCommand(sql, accConn);
                int a = myCommand.ExecuteNonQuery();
                return a == 1 ? "OK" : "fail";
            }
            catch (Exception ex)
            {
                return ex.Message + "  -->RemoveData";
            }
        }


        #endregion  

        /// <summary>
        /// 获取表格
        /// </summary>
        /// <returns></returns>
        public DataGridView GetDataGridView() { return dataGridView1; }

        /// <summary>
        /// 添加分类查询
        /// </summary>
        /// <returns></returns>
        public string AddClassifyQuery(string classifyName, string[] classifyItems, string fieldName, string fieldType)
        {
            this.combox_classify1.SelectedIndexChanged -= new System.EventHandler(this.combox_classify_SelectedIndexChanged);
            this.combox_classify2.SelectedIndexChanged -= new System.EventHandler(this.combox_classify_SelectedIndexChanged);
            this.combox_classify3.SelectedIndexChanged -= new System.EventHandler(this.combox_classify_SelectedIndexChanged);

            ComboBox[] combox = new ComboBox[] { combox_classify1, combox_classify2, combox_classify3 };
            Label[] label = new Label[] { label_classify1, label_classify2, label_classify3 };
            try
            {
                for (int i = 0; i < label.Length; i++)
                {
                    if (!label[i].Visible)
                    {
                        label[i].Visible = true;
                        label[i].Text = classifyName;
                        label[i].Tag = fieldName + "," + fieldType;
                        combox[i].Visible = true;
                        for (int j = 0; j < classifyItems.Length; j++)
                        {
                            combox[i].Items.Add(classifyItems[j]);
                        }
                        combox[i].Text = classifyItems[0];
                        break;
                    }
                }
                return "OK";
            }
            catch (Exception ex)
            {
                return ex.Message + "  -->AddClassifyQuery";
            }
            finally
            {
                this.combox_classify1.SelectedIndexChanged += new System.EventHandler(this.combox_classify_SelectedIndexChanged);
                this.combox_classify2.SelectedIndexChanged += new System.EventHandler(this.combox_classify_SelectedIndexChanged);
                this.combox_classify3.SelectedIndexChanged += new System.EventHandler(this.combox_classify_SelectedIndexChanged);
            }

        }

        public void IsHideRstPanel(bool isHide)
        {
            tableLayoutPanel1.Visible = !isHide;

        }


        #region  转换值
        private void transArr(string[] fieldValues, string[] fieldTypes)
        {
            for (int i = 0; i < fieldValues.Length; i++)
            {
                switch (fieldTypes[i])
                {
                    case "string":
                        fieldValues[i] = "'" + fieldValues[i] + "'";
                        break;
                    case "int":
                        fieldValues[i] = fieldValues[i];
                        break;
                    case "bool":
                        fieldValues[i] = bool.Parse(fieldValues[i]) ? -1 + "" : 0 + "";
                        break;
                    case "double":
                        fieldValues[i] = fieldValues[i];
                        break;
                    default:
                        break;
                }
            }
        }
        private void transStr(ref string value, string type)
        {
            switch (type)
            {
                case "int":
                    break;
                case "bool":
                    value = bool.Parse(value) ? "-1" : "0";
                    break;
                case "string":
                    value = "'" + value + "'";
                    break;
                case "String":
                    value = "'" + value + "'";
                    break;
                case "double":
                    break;
                default:
                    break;
            }
        }
        #endregion

        /// <summary>
        /// 获取表名
        /// </summary>
        /// <returns></returns>
        public string GetTableName()
        {
            StringBuilder sName = new StringBuilder();
            DataTable dt = accConn.GetOleDbSchemaTable(OleDbSchemaGuid.Tables, new object[] { null, null, null, "TABLE" });
            for (int i = 0, maxI = dt.Rows.Count; i < maxI; i++)
            {
                // 获取第i个Access数据库中的表名
                string sTempTableName = dt.Rows[i]["TABLE_NAME"].ToString();
                sName.Append(string.Format("{0},", sTempTableName));
            }
            return sName.ToString();
        }

        /// <summary>
        /// 取指定表所有字段名称
        /// </summary>
        /// <returns></returns>
        public List<string> GetTableFieldNameList(string TableName)
        {
            List<string> list = new List<string>();
            try
            {
                if (accConn.State == ConnectionState.Closed)
                    accConn.Open();
                using (OleDbCommand cmd = new OleDbCommand())
                {
                    cmd.CommandText = "SELECT TOP 1 * FROM [" + TableName + "]";
                    cmd.Connection = accConn;
                    OleDbDataReader dr = cmd.ExecuteReader();
                    for (int i = 0; i < dr.FieldCount; i++)
                    {
                        list.Add(dr.GetName(i));
                    }
                }
                return list;
            }
            catch (Exception e)
            {
                throw e;
            }
        }


        private void ShowData(OleDbDataAdapter da)
        {
            DataTable dt = new DataTable();
            da.Fill(dt);

            dataGridView1.DataSource = dt;
            dataGridView1.CurrentCell = dataGridView1.Rows[this.dataGridView1.Rows.Count - 1].Cells[0];

            int rstIdx = -1;
            for (int i = 0; i < dt.Columns.Count; i++)
            {
                dataGridView1.Columns[i].AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
                if (dataGridView1.Columns[i].HeaderText == "结果" || dataGridView1.Columns[i].HeaderText == "result")
                {
                    rstIdx = i;
                }
            }
            double NgCnt = 0;
            double total = -1;
            if (rstIdx != -1)
            {
                for (int i = 0; i < dataGridView1.Rows.Count; i++)
                {
                    string rst = dataGridView1.Rows[i].Cells[rstIdx].Value.ToString();
                    if (rst == "NG")
                    {
                        NgCnt++;
                    }
                    total++;
                }
                label_NgCnt.Text = NgCnt.ToString();
                label_total.Text = total.ToString();
                double yield = NgCnt / total;
                label_yield.Text = ((1 - yield) * 100).ToString("0.00") + "%";
                dataGridView1.DataBindings.Clear();
            }
            else
            {
                label_NgCnt.Text = "0";
                label_total.Text = "0";
                label_yield.Text = "0%";
            }
        }
        private void ShowTableNames(ComboBox combox)
        {
            combox_table_names.Items.Clear();
            string[] tableNames = GetTableName().Split(',');
            int LastIndex = -1;
            for (int i = tableNames.Length - 1; i >= 0; i--)
            {
                if (!string.IsNullOrEmpty(tableNames[i]))
                {
                    combox_table_names.Items.Add(tableNames[i]);
                    LastIndex = LastIndex == -1 ? i : LastIndex;
                }
            }
            if (combox_table_names.Items.Count > 0)
            {
                combox_table_names.Text = tableNames[LastIndex];
            }
        }

        private void combox_table_names_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (!string.IsNullOrEmpty(combox_table_names.Text))
            {
                string str = Query(combox_table_names.Text);
                if (str != "OK")
                {
                    MessageBox.Show(str);
                }
            }
        }


        private void combox_classify_SelectedIndexChanged(object sender, EventArgs e)
        {

            ComboBox combox = (ComboBox)sender;
            string str = "";
            switch (combox.Name)
            {
                case "combox_classify1":
                    string[] fieldParam1 = label_classify1.Tag.ToString().Split(',');
                    str = Query(combox_table_names.Text, fieldParam1[0], combox_classify1.Text, fieldParam1[1]);
                    break;
                case "combox_classify2":
                    string[] fieldParam2 = label_classify1.Tag.ToString().Split(',');
                    str = Query(combox_table_names.Text, fieldParam2[0], combox_classify2.Text, fieldParam2[1]);
                    break;
                case "combox_classify3":
                    string[] fieldParam3 = label_classify1.Tag.ToString().Split(',');
                    str = Query(combox_table_names.Text, fieldParam3[0], combox_classify3.Text, fieldParam3[1]);
                    break;
                default:
                    break;
            }
            if (str != "OK")
            {
                MessageBox.Show(str);
            }
        }

        private void dateTimePicker_ValueChanged(object sender, EventArgs e)
        {
            //MessageBox.Show(dateTimePicker_min.Value.ToString());
        }
        private void btn_QueryTimeRange_Click(object sender, EventArgs e)
        {
            string name = "";
            try
            {
                name = GetTableName();
            }
            catch (Exception)
            {
                //return "Get data base table fail  -->btn_QueryTimeRange_Click";
            }
            string[] nameArr = name.Split(',');//获取表格名
            string sql = "";
            for (int i = 0; i < nameArr.Length; i++)
            {
                if (IsDate(nameArr[i]))
                {
                    DateTime date_min = dateTimePicker_min.Value;//获取输入时间中的日期
                    DateTime date_max = dateTimePicker_max.Value;
                    sql += "select * from [" + nameArr[i] + "] where 时间 between #" + date_min + "# and #" + date_max + "# union ";
                }
            }
            if (sql.Length > 5)
            {
                string a = sql.TrimEnd().Substring(sql.TrimEnd().Length - 5, 5);
                if (a == "union")
                {
                    sql = sql.TrimEnd().Substring(0, sql.TrimEnd().Length - 5);
                }
            }
            else { MessageBox.Show("此时间段没有值"); return; }
            da = new OleDbDataAdapter(sql, accConn);

            try
            {
                ShowData(da);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }
        private void btn_QueryTimeRange_Click1(object sender, EventArgs e)
        {
            string name = "";
            try
            {
                name = GetTableName();
            }
            catch (Exception)
            {
                //return "Get data base table fail  -->btn_QueryTimeRange_Click";
            }
            DateTime dt_min = dateTimePicker_min.Value;//获取输入最小时间

            string[] nameArr = name.Split(',');//获取表格名
            string sql = "";
            for (int i = 0; i < nameArr.Length; i++)
            {
                if (IsDate(nameArr[i]))
                {
                    DateTime dt_table_name = DateTime.Parse(nameArr[i]);// 2019-7-12 00:00:00
                    DateTime date_min = dateTimePicker_min.Value.Date;//获取输入时间中的日期
                    DateTime date_max = dateTimePicker_max.Value.Date;
                    if (dt_table_name > dateTimePicker_min.Value && dt_table_name < dateTimePicker_max.Value && dt_table_name != date_max)
                    {
                        sql += "select * from [" + nameArr[i] + "] union ";
                    }

                    string time_min = dateTimePicker_min.Value.TimeOfDay.ToString().Substring(0, 8);//获取输入值的时间
                    string time_max = dateTimePicker_max.Value.TimeOfDay.ToString().Substring(0, 8);
                    List<string> list = GetTableFieldNameList(nameArr[i]);
                    if (dt_table_name == date_max && dt_table_name == date_min)
                    {
                        if (list.Contains("时间"))
                        {
                            sql += "select * from [" + nameArr[i] + "] where 时间 between #" + time_min + "# and #" + time_max + "# union ";
                        }
                        else if (list.Contains("time"))
                        {
                            sql += "select * from [" + nameArr[i] + "] where time between  #" + time_min + "# and #" + time_max + "# union ";
                        }
                    }
                    else
                    {
                        if (dt_table_name == date_min)
                        {
                            if (list.Contains("时间"))
                            {
                                sql += "select * from [" + nameArr[i] + "] where 时间 > #" + time_min + "#  union ";
                            }
                            else if (list.Contains("time"))
                            {
                                sql += "select * from [" + nameArr[i] + "] where time >  #" + time_min + "# union ";
                            }
                        }
                        if (dt_table_name == date_max)
                        {
                            if (list.Contains("时间"))
                            {
                                sql += "select * from [" + nameArr[i] + "] where 时间 < #" + time_max + "#  union ";
                            }
                            else if (list.Contains("time"))
                            {
                                sql += "select * from [" + nameArr[i] + "] where time < #" + time_max + "#  union ";
                            }
                        }
                    }

                }
            }
            if (sql.Length > 5)
            {
                string a = sql.TrimEnd().Substring(sql.TrimEnd().Length - 5, 5);
                if (a == "union")
                {
                    sql = sql.TrimEnd().Substring(0, sql.TrimEnd().Length - 5);
                }
            }
            else { MessageBox.Show("此时间段没有值"); return; }
            da = new OleDbDataAdapter(sql, accConn);

            try
            {
                ShowData(da);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }


        }


        private void 删除ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            string tableName = combox_table_names.Text;
            string state = RemoveTable(tableName);
            if (state == "OK")
            {
                MessageBox.Show("删除表《" + tableName + "》成功");
                ShowTableNames(combox_table_names);
            }
            else
            {
                MessageBox.Show("删除失败！" + state);
            }
        }

        private void 删除此行ToolStripMenuItem_Click(object sender, EventArgs e)
        {

            DataGridViewSelectedCellCollection CellList = dataGridView1.SelectedCells;
            string tableName = combox_table_names.Text;
            StringBuilder rst = new StringBuilder();
            for (int j = 0; j < CellList.Count; j++)
            {
                int RowIndex = CellList[j].RowIndex;
                int fieldCnt = dataGridView1.Rows[RowIndex].Cells.Count;
                string[] fieldNames = new string[fieldCnt];
                string[] fieldValues = new string[fieldCnt];
                string[] fieldTypes = new string[fieldCnt];
                for (int i = 0; i < fieldCnt; i++)
                {
                    fieldNames[i] = dataGridView1.Columns[i].HeaderText;
                    fieldValues[i] = dataGridView1.Rows[RowIndex].Cells[i].Value.ToString();
                    Type a = dataGridView1.Rows[RowIndex].Cells[i].Value.GetType();
                    fieldTypes[i] = a.Name;
                }
                string state = RemoveData("[" + tableName + "]", fieldNames, fieldValues, fieldTypes);
                if (state != "OK")
                {
                    rst.Append((RowIndex + 1) + ",");
                }
            }
            if (string.IsNullOrEmpty(rst.ToString()))
            {
                MessageBox.Show("删除成功！");
            }
            else
            {
                MessageBox.Show("第 " + rst.ToString().Substring(0, rst.ToString().Length - 1) + " 行删除失败！");
            }
            Query(combox_table_names.Text);

        }

        /// <summary>
        /// 是否为日期型字符串
        /// </summary>
        /// <param name="StrSource">日期字符串(2008-05-08)</param>
        /// <returns></returns>
        public static bool IsDate(string StrSource)
        {
            return Regex.IsMatch(StrSource, @"^((((1[6-9]|[2-9]\d)\d{2})-(0?[13578]|1[02])-(0?[1-9]|[12]\d|3[01]))|(((1[6-9]|[2-9]\d)\d{2})-(0?[13456789]|1[012])-(0?[1-9]|[12]\d|30))|(((1[6-9]|[2-9]\d)\d{2})-0?2-(0?[1-9]|1\d|2[0-9]))|(((1[6-9]|[2-9]\d)(0[48]|[2468][048]|[13579][26])|((16|[2468][048]|[3579][26])00))-0?2-29-))$");
        }
        /// <summary>
        /// 禁止界面上的删除操作
        /// </summary>
        /// <param name="flag"></param>
        public void ForbiddenUIdel(bool flag)
        {
            contextMenuStrip1.Items[0].Visible = !flag;
            contextMenuStrip2.Items[0].Visible = !flag;
        }

        private void output_csv_Click(object sender, EventArgs e)
        {
            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < dataGridView1.Rows.Count; i++)
            {
                for (int j = 0; j < dataGridView1.Rows[i].Cells.Count; j++)
                {
                    sb.Append(dataGridView1.Rows[i].Cells[j].Value + ",");
                }
                sb.Append("\r\n");
            }
            if (!Directory.Exists("DataRecord"))
            {
                Directory.CreateDirectory("DataRecord");
            }
            byte[] s = Encoding.GetEncoding("GB2312").GetBytes(sb.ToString());
            File.WriteAllBytes(@"DataRecord\" + combox_table_names.Text + ".csv", s);
            //File.WriteAllText(@"DataRecord\"+combox_table_names.Text+".csv", sb.ToString());
            MessageBox.Show("导出成功，路径为：" + @"DataRecord\" + combox_table_names.Text + ".csv");
        }

        private void dataGridView1_DoubleClick(object sender, EventArgs e)
        {
            dgv_doubleClick?.Invoke(sender, combox_table_names.SelectedItem.ToString());
        }

        public static event Action<object, string> dgv_doubleClick;
    }
     
}
