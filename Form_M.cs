using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Text;
using System.Linq;
using System.Security;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml.Linq;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;

namespace WindowsFormsApp1
{
    public partial class Form_M : Form
    {
        public Form_M()
        {
            InitializeComponent();
        }

        private void add_btn_Click(object sender, EventArgs e) // 新增商品
        {
            if (comboBox1.Text == "  ----選取----" || textBox2.Text == "" || textBox3.Text == "")
                MessageBox.Show("商品資訊不完整，請再次檢查");
            else // 每個商品資訊都有輸入
            {
                if (Regex.IsMatch(textBox2.Text, @"^[^0-9]+$") && Regex.IsMatch(textBox3.Text, @"^[0-9]+$")) // 正規表達式
                {
                    SqlConnection conn = new SqlConnection("Data Source=localhost;Initial Catalog=TestDB;Integrated Security=True;Connect Timeout=30;Encrypt=False;");
                    conn.Open();
                    SqlCommand cmd = new SqlCommand($"insert into {comboBox1.Text} values (@類別, @商品, @價格, @狀態)", conn);
                    SqlCommand cmd2 = new SqlCommand($"SELECT * FROM {comboBox1.Text}", conn);
                    SqlDataReader DataReader = cmd2.ExecuteReader();
                    int num = 0;
                    while (DataReader.Read())
                    {
                        if (textBox2.Text == DataReader[2].ToString())
                            num++;
                    }
                    DataReader.Close();
                    if (num > 0)
                        MessageBox.Show($"新增商品失敗，類別\"{comboBox1.Text}\"中已存在此商品");
                    else
                    {
                        cmd.Parameters.AddWithValue("@類別", comboBox1.Text);
                        cmd.Parameters.AddWithValue("@商品", textBox2.Text);
                        cmd.Parameters.AddWithValue("@價格", textBox3.Text);
                        cmd.Parameters.AddWithValue("@狀態", "加入清單");
                        cmd.ExecuteNonQuery();
                        conn.Close();
                        MessageBox.Show("商品新增成功");
                        productList_Show();
                    }
                }
                else
                    MessageBox.Show("商品新增失敗，請檢查輸入型態");
            }
            if(dataGridView2.SelectedCells.Count > 0)
                dataGridView2.SelectedCells[0].Selected = false;
        }

        private int rows_cnt = 0; // 紀錄類別筆數
        private void Form1_Load(object sender, EventArgs e)
        {
            deleteProduct.Enabled = false;
            dataGridView1.Visible = false;
            SqlConnection conn = new SqlConnection("Data Source=localhost;Initial Catalog=TestDB;Integrated Security=True;Connect Timeout=30;Encrypt=False;");
            conn.Open();
            SqlCommand cmd = new SqlCommand("SELECT 類別 FROM 類別清單", conn);
            SqlDataReader DataReader = cmd.ExecuteReader();
            comboBox1.Items.Add("  ----選取----");
            while (DataReader.Read()) // 查詢資料表中有幾筆商品類別資料
            {
                comboBox1.Items.Add(DataReader[0].ToString());
                rows_cnt++;
            }
            DataReader.Close();
            conn.Close();
            comboBox1.SelectedIndex = 0;
            classList_Show();
            dataGridView2.Columns[0].SortMode = DataGridViewColumnSortMode.NotSortable;
            dataGridView2.ReadOnly = true;
            label4.Text = $"目前已有{rows_cnt}個類別";
            if (rows_cnt > 0)
                dataGridView2.Rows[0].Selected = false;
        }


