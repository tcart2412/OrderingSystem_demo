using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.ToolBar;

namespace WindowsFormsApp1
{
    public partial class Form_O : Form
    {
        public Form_O()
        {
            InitializeComponent();
        }

        private void productList_Show(System.Windows.Forms.Button btn_input)//顯示商品清單
        {
            dataGridView1.Columns.Clear();
            dataGridView1.AllowUserToAddRows = false;
            dataGridView1.AllowUserToDeleteRows = false;
            dataGridView1.AllowUserToResizeColumns = false;
            dataGridView1.AllowUserToResizeRows = false;
            try
            {
                dataGridView1.Visible = true;
                SqlConnection conn = new SqlConnection("Data Source=localhost;Initial Catalog=TestDB;Integrated Security=True;Connect Timeout=30;Encrypt=False;");
                conn.Open();
                SqlCommand cmd = new SqlCommand($"Select * from {btn_input.Text}", conn);
                SqlDataAdapter da = new SqlDataAdapter(cmd);
                DataTable dt = new DataTable();
                da.Fill(dt); // sqlDataAdapter 填入 DataSet
                dataGridView1.DataSource = dt;

                DataGridViewTextBoxColumn newCol = new DataGridViewTextBoxColumn();
                DataGridViewTextBoxColumn stateCol = new DataGridViewTextBoxColumn();
                DataGridViewButtonColumn btnCol = new DataGridViewButtonColumn();
                DataGridViewComboBoxColumn comCol = new DataGridViewComboBoxColumn();//那列全部都是下拉選單
                newCol.HeaderText = "商品需求";
                btnCol.HeaderText = "加入購買";
                btnCol.Text = "加入列表";
                comCol.HeaderText = "數量";
                btnCol.UseColumnTextForButtonValue = true; // 按鈕上的文字使用該欄位的文字
                dataGridView1.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;

                for (int i = 1; i <= 10; i++)
                    comCol.Items.Add(i.ToString());

                dataGridView1.Columns.Insert(4, comCol);
                dataGridView1.Columns.Insert(5, newCol);
                dataGridView1.Columns.Insert(6, btnCol);

                dataGridView1.Columns[0].ReadOnly = true;
                dataGridView1.Columns[1].ReadOnly = true;
                dataGridView1.Columns[2].ReadOnly = true;
                dataGridView1.Columns[3].ReadOnly = true;
                dataGridView1.Columns[1].Visible = false; // 隱藏商品類別欄位

                SqlCommand cmd2 = new SqlCommand("Select * from 需求", conn);
                for (int i = 0; i < dataGridView1.Rows.Count; i++)
                {
                    DataGridViewComboBoxCell comCell = new DataGridViewComboBoxCell();
                    SqlDataReader DataReader = cmd2.ExecuteReader();
                    int num = 0;
                    while (DataReader.Read())
                    {
                        if (dataGridView1.Rows[i].Cells["商品"].Value.ToString() == DataReader[0].ToString())
                        {
                            comCell.Items.Add($"{DataReader[1]} + {DataReader[2]}元");
                            num++;
                        }
                    }
                    DataReader.Close();//一定要關起來，卡在讀取空資料
                    if (num > 0)
                    {
                        comCell.Items.Add("N/A");
                        dataGridView1.Rows[i].Cells[5] = comCell;
                    }
                    else
                    {
                        dataGridView1.Rows[i].Cells[5].Value = "N/A";
                        dataGridView1.Rows[i].Cells[5].ReadOnly = true;
                    }
                    
                }
                conn.Close();
                for (int i = 0; i < dataGridView1.Columns.Count; i++)
                    dataGridView1.Columns[i].SortMode = DataGridViewColumnSortMode.NotSortable;
                dataGridView1.Columns[0].Width = 50;
                dataGridView1.Columns[2].Width = 90;
                dataGridView1.Columns[5].Width = 70;
                dataGridView1.Columns[3].Width = 50;
                dataGridView1.Columns[4].Width = 50;
                dataGridView1.Columns[6].Width = 70;
            }
            catch (Exception) { }
        }

        private System.Windows.Forms.Button[] btns_arr = new System.Windows.Forms.Button[18];

