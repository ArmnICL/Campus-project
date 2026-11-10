using System;
using System.Collections.Generic;

namespace CampusFacilitiesIssueRegister
{
    // Business-logic class interacting with FacilityIssue model (not a Form).
    public sealed class IssueManager
    {
        private static readonly HashSet<string> Priorities = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
            { "Low", "Medium", "High" };
        private static readonly HashSet<string> Statuses = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
            { "Open", "In Progress", "Closed" };

        public bool TryValidate(FacilityIssue issue, out string message)
        {
            if (issue == null) { message = "Issue details are missing."; return false; }
            if (issue.Title.Length < 3 || issue.Title.Length > 100)
            { message = "Issue title must contain 3 to 100 characters."; return false; }
            if (string.IsNullOrWhiteSpace(issue.Location))
            { message = "Location is required."; return false; }
            if (issue.Location.Length > 100)
            { message = "Location must be 100 characters or fewer."; return false; }
            if (string.IsNullOrWhiteSpace(issue.ReportedBy))
            { message = "Reported by is required."; return false; }
            if (issue.ReportedBy.Length > 100)
            { message = "Reported by must be 100 characters or fewer."; return false; }
            if (issue.Description.Length < 10 || issue.Description.Length > 500)
            { message = "Description must contain 10 to 500 characters."; return false; }
            if (!Priorities.Contains(issue.Priority) || !Statuses.Contains(issue.Status))
            { message = "Choose a valid priority and status."; return false; }
            if (issue.ReportedDate > DateTime.Today)
            { message = "Reported date cannot be in the future."; return false; }
            message = "";
            return true;
        }
    }
}
