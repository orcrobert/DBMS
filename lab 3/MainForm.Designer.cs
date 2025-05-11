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
            label1 = new Label();
            label2 = new Label();
            addMemberButton = new Button();
            deleteMemberButton = new Button();
            updateMemberButton = new Button();
            scenarioComboBox = new ComboBox();
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
            addBandButton.Location = new Point(12, 371);
            addBandButton.Name = "addBandButton";
            addBandButton.Size = new Size(75, 23);
            addBandButton.TabIndex = 3;
            addBandButton.Text = "Add Band";
            addBandButton.UseVisualStyleBackColor = true;
            addBandButton.Click += addBandButton_Click;
            // 
            // deleteBandButton
            // 
            deleteBandButton.Location = new Point(116, 371);
            deleteBandButton.Name = "deleteBandButton";
            deleteBandButton.Size = new Size(87, 23);
            deleteBandButton.TabIndex = 4;
            deleteBandButton.Text = "Delete Band";
            deleteBandButton.UseVisualStyleBackColor = true;
            deleteBandButton.Click += deleteBandButton_Click;
            // 
            // updateBandButton
            // 
            updateBandButton.Location = new Point(230, 371);
            updateBandButton.Name = "updateBandButton";
            updateBandButton.Size = new Size(87, 23);
            updateBandButton.TabIndex = 5;
            updateBandButton.Text = "Update Band";
            updateBandButton.UseVisualStyleBackColor = true;
            updateBandButton.Click += updateBandButton_Click;
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Font = new Font("Segoe UI", 9.75F, FontStyle.Bold, GraphicsUnit.Point, 0);
            label1.Location = new Point(12, 344);
            label1.Name = "label1";
            label1.Size = new Size(111, 17);
            label1.TabIndex = 6;
            label1.Text = "Band Operations";
            // 
            // label2
            // 
            label2.AutoSize = true;
            label2.Font = new Font("Segoe UI", 9.75F, FontStyle.Bold, GraphicsUnit.Point, 0);
            label2.Location = new Point(12, 416);
            label2.Name = "label2";
            label2.Size = new Size(131, 17);
            label2.TabIndex = 7;
            label2.Text = "Member Operations";
            // 
            // addMemberButton
            // 
            addMemberButton.Location = new Point(12, 446);
            addMemberButton.Name = "addMemberButton";
            addMemberButton.Size = new Size(86, 23);
            addMemberButton.TabIndex = 8;
            addMemberButton.Text = "Add Member";
            addMemberButton.UseVisualStyleBackColor = true;
            addMemberButton.Click += addMemberButton_Click;
            // 
            // deleteMemberButton
            // 
            deleteMemberButton.Location = new Point(116, 446);
            deleteMemberButton.Name = "deleteMemberButton";
            deleteMemberButton.Size = new Size(97, 23);
            deleteMemberButton.TabIndex = 9;
            deleteMemberButton.Text = "Delete Member";
            deleteMemberButton.UseVisualStyleBackColor = true;
            deleteMemberButton.Click += deleteMemberButton_Click;
            // 
            // updateMemberButton
            // 
            updateMemberButton.Location = new Point(230, 446);
            updateMemberButton.Name = "updateMemberButton";
            updateMemberButton.Size = new Size(97, 23);
            updateMemberButton.TabIndex = 10;
            updateMemberButton.Text = "Update Member";
            updateMemberButton.UseVisualStyleBackColor = true;
            updateMemberButton.Click += updateMemberButton_Click;
            // 
            // scenarioComboBox
            // 
            scenarioComboBox.FormattingEnabled = true;
            scenarioComboBox.Location = new Point(429, 12);
            scenarioComboBox.Name = "scenarioComboBox";
            scenarioComboBox.Size = new Size(121, 23);
            scenarioComboBox.TabIndex = 11;
            scenarioComboBox.SelectedIndexChanged += scenarioComboBox_SelectedIndexChanged;
            // 
            // MainForm
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(568, 498);
            Controls.Add(scenarioComboBox);
            Controls.Add(updateMemberButton);
            Controls.Add(deleteMemberButton);
            Controls.Add(addMemberButton);
            Controls.Add(label2);
            Controls.Add(label1);
            Controls.Add(updateBandButton);
            Controls.Add(deleteBandButton);
            Controls.Add(addBandButton);
            Controls.Add(showMembersButton);
            Controls.Add(showBandsButton);
            Controls.Add(dataGridView1);
            Name = "MainForm";
            Text = "Form1";
            Load += Form1_Load;
            ((System.ComponentModel.ISupportInitialize)dataGridView1).EndInit();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private DataGridView dataGridView1;
        private Button showBandsButton;
        private Button showMembersButton;
        private Button addBandButton;
        private Button deleteBandButton;
        private Button updateBandButton;
        private Label label1;
        private Label label2;
        private Button addMemberButton;
        private Button deleteMemberButton;
        private Button updateMemberButton;
        private ComboBox scenarioComboBox;
    }
}