        private void linkLabel1_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e) // 進入管理介面
        {
            dataGridView1.Visible = false;
            Login login = new Login();
            Form_M fm = new Form_M();
            if (Login.loginAccount == "") // 進行是否目前有帳號登入的判斷
                login.ShowDialog(this);
            else
                fm.ShowDialog(this);
            SqlConnection conn = new SqlConnection("Data Source=localhost;Initial Catalog=TestDB;Integrated Security=True;Connect Timeout=30;Encrypt=False;");
            conn.Open();
            SqlCommand cmd = new SqlCommand($"Select * from 類別清單", conn);
            SqlDataReader DataReader = cmd.ExecuteReader();
            for(int n = groupBox1.Controls.Count - 1; n >= 0; n--) // 將groupBox1中的按鈕全部清除
            {
                if (groupBox1.Controls[n].Name == "btn")
                    groupBox1.Controls.Remove(groupBox1.Controls[n]);
            }
            Array.Clear(btns_arr, 0, btns_arr.Length); // 清空陣列
            int i = 0, k = 0, c = 0;
            while (DataReader.Read())
            {
                btns_arr[i] = new System.Windows.Forms.Button();
                btns_arr[i].Name = "btn";
                btns_arr[i].Text = DataReader[1].ToString();
                if (20 + (60 * k) > 500)
                {
                    k = 0;
                    c++;
                    btns_arr[i].Location = new Point(14 + (100 * c), 20 + (60 * k));
                    btns_arr[i].Size = new Size(100, 40);
                    k++;
                }
                else
                {
                    btns_arr[i].Location = new Point(14 + (100 * c), 20 + (60 * k));
                    btns_arr[i].Size = new Size(100, 40);
                    k++;
                }
                btns_arr[i].Click += new EventHandler(Buttons_Click);
                btns_arr[i].Cursor = Cursors.Hand;
                btns_arr[i].FlatStyle = FlatStyle.Flat;
                btns_arr[i].FlatAppearance.MouseOverBackColor = Color.SandyBrown;
                Controls.Add(btns_arr[i]);
                groupBox1.Controls.Add(btns_arr[i]);
                i++;
            }
        }

        void Buttons_Click(object sender, EventArgs e)
        {
            for(int i = 0; i < groupBox1.Controls.Count; i++)
            {
                if (groupBox1.Controls[i].BackColor == Color.SandyBrown)
                    groupBox1.Controls[i].BackColor = Color.AntiqueWhite;
            }
            System.Windows.Forms.Button btn = (System.Windows.Forms.Button)sender;
            productList_Show(btn);
            btn.BackColor = Color.SandyBrown;
        }
        private void Form_O_Load(object sender, EventArgs e)
        {
            dataGridView2.Columns[0].Width = 50;
            dataGridView2.Columns[2].Width = 30;
            dataGridView2.Columns[4].Width = 45;
            dataGridView2.Columns[5].Width = 40;
            dataGridView2.Columns[3].Width = 50;
            dataGridView2.Columns[1].Width = 70;
            dataGridView1.Visible = false;
            //dataGridView1.EditingControlShowing += dataGridView1_EditingControlShowing;
            SqlConnection conn = new SqlConnection("Data Source=localhost;Initial Catalog=TestDB;Integrated Security=True;Connect Timeout=30;Encrypt=False;");
            conn.Open();
            SqlCommand cmd = new SqlCommand("SELECT 類別 FROM 類別清單", conn);
            SqlDataReader DataReader = cmd.ExecuteReader();
            int num = 0;
            string[] classArray = new string[18];
            int i = 0, k= 0, c = 0; // i 紀錄 btns_arr 的索引值，num 紀錄有幾筆類別ps. i跟num功用一樣，只是想增加可讀性
            while (DataReader.Read())
            {
                btns_arr[i] = new System.Windows.Forms.Button();
                btns_arr[i].Name = "btn";
                btns_arr[i].Text = DataReader[0].ToString();
                if (20 + (60 * k) > 500)
                {
                    k = 0;
                    c++;
                    btns_arr[i].Location = new Point(14 + (100 * c), 20 + (60 * k));
                    btns_arr[i].Size = new Size(100, 40);
                    k++;
                }
                else
                {
                    btns_arr[i].Location = new Point(14 + (100 * c), 20 + (60 * k));
                    btns_arr[i].Size = new Size(100, 40);
                    k++;
                }
                btns_arr[i].Click += new EventHandler(Buttons_Click);
                btns_arr[i].Cursor = Cursors.Hand;
                btns_arr[i].FlatStyle = FlatStyle.Flat;
                btns_arr[i].FlatAppearance.MouseOverBackColor = Color.SandyBrown;
                Controls.Add(btns_arr[i]);
                groupBox1.Controls.Add(btns_arr[i]);
                i++;
                classArray[num] = DataReader[0].ToString();
                num++;
            }
            DataReader.Close();

            // 將資料庫中的狀態欄位設為加入列表，每個按鈕上文字一開始都為加入列表
            for (int j = 0; j < num; j++)
            {
                string cmdText = $"UPDATE {classArray[j]} " +
                                 $"SET 狀態 = '加入列表' ";
                SqlCommand cmd2 = new SqlCommand(cmdText, conn);
                cmd2.ExecuteNonQuery();
            }
            conn.Close();
            for (int j = 0; j < dataGridView2.Columns.Count; j++)//讓Column的Header置中
                dataGridView2.Columns[j].SortMode = DataGridViewColumnSortMode.NotSortable;
        }

        private void dataGridView1_CellClick(object sender, DataGridViewCellEventArgs e)//讓Cell中的comboBox點一次就可以下拉
        {
            if (e.RowIndex >= 0 && e.ColumnIndex >= 0)
            {
                DataGridViewComboBoxCell comCell = dataGridView1.CurrentCell as DataGridViewComboBoxCell;
                //DataGridViewButtonCell btnCell = dataGridView1.CurrentCell as DataGridViewButtonCell;
                if (comCell != null) // 如果點擊的是 comboBox
                {
                    dataGridView1.BeginEdit(false); // 取消"開始編輯"
                    DataGridViewComboBoxEditingControl comboEdit = dataGridView1.EditingControl as DataGridViewComboBoxEditingControl; 
                    comboEdit.DropDownWidth = dataGridView1.Columns[e.ColumnIndex].Width; // 下拉寬度和欄位寬度一樣
                    if (comboEdit != null) // 如果在 Cell 中編輯的是 comboBox
                        comboEdit.DroppedDown = true; // 選單此時下拉
                }
                //DataGridViewButtonColumn btnCol = dataGridView1.Columns[e.ColumnIndex] as DataGridViewButtonColumn;
                //if (btnCell != null && btnCol != null)
                //{
                //    btnCol.Text = "已加入訂單";
                //    btnCol.UseColumnTextForButtonValue = true;
                //}
            }
        }

        public static int count = 0; // 紀錄訂單筆數
        public static string[,] orderArray = new string[100, 4]; // dg2 : 商品 數量 需求 價格

        private void dataGridView1_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
            string text = dataGridView1.Rows[e.RowIndex].Cells[4].FormattedValue.ToString(); // 數量欄位的值
            string text2 = dataGridView1.Rows[e.RowIndex].Cells[5].FormattedValue.ToString(); // 需求欄位的值
            if (dataGridView1.Rows[e.RowIndex].Cells[e.ColumnIndex] is DataGridViewButtonCell)//當按下"加入列表"按鈕
            {
                if (text != "")//如果有選擇商品數量
                {
                    dataGridView1.Rows[e.RowIndex].Cells[7].Value = "已加入"; // 商品狀態設定為"已加入"
                    SqlConnection conn = new SqlConnection("Data Source=localhost;Initial Catalog=TestDB;Integrated Security=True;Connect Timeout=30;Encrypt=False;");
                    conn.Open();
                    string cmdText = $"UPDATE {dataGridView1.Rows[e.RowIndex].Cells[1].Value} " +
                                     $"SET 狀態 = '已加入' " +
                                     $"WHERE 編號 = {dataGridView1.Rows[e.RowIndex].Cells[0].Value}";
                    SqlCommand cmd = new SqlCommand(cmdText, conn);
                    cmd.ExecuteNonQuery();
                    conn.Close();

                    dataGridView2.Rows.Add();
                    dataGridView2.Rows[count].Cells[0].Value = dataGridView1.Rows[e.RowIndex].Cells[1].Value.ToString(); // 設定商品類別
                    dataGridView2.Rows[count].Cells[1].Value = dataGridView1.Rows[e.RowIndex].Cells[2].Value.ToString(); // 設定商品名稱
                    dataGridView2.Rows[count].Cells[2].Value = dataGridView1.Rows[e.RowIndex].Cells[4].Value.ToString(); // 設定商品數量
                    if(text2 != "" && text2 != "N/A") // 設定商品需求
                        dataGridView2.Rows[count].Cells[3].Value = dataGridView1.Rows[e.RowIndex].Cells[5].Value.ToString();
                    else
                        dataGridView2.Rows[count].Cells[3].Value = "N/A";
                    dataGridView2.Rows[count].Cells[4].Value = dataGridView1.Rows[e.RowIndex].Cells[3].Value.ToString(); //設定商品價格

                    for (int i = 0; i < dataGridView2.Columns.Count; i++)//讓Column的Header置中
                        dataGridView2.Columns[i].SortMode = DataGridViewColumnSortMode.NotSortable;

                    count++; //每按下一次加入列表就 + 1
                }
                else
                    MessageBox.Show("請選擇商品數量", "下單失敗", MessageBoxButtons.OK);
            }
        }

        public static string noteText = "";

        private void button1_Click(object sender, EventArgs e) // 結帳按鈕
        {
            Form_R fd = new Form_R();
            for(int i = 0; i < dataGridView2.RowCount; i++)
            {
                orderArray[i, 0] = dataGridView2.Rows[i].Cells[1].Value.ToString();
                orderArray[i, 1] = dataGridView2.Rows[i].Cells[2].Value.ToString();
                orderArray[i, 2] = dataGridView2.Rows[i].Cells[3].Value.ToString();
                orderArray[i, 3] = dataGridView2.Rows[i].Cells[4].Value.ToString();
            }
            fd.ShowDialog(this);
        }

        private void dataGridView2_CellContentClick(object sender, DataGridViewCellEventArgs e) // 當按下移除按鈕
        {
            if(e.RowIndex != -1)
            {
                if(dataGridView2.Rows[e.RowIndex].Cells[e.ColumnIndex] is DataGridViewButtonCell) 
                {
                    SqlConnection conn = new SqlConnection("Data Source=localhost;Initial Catalog=TestDB;Integrated Security=True;Connect Timeout=30;Encrypt=False;");
                    conn.Open();
                    string cmdText = $"UPDATE {dataGridView2.Rows[e.RowIndex].Cells[0].Value} " +
                                     $"SET 狀態 = '加入列表' " +
                                     $"WHERE 商品 = '{dataGridView2.Rows[e.RowIndex].Cells[1].Value}'";
                    SqlCommand cmd = new SqlCommand(cmdText, conn);
                    cmd.ExecuteNonQuery();
                    conn.Close();
                    dataGridView2.Rows.RemoveAt(e.RowIndex);
                    dataGridView1.Visible = false;
                    count--;
                } 
            }

        }

        private void groupBox1_Paint(object sender, PaintEventArgs e) // 去掉groupBox1外框線
        {
            e.Graphics.Clear(this.BackColor);
        }

        private void textBox1_TextChanged(object sender, EventArgs e) // 在輸入的同時把資料設給noteText，以便給Form_R存取
        {
            noteText = textBox1.Text;
        }

        //private void dataGridView1_EditingControlShowing(object sender, DataGridViewEditingControlShowingEventArgs e)
        //{
        //    if(dataGridView1.CurrentCell.ColumnIndex == 6)
        //    {
        //        e.Control.Click += Control_Click;
        //    }
        //}

        //private void Control_Click(object sender, EventArgs e)
        //{
        //    (sender as System.Windows.Forms.Button).Text = "已加入訂單";
        //    dataGridView1.Visible = false;
        //}
    }
}
