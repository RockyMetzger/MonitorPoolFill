namespace MonitorPoolFill
{
    partial class MonitorPoolFill
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
            this.rtb_PoolFill = new System.Windows.Forms.RichTextBox();
            this.btn_Reset = new System.Windows.Forms.Button();
            this.tb_ErrorList = new System.Windows.Forms.TextBox();
            this.pnl_Status = new System.Windows.Forms.Panel();
            this.lbl_FlowStatus = new System.Windows.Forms.Label();
            this.tb_FlowStarted = new System.Windows.Forms.TextBox();
            this.tb_FlowStopped = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.tb_FlowThisFIll = new System.Windows.Forms.TextBox();
            this.pnl_Status.SuspendLayout();
            this.SuspendLayout();
            // 
            // rtb_PoolFill
            // 
            this.rtb_PoolFill.Location = new System.Drawing.Point(43, 64);
            this.rtb_PoolFill.Name = "rtb_PoolFill";
            this.rtb_PoolFill.ScrollBars = System.Windows.Forms.RichTextBoxScrollBars.ForcedVertical;
            this.rtb_PoolFill.Size = new System.Drawing.Size(795, 545);
            this.rtb_PoolFill.TabIndex = 0;
            this.rtb_PoolFill.Text = "";
            // 
            // btn_Reset
            // 
            this.btn_Reset.Location = new System.Drawing.Point(43, 637);
            this.btn_Reset.Name = "btn_Reset";
            this.btn_Reset.Size = new System.Drawing.Size(75, 23);
            this.btn_Reset.TabIndex = 1;
            this.btn_Reset.Text = "Reset";
            this.btn_Reset.UseVisualStyleBackColor = true;
            this.btn_Reset.Click += new System.EventHandler(this.btn_Reset_Click);
            // 
            // tb_ErrorList
            // 
            this.tb_ErrorList.Location = new System.Drawing.Point(291, 623);
            this.tb_ErrorList.Name = "tb_ErrorList";
            this.tb_ErrorList.Size = new System.Drawing.Size(428, 20);
            this.tb_ErrorList.TabIndex = 2;
            // 
            // pnl_Status
            // 
            this.pnl_Status.BackColor = System.Drawing.Color.LightGreen;
            this.pnl_Status.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.pnl_Status.Controls.Add(this.lbl_FlowStatus);
            this.pnl_Status.Location = new System.Drawing.Point(43, 12);
            this.pnl_Status.Name = "pnl_Status";
            this.pnl_Status.Size = new System.Drawing.Size(98, 33);
            this.pnl_Status.TabIndex = 4;
            // 
            // lbl_FlowStatus
            // 
            this.lbl_FlowStatus.AutoSize = true;
            this.lbl_FlowStatus.Location = new System.Drawing.Point(17, 9);
            this.lbl_FlowStatus.Name = "lbl_FlowStatus";
            this.lbl_FlowStatus.Size = new System.Drawing.Size(46, 13);
            this.lbl_FlowStatus.TabIndex = 0;
            this.lbl_FlowStatus.Text = "Flow Off";
            // 
            // tb_FlowStarted
            // 
            this.tb_FlowStarted.Location = new System.Drawing.Point(206, 27);
            this.tb_FlowStarted.Name = "tb_FlowStarted";
            this.tb_FlowStarted.Size = new System.Drawing.Size(160, 20);
            this.tb_FlowStarted.TabIndex = 5;
            // 
            // tb_FlowStopped
            // 
            this.tb_FlowStopped.Location = new System.Drawing.Point(413, 27);
            this.tb_FlowStopped.Name = "tb_FlowStopped";
            this.tb_FlowStopped.Size = new System.Drawing.Size(138, 20);
            this.tb_FlowStopped.TabIndex = 6;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(206, 11);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(80, 13);
            this.label1.TabIndex = 7;
            this.label1.Text = "Flow Start Time";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(410, 12);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(80, 13);
            this.label2.TabIndex = 8;
            this.label2.Text = "Flow Stop Time";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(578, 12);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(67, 13);
            this.label3.TabIndex = 10;
            this.label3.Text = "Flow Gallons";
            // 
            // tb_FlowThisFIll
            // 
            this.tb_FlowThisFIll.Location = new System.Drawing.Point(581, 27);
            this.tb_FlowThisFIll.Name = "tb_FlowThisFIll";
            this.tb_FlowThisFIll.Size = new System.Drawing.Size(138, 20);
            this.tb_FlowThisFIll.TabIndex = 9;
            // 
            // MonitorPoolFill
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(892, 694);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.tb_FlowThisFIll);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.tb_FlowStopped);
            this.Controls.Add(this.tb_FlowStarted);
            this.Controls.Add(this.pnl_Status);
            this.Controls.Add(this.tb_ErrorList);
            this.Controls.Add(this.btn_Reset);
            this.Controls.Add(this.rtb_PoolFill);
            this.Name = "MonitorPoolFill";
            this.Text = "Monitor Pool Fill";
            this.pnl_Status.ResumeLayout(false);
            this.pnl_Status.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.RichTextBox rtb_PoolFill;
        private System.Windows.Forms.Button btn_Reset;
        private System.Windows.Forms.TextBox tb_ErrorList;
        private System.Windows.Forms.Panel pnl_Status;
        private System.Windows.Forms.Label lbl_FlowStatus;
        private System.Windows.Forms.TextBox tb_FlowStarted;
        private System.Windows.Forms.TextBox tb_FlowStopped;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.TextBox tb_FlowThisFIll;
    }
}

