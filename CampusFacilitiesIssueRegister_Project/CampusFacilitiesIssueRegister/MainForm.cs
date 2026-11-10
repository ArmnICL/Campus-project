using System;
using System.Data;
using System.Drawing;
using System.Windows.Forms;

namespace CampusFacilitiesIssueRegister
{
    public sealed class MainForm : Form
    {
        private readonly IssueManager manager = new IssueManager();
        private readonly IssueRepository repository = new IssueRepository();
        private readonly TextBox txtTitle = new TextBox();
        private readonly TextBox txtLocation = new TextBox();
        private readonly TextBox txtReportedBy = new TextBox();
        private readonly TextBox txtDescription = new TextBox();
        private readonly TextBox txtSearch = new TextBox();
        private readonly ComboBox cmbCategory = new ComboBox();
        private readonly ComboBox cmbPriority = new ComboBox();
        private readonly ComboBox cmbStatus = new ComboBox();
        private readonly DateTimePicker dtpReportedDate = new DateTimePicker();
        private readonly DataGridView dgvIssues = new DataGridView();
        private readonly Label lblSelection = new Label();
        private int selectedIssueId;

        public MainForm()
        {
            Text = "Campus Facilities Issue Register";
            ClientSize = new Size(1100, 690);
            MinimumSize = new Size(1116, 730);
            StartPosition = FormStartPosition.CenterScreen;
            Font = new Font("Segoe UI", 9F);
            BuildInterface();
            LoadIssues();
        }

        private void BuildInterface()
        {
            Label heading = new Label { Text = "Campus Facilities Issue Register", Font = new Font("Segoe UI", 17F, FontStyle.Bold), AutoSize = true, Location = new Point(22, 18) };
            Controls.Add(heading);
            Label sub = new Label { Text = "Record and manage campus maintenance issues", AutoSize = true, ForeColor = Color.DimGray, Location = new Point(25, 58) };
            Controls.Add(sub);

            AddInput("Issue title *", txtTitle, 22, 100, 320);
            AddInput("Location *", txtLocation, 375, 100, 320);
            AddInput("Reported by *", txtReportedBy, 728, 100, 320);
            AddCombo("Category", cmbCategory, new[] { "General", "Electrical", "Plumbing", "Furniture", "IT", "Cleaning", "Safety" }, 22, 160, 230);
            AddCombo("Priority", cmbPriority, new[] { "Low", "Medium", "High" }, 275, 160, 230);
            AddCombo("Status", cmbStatus, new[] { "Open", "In Progress", "Closed" }, 528, 160, 230);
            Label dateLabel = new Label { Text = "Reported date", Location = new Point(781, 160), AutoSize = true };
            Controls.Add(dateLabel);
            dtpReportedDate.Location = new Point(781, 182);
            dtpReportedDate.Width = 267;
            dtpReportedDate.Format = DateTimePickerFormat.Short;
            dtpReportedDate.Value = DateTime.Today;
            Controls.Add(dtpReportedDate);
            AddInput("Description * (at least 10 characters)", txtDescription, 22, 225, 1026);
            txtDescription.Multiline = true;
            txtDescription.Height = 58;
            txtDescription.ScrollBars = ScrollBars.Vertical;

            int x = 22;
            AddButton("Add issue", x, 330, (s, e) => SaveIssue(false)); x += 130;
            AddButton("Update selected", x, 330, (s, e) => SaveIssue(true)); x += 150;
            AddButton("Delete selected", x, 330, (s, e) => DeleteSelected()); x += 150;
            AddButton("Clear form", x, 330, (s, e) => ClearForm()); x += 130;
            AddButton("Refresh", x, 330, (s, e) => LoadIssues());
            lblSelection.Text = "No record selected";
            lblSelection.AutoSize = true;
            lblSelection.Location = new Point(22, 379);
            Controls.Add(lblSelection);

            Label searchLabel = new Label { Text = "Search issue title, location or status", Location = new Point(22, 412), AutoSize = true };
            Controls.Add(searchLabel);
            txtSearch.Name = "txtSearch";
            txtSearch.Location = new Point(22, 435);
            txtSearch.Width = 490;
            Controls.Add(txtSearch);
            txtSearch.TextChanged += (s, e) => LoadIssues();
            AddButton("Clear search", 525, 433, (s, e) => txtSearch.Clear());

            dgvIssues.Name = "dgvIssues";
            dgvIssues.Location = new Point(22, 482);
            dgvIssues.Size = new Size(1026, 175);
            dgvIssues.ReadOnly = true;
            dgvIssues.MultiSelect = false;
            dgvIssues.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            dgvIssues.AllowUserToAddRows = false;
            dgvIssues.AllowUserToDeleteRows = false;
            dgvIssues.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            dgvIssues.RowHeadersVisible = false;
            dgvIssues.SelectionChanged += IssuesSelectionChanged;
            Controls.Add(dgvIssues);
        }

        private void AddInput(string labelText, TextBox box, int x, int y, int width)
        {
            Controls.Add(new Label { Text = labelText, AutoSize = true, Location = new Point(x, y) });
            box.Location = new Point(x, y + 22);
            box.Width = width;
            Controls.Add(box);
        }

        private void AddCombo(string labelText, ComboBox combo, string[] values, int x, int y, int width)
        {
            Controls.Add(new Label { Text = labelText, AutoSize = true, Location = new Point(x, y) });
            combo.DropDownStyle = ComboBoxStyle.DropDownList;
            combo.Items.AddRange(values);
            combo.SelectedIndex = labelText == "Priority" ? 1 : 0;
            combo.Location = new Point(x, y + 22);
            combo.Width = width;
            Controls.Add(combo);
        }

        private void AddButton(string title, int x, int y, EventHandler action)
        {
            Button btn = new Button { Text = title, Location = new Point(x, y), Size = new Size(title.Length > 12 ? 140 : 120, 34) };
            btn.Click += action;
            Controls.Add(btn);
        }

        private FacilityIssue CollectIssue()
        {
            return new FacilityIssue
            {
                IssueId = selectedIssueId,
                Title = txtTitle.Text,
                Location = txtLocation.Text,
                ReportedBy = txtReportedBy.Text,
                Description = txtDescription.Text,
                Category = cmbCategory.Text,
                Priority = cmbPriority.Text,
                Status = cmbStatus.Text,
                ReportedDate = dtpReportedDate.Value
            };
        }

        private void SaveIssue(bool update)
        {
            if (update && selectedIssueId == 0)
            {
                MessageBox.Show("Select an existing issue from the table before updating.", "Select an issue");
                return;
            }
            FacilityIssue issue = CollectIssue();
            string error;
            if (!manager.TryValidate(issue, out error))
            {
                MessageBox.Show(error, "Input validation", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            try
            {
                if (update)
                {
                    if (!repository.Update(issue)) { MessageBox.Show("The selected issue no longer exists."); LoadIssues(); return; }
                }
                else repository.Add(issue);
                ClearForm();
                LoadIssues();
                MessageBox.Show(update ? "Issue updated successfully." : "Issue added successfully.", "Saved");
            }
            catch (Exception ex) { MessageBox.Show("Database operation failed: " + ex.Message, "Error"); }
        }

        private void DeleteSelected()
        {
            if (selectedIssueId == 0) { MessageBox.Show("Select an issue to delete."); return; }
            if (MessageBox.Show("Permanently delete issue #" + selectedIssueId + "?", "Confirm delete", MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes)
                return;
            try
            {
                repository.Delete(selectedIssueId);
                ClearForm();
                LoadIssues();
            }
            catch (Exception ex) { MessageBox.Show("Delete failed: " + ex.Message, "Error"); }
        }

        private void LoadIssues()
        {
            try
            {
                dgvIssues.SelectionChanged -= IssuesSelectionChanged;
                dgvIssues.DataSource = repository.GetIssues(txtSearch.Text);
                dgvIssues.ClearSelection();
                dgvIssues.SelectionChanged += IssuesSelectionChanged;
            }
            catch (Exception ex) { MessageBox.Show("Unable to load issues: " + ex.Message, "Error"); }
        }

        private void IssuesSelectionChanged(object sender, EventArgs e)
        {
            if (dgvIssues.SelectedRows.Count == 0) return;
            DataRowView row = dgvIssues.SelectedRows[0].DataBoundItem as DataRowView;
            if (row == null) return;
            selectedIssueId = Convert.ToInt32(row["IssueId"]);
            txtTitle.Text = Convert.ToString(row["Title"]);
            txtLocation.Text = Convert.ToString(row["Location"]);
            txtReportedBy.Text = Convert.ToString(row["ReportedBy"]);
            txtDescription.Text = Convert.ToString(row["Description"]);
            cmbCategory.SelectedItem = Convert.ToString(row["Category"]);
            cmbPriority.SelectedItem = Convert.ToString(row["Priority"]);
            cmbStatus.SelectedItem = Convert.ToString(row["Status"]);
            dtpReportedDate.Value = Convert.ToDateTime(row["ReportedDate"]);
            lblSelection.Text = "Selected issue #" + selectedIssueId;
        }

        private void ClearForm()
        {
            selectedIssueId = 0;
            txtTitle.Clear(); txtLocation.Clear(); txtReportedBy.Clear(); txtDescription.Clear();
            cmbCategory.SelectedIndex = 0; cmbPriority.SelectedIndex = 1; cmbStatus.SelectedIndex = 0;
            dtpReportedDate.Value = DateTime.Today;
            dgvIssues.ClearSelection();
            lblSelection.Text = "No record selected";
        }
    }
}
