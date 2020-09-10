using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.OleDb;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace DBOperator
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {


            dbShow1.ConnectMdb(@"s.mdb");//连接数据
            string ab = dbShow1.GetTableName();
            dbShow1.CreateTable("s.mdb", "table111", new string[] { "mint", "mdouble", "mbool", "mstring" }, new string[] { "int", "double", "bool", "string" });
            dbShow1.InsertData("table111", new string[] { "mint", "mdouble", "mbool", "mstring" }, new string[] { "1", "1", "true", "2020/1/3 14:34:06" },new string[] {"int","double","bool","string" });
            //dbShow1.Query("table2");
            string a = dbShow1.AddClassifyQuery("工位", new string[] { "0", "2", "345", "4" }, "mint", "int");
            string b = dbShow1.AddClassifyQuery("工位", new string[] { "0", "2", "345", "4" }, "mint", "int");
            dbShow1.IsHideRstPanel(false);
            dbShow1.CreateTable(@"s.mdb", "2019-07-23", new string[] { "nint", "nstring" }, new string[] { "int", "string" });
            dbShow1.ForbiddenUIdel(false);
            //dbShow1.CreateTable(@"s.mdb", "2019-07-24", new string[] { "nint", "nstring" }, new string[] { "int", "string" });
            //for (int i = 0; i < 20; i++)
            //{
            //    dbShow1.InsertData("table1", new string[] { "mint", "mstring", "mbool", "mdouble" }, new string[] { i.ToString(), "abcd", "true", "66.66" }, new string[] { "int", "string", "bool", "double" });

            //}
            //dbShow1.UpdateData("table1", new string[] { "mint", "mstring", "mbool", "mdouble" }, new string[] { "345", "abcd", "true", "66.66" }, new string[] { "int", "string", "bool", "double" },"mstring","abcd","string");
            //dbShow1.UpdateData("table1", new string[] { "mint" }, new string[] { "345"}, new string[] { "int" }, "mstring", "abcd", "string");
            //dbShow1.Query()
            //dbShow1.Query("2019-07-25", "time", "12:00:00","String");
            //dbShow1.CreateTable(@"s.mdb","table2",new string[] { "a","b","c"},new string[] { "bool", "bool", "bool" });

            //新用法：TreeView
            //this.hsComboBox1.TreeView.Nodes.Add("123123");
            //TreeView treeView = new TreeView();
            //this.hsComboBox1.SetDropDown(treeView);


        }



        //软件操作说明
        private void Describe()
        {

            /*
             代码操作部分
             */
            dbShow1.ConnectMdb(@"s.mdb");//连接数据，地址不存在则创建
            //创建表格
            dbShow1.CreateTable(@"s.mdb", "table11", new string[] { "ID", "Name" }, new string[] { "int", "string" });
            //删除表格
            dbShow1.RemoveTable("table11");
            
            //数据库 增、删除、改、查
            dbShow1.InsertData("table11", new string[] { "ID", "Name" }, new string[] { "1", "kitty" }, new string[] { "int", "string" });
            dbShow1.RemoveData("table2", new string[] { "ID", "Name" }, new string[] { "1", "kitty" }, new string[] { "int", "string" });
            dbShow1.UpdateData("table11", new string[] { "ID", "Name" }, new string[] { "1", "kitty" }, new string[] { "int", "string" }, "ID", "1", "int");
            dbShow1.Query("table11");

            //添加分类查询
            dbShow1.AddClassifyQuery("工位", new string[] { "1", "2", "3", "4" }, "ID", "int");

            //隐藏结果显示
            dbShow1.IsHideRstPanel(true);

            //禁止在UI界面上删除数据及表格
            dbShow1.ForbiddenUIdel(true);
            
            
       
        }

       
        
    }








}
