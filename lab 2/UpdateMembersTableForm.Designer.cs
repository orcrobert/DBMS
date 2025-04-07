namespace lab_1
{
    partial class UpdateMembersTableForm
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
            nameTextBox = new TextBox();
            instrumentTextBox = new TextBox();
            label1 = new Label();
            label2 = new Label();
            updateButton = new Button();
            SuspendLayout();
            // 
            // nameTextBox
            // 
            nameTextBox.Location = new Point(124, 86);
            nameTextBox.Name = "nameTextBox";
            nameTextBox.Size = new Size(100, 23);
            nameTextBox.TabIndex = 0;
            // 
            // instrumentTextBox
            // 
            instrumentTextBox.Location = new Point(124, 145);
            instrumentTextBox.Name = "instrumentTextBox";
            instrumentTextBox.Size = new Size(100, 23);
            instrumentTextBox.TabIndex = 1;
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Location = new Point(44, 94);
            label1.Name = "label1";
            label1.Size = new Size(39, 15);
            label1.TabIndex = 2;
            label1.Text = "Name";
            // 
            // label2
            // 
            label2.AutoSize = true;
            label2.Location = new Point(44, 153);
            label2.Name = "label2";
            label2.Size = new Size(65, 15);
            label2.TabIndex = 3;
            label2.Text = "Instrument";
            // 
            // updateButton
            // 
            updateButton.Location = new Point(98, 222);
            updateButton.Name = "updateButton";
            updateButton.Size = new Size(114, 23);
            updateButton.TabIndex = 4;
            updateButton.Text = "Add Member";
            updateButton.UseVisualStyleBackColor = true;
            updateButton.Click += updateButton_Click;
            // 
            // UpdateMembersTableForm
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(320, 300);
            Controls.Add(updateButton);
            Controls.Add(label2);
            Controls.Add(label1);
            Controls.Add(instrumentTextBox);
            Controls.Add(nameTextBox);
            Name = "UpdateMembersTableForm";
            Text = "UpdateMembersTableForm";
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private TextBox nameTextBox;
        private TextBox instrumentTextBox;
        private Label label1;
        private Label label2;
        private Button updateButton;
    }
}