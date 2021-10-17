namespace grace
{
    partial class frm_Terminal
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(frm_Terminal));
            this.btn_run = new System.Windows.Forms.Button();
            this.txt_console = new System.Windows.Forms.TextBox();
            this.cmb_command = new System.Windows.Forms.ComboBox();
            this.lbl_trademark = new System.Windows.Forms.Label();
            this.label1 = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // btn_run
            // 
            this.btn_run.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btn_run.BackColor = System.Drawing.Color.Black;
            this.btn_run.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
            this.btn_run.ForeColor = System.Drawing.Color.White;
            this.btn_run.Location = new System.Drawing.Point(523, 1);
            this.btn_run.Name = "btn_run";
            this.btn_run.Size = new System.Drawing.Size(82, 25);
            this.btn_run.TabIndex = 1;
            this.btn_run.Text = "run";
            this.btn_run.UseVisualStyleBackColor = false;
            this.btn_run.Click += new System.EventHandler(this.button1_Click);
            // 
            // txt_console
            // 
            this.txt_console.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.txt_console.BackColor = System.Drawing.Color.Black;
            this.txt_console.Font = new System.Drawing.Font("Arial Rounded MT Bold", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.txt_console.ForeColor = System.Drawing.Color.White;
            this.txt_console.Location = new System.Drawing.Point(0, 25);
            this.txt_console.Multiline = true;
            this.txt_console.Name = "txt_console";
            this.txt_console.ReadOnly = true;
            this.txt_console.ScrollBars = System.Windows.Forms.ScrollBars.Both;
            this.txt_console.Size = new System.Drawing.Size(604, 398);
            this.txt_console.TabIndex = 2;
            this.txt_console.WordWrap = false;
            this.txt_console.KeyDown += new System.Windows.Forms.KeyEventHandler(this.txt_console_KeyDown);
            // 
            // cmb_command
            // 
            this.cmb_command.AllowDrop = true;
            this.cmb_command.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.cmb_command.BackColor = System.Drawing.Color.Black;
            this.cmb_command.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.cmb_command.ForeColor = System.Drawing.Color.White;
            this.cmb_command.FormattingEnabled = true;
            this.cmb_command.Location = new System.Drawing.Point(0, 3);
            this.cmb_command.Name = "cmb_command";
            this.cmb_command.Size = new System.Drawing.Size(522, 21);
            this.cmb_command.TabIndex = 0;
            this.cmb_command.DragDrop += new System.Windows.Forms.DragEventHandler(this.cmb_command_DragDrop);
            this.cmb_command.Enter += new System.EventHandler(this.cmb_command_Enter);
            this.cmb_command.Leave += new System.EventHandler(this.cmb_command_Leave);
            // 
            // lbl_trademark
            // 
            this.lbl_trademark.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.lbl_trademark.AutoSize = true;
            this.lbl_trademark.BackColor = System.Drawing.Color.Black;
            this.lbl_trademark.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lbl_trademark.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(224)))), ((int)(((byte)(224)))), ((int)(((byte)(224)))));
            this.lbl_trademark.Location = new System.Drawing.Point(542, 372);
            this.lbl_trademark.Name = "lbl_trademark";
            this.lbl_trademark.Size = new System.Drawing.Size(39, 13);
            this.lbl_trademark.TabIndex = 3;
            this.lbl_trademark.Text = "grace";
            // 
            // label1
            // 
            this.label1.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.label1.AutoSize = true;
            this.label1.BackColor = System.Drawing.Color.Black;
            this.label1.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Italic, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label1.ForeColor = System.Drawing.Color.Silver;
            this.label1.Location = new System.Drawing.Point(494, 385);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(87, 13);
            this.label1.TabIndex = 4;
            this.label1.Text = "Terminal console";
            // 
            // frm_Terminal
            // 
            this.AcceptButton = this.btn_run;
            this.AllowDrop = true;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.Black;
            this.ClientSize = new System.Drawing.Size(605, 421);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.lbl_trademark);
            this.Controls.Add(this.cmb_command);
            this.Controls.Add(this.txt_console);
            this.Controls.Add(this.btn_run);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "frm_Terminal";
            this.Text = "Terminal";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.frm_Terminal_FormClosing);
            this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.frm_Terminal_FormClosed);
            this.Load += new System.EventHandler(this.frm_terminal_Load);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button btn_run;
        private System.Windows.Forms.TextBox txt_console;
        private System.Windows.Forms.ComboBox cmb_command;
        private System.Windows.Forms.Label lbl_trademark;
        private System.Windows.Forms.Label label1;
    }
}

