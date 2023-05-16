using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace WindowsFormsApp1
{
    public partial class Signin : Form
    {
        public Signin()
        {
            InitializeComponent();
        }

        private void linkLabel1_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            Login login = new Login();
            this.Visible = false;
            login.ShowDialog();
            //
        }
        private void button1_Click(object sender, EventArgs e)
        {
            SqlConnection conn = new SqlConnection("Data Source=localhost;Initial Catalog='我的資料庫';Integrated Security=True;Connect Timeout=30;Encrypt=False;");
            conn.Open();
            SqlCommand cmd = new SqlCommand($"insert into 帳密 values (@帳號, @密碼)", conn);//sql語法
            SqlCommand cmd2 = new SqlCommand("select * from 帳密", conn);//sql語法
            SqlCommand cmd3 = new SqlCommand($"DELETE FROM 帳密 WHERE 帳號 = '{textBox1.Text}'", conn);
            cmd.Parameters.AddWithValue("@帳號", textBox1.Text);
            cmd.Parameters.AddWithValue("@密碼", textBox2.Text);
            //cmd3.ExecuteNonQuery();
            SqlDataReader reader = cmd2.ExecuteReader();
            bool isExist = false;
            while (reader.Read())
            {
                if (textBox1.Text == reader[0].ToString())
                    isExist = true;
            }
            reader.Close();
            if (textBox3.Text == "0000")
            {
                if (isExist)
                    MessageBox.Show("帳號重複");
                else
                {
                    cmd.ExecuteNonQuery();//新增帳號
                    MessageBox.Show("帳號新增成功");
                }
            }
            else
                MessageBox.Show("此帳號無法註冊");
        }
    }
}
