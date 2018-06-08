namespace Exporter
{
    partial class FrmWF
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FrmWF));
            this.lblTotalExpCount = new System.Windows.Forms.Label();
            this.lblCurrentItem = new System.Windows.Forms.Label();
            this.lblTotalCount = new System.Windows.Forms.Label();
            this.lblAppdata1 = new System.Windows.Forms.Label();
            this.btnStop = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // lblTotalExpCount
            // 
            this.lblTotalExpCount.AutoSize = true;
            this.lblTotalExpCount.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblTotalExpCount.Location = new System.Drawing.Point(23, 9);
            this.lblTotalExpCount.Name = "lblTotalExpCount";
            this.lblTotalExpCount.Size = new System.Drawing.Size(107, 15);
            this.lblTotalExpCount.TabIndex = 0;
            this.lblTotalExpCount.Text = "Total Export Count";
            // 
            // lblCurrentItem
            // 
            this.lblCurrentItem.AutoSize = true;
            this.lblCurrentItem.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblCurrentItem.Location = new System.Drawing.Point(23, 47);
            this.lblCurrentItem.Name = "lblCurrentItem";
            this.lblCurrentItem.Size = new System.Drawing.Size(59, 15);
            this.lblCurrentItem.TabIndex = 1;
            this.lblCurrentItem.Text = "Appdata1";
            // 
            // lblTotalCount
            // 
            this.lblTotalCount.AutoSize = true;
            this.lblTotalCount.Location = new System.Drawing.Point(181, 11);
            this.lblTotalCount.Name = "lblTotalCount";
            this.lblTotalCount.Size = new System.Drawing.Size(13, 13);
            this.lblTotalCount.TabIndex = 2;
            this.lblTotalCount.Text = "0";
            // 
            // lblAppdata1
            // 
            this.lblAppdata1.AutoSize = true;
            this.lblAppdata1.Location = new System.Drawing.Point(181, 49);
            this.lblAppdata1.Name = "lblAppdata1";
            this.lblAppdata1.Size = new System.Drawing.Size(13, 13);
            this.lblAppdata1.TabIndex = 3;
            this.lblAppdata1.Text = "0";
            // 
            // btnStop
            // 
            this.btnStop.Location = new System.Drawing.Point(138, 85);
            this.btnStop.Name = "btnStop";
            this.btnStop.Size = new System.Drawing.Size(88, 23);
            this.btnStop.TabIndex = 4;
            this.btnStop.Text = "Stop";
            this.btnStop.UseVisualStyleBackColor = true;
            this.btnStop.Click += new System.EventHandler(this.btnStop_Click);
            // 
            // FrmWF
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(372, 120);
            this.Controls.Add(this.btnStop);
            this.Controls.Add(this.lblAppdata1);
            this.Controls.Add(this.lblTotalCount);
            this.Controls.Add(this.lblCurrentItem);
            this.Controls.Add(this.lblTotalExpCount);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MaximizeBox = false;
            this.Name = "FrmWF";
            this.Text = "Exporter";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.FrmWF_FormClosing);
            this.Load += new System.EventHandler(this.Form1_Load);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label lblTotalExpCount;
        private System.Windows.Forms.Label lblCurrentItem;
        private System.Windows.Forms.Label lblTotalCount;
        private System.Windows.Forms.Label lblAppdata1;
        private System.Windows.Forms.Button btnStop;
    }
}

