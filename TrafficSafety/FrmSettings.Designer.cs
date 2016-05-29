namespace TrafficSafety
{
    partial class FrmSettings
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.label1 = new System.Windows.Forms.Label();
            this.txt_turningRatio = new System.Windows.Forms.TextBox();
            this.txt_straightRatio = new System.Windows.Forms.TextBox();
            this.label2 = new System.Windows.Forms.Label();
            this.txt_flowRatio = new System.Windows.Forms.TextBox();
            this.label3 = new System.Windows.Forms.Label();
            this.button1 = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(29, 36);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(111, 15);
            this.label1.TabIndex = 0;
            this.label1.Text = "TURNING RATIO";
            // 
            // txt_turningRatio
            // 
            this.txt_turningRatio.Location = new System.Drawing.Point(146, 26);
            this.txt_turningRatio.Name = "txt_turningRatio";
            this.txt_turningRatio.Size = new System.Drawing.Size(100, 25);
            this.txt_turningRatio.TabIndex = 1;
            // 
            // txt_straightRatio
            // 
            this.txt_straightRatio.Location = new System.Drawing.Point(146, 66);
            this.txt_straightRatio.Name = "txt_straightRatio";
            this.txt_straightRatio.Size = new System.Drawing.Size(100, 25);
            this.txt_straightRatio.TabIndex = 3;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(29, 76);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(119, 15);
            this.label2.TabIndex = 2;
            this.label2.Text = "STRAIGHT RATIO";
            // 
            // txt_flowRatio
            // 
            this.txt_flowRatio.Location = new System.Drawing.Point(146, 106);
            this.txt_flowRatio.Name = "txt_flowRatio";
            this.txt_flowRatio.Size = new System.Drawing.Size(100, 25);
            this.txt_flowRatio.TabIndex = 5;
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(29, 116);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(87, 15);
            this.label3.TabIndex = 4;
            this.label3.Text = "FLOW RATIO";
            // 
            // button1
            // 
            this.button1.Location = new System.Drawing.Point(155, 166);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(75, 23);
            this.button1.TabIndex = 6;
            this.button1.Text = "OK";
            this.button1.UseVisualStyleBackColor = true;
            this.button1.Click += new System.EventHandler(this.button1_Click);
            // 
            // FrmSettings
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(389, 207);
            this.Controls.Add(this.button1);
            this.Controls.Add(this.txt_flowRatio);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.txt_straightRatio);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.txt_turningRatio);
            this.Controls.Add(this.label1);
            this.Name = "FrmSettings";
            this.Text = "FrmSettings";
            this.Load += new System.EventHandler(this.FrmSettings_Load);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TextBox txt_turningRatio;
        private System.Windows.Forms.TextBox txt_straightRatio;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.TextBox txt_flowRatio;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Button button1;
    }
}