namespace APB_QR_server
{
    partial class FormClosePayment
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
            this.buttonCancelPayment = new System.Windows.Forms.Button();
            this.richTextBox1 = new System.Windows.Forms.RichTextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.labelStatusState = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // buttonCancelPayment
            // 
            this.buttonCancelPayment.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.buttonCancelPayment.Font = new System.Drawing.Font("Microsoft Sans Serif", 18F);
            this.buttonCancelPayment.Location = new System.Drawing.Point(12, 155);
            this.buttonCancelPayment.Name = "buttonCancelPayment";
            this.buttonCancelPayment.Size = new System.Drawing.Size(413, 54);
            this.buttonCancelPayment.TabIndex = 0;
            this.buttonCancelPayment.Text = "Отменить платёж";
            this.buttonCancelPayment.UseVisualStyleBackColor = true;
            // 
            // richTextBox1
            // 
            this.richTextBox1.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.richTextBox1.Font = new System.Drawing.Font("Microsoft Sans Serif", 18F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.richTextBox1.Location = new System.Drawing.Point(12, 49);
            this.richTextBox1.Name = "richTextBox1";
            this.richTextBox1.ReadOnly = true;
            this.richTextBox1.Size = new System.Drawing.Size(413, 100);
            this.richTextBox1.TabIndex = 1;
            this.richTextBox1.Text = "Отменить платеж ";
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Font = new System.Drawing.Font("Microsoft Sans Serif", 15F);
            this.label1.Location = new System.Drawing.Point(7, 9);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(119, 25);
            this.label1.TabIndex = 2;
            this.label1.Text = "Состояние:";
            // 
            // labelStatusState
            // 
            this.labelStatusState.AutoSize = true;
            this.labelStatusState.Font = new System.Drawing.Font("Microsoft Sans Serif", 15F);
            this.labelStatusState.Location = new System.Drawing.Point(132, 9);
            this.labelStatusState.Name = "labelStatusState";
            this.labelStatusState.Size = new System.Drawing.Size(64, 25);
            this.labelStatusState.TabIndex = 3;
            this.labelStatusState.Text = "label2";
            // 
            // FormClosePayment
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(437, 222);
            this.ControlBox = false;
            this.Controls.Add(this.labelStatusState);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.richTextBox1);
            this.Controls.Add(this.buttonCancelPayment);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "FormClosePayment";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.TopMost = true;
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.FormClosePayment_FormClosing);
            this.Load += new System.EventHandler(this.FormClosePayment_Load);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion
        private System.Windows.Forms.RichTextBox richTextBox1;
        public System.Windows.Forms.Button buttonCancelPayment;
        private System.Windows.Forms.Label label1;
        public System.Windows.Forms.Label labelStatusState;
    }
}