namespace lab_1
{
    partial class UpdateBandsTableForm
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
            bandNameTb = new TextBox();
            genreTb = new TextBox();
            themeTb = new TextBox();
            label1 = new Label();
            label2 = new Label();
            label3 = new Label();
            addBandButton = new Button();
            SuspendLayout();
            // 
            // bandNameTb
            // 
            bandNameTb.Location = new Point(144, 65);
            bandNameTb.Name = "bandNameTb";
            bandNameTb.Size = new Size(160, 23);
            bandNameTb.TabIndex = 0;
            // 
            // genreTb
            // 
            genreTb.Location = new Point(144, 115);
            genreTb.Name = "genreTb";
            genreTb.Size = new Size(160, 23);
            genreTb.TabIndex = 1;
            // 
            // themeTb
            // 
            themeTb.Location = new Point(144, 165);
            themeTb.Name = "themeTb";
            themeTb.Size = new Size(160, 23);
            themeTb.TabIndex = 2;
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Location = new Point(69, 73);
            label1.Name = "label1";
            label1.Size = new Size(69, 15);
            label1.TabIndex = 3;
            label1.Text = "Band Name";
            // 
            // label2
            // 
            label2.AutoSize = true;
            label2.Location = new Point(69, 123);
            label2.Name = "label2";
            label2.Size = new Size(38, 15);
            label2.TabIndex = 4;
            label2.Text = "Genre";
            // 
            // label3
            // 
            label3.AutoSize = true;
            label3.Location = new Point(69, 173);
            label3.Name = "label3";
            label3.Size = new Size(48, 15);
            label3.TabIndex = 5;
            label3.Text = "Themes";
            // 
            // addBandButton
            // 
            addBandButton.Location = new Point(161, 248);
            addBandButton.Name = "addBandButton";
            addBandButton.Size = new Size(75, 23);
            addBandButton.TabIndex = 6;
            addBandButton.Text = "Add Band";
            addBandButton.UseVisualStyleBackColor = true;
            addBandButton.Click += addBandButton_Click;
            // 
            // UpdateBandsTableForm
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(391, 344);
            Controls.Add(addBandButton);
            Controls.Add(label3);
            Controls.Add(label2);
            Controls.Add(label1);
            Controls.Add(themeTb);
            Controls.Add(genreTb);
            Controls.Add(bandNameTb);
            Name = "UpdateBandsTableForm";
            Text = "Add Band";
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private TextBox bandNameTb;
        private TextBox genreTb;
        private TextBox themeTb;
        private Label label1;
        private Label label2;
        private Label label3;
        private Button addBandButton;
    }
}