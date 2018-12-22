using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using Dapper;

namespace DatabaseCodeScriptTool
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void TestLinkBtn_Click(object sender, RoutedEventArgs e)
        {
            string connStr = this.sqlLinkStr.Text;
            var connection = GetConnection(connStr);
            List<DatabaseModel> database = new List<DatabaseModel>();
            string sqlStr = "SELECT name,dbid,filename FROM  master..sysdatabases";   //查询数据库名

            #region 验证数据库连接
            try
            {
                connection.Open();
                if (connection.State == System.Data.ConnectionState.Open)
                {
                    MessageBox.Show("连接成功！");
                    database = connection.Query<DatabaseModel>(sqlStr).AsList();
                }
                else
                {
                    MessageBox.Show("连接失败！");
                    return;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("连接失败!\n\r" + ex.Message + ex.StackTrace);
                return;
            }
            finally
            {
                connection.Close();
            }
            #endregion
            if (database.Count > 0)
            {
                List<ComboBoxItem> items = new List<ComboBoxItem>();
                foreach (var a in database)
                {
                    ComboBoxItem item = new ComboBoxItem();
                    item.Content = a.name;
                    item.HorizontalAlignment = HorizontalAlignment.Left;
                    item.Width = 169;
                    items.Add(item);
                }
                this.searchDatabase.ItemsSource = items;
                this.sqlLinkStr.IsEnabled = false;
            }
        }

        private void SearchTabelBtn_Click(object sender, RoutedEventArgs e)
        {
            string databaseName = this.searchDatabase.Text;
            List<string> tableNames = new List<string>();
            if (string.IsNullOrWhiteSpace(databaseName))
            {
                MessageBox.Show("请先选择数据库！", "错误", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            else {
                string connStr = this.sqlLinkStr.Text;
                connStr += $"database={databaseName};";
                string sqlStr = "select name from sys.tables";
                using (var connection = GetConnection(connStr))
                {
                    connection.Open();
                    tableNames = connection.Query<string>(sqlStr).AsList();
                }
                tableNames.Reverse();
                this.tablesLog.Text = "";
                foreach (var a in tableNames)
                {
                    this.tablesLog.Text += a+"\n\r";
                }
                this.tablesLog.Text += $"当前查询数据库[{this.searchDatabase.Text}]共有{tableNames.Count}个表";
                this.tablesLog.Focus();
            }
        }

        private void BeginBtn_Click(object sender, RoutedEventArgs e)
        {
            if (!Inspect())
            {
                MessageBox.Show("请将数据填写完整！");
                return;
            }

            string connStr = this.sqlLinkStr.Text;
            string databaseName = this.searchDatabase.Text;
            connStr += $"database={databaseName};";
            string sqlStr = "select name from sys.tables";  //查询表名
            List<string> tableNames = new List<string>();   //表名列表
            List<string> dalCode = new List<string>();  //存放DAL的代码
            List<string> modelCode = new List<string>();   //存放Model的代码

            string fileType = "";
            if ((bool)this.fileType1.IsChecked)
            {
                fileType = "cs";
            }
            if ((bool)this.fileType2.IsChecked)
            {
                fileType = "txt";
            }

            using (var connection = GetConnection(connStr))
            {
                connection.Open();
                tableNames = connection.Query<string>(sqlStr).AsList();
            }
            if (tableNames.Count > 0)
            {
                tableNames.Reverse();
                CreateDirectory();
                bool bolDal = true;
                bool bolModel = true;
                if ((bool)this.saveManner2.IsChecked)
                {
                    foreach (var a in tableNames)
                    {
                        string tbName = "";
                        if ((bool)this.notTableHead.IsChecked)
                        {
                            tbName = a;
                        }
                        else
                        {
                            tbName = a.Substring(this.tableHead.Text.Length);
                        }
                        if ((bool)this.classObject1.IsChecked)
                        {
                            bolDal = PrintTXTFile($"DAL/DAL.{fileType}", CreateDalCode(a, connStr), true);
                            dalCode.Add(CreateDalCode(a, connStr));
                        }
                        if ((bool)this.classObject2.IsChecked)
                        {
                            bolModel = PrintTXTFile($"Model/{tbName}.{fileType}", CreateModelCode(a, connStr), false);
                            modelCode.Add(CreateModelCode(a, connStr));
                        }
                    }
                }
                else {
                    foreach (var a in tableNames)
                    {
                        string tbName = "";
                        if ((bool)this.notTableHead.IsChecked)
                        {
                            tbName = a;
                        }
                        else
                        {
                            tbName = a.Substring(this.tableHead.Text.Length);
                        }
                        if ((bool)this.classObject1.IsChecked)
                        {
                            bolDal = PrintTXTFile($"DAL/{tbName}DAL.{fileType}", CreateDalCode(a, connStr), false);
                            dalCode.Add(CreateDalCode(a, connStr));
                        }
                        if ((bool)this.classObject2.IsChecked)
                        {
                            bolModel = PrintTXTFile($"Model/{tbName}.{fileType}", CreateModelCode(a, connStr), false);
                            modelCode.Add(CreateModelCode(a, connStr));
                        }
                    }
                }
                if (!bolDal)
                {
                    MessageBox.Show("输出dal文件失败", "失败", MessageBoxButton.OK, MessageBoxImage.Error);
                }
                if (!bolModel)
                {
                    MessageBox.Show("输出Model文件失败", "失败", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            MessageBox.Show("输出成功！", "完成", MessageBoxButton.OK, MessageBoxImage.Information);

        }

        private void ReloadBtn_Click(object sender, RoutedEventArgs e)
        {
            this.sqlLinkStr.IsEnabled = true;
            this.sqlLinkStr.Text = "";
            this.searchDatabase.Text = "";
            this.searchDatabase.Items.Remove(this.searchDatabase.Items);
            this.fileType1.IsChecked = false;
            this.fileType2.IsChecked = false;
            this.saveManner1.IsChecked = false;
            this.saveManner2.IsChecked = false;
            this.classObject1.IsChecked = false;
            this.classObject2.IsChecked = false;
            this.tablesLog.Text = "";
        }

        private void ExitBtn_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void NotTableHead_Click(object sender, RoutedEventArgs e)
        {
            if ((bool)this.notTableHead.IsChecked)
            {
                this.tableHead.IsEnabled = false;
            }
            else
            {
                this.tableHead.IsEnabled = true;
            }
        }

        /// <summary>
        /// 取得数据库连接对象
        /// </summary>
        /// <param name="connStr">连接字符串</param>
        /// <returns></returns>
        private System.Data.SqlClient.SqlConnection GetConnection(string connStr) {
            System.Data.SqlClient.SqlConnection connection = new System.Data.SqlClient.SqlConnection(connStr);
            return connection;
        }

        /// <summary>
        /// 检测数据是否完整
        /// </summary>
        /// <returns></returns>
        private bool Inspect() {
            if (string.IsNullOrWhiteSpace(this.sqlLinkStr.Text)) { return false; }
            if (string.IsNullOrWhiteSpace(this.searchDatabase.Text)) { return false; }
            if ((!(bool)this.fileType1.IsChecked) && (!(bool)this.fileType2.IsChecked)){return false;}
            if ((!(bool)this.saveManner1.IsChecked) && (!(bool)this.saveManner2.IsChecked)) { return false; }
            if ((!(bool)this.classObject1.IsChecked) && (!(bool)this.classObject2.IsChecked)) { return false; }
            return true;
        }

        /// <summary>
        /// 创建一个DAL的代码
        /// </summary>
        /// <param name="tableName"></param>
        /// <param name="connStr"></param>
        /// <returns></returns>
        private string CreateDalCode(string tableName,string connStr) {
            string sqlstr = $" select name from syscolumns where id in (select object_id from sys.tables where name='{tableName}') ";  //查询列名
            List<string> columnNames = new List<string>();
            string code = null;
            string varTableName = ""; //{0}表名
            string varCreateTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");  //{1}创建时间
            string varColumnStr = "";  //{2}插入用到的列名
            string varColumnParam = "";  //{3}插入的参数
            string varSetStr = "";  //{4}更新中的set内容
            string varTableHead = "";  //{5}表前缀

            #region  变量处理
            using (var connection = GetConnection(connStr))
            {
                connection.Open();
                columnNames = connection.Query<string>(sqlstr).AsList();
            }
            if ((bool)this.notTableHead.IsChecked)
            {
                varTableName = tableName;
                varTableHead = "";
            }
            else {
                varTableHead = this.tableHead.Text;
                varTableName = tableName.Substring(varTableHead.Length);
            }
            foreach (var a in columnNames)
            {
                if (a == "id")
                { continue; }
                varColumnStr += a + ",";
                varColumnParam += $"@{a},";
                varSetStr += $"{a}=@{a},";
            }
            varColumnStr = varColumnStr.Substring(0, varColumnStr.Length - 1);
            varColumnParam = varColumnParam.Substring(0, varColumnParam.Length - 1);
            varSetStr = varSetStr.Substring(0, varSetStr.Length - 1);
            #endregion

            string modelStr = GetTxt("../../CodeDalModel.txt");  //取得模版里的内容
            code = string.Format(modelStr.ToString(), varTableName, varCreateTime, varColumnStr, varColumnParam, varSetStr, varTableHead,"{","}");
            return code;

        }

        /// <summary>
        /// 创建一个Model的代码
        /// </summary>
        /// <param name="tableName"></param>
        /// <param name="connStr"></param>
        /// <returns></returns>
        private string CreateModelCode(string tableName, string connStr) {
            string code = null;
            string varTableName = ""; //{0}表名
            string varTableHead = "";  //{5}表前缀
            string codeModel1 = GetTxt("../../CodeModel1.txt");
            string codeModel2 = GetTxt("../../CodeModel2.txt");
            string codeModel2Str = "";
            string sqlstr = $" select name,usertype from syscolumns where id in (select object_id from sys.tables where name='{tableName}') ";  //查询列名
            List<ColumnModel> columnNames = new List<ColumnModel>();
            using (var connection = GetConnection(connStr))
            {
                connection.Open();
                columnNames = connection.Query<ColumnModel>(sqlstr).AsList();
            }
            if ((bool)this.notTableHead.IsChecked)
            {
                varTableName = tableName;
                varTableHead = "";
            }
            else
            {
                varTableHead = this.tableHead.Text;
                varTableName = tableName.Substring(varTableHead.Length);
            }
            foreach (var a in columnNames)
            {
                string type = GetType(a.Usertype);
                codeModel2Str += string.Format(codeModel2, a.Name, type,"{","}");
            }
            code = string.Format(codeModel1, varTableName, codeModel2Str, DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),"{","}");
            return code;
        }

        /// <summary>
        /// 读取指定的文件（UTF-8）
        /// </summary>
        /// <param name="path">文件路径</param>
        /// <returns></returns>
        private static string GetTxt(string path)
        {
            StreamReader sr = new StreamReader(path, Encoding.UTF8);
            string str = sr.ReadToEnd();
            sr.Close();
            sr.Dispose();
            return str;
        }

        /// <summary>
        /// 数据库类型转换成C#类型
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        private string GetType(int value) {
            string type = "";
            switch (value)
            {
                case 2: type = "string"; break;
                case 6: type = "int"; break;
                case 7:type = "int"; break;
                case 12: type = "DateTime"; break;
                default:type = "string";break;
            }
            return type;
        }
        /// <summary>
        /// 文件输出至指定路径
        /// </summary>
        /// <param name="path">文件物理路径，例：C:\test.txt</param>
        /// <param name="content">要输出的内容，以utf-8的格式</param>
        /// <returns>输出结果</returns>
        public static bool PrintTXTFile(string path, string content,bool append)
        {
            try
            {
                StreamWriter sw = new StreamWriter(path, append, Encoding.UTF8);
                sw.WriteLine(content);
                sw.Close();
                sw.Dispose();
                return true;
            }
            catch (Exception ex)
            {
                return false;
            }
        }

        /// <summary>
        /// 创建路径
        /// </summary>
        /// <returns></returns>
        private bool CreateDirectory() {
            try
            {
                if (!System.IO.Directory.Exists("Model"))
                {
                    Directory.CreateDirectory("Model");
                }
                if (!System.IO.Directory.Exists("DAL"))
                {
                    Directory.CreateDirectory("DAL");
                }
            }
            catch { return false; }
            return true;
        }
    }
    public class DatabaseModel {
        private string _name;
        private string _dbid;
        private string _filename;

        public string name { get => _name; set => _name = value; }
        public string dbid { get => _dbid; set => _dbid = value; }
        public string filename { get => _filename; set => _filename = value; }
    }

    public class ColumnModel {
        public ColumnModel() { }
        private string name;
        private int usertype;

        public string Name { get => name; set => name = value; }
        public int Usertype { get => usertype; set => usertype = value; }
    }
}
