﻿namespace InputBinding;

partial class InputtingForm
{

    #region Windows Form Designer generated code

    ///<summary>
    ///Required method for Designer support - do not modify
    ///the contents of this method with the code editor.
    ///</summary>
    private void InitializeComponent()
    {
        this.button1 = new System.Windows.Forms.Button();
        this.button2 = new System.Windows.Forms.Button();
        this.button3 = new System.Windows.Forms.Button();
        this.checkBox1 = new System.Windows.Forms.CheckBox();
        this.checkBox2 = new System.Windows.Forms.CheckBox();
        this.checkBox3 = new System.Windows.Forms.CheckBox();
        this.checkBox4 = new System.Windows.Forms.CheckBox();
        this.checkBox5 = new System.Windows.Forms.CheckBox();
        this.checkBox6 = new System.Windows.Forms.CheckBox();
        this.checkBox7 = new System.Windows.Forms.CheckBox();
        this.label1 = new System.Windows.Forms.Label();
        this.label2 = new System.Windows.Forms.Label();
        this.label3 = new System.Windows.Forms.Label();
        this.label5 = new System.Windows.Forms.Label();
        this.label4 = new System.Windows.Forms.Label();
        this.groupBox1 = new System.Windows.Forms.GroupBox();
        this.groupBox2 = new System.Windows.Forms.GroupBox();
        this.groupBox3 = new System.Windows.Forms.GroupBox();
        this.groupBox1.SuspendLayout();
        this.groupBox2.SuspendLayout();
        this.groupBox3.SuspendLayout();
        this.SuspendLayout();
        //
        //button1
        //
        this.button1.Font = new System.Drawing.Font("微软雅黑", 10.5F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
        this.button1.Location = new System.Drawing.Point(81, 489);
        this.button1.Name = "button1";
        this.button1.Size = new System.Drawing.Size(142, 32);
        this.button1.TabIndex = 3;
        this.button1.Text = "保存配置并退出";
        this.button1.UseVisualStyleBackColor = true;
        this.button1.Click += new System.EventHandler(this.Button1_Click);
        //
        //button2
        //
        this.button2.Font = new System.Drawing.Font("微软雅黑", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
        this.button2.Location = new System.Drawing.Point(15, 155);
        this.button2.Name = "button2";
        this.button2.Size = new System.Drawing.Size(116, 32);
        this.button2.TabIndex = 9;
        this.button2.Text = "临时关闭自动切换";
        this.button2.UseVisualStyleBackColor = true;
        this.button2.Click += new System.EventHandler(this.Button2_Click);
        //
        //button3
        //
        this.button3.Font = new System.Drawing.Font("微软雅黑", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
        this.button3.Location = new System.Drawing.Point(137, 155);
        this.button3.Name = "button3";
        this.button3.Size = new System.Drawing.Size(132, 32);
        this.button3.TabIndex = 10;
        this.button3.Text = "打开自动切换";
        this.button3.UseVisualStyleBackColor = true;
        this.button3.Click += new System.EventHandler(this.Button3_Click);
        //
        //checkBox1
        //
        this.checkBox1.AutoSize = true;
        this.checkBox1.Checked = true;
        this.checkBox1.CheckState = System.Windows.Forms.CheckState.Checked;
        this.checkBox1.Font = new System.Drawing.Font("微软雅黑", 10.5F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
        this.checkBox1.Location = new System.Drawing.Point(10, 54);
        this.checkBox1.Name = "checkBox1";
        this.checkBox1.Size = new System.Drawing.Size(167, 24);
        this.checkBox1.TabIndex = 6;
        this.checkBox1.Text = "Shift切换中英文(默认)";
        this.checkBox1.UseVisualStyleBackColor = true;
        this.checkBox1.CheckedChanged += new System.EventHandler(this.CheckBox1_CheckedChanged);
        //
        //checkBox2
        //
        this.checkBox2.AutoSize = true;
        this.checkBox2.Font = new System.Drawing.Font("微软雅黑", 10.5F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
        this.checkBox2.Location = new System.Drawing.Point(10, 84);
        this.checkBox2.Name = "checkBox2";
        this.checkBox2.Size = new System.Drawing.Size(121, 24);
        this.checkBox2.TabIndex = 7;
        this.checkBox2.Text = "Ctrl切换中英文";
        this.checkBox2.UseVisualStyleBackColor = true;
        this.checkBox2.CheckedChanged += new System.EventHandler(this.CheckBox2_CheckedChanged);
        //
        //checkBox3
        //
        this.checkBox3.AutoSize = true;
        this.checkBox3.Enabled = false;
        this.checkBox3.Font = new System.Drawing.Font("微软雅黑", 10.5F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
        this.checkBox3.Location = new System.Drawing.Point(15, 25);
        this.checkBox3.Name = "checkBox3";
        this.checkBox3.Size = new System.Drawing.Size(196, 24);
        this.checkBox3.TabIndex = 8;
        this.checkBox3.Text = "消除切换后命令行多余字母";
        this.checkBox3.UseVisualStyleBackColor = true;
        this.checkBox3.CheckedChanged += new System.EventHandler(this.CheckBox3_CheckedChanged);
        //
        //checkBox4
        //
        this.checkBox4.AutoSize = true;
        this.checkBox4.Font = new System.Drawing.Font("微软雅黑", 10.5F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
        this.checkBox4.Location = new System.Drawing.Point(10, 54);
        this.checkBox4.Name = "checkBox4";
        this.checkBox4.Size = new System.Drawing.Size(89, 24);
        this.checkBox4.TabIndex = 11;
        this.checkBox4.Text = "Ctrl+空格";
        this.checkBox4.UseVisualStyleBackColor = true;
        this.checkBox4.CheckedChanged += new System.EventHandler(this.CheckBox4_CheckedChanged);
        //
        //checkBox5
        //
        this.checkBox5.AutoSize = true;
        this.checkBox5.Font = new System.Drawing.Font("微软雅黑", 10.5F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
        this.checkBox5.Location = new System.Drawing.Point(10, 84);
        this.checkBox5.Name = "checkBox5";
        this.checkBox5.Size = new System.Drawing.Size(92, 24);
        this.checkBox5.TabIndex = 12;
        this.checkBox5.Text = "Ctrl+Shift";
        this.checkBox5.UseVisualStyleBackColor = true;
        this.checkBox5.CheckedChanged += new System.EventHandler(this.CheckBox5_CheckedChanged);
        //
        //checkBox6
        //
        this.checkBox6.AutoSize = true;
        this.checkBox6.Font = new System.Drawing.Font("微软雅黑", 10.5F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
        this.checkBox6.Location = new System.Drawing.Point(10, 114);
        this.checkBox6.Name = "checkBox6";
        this.checkBox6.Size = new System.Drawing.Size(93, 24);
        this.checkBox6.TabIndex = 13;
        this.checkBox6.Text = "Win+空格";
        this.checkBox6.UseVisualStyleBackColor = true;
        this.checkBox6.CheckedChanged += new System.EventHandler(this.CheckBox6_CheckedChanged);
        //
        //checkBox7
        //
        this.checkBox7.AutoSize = true;
        this.checkBox7.Enabled = false;
        this.checkBox7.Font = new System.Drawing.Font("微软雅黑", 10.5F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
        this.checkBox7.Location = new System.Drawing.Point(15, 91);
        this.checkBox7.Name = "checkBox7";
        this.checkBox7.Size = new System.Drawing.Size(238, 24);
        this.checkBox7.TabIndex = 18;
        this.checkBox7.Text = "解决切换后需多按一次命令首字母";
        this.checkBox7.UseVisualStyleBackColor = true;
        this.checkBox7.CheckedChanged += new System.EventHandler(this.CheckBox7_CheckedChanged);
        //
        //label1
        //
        this.label1.AutoSize = true;
        this.label1.Font = new System.Drawing.Font("微软雅黑", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
        this.label1.Location = new System.Drawing.Point(87, 524);
        this.label1.Name = "label1";
        this.label1.Size = new System.Drawing.Size(142, 17);
        this.label1.TabIndex = 4;
        this.label1.Text = "(.ini配置文件在dll文件夹)";
        //
        //label2
        //
        this.label2.AutoSize = true;
        this.label2.Font = new System.Drawing.Font("微软雅黑", 10.5F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
        this.label2.Location = new System.Drawing.Point(6, 26);
        this.label2.Name = "label2";
        this.label2.Size = new System.Drawing.Size(169, 20);
        this.label2.TabIndex = 16;
        this.label2.Text = "中/英文状态切换快捷键：";
        //
        //label3
        //
        this.label3.AutoSize = true;
        this.label3.Font = new System.Drawing.Font("微软雅黑", 10.5F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
        this.label3.Location = new System.Drawing.Point(6, 26);
        this.label3.Name = "label3";
        this.label3.Size = new System.Drawing.Size(214, 20);
        this.label3.TabIndex = 17;
        this.label3.Text = "中文(CH)/英文(EN)切换快捷键：";
        //
        //label5
        //
        this.label5.AutoSize = true;
        this.label5.Enabled = false;
        this.label5.Font = new System.Drawing.Font("微软雅黑", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
        this.label5.Location = new System.Drawing.Point(12, 117);
        this.label5.Name = "label5";
        this.label5.Size = new System.Drawing.Size(240, 34);
        this.label5.TabIndex = 19;
        this.label5.Text = "(部分输入法和CAD版本存在切换后\r\n需多按一次首命令首字母，勾选此项可解决)";
        //
        //label4
        //
        this.label4.AutoSize = true;
        this.label4.Enabled = false;
        this.label4.Font = new System.Drawing.Font("微软雅黑", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
        this.label4.Location = new System.Drawing.Point(12, 51);
        this.label4.Name = "label4";
        this.label4.Size = new System.Drawing.Size(204, 34);
        this.label4.TabIndex = 17;
        this.label4.Text = "(部分输入法和CAD版本存在切换后\r\n命令行有多余字母，勾选此项可消除)";
        //
        //groupBox1
        //
        this.groupBox1.Controls.Add(this.label2);
        this.groupBox1.Controls.Add(this.checkBox1);
        this.groupBox1.Controls.Add(this.checkBox2);
        this.groupBox1.Font = new System.Drawing.Font("微软雅黑", 10.5F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
        this.groupBox1.Location = new System.Drawing.Point(12, 12);
        this.groupBox1.Name = "groupBox1";
        this.groupBox1.Size = new System.Drawing.Size(275, 117);
        this.groupBox1.TabIndex = 14;
        this.groupBox1.TabStop = false;
        this.groupBox1.Text = "Shift懒人版";
        //
        //groupBox2
        //
        this.groupBox2.Controls.Add(this.label3);
        this.groupBox2.Controls.Add(this.checkBox4);
        this.groupBox2.Controls.Add(this.checkBox6);
        this.groupBox2.Controls.Add(this.checkBox5);
        this.groupBox2.Font = new System.Drawing.Font("微软雅黑", 10.5F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
        this.groupBox2.Location = new System.Drawing.Point(12, 135);
        this.groupBox2.Name = "groupBox2";
        this.groupBox2.Size = new System.Drawing.Size(275, 149);
        this.groupBox2.TabIndex = 15;
        this.groupBox2.TabStop = false;
        this.groupBox2.Text = "进阶版";
        //
        //groupBox3
        //
        this.groupBox3.Controls.Add(this.button2);
        this.groupBox3.Controls.Add(this.button3);
        this.groupBox3.Controls.Add(this.label4);
        this.groupBox3.Controls.Add(this.label5);
        this.groupBox3.Controls.Add(this.checkBox3);
        this.groupBox3.Controls.Add(this.checkBox7);
        this.groupBox3.Font = new System.Drawing.Font("微软雅黑", 10.5F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
        this.groupBox3.Location = new System.Drawing.Point(12, 290);
        this.groupBox3.Name = "groupBox3";
        this.groupBox3.Size = new System.Drawing.Size(275, 193);
        this.groupBox3.TabIndex = 16;
        this.groupBox3.TabStop = false;
        this.groupBox3.Text = "其它";
        //
        //InputtingForm
        //
        this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
        this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
        this.ClientSize = new System.Drawing.Size(296, 550);
        this.Controls.Add(this.groupBox3);
        this.Controls.Add(this.groupBox2);
        this.Controls.Add(this.groupBox1);
        this.Controls.Add(this.button1);
        this.Controls.Add(this.label1);
        this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
        this.MaximizeBox = false;
        this.MinimizeBox = false;
        this.Name = "InputtingForm";
        this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
        this.Text = "输入法自动切换";
        this.Load += new System.EventHandler(this.Form1_Load);
        this.groupBox1.ResumeLayout(false);
        this.groupBox1.PerformLayout();
        this.groupBox2.ResumeLayout(false);
        this.groupBox2.PerformLayout();
        this.groupBox3.ResumeLayout(false);
        this.groupBox3.PerformLayout();
        this.ResumeLayout(false);
        this.PerformLayout();

    }

    #endregion
    private System.Windows.Forms.Button button1;
    private System.Windows.Forms.Button button2;
    private System.Windows.Forms.Button button3;

    private System.Windows.Forms.CheckBox checkBox1;
    private System.Windows.Forms.CheckBox checkBox2;
    private System.Windows.Forms.CheckBox checkBox3;
    private System.Windows.Forms.CheckBox checkBox4;
    private System.Windows.Forms.CheckBox checkBox5;
    private System.Windows.Forms.CheckBox checkBox6;
    private System.Windows.Forms.CheckBox checkBox7;
    private System.Windows.Forms.GroupBox groupBox1;
    private System.Windows.Forms.GroupBox groupBox2;
    private System.Windows.Forms.GroupBox groupBox3;

    private System.Windows.Forms.Label label1;
    private System.Windows.Forms.Label label2;
    private System.Windows.Forms.Label label3;
    private System.Windows.Forms.Label label4;
    private System.Windows.Forms.Label label5;
}