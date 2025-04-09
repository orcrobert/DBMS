using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Xml;

namespace lab_1
{
    public partial class UpdateMembersTableForm : Form
    {
        private string operation;
        private string tableName;
        private int? recordId;
        private int? foreignKeyId;
        private XmlNode tableConfig;
        private string primaryKeyColumn;

        private SqlConnection conn;

        public UpdateMembersTableForm(SqlConnection connection, string tableName, string operation, int? recordId = null, int? foreignKeyId = null)
        {
            InitializeComponent();
            this.conn = connection;
            this.tableName = tableName;
            this.operation = operation;
            this.recordId = recordId;
            this.foreignKeyId = foreignKeyId;
            this.Text = $"{operation.First().ToString().ToUpper() + operation.Substring(1)} {GetSingularTableName(tableName)}";
            UpdateButtonFormText();
            this.Load += UpdateGenericEntityForm_Load;
        }

        private string GetSingularTableName(string pluralTableName)
        {
            if (pluralTableName.ToLower() == "members") return "Member";
            if (pluralTableName.ToLower() == "albums") return "Album";
            return pluralTableName;
        }

        private void UpdateGenericEntityForm_Load(object sender, EventArgs e)
        {
            try
            {
                XmlDocument config = new XmlDocument();
                config.Load("MasterDetailConfig.xml");
                string scenarioName = (this.Owner as MainForm)?.GetCurrentScenarioName();
                if (string.IsNullOrEmpty(scenarioName))
                {
                    MessageBox.Show("Could not determine the current scenario.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    this.Close();
                    return;
                }

                string tablePath = $"Configuration/Scenario[@name='{scenarioName}']/*[Name='{tableName}']";
                tableConfig = config.SelectSingleNode(tablePath);

                if (tableConfig != null)
                {
                    primaryKeyColumn = tableConfig.SelectSingleNode("PrimaryKeyColumn")?.InnerText;
                    XmlNode columnsNode = tableConfig.SelectSingleNode("Columns");
                    if (columnsNode != null)
                    {
                        bool isMember = tableName.ToLower() == "members";

                        Label instrumentLabel = this.Controls.OfType<Label>().FirstOrDefault(lbl => lbl.Name == "label2");
                        TextBox instrumentTextBox = this.Controls.OfType<TextBox>().FirstOrDefault(txt => txt.Name == "instrumentTextBox");
                        Label releaseDateLabel = this.Controls.OfType<Label>().FirstOrDefault(lbl => lbl.Name == "releaseDateLabel");
                        DateTimePicker releaseDatePicker = this.Controls.OfType<DateTimePicker>().FirstOrDefault(picker => picker.Name == "releaseDatePicker");
                        Label genreLabel = this.Controls.OfType<Label>().FirstOrDefault(lbl => lbl.Name == "genreLabel");
                        TextBox genreTextBox = this.Controls.OfType<TextBox>().FirstOrDefault(txt => txt.Name == "genreTextBox");
                        Label nameLabel = this.Controls.OfType<Label>().FirstOrDefault(lbl => lbl.Name == "label1");

                        if (isMember)
                        {
                            nameLabel.Text = "Name";
                            if (instrumentLabel != null) instrumentLabel.Visible = true;
                            if (instrumentTextBox != null) instrumentTextBox.Visible = true;
                            if (releaseDateLabel != null) releaseDateLabel.Visible = false;
                            if (releaseDatePicker != null) releaseDatePicker.Visible = false;
                            if (genreLabel != null) genreLabel.Visible = false;
                            if (genreTextBox != null) genreTextBox.Visible = false;
                        }
                        else if (tableName.ToLower() == "albums")
                        {
                            nameLabel.Text = "Album Name";
                            if (instrumentLabel != null) instrumentLabel.Visible = false;
                            if (instrumentTextBox != null) instrumentTextBox.Visible = false;
                            if (releaseDateLabel != null) releaseDateLabel.Visible = true;
                            if (releaseDatePicker != null) releaseDatePicker.Visible = true;
                            if (genreLabel != null) genreLabel.Visible = true;
                            if (genreTextBox != null) genreTextBox.Visible = true;
                        }
                    }

                    if (operation == "update" && recordId.HasValue)
                    {
                        LoadExistingData();
                    }
                }
                else
                {
                    MessageBox.Show($"Configuration for table '{tableName}' not found.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    this.Close();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading configuration: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                this.Close();
            }
        }

        private void LoadExistingData()
        {
            string selectQuery = tableConfig.SelectSingleNode("SelectSingleQuery")?.InnerText;
            string pkColumn = tableConfig.SelectSingleNode("PrimaryKeyColumn")?.InnerText;

            if (!string.IsNullOrEmpty(selectQuery) && !string.IsNullOrEmpty(pkColumn) && recordId.HasValue)
            {
                string query = selectQuery;
                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@Id", recordId.Value);
                    try
                    {
                        if (conn.State != ConnectionState.Open) conn.Open();
                        using (SqlDataReader reader = cmd.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                for (int i = 0; i < reader.FieldCount; i++)
                                {
                                    string columnName = reader.GetName(i);
                                    if (tableName.ToLower() == "members")
                                    {
                                        if (columnName == "MemberName")
                                        {
                                            nameTextBox.Text = reader[columnName].ToString();
                                        }
                                        else if (columnName == "MemberInstrument")
                                        {
                                            instrumentTextBox.Text = reader[columnName].ToString();
                                        }
                                    }
                                    else if (tableName.ToLower() == "albums")
                                    {
                                        if (columnName == "AlbumName")
                                        {
                                            nameTextBox.Text = reader[columnName].ToString();
                                        }
                                        else if (columnName == "ReleaseDate")
                                        {
                                            if (reader[columnName] != DBNull.Value)
                                            {
                                                DateTimePicker releaseDatePicker = this.Controls.OfType<DateTimePicker>().FirstOrDefault(picker => picker.Name == "releaseDatePicker");
                                                if (releaseDatePicker != null)
                                                {
                                                    releaseDatePicker.Value = Convert.ToDateTime(reader[columnName]);
                                                }
                                            }
                                        }
                                        else if (columnName == "Genre")
                                        {
                                            TextBox genreTextBox = this.Controls.OfType<TextBox>().FirstOrDefault(txt => txt.Name == "genreTextBox");
                                            if (genreTextBox != null)
                                            {
                                                genreTextBox.Text = reader[columnName].ToString();
                                            }
                                        }
                                        else if (columnName == "BandId" && foreignKeyId == null)
                                        {
                                            foreignKeyId = Convert.ToInt32(reader[columnName]);
                                        }
                                    }
                                }
                            }
                            else
                            {
                                MessageBox.Show($"Record with ID {recordId.Value} not found in table '{tableName}'.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                                this.Close();
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Error fetching data for update: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        this.Close();
                    }
                    finally
                    {
                        if (conn.State == ConnectionState.Open) conn.Close();
                    }
                }
            }
        }

        private void UpdateButtonFormText()
        {
            string entityName = GetSingularTableName(tableName);
            if (this.operation == "add")
            {
                updateButton.Text = $"Add {entityName}";
            }
            else if (this.operation == "update")
            {
                updateButton.Text = $"Update {entityName}";
            }
            else if (this.operation == "delete")
            {
                updateButton.Text = $"Delete {entityName}";
            }
        }

        private void updateButton_Click(object sender, EventArgs e)
        {
            if (tableConfig == null) return;

            string commandText = "";
            SqlCommand command = new SqlCommand();
            command.Connection = conn;
            Dictionary<string, object> columnValues = new Dictionary<string, object>();
            XmlNode columnsNode = tableConfig.SelectSingleNode("Columns");

            if (columnsNode != null)
            {
                foreach (XmlNode columnNode in columnsNode.SelectNodes("Column"))
                {
                    string columnName = columnNode.Attributes["name"]?.Value;
                    string isForeignKey = columnNode.Attributes["isForeignKey"]?.Value;

                    if (operation == "add" && isForeignKey?.ToLower() == "true")
                    {
                        continue;
                    }

                    if (tableName.ToLower() == "members")
                    {
                        if (columnName == "MemberName")
                        {
                            columnValues[columnName] = nameTextBox.Text;
                        }
                        else if (columnName == "MemberInstrument")
                        {
                            columnValues[columnName] = instrumentTextBox.Text;
                        }
                    }
                    else if (tableName.ToLower() == "albums")
                    {
                        if (columnName == "AlbumName")
                        {
                            columnValues[columnName] = nameTextBox.Text;
                        }
                        else if (columnName == "ReleaseDate")
                        {
                            DateTimePicker releaseDatePicker = this.Controls.OfType<DateTimePicker>().FirstOrDefault(picker => picker.Name == "releaseDatePicker");
                            if (releaseDatePicker != null)
                            {
                                columnValues[columnName] = releaseDatePicker.Value;
                            }
                        }
                        else if (columnName == "Genre")
                        {
                            TextBox genreTextBox = this.Controls.OfType<TextBox>().FirstOrDefault(txt => txt.Name == "genreTextBox");
                            if (genreTextBox != null)
                            {
                                columnValues[columnName] = genreTextBox.Text;
                            }
                        }
                        else if (columnName == "BandId" && operation == "update")
                        {
                            continue;
                        }
                    }
                }
            }

            try
            {
                if (operation == "add")
{
    string columns = string.Join(", ", columnValues.Keys);
    string parameters = string.Join(", ", columnValues.Keys.Select(key => $"@{key}"));
    commandText = $"INSERT INTO {tableName} ({columns}) VALUES ({parameters})";

    string foreignKeyColumnConfig = tableConfig.SelectSingleNode("ForeignKeyColumn")?.InnerText;

    if (foreignKeyId.HasValue && !string.IsNullOrEmpty(foreignKeyColumnConfig))
    {
        if (tableName.ToLower() == "albums" && foreignKeyColumnConfig.ToLower() == "bandid")
        {
            string foreignKeyColumnName = foreignKeyColumnConfig;
            commandText = $"INSERT INTO {tableName} ({foreignKeyColumnName}, {columns}) VALUES (@{foreignKeyColumnName}, {parameters})";
            command.Parameters.AddWithValue($"@{foreignKeyColumnName}", foreignKeyId.Value);
        }
        else if (tableName.ToLower() == "members" && foreignKeyColumnConfig.ToLower() == "bandid")
        {
            string foreignKeyColumnName = foreignKeyColumnConfig;
            commandText = $"INSERT INTO {tableName} ({foreignKeyColumnName}, {columns}) VALUES (@{foreignKeyColumnName}, {parameters})";
            command.Parameters.AddWithValue($"@{foreignKeyColumnName}", foreignKeyId.Value);
        }
    }
    else if (tableName.ToLower() == "albums" && foreignKeyColumnConfig?.ToLower() == "bandid")
    {
        MessageBox.Show("BandId is required for adding a new album.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
        return;
    }
    else if (tableName.ToLower() == "members" && foreignKeyColumnConfig?.ToLower() == "bandid")
    {
        MessageBox.Show("BandId is required for adding a new member.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
        return;
    }

    foreach (var pair in columnValues)
    {
        command.Parameters.AddWithValue($"@{pair.Key}", pair.Value);
    }
}
                else if (operation == "update" && recordId.HasValue && !string.IsNullOrEmpty(primaryKeyColumn))
                {
                    string updates = string.Join(", ", columnValues.Keys.Select(key => $"{key} = @{key}"));
                    commandText = $"UPDATE {tableName} SET {updates} WHERE {primaryKeyColumn} = @Id";
                    command.Parameters.AddWithValue("@Id", recordId.Value);
                    foreach (var pair in columnValues)
                    {
                        command.Parameters.AddWithValue($"@{pair.Key}", pair.Value);
                    }
                }
                else if (operation == "delete" && recordId.HasValue && !string.IsNullOrEmpty(primaryKeyColumn))
                {
                    commandText = $"DELETE FROM {tableName} WHERE {primaryKeyColumn} = @Id";
                    command.Parameters.AddWithValue("@Id", recordId.Value);
                }

                MessageBox.Show(commandText);

                if (conn.State != ConnectionState.Open) conn.Open();
                command.CommandText = commandText;
                int rowsAffected = command.ExecuteNonQuery();
                if (rowsAffected > 0)
                {
                    MessageBox.Show($"{GetSingularTableName(tableName)} {operation}d successfully.");
                    this.DialogResult = DialogResult.OK;
                    this.Close();
                }
                else
                {
                    MessageBox.Show("Error saving data.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Database error: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                if (conn.State == ConnectionState.Open) conn.Close();
            }
        }
    }
}