using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Text.RegularExpressions;
using System.Data.SqlClient;

namespace WindowsFormsApp1
{
    public partial class Form_R : Form
    {
        public Form_R()
        {
            InitializeComponent();
        }

        private int total = 0;
        private int randNum;
        private void Form_D_Load(object sender, EventArgs e)
        {
            int productNum = 0, productPrice = 0, extraPrice = 0;
            string text = "";
            if (Form_O.noteText != "") // 當有訂單備註
                label10.Text = Form_O.noteText;
            else
                label10.Text = "\n**無**";
            Label[,] labelArray = new Label[100, 4];
            int num = 0;
            int y = 0;
            for (int i = 0; i < Form_O.count; i++)//動態設置Label物件、Label位置
            {
                for (int j = 0; j < 4; j++)
                {
                    int x;
                    labelArray[i, j] = new Label();
                    if (j == 0)
                    {
                        labelArray[i, j].Text = "x " + Form_O.orderArray[i, 1];//數量標籤
                        x = 0;
                    }
                    else if (j == 1)
                    {
                        labelArray[i, j].Text = Form_O.orderArray[i, 0];//商品名稱標籤
                        x = 51;
                    }
                    else if (j == 2)
                    {
                        text = Form_O.orderArray[i, 2];
                        if (text != "N/A")
                        {
                            //text = text.Substring(5, 2); // 提取需求金額
                            labelArray[i, j].Text = Form_O.orderArray[i, 2]; // 商品需求標籤
                        }
                        else
                        {
                            extraPrice = 0;
                            labelArray[i, j].Text = "無"; // 商品需求標籤
                        }
                        x = 135;
                    }
                    else
                    {
                        labelArray[i, j].Text = Form_O.orderArray[i, 3] + " $";//商品價格標籤
                        x = 230;
                    }

                    if(num % 3 != 0 && num != 0)//如果還沒有換行
                        labelArray[i, j].Location = new Point(5 + x, 15 + y);
                    else
                    {
                        labelArray[i, j].Location = new Point(5 + x, 15 + y);
                    }
                    labelArray[i, j].Name = "lb" + num; // label的 Name 屬性
                    labelArray[i, j].AutoSize = true;
                    this.Controls.Add(labelArray[i, j]);
                    this.panel1.Controls.Add(labelArray[i, j]); // 將 Label 加入控件
                    num++;
                }
                y += 20;
                productNum = Convert.ToInt32(Form_O.orderArray[i, 1]);//數量
                SqlConnection conn = new SqlConnection("Data Source=localhost;Initial Catalog=TestDB;Integrated Security=True;Connect Timeout=30;Encrypt=False;");
                conn.Open();
                SqlCommand cmd = new SqlCommand("SELECT * FROM 需求", conn);
                SqlDataReader DataReader = cmd.ExecuteReader();
                //c8 + 3元
                string[] subs = labelArray[i, 2].Text.Split(' ');
                while (DataReader.Read())
                {
                    if (subs[0] == DataReader[1].ToString()) // 如果該商品須需求有在table中
                        extraPrice = Convert.ToInt32(DataReader[2].ToString());
                }
                productPrice = Convert.ToInt32(Form_O.orderArray[i, 3]);//商品價格
                total += (productNum * (productPrice + extraPrice));
            }
            Random rd = new Random();
            if (Form_O.count > 0) // 當有訂單時
                randNum = rd.Next(50, 100); // 50 - 100 之間隨機整數
            else
                randNum = 0;
            total += randNum;
            label8.Text = "小費 : " + randNum.ToString() + " 元";
            label6.Text = "合計 : " + total.ToString() + " 元";
        }
    }

}
