namespace lab_1
{
    partial class MainForm
    {
        /// <summary>
        ///  Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        ///  Clean up any resources being used.
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
        ///  Required method for Designer support - do not modify
        ///  the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            dataGridView1 = new DataGridView();
            showBandsButton = new Button();
            showMembersButton = new Button();
            addBandButton = new Button();
            deleteBandButton = new Button();
            updateBandButton = new Button();
            ((System.ComponentModel.ISupportInitialize)dataGridView1).BeginInit();
            SuspendLayout();
            // 
            // dataGridView1
            // 
            dataGridView1.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            dataGridView1.Location = new Point(12, 41);
            dataGridView1.Name = "dataGridView1";
            dataGridView1.Size = new Size(538, 291);
            dataGridView1.TabIndex = 0;
            dataGridView1.SelectionChanged += dataGridView1_SelectionChanged;
            // 
            // showBandsButton
            // 
            showBandsButton.Location = new Point(12, 12);
            showBandsButton.Name = "showBandsButton";
            showBandsButton.Size = new Size(108, 23);
            showBandsButton.TabIndex = 1;
            showBandsButton.Text = "Show Bands";
            showBandsButton.UseVisualStyleBackColor = true;
            showBandsButton.Click += showBandsButton_Click;
            // 
            // showMembersButton
            // 
            showMembersButton.Location = new Point(137, 12);
            showMembersButton.Name = "showMembersButton";
            showMembersButton.Size = new Size(111, 23);
            showMembersButton.TabIndex = 2;
            showMembersButton.Text = "Show Members";
            showMembersButton.UseVisualStyleBackColor = true;
            showMembersButton.Click += showMembersButton_Click;
            // 
            // addBandButton
            // 
            addBandButton.Location = new Point(12, 352);
            addBandButton.Name = "addBandButton";
            addBandButton.Size = new Size(75, 23);
            addBandButton.TabIndex = 3;
            addBandButton.Text = "Add Band";
            addBandButton.UseVisualStyleBackColor = true;
            addBandButton.Click += addBandButton_Click;
            // 
            // deleteBandButton
            // 
            deleteBandButton.Location = new Point(116, 352);
            deleteBandButton.Name = "deleteBandButton";
            deleteBandButton.Size = new Size(87, 23);
            deleteBandButton.TabIndex = 4;
            deleteBandButton.Text = "Delete Band";
            deleteBandButton.UseVisualStyleBackColor = true;
            deleteBandButton.Click += deleteBandButton_Click;
            // 
            // updateBandButton
            // 
            updateBandButton.Location = new Point(230, 352);
            updateBandButton.Name = "updateBandButton";
            updateBandButton.Size = new Size(87, 23);
            updateBandButton.TabIndex = 5;
            updateBandButton.Text = "Update Band";
            updateBandButton.UseVisualStyleBackColor = true;
            updateBandButton.Click += updateBandButton_Click;
            // 
            // Form1
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(568, 498);
            Controls.Add(updateBandButton);
            Controls.Add(deleteBandButton);
            Controls.Add(addBandButton);
            Controls.Add(showMembersButton);
            Controls.Add(showBandsButton);
            Controls.Add(dataGridView1);
            Name = "Form1";
            Text = "Form1";
            Load += Form1_Load;
            ((System.ComponentModel.ISupportInitialize)dataGridView1).EndInit();
            ResumeLayout(false);
        }

        #endregion

        private DataGridView dataGridView1;
        private Button showBandsButton;
        private Button showMembersButton;
        private Button addBandButton;
        private Button deleteBandButton;
        private Button updateBandButton;
    }
}
