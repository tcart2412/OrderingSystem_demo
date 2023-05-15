using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Security.Policy;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;

namespace WindowsFormsApp1
{
    public partial class Form_S : Form
    {
        public Form_S()
        {
            InitializeComponent();
            button3.DialogResult = DialogResult.OK;
        }

        public string[] demand_array = new string[10];//商品需求陣列
        public string[] price_array = new string[10];//需求價格陣列
        public static string adjust_Price = "";
        private int needNum = 0;
        private void P_Settings_Load(object sender, EventArgs e)
        {
            Show_Data();
            panel1.Visible = false;
            panel2.Visible = false;
            SqlConnection conn = new SqlConnection("Data Source=localhost;Initial Catalog=TestDB;Integrated Security=True;Connect Timeout=30;Encrypt=False;");
            conn.Open();
            SqlCommand cmd2 = new SqlCommand("Select * from 需求 Where 商品 = '" + Form_M.product_name + "'", conn);
            SqlDataReader DataReader = cmd2.ExecuteReader();
            while (DataReader.Read())
                needNum++;
            demand_show.Text = $"此商品目前已有{needNum}項商品需求";
            dataGridView1.SelectedCells[0].Selected = false;
        }

        private int x = 0;

        private void add_Click(object sender, EventArgs e)//新增商品需求按鈕
        {
            DialogResult dr = MessageBox.Show("確定要新增此商品需求嗎?", "確認", MessageBoxButtons.YesNo);
            if(dr == DialogResult.Yes)
            {
                if(textBox1.Text == "" || textBox2.Text == "")
                    MessageBox.Show("欄位不可為空");
                else
                {
                    if(Regex.IsMatch(textBox1.Text, @"^[^0-9]+$") && Regex.IsMatch(textBox2.Text, @"^[0-9]+$")) // 對需求名稱和價格做輸入規範
                    {
                        if((needNum + x) >= 5)
                            MessageBox.Show("商品需求數量已達最大值(5)", "警告", MessageBoxButtons.OK);
                        else
                        {
                            SqlConnection conn = new SqlConnection("Data Source=localhost;Initial Catalog=TestDB;Integrated Security=True;Connect Timeout=30;Encrypt=False;");
                            conn.Open();
                            SqlCommand cmd = new SqlCommand($"SELECT * FROM 需求 WHERE 商品 = '{Form_M.product_name}'", conn);
                            SqlDataReader DataReader = cmd.ExecuteReader();
                            int num = 0;
                            while (DataReader.Read())
                            {
                                if (textBox1.Text == DataReader[1].ToString())
                                    num++;
                            }
                            DataReader.Close();
                            if (num > 0)
                                MessageBox.Show("新增需求失敗，此需求已經存在");
                            else
                            {
                                demand_array[x] = textBox1.Text;
                                price_array[x] = textBox2.Text;
                                SqlCommand cmd2 = new SqlCommand($"insert into 需求 values (@商品, @需求, @價格)", conn);
                                cmd2.Parameters.AddWithValue("@商品", Form_M.product_name);
                                cmd2.Parameters.AddWithValue("@需求", demand_array[x]);
                                cmd2.Parameters.AddWithValue("@價格", Convert.ToInt32(price_array[x]));
                                cmd2.ExecuteNonQuery();
                                x++;
                                demand_show.Text = $"此商品目前已有{needNum + x}項商品需求";
                                textBox1.Text = "";
                                textBox2.Text = "";
                                MessageBox.Show("商品需求加入成功");
                            }
                        }
                    }
                    else
                        MessageBox.Show("加入商品需求失敗，請檢查輸入");
                }
            }
        }

        private void Show_Data()
        {
            DataGridViewTextBoxColumn colCass = new DataGridViewTextBoxColumn();
            DataGridViewTextBoxColumn colName = new DataGridViewTextBoxColumn();
            DataGridViewTextBoxColumn colPrice = new DataGridViewTextBoxColumn();
            colCass.HeaderText = "類別";
            colName.HeaderText = "商品名稱";
            colPrice.HeaderText = "價格";
            dataGridView1.Columns.Add(colCass);
            dataGridView1.Columns.Add(colName);
            dataGridView1.Columns.Add(colPrice);
            dataGridView1.Rows.Add(new object[] { Form_M.product_class, Form_M.product_name, Form_M.product_price });
            for (int i = 0; i < dataGridView1.Columns.Count; i++)
                dataGridView1.Columns[i].SortMode = DataGridViewColumnSortMode.NotSortable;
        }

        private int click_time1 = 1;
        private int click_time2 = 1;

        private void checkBox1_CheckedChanged(object sender, EventArgs e)//勾一下顯示再勾一下關閉
        {
            if(click_time1 % 2 != 0)
                panel1.Visible = true;
            else
                panel1.Visible = false;
            click_time1++;
        }

        private void checkBox2_CheckedChanged(object sender, EventArgs e)
        {
            if(click_time2 % 2 != 0)
                panel2.Visible = true;
            else
                panel2.Visible = false;
            click_time2++;
        }

        private void deleteCheck_CheckedChanged(object sender, EventArgs e)
        {
            if (deleteCheck.Checked)
            {
                DialogResult dr = MessageBox.Show("確定要立即刪除此商品嗎?", "警告", MessageBoxButtons.YesNo);
                if (dr == DialogResult.Yes)
                {
                    SqlConnection conn = new SqlConnection("Data Source=localhost;Initial Catalog=TestDB;Integrated Security=True;Connect Timeout=30;Encrypt=False;");
                    conn.Open();
                    SqlCommand cmd = new SqlCommand($"DELETE FROM {Form_M.product_class} WHERE 商品 = '{Form_M.product_name}'", conn);
                    cmd.ExecuteNonQuery();
                    MessageBox.Show("商品刪除成功");
                    this.Close();
                }
                else
                    deleteCheck.Checked = false;
            }
        }

        private void modBtn_Click(object sender, EventArgs e)
        {
            DialogResult dr = MessageBox.Show("確定要修改商品價格嗎", "確定", MessageBoxButtons.YesNo);
            if(dr == DialogResult.Yes)
            {
                if (textBox4.Text != "")
                {
                    if (Regex.IsMatch(textBox4.Text, @"^[0-9]+$"))
                    {
                        SqlConnection conn = new SqlConnection("Data Source=localhost;Initial Catalog=TestDB;Integrated Security=True;Connect Timeout=30;Encrypt=False;");
                        conn.Open();
                        string cmdText = $"UPDATE {dataGridView1.Rows[0].Cells[0].Value} " +
                                         $"SET 價格 = '{textBox4.Text}' " +
                                         $"WHERE 商品 = '{dataGridView1.Rows[0].Cells[1].Value}'"; // 這裡商品後面一定要加單引號
                        SqlCommand cmd = new SqlCommand(cmdText, conn);
                        cmd.ExecuteNonQuery();
                        conn.Close();
                        MessageBox.Show("商品數量已更新");
                        textBox4.Text = "";
                    }
                    else
                        MessageBox.Show("商品數量修改失敗，請檢查輸入");
                }
                else
                    MessageBox.Show("欄位不可為空");
            }
        }
    }
}