        private void productList_Show()
        {
            dataGridView1.Columns.Clear();
            dataGridView1.AllowUserToAddRows = false;
            dataGridView1.AllowUserToDeleteRows = false;
            dataGridView1.AllowUserToResizeColumns = false;
            try
            {
                dataGridView1.Visible = true;
                SqlConnection conn = new SqlConnection("Data Source=localhost;Initial Catalog=TestDB;Integrated Security=True;Connect Timeout=30;Encrypt=False;");
                conn.Open();
                SqlCommand cmd = new SqlCommand($"Select * from {comboBox1.Text}", conn);
                SqlDataAdapter da = new SqlDataAdapter(cmd);
                DataTable dt = new DataTable();
                da.Fill(dt); // sqlDataAdapter 填入 DataSet
                dataGridView1.DataSource = dt;

                DataGridViewTextBoxColumn newCol = new DataGridViewTextBoxColumn();
                DataGridViewLinkColumn linCol = new DataGridViewLinkColumn();
                DataGridViewButtonColumn btnCol = new DataGridViewButtonColumn();
                DataGridViewComboBoxColumn comCol = new DataGridViewComboBoxColumn();
                linCol.HeaderText = "設定";
                linCol.Text = "進階設定";
                linCol.LinkColor = Color.Black;
                linCol.UseColumnTextForLinkValue = true;
                linCol.TrackVisitedState = false;
                newCol.HeaderText = "商品需求";
                btnCol.HeaderText = "加入列表";
                btnCol.Text = "加入列表";
                comCol.HeaderText = "數量";
                btnCol.UseColumnTextForButtonValue = true; // 按鈕上的文字使用該欄位的文字
                dataGridView1.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
                
                for (int i = 1; i <= 10; i++)
                    comCol.Items.Add(i.ToString());

                dataGridView1.Columns.Insert(4, comCol);
                dataGridView1.Columns.Insert(5, newCol);
                dataGridView1.Columns.Insert(6, btnCol); 
                dataGridView1.Columns.Insert(0, linCol);


                dataGridView1.Columns[0].ReadOnly = true;
                dataGridView1.Columns[1].ReadOnly = true;
                dataGridView1.Columns[2].ReadOnly = true;
                dataGridView1.Columns[3].ReadOnly = true;
                dataGridView1.Columns[5].Visible = false; // 加入數量欄位隱藏
                dataGridView1.Columns[7].Visible = false; // 加入列表欄位隱藏

                SqlCommand cmd2 = new SqlCommand("Select * from 需求", conn);
                for (int i = 0; i < dataGridView1.Rows.Count; i++)
                {
                    DataGridViewComboBoxCell comCell = new DataGridViewComboBoxCell();
                    SqlDataReader DataReader = cmd2.ExecuteReader();
                    int num = 0;

                    while (DataReader.Read())//讀取需求table
                    {
                        /*  用該列的商品名稱來判斷, 如果該商品的名稱有在table資料中就表示該商品有需求項
                            查看資料庫中此商品有沒有需求項, 有的話全部抓出來加入到下拉選單中, 並用 num 此商品在資料庫中紀錄了幾筆需求項  */
                        if (dataGridView1.Rows[i].Cells["商品"].Value.ToString() == DataReader[0].ToString())
                        {
                            comCell.Items.Add($"{DataReader[1]} + {DataReader[2]}元");
                            num++;
                        }
                    }
                    DataReader.Close();//一定要關起來，卡在讀取空資料

                    if (num > 0) // 此商品有需求項
                    {
                        comCell.Items.Add("N/A");
                        dataGridView1.Rows[i].Cells[6] = comCell;
                    }
                    else // 此商品無任何需求項
                    {
                        dataGridView1.Rows[i].Cells[6].Value = "N/A";
                        dataGridView1.Rows[i].Cells[6].ReadOnly = true;
                    }
                }
                conn.Close();
                //HeaderText標題置中
                for (int i = 0; i < dataGridView1.Columns.Count; i++)
                    dataGridView1.Columns[i].SortMode = DataGridViewColumnSortMode.NotSortable;
                dataGridView1.Columns[3].Width = 100;
                dataGridView1.Columns[1].Width = 50;
                dataGridView1.Columns[6].Width = 90;
                dataGridView1.Columns[8].Width = 90;
            }
            catch (Exception) { }
        }
        private void classList_Show()
        {
            SqlConnection conn = new SqlConnection("Data Source=localhost;Initial Catalog=TestDB;Integrated Security=True;Connect Timeout=30;Encrypt=False;");
            conn.Open();
            SqlCommand cmd = new SqlCommand($"Select * from 類別清單", conn);
            SqlDataAdapter da = new SqlDataAdapter(cmd);
            DataTable dt = new DataTable();
            da.Fill(dt); // sqlDataAdapter 填入 DataSet
            dataGridView2.DataSource = dt;
            if(dataGridView2.Rows.Count > 0)
                dataGridView2.CurrentCell.Selected = false;
        }

        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (comboBox1.Text != "  ----選取----")
                productList_Show();
            else
                dataGridView1.Visible = false;
            textBox2.Text = "";
            textBox3.Text = "";
            deleteProduct.BackColor = Color.White;
            deleteProduct.Enabled = false;
            if (dataGridView2.SelectedCells.Count > 0)
                dataGridView2.SelectedCells[0].Selected = false;
        }

        // 在點擊進階設定時該列的商品資訊(類別、名稱、價格)，再傳到Form_S中
        public static string product_class;
        public static string product_name;
        public static string product_price;

        private void dataGridView1_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
            //如果該 Cell 的 Column 類型為 DataGridViewLinkColumn
            if(dataGridView1.Columns[e.ColumnIndex] is DataGridViewLinkColumn && e.RowIndex != -1 ) // 按下進階設定
            {
                product_class = dataGridView1.Rows[e.RowIndex].Cells[2].Value.ToString();
                product_name = dataGridView1.Rows[e.RowIndex].Cells[3].Value.ToString();
                product_price = dataGridView1.Rows[e.RowIndex].Cells[4].Value.ToString();
                Form_S p1 = new Form_S();
                p1.ShowDialog(this);
                dataGridView1.Visible = false;
            }
        }

        private void dataGridView1_CellClick(object sender, DataGridViewCellEventArgs e) // 點選Cell時馬上下拉選單
        {
            if(e.RowIndex >= 0 && e.ColumnIndex >= 0)
            {
                DataGridViewComboBoxCell comCell = dataGridView1.CurrentCell as DataGridViewComboBoxCell;
                if (comCell != null)
                {
                    dataGridView1.BeginEdit(false);
                    DataGridViewComboBoxEditingControl comboEdit = dataGridView1.EditingControl as DataGridViewComboBoxEditingControl;
                    comboEdit.DropDownWidth = dataGridView1.Columns[e.ColumnIndex].Width;
                    if (comboEdit != null)
                        comboEdit.DroppedDown = true;//選單此時下拉
                }
                //DataGridViewTextBoxColumn textbox = dataGridView1.Columns[e.ColumnIndex] as DataGridViewTextBoxColumn;
                //if (textbox != null) //如果該列是TextBox列
                //{
                //    dataGridView1.BeginEdit(false); //編輯狀態為True
                //}
            }
        }

        private void save_btn_Click(object sender, EventArgs e) // 儲存類別
        {
            SqlConnection conn = new SqlConnection("Data Source=localhost;Initial Catalog=TestDB;Integrated Security=True;Connect Timeout=30;Encrypt=False;");
            conn.Open();
            if (textBox1.Text != "" && rows_cnt + 1 < 19)
            {
                if(Regex.IsMatch(textBox1.Text, @"^[^0-9]+$"))
                {
                    SqlCommand cmd3 = new SqlCommand("SELECT * FROM 類別清單", conn);
                    SqlDataReader DataReader = cmd3.ExecuteReader();
                    int num = 0;
                    while(DataReader.Read())
                    {
                        if (textBox1.Text == DataReader[1].ToString())
                            num++;
                    }
                    DataReader.Close();
                    if (num > 0)
                        MessageBox.Show("類別輸入重複");
                    else
                    {
                        SqlCommand cmd = new SqlCommand("insert into 類別清單 values (@類別)", conn);
                        cmd.Parameters.AddWithValue("@類別", textBox1.Text);
                        cmd.ExecuteNonQuery();
                        SqlCommand cmd2 = new SqlCommand($"CREATE TABLE {textBox1.Text}" +
                                                            "(編號 INT IDENTITY(1, 1) NOT NULL , " +
                                                            "類別 VARCHAR(50) NOT NULL , " +
                                                            "商品 VARCHAR(50) NOT NULL , " +
                                                            "價格 INT NOT NULL , " +
                                                            "狀態 VARCHAR(50) NOT NULL, "+
                                                            "PRIMARY KEY CLUSTERED ([編號] ASC))", conn);
                        cmd2.ExecuteNonQuery();// 若輸入類別名稱不符合正規，拋出例外，則此時執行 catch {}
                        conn.Close();
                        MessageBox.Show("類別已儲存!", "成功", MessageBoxButtons.OK);
                        classList_Show();
                        comboBox1.Items.Add(textBox1.Text);
                        textBox1.Text = "";
                        rows_cnt++;
                        label4.Text = $"目前已有{rows_cnt}個類別";
                    }
                }
                else
                    MessageBox.Show("請輸入有效類別名稱");
            }
            else if (textBox1.Text == "")
                MessageBox.Show("類別名稱不可為空", "輸入錯誤");
            else
                MessageBox.Show("儲存類別數量已超出額定(18)");
        }

        private void dataGridView2_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            deleteProduct.Enabled = true;
            deleteProduct.BackColor = Color.LightCoral;
            deleteProduct.ForeColor = Color.Black;
            dataGridView1.Columns.Clear();
            dataGridView1.AllowUserToAddRows = false;
            dataGridView1.AllowUserToDeleteRows = false;
            dataGridView1.AllowUserToResizeColumns = false;
            try
            {
                dataGridView1.Visible = true;
                SqlConnection conn = new SqlConnection("Data Source=localhost;Initial Catalog=TestDB;Integrated Security=True;Connect Timeout=30;Encrypt=False;");
                conn.Open();
                SqlCommand cmd = new SqlCommand($"Select * from {dataGridView2.CurrentCell.Value}", conn);
                SqlDataAdapter da = new SqlDataAdapter(cmd);
                DataTable dt = new DataTable();
                da.Fill(dt); // sqlDataAdapter 填入 DataSet
                dataGridView1.DataSource = dt;

                DataGridViewTextBoxColumn newCol = new DataGridViewTextBoxColumn();
                DataGridViewLinkColumn linCol = new DataGridViewLinkColumn();
                DataGridViewButtonColumn btnCol = new DataGridViewButtonColumn();
                DataGridViewComboBoxColumn comCol = new DataGridViewComboBoxColumn();
                linCol.HeaderText = "設定";
                linCol.Text = "進階設定";
                linCol.LinkColor = Color.Black;
                linCol.UseColumnTextForLinkValue = true;
                linCol.TrackVisitedState = false;
                newCol.HeaderText = "商品需求";
                btnCol.HeaderText = "加入列表";
                btnCol.Text = "加入列表";
                comCol.HeaderText = "數量";
                btnCol.UseColumnTextForButtonValue = true; // 按鈕上的文字使用該欄位的文字
                dataGridView1.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;

                for (int i = 1; i <= 10; i++)
                    comCol.Items.Add(i.ToString());

                dataGridView1.Columns.Insert(4, comCol);
                dataGridView1.Columns.Insert(5, newCol);
                dataGridView1.Columns.Insert(6, btnCol);
                dataGridView1.Columns.Insert(0, linCol);


                dataGridView1.Columns[0].ReadOnly = true;
                dataGridView1.Columns[1].ReadOnly = true;
                dataGridView1.Columns[2].ReadOnly = true;
                dataGridView1.Columns[3].ReadOnly = true;
                dataGridView1.Columns[5].Visible = false; // 加入數量欄位隱藏
                dataGridView1.Columns[7].Visible = false; // 加入列表欄位隱藏

                SqlCommand cmd2 = new SqlCommand("Select * from 需求", conn);
                for (int i = 0; i < dataGridView1.Rows.Count; i++)
                {
                    DataGridViewComboBoxCell comCell = new DataGridViewComboBoxCell();
                    SqlDataReader DataReader = cmd2.ExecuteReader();
                    int num = 0;

                    while (DataReader.Read())//讀取需求table
                    {
                        /*  用該列的商品名稱來判斷, 如果該商品的名稱有在table資料中就表示該商品有需求項
                            查看資料庫中此商品有沒有需求項, 有的話全部抓出來加入到下拉選單中, 並用 num 此商品在資料庫中紀錄了幾筆需求項  */
                        if (dataGridView1.Rows[i].Cells["商品"].Value.ToString() == DataReader[0].ToString())
                        {
                            comCell.Items.Add($"{DataReader[1]} + {DataReader[2]}元");
                            num++;
                        }
                    }
                    DataReader.Close();//一定要關起來，卡在讀取空資料

                    if (num > 0) // 此商品有需求項
                    {
                        comCell.Items.Add("N/A");
                        dataGridView1.Rows[i].Cells[6] = comCell;
                    }
                    else // 此商品無任何需求項
                    {
                        dataGridView1.Rows[i].Cells[6].Value = "N/A";
                        dataGridView1.Rows[i].Cells[6].ReadOnly = true;
                    }
                }
                conn.Close();
                //HeaderText標題置中
                for (int i = 0; i < dataGridView1.Columns.Count; i++)
                    dataGridView1.Columns[i].SortMode = DataGridViewColumnSortMode.NotSortable;
                dataGridView1.Columns[3].Width = 100;
                dataGridView1.Columns[1].Width = 50;
                dataGridView1.Columns[6].Width = 90;
                dataGridView1.Columns[8].Width = 90;
            }
            catch (Exception) { }
        }


        private void clearClass_Click(object sender, EventArgs e) // 清除所有類別
        {
            DialogResult dr = MessageBox.Show("確定要刪除所有類別嗎，類別中的所有商品也會全數刪除", "警告", MessageBoxButtons.YesNo);
            if(dr == DialogResult.Yes)
            {
                SqlConnection conn = new SqlConnection("Data Source=localhost;Initial Catalog=TestDB;Integrated Security=True;Connect Timeout=30;Encrypt=False;");
                conn.Open();
                SqlCommand cmd = new SqlCommand("TRUNCATE TABLE 類別清單", conn);
                cmd.ExecuteNonQuery();
                for (int i = comboBox1.Items.Count - 1; i > 0; i--)
                {
                    SqlCommand cmd2 = new SqlCommand($"DROP TABLE {comboBox1.Items[i]}", conn);
                    cmd2.ExecuteNonQuery();
                    comboBox1.Items.RemoveAt(i);
                }
                classList_Show();
                dataGridView1.Visible = false;
                rows_cnt = 0;
                label4.Text = $"目前已有{rows_cnt}個類別";
            }
        }

        private void deleteProduct_Click(object sender, EventArgs e) // 刪除該類別的全部商品
        {
            DialogResult dr = MessageBox.Show("確定要刪除所有商品項目嗎?", "警告", MessageBoxButtons.YesNo);
            if (dr == DialogResult.Yes)
            {
                if (dataGridView1.Rows.Count > 0)
                {
                    SqlConnection conn = new SqlConnection("Data Source=localhost;Initial Catalog=TestDB;Integrated Security=True;Connect Timeout=30;Encrypt=False;");
                    conn.Open();
                    SqlCommand cmd;
                    if (dataGridView2.SelectedCells.Count > 0)
                        cmd = new SqlCommand($"TRUNCATE TABLE {dataGridView2.SelectedCells[0].Value}", conn);
                    else
                        cmd = new SqlCommand($"TRUNCATE TABLE {comboBox1.Text}", conn);
                    cmd.ExecuteNonQuery();
                    productList_Show();
                }
                MessageBox.Show("所有商品項目刪除成功");
                if (dataGridView2.SelectedCells.Count > 0)
                    dataGridView2.SelectedCells[0].Selected = false;
            }
        }

        private void dataGridView2_CellMouseUp(object sender, DataGridViewCellMouseEventArgs e) // dataGridView2 滑鼠點擊事件
        {
            ContextMenuStrip contextMenuStrip = new System.Windows.Forms.ContextMenuStrip(); // 滑鼠右鍵選單
            dataGridView2.ContextMenuStrip = contextMenuStrip;
            ToolStripMenuItem toolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            toolStripMenuItem.Text = "刪除此類別";
            toolStripMenuItem.Click += toolStripMenuItem_Click;
            contextMenuStrip.Items.Add(toolStripMenuItem);
            if (e.Button == MouseButtons.Right && dataGridView2.CurrentCell.Visible.ToString() != "類別")
            {
                if(dataGridView2.SelectedCells.Count > 0)
                    dataGridView2.SelectedCells[0].Selected = false;
                if(e.RowIndex >= 0)
                {
                    dataGridView2.Rows[e.RowIndex].Cells[e.ColumnIndex].Selected = true;
                    contextMenuStrip.Show(MousePosition.X, MousePosition.Y);
                }
            }
        }

        private void toolStripMenuItem_Click(object sender, EventArgs e) // 按下右鍵刪除選項的處理事件
        {
            DialogResult dr = MessageBox.Show($"確定要刪除'{dataGridView2.SelectedCells[0].Value}'類別嗎?", "警告", MessageBoxButtons.YesNo);
            if (dr == DialogResult.Yes)
            {
                comboBox1.Items.RemoveAt(dataGridView2.SelectedCells[0].RowIndex + 1); // 要先刪除 ccomboBox 中的 item
                SqlConnection conn = new SqlConnection("Data Source=localhost;Initial Catalog=TestDB;Integrated Security=True;Connect Timeout=30;Encrypt=False;");
                conn.Open();
                SqlCommand cmd = new SqlCommand($"DELETE FROM 類別清單 WHERE 類別 = '{dataGridView2.SelectedCells[0].Value}'", conn);
                SqlCommand cmd2 = new SqlCommand($"DROP TABLE {dataGridView2.SelectedCells[0].Value}", conn);
                cmd.ExecuteNonQuery();
                cmd2.ExecuteNonQuery();
                rows_cnt--;
                label4.Text = $"目前已有{rows_cnt}個類別";
                classList_Show();
                comboBox1.SelectedIndex = 0;
                dataGridView1.Visible = false;
            }
        }
    }
}
