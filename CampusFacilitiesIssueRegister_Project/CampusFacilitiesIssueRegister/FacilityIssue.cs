using System;

namespace CampusFacilitiesIssueRegister
{
    // Model: holds one campus facilities issue, using private backing fields.
    public sealed class FacilityIssue
    {
        private int _issueId;
        private string _title = "";
        private string _location = "";
        private string _category = "General";
        private string _priority = "Medium";
        private string _status = "Open";
        private string _reportedBy = "";
        private string _description = "";
        private DateTime _reportedDate = DateTime.Today;

        public int IssueId { get { return _issueId; } set { _issueId = value; } }
        public string Title { get { return _title; } set { _title = (value ?? "").Trim(); } }
        public string Location { get { return _location; } set { _location = (value ?? "").Trim(); } }
        public string Category { get { return _category; } set { _category = value ?? "General"; } }
        public string Priority { get { return _priority; } set { _priority = value ?? "Medium"; } }
        public string Status { get { return _status; } set { _status = value ?? "Open"; } }
        public string ReportedBy { get { return _reportedBy; } set { _reportedBy = (value ?? "").Trim(); } }
        public string Description { get { return _description; } set { _description = (value ?? "").Trim(); } }
        public DateTime ReportedDate { get { return _reportedDate; } set { _reportedDate = value.Date; } }
    }
}
