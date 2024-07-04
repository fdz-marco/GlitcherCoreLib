namespace glitcher.core
{
    partial class LogViewer
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
            lv_Log = new ListView();
            lb_lTitle = new Label();
            btn_ClearAll = new Button();
            btn_Copy = new Button();
            tableLayoutPanel1 = new TableLayoutPanel();
            tableLayoutPanel2 = new TableLayoutPanel();
            tableLayoutPanel1.SuspendLayout();
            tableLayoutPanel2.SuspendLayout();
            SuspendLayout();
            // 
            // lv_Log
            // 
            lv_Log.Dock = DockStyle.Fill;
            lv_Log.GridLines = true;
            lv_Log.HeaderStyle = ColumnHeaderStyle.Nonclickable;
            lv_Log.Location = new Point(3, 18);
            lv_Log.Name = "lv_Log";
            lv_Log.Size = new Size(1294, 637);
            lv_Log.TabIndex = 0;
            lv_Log.UseCompatibleStateImageBehavior = false;
            lv_Log.View = View.Details;
            // 
            // lb_lTitle
            // 
            lb_lTitle.AutoSize = true;
            lb_lTitle.Dock = DockStyle.Fill;
            lb_lTitle.Location = new Point(3, 0);
            lb_lTitle.Name = "lb_lTitle";
            lb_lTitle.Size = new Size(1294, 15);
            lb_lTitle.TabIndex = 1;
            lb_lTitle.Text = "Logged Events";
            // 
            // btn_ClearAll
            // 
            btn_ClearAll.Dock = DockStyle.Fill;
            btn_ClearAll.Location = new Point(978, 3);
            btn_ClearAll.Name = "btn_ClearAll";
            btn_ClearAll.Size = new Size(319, 28);
            btn_ClearAll.TabIndex = 2;
            btn_ClearAll.Text = "Clear All";
            btn_ClearAll.UseVisualStyleBackColor = true;
            btn_ClearAll.Click += btn_ClearAll_Click;
            // 
            // btn_Copy
            // 
            btn_Copy.Dock = DockStyle.Fill;
            btn_Copy.Location = new Point(653, 3);
            btn_Copy.Name = "btn_Copy";
            btn_Copy.Size = new Size(319, 28);
            btn_Copy.TabIndex = 3;
            btn_Copy.Text = "Copy";
            btn_Copy.UseVisualStyleBackColor = true;
            btn_Copy.Click += btn_Copy_Click;
            // 
            // tableLayoutPanel1
            // 
            tableLayoutPanel1.ColumnCount = 1;
            tableLayoutPanel1.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            tableLayoutPanel1.Controls.Add(tableLayoutPanel2, 0, 2);
            tableLayoutPanel1.Controls.Add(lb_lTitle, 0, 0);
            tableLayoutPanel1.Controls.Add(lv_Log, 0, 1);
            tableLayoutPanel1.Dock = DockStyle.Fill;
            tableLayoutPanel1.Location = new Point(0, 0);
            tableLayoutPanel1.Name = "tableLayoutPanel1";
            tableLayoutPanel1.RowCount = 3;
            tableLayoutPanel1.RowStyles.Add(new RowStyle());
            tableLayoutPanel1.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
            tableLayoutPanel1.RowStyles.Add(new RowStyle());
            tableLayoutPanel1.Size = new Size(1300, 692);
            tableLayoutPanel1.TabIndex = 4;
            // 
            // tableLayoutPanel2
            // 
            tableLayoutPanel2.ColumnCount = 3;
            tableLayoutPanel2.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50F));
            tableLayoutPanel2.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 25F));
            tableLayoutPanel2.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 25F));
            tableLayoutPanel2.Controls.Add(btn_ClearAll, 2, 0);
            tableLayoutPanel2.Controls.Add(btn_Copy, 1, 0);
            tableLayoutPanel2.Dock = DockStyle.Fill;
            tableLayoutPanel2.Location = new Point(0, 658);
            tableLayoutPanel2.Margin = new Padding(0);
            tableLayoutPanel2.Name = "tableLayoutPanel2";
            tableLayoutPanel2.RowCount = 1;
            tableLayoutPanel2.RowStyles.Add(new RowStyle());
            tableLayoutPanel2.Size = new Size(1300, 34);
            tableLayoutPanel2.TabIndex = 5;
            // 
            // LogViewer
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(1300, 692);
            Controls.Add(tableLayoutPanel1);
            Name = "LogViewer";
            ShowIcon = false;
            ShowInTaskbar = false;
            StartPosition = FormStartPosition.CenterScreen;
            Text = "LogViewer";
            tableLayoutPanel1.ResumeLayout(false);
            tableLayoutPanel1.PerformLayout();
            tableLayoutPanel2.ResumeLayout(false);
            ResumeLayout(false);
        }

        #endregion

        private ListView lv_Log;
        private Label lb_lTitle;
        private Button btn_ClearAll;
        private Button btn_Copy;
        private TableLayoutPanel tableLayoutPanel1;
        private TableLayoutPanel tableLayoutPanel2;
    }
}