namespace Domain.Constants;

public static class Permissions
{
    public static class Servers
    {
        public const string View = "Permissions.Servers.View";
        public const string Create = "Permissions.Servers.Create";
        public const string Edit = "Permissions.Servers.Edit";
        public const string Delete = "Permissions.Servers.Delete";
    }

    public static class Alerts
    {
        public const string View = "Permissions.Alerts.View";
        public const string Resolve = "Permissions.Alerts.Resolve";
    }

    public static class Reports
    {
        public const string View = "Permissions.Reports.View";
        public const string Create = "Permissions.Reports.Create";
        public const string Download = "Permissions.Reports.Download";
    }

    public static class Users
    {
        public const string View = "Permissions.Users.View";
        public const string Manage = "Permissions.Users.Manage";
    }
}

public static class Roles
{
    public const string Admin = "Admin";
    public const string Operator = "Operator";
    public const string Viewer = "Viewer";
}
