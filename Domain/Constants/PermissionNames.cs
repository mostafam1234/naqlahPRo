namespace Domain.Constants
{
    /// <summary>
    /// Single source of truth for permission names. Stored in NA_RoleClaims with ClaimType = "Permission", ClaimValue = name.
    /// </summary>
    public static class PermissionNames
    {
        public const string ClaimType = "Permission";

        /// <summary>
        /// Full control: bypasses all permission checks. If user has this, they are allowed everywhere.
        /// </summary>
        public const string FullControl = "FullControl";

        // Admin who can assign permissions to roles (required for Role Permissions page)
        public const string CanManageRolePermissions = "CanManageRolePermissions";

        // Admin Home
        public const string CanViewAdminHome = "CanViewAdminHome";

        // Profile
        public const string CanViewProfile = "CanViewProfile";
        public const string ProfileActions = "ProfileActions";

        // Orders
        public const string CanViewAllOrders = "CanViewAllOrders";
        public const string OrderActions = "OrderActions";
        public const string CanViewOrderDetails = "CanViewOrderDetails";
        public const string CanViewControlCaptainOrders = "CanViewControlCaptainOrders";
        public const string CanViewOrderTracking = "CanViewOrderTracking";

        // Users - Captains
        public const string CanViewCaptains = "CanViewCaptains";
        public const string CaptainActions = "CaptainActions";

        // Users - System users
        public const string CanViewSystemUsers = "CanViewSystemUsers";
        public const string SystemUserActions = "SystemUserActions";

        // New captain applications
        public const string CanViewNewCaptainApplications = "CanViewNewCaptainApplications";
        public const string NewCaptainActions = "NewCaptainActions";

        // Vehicles
        public const string CanViewVehicles = "CanViewVehicles";
        public const string VehicleActions = "VehicleActions";

        // Main categories
        public const string CanViewMainCategories = "CanViewMainCategories";
        public const string MainCategoryActions = "MainCategoryActions";

        // Assistant works
        public const string CanViewAssistantWorks = "CanViewAssistantWorks";
        public const string AssistantWorkActions = "AssistantWorkActions";

        // Wallet
        public const string CanViewWalletCaptain = "CanViewWalletCaptain";
        public const string WalletCaptainActions = "WalletCaptainActions";
        public const string CanViewCustomerWalletTransactions = "CanViewCustomerWalletTransactions";
        public const string CustomerWalletTransactionsActions = "CustomerWalletTransactionsActions";

        // Categories control
        public const string CanViewCategoriesControl = "CanViewCategoriesControl";
        public const string CategoriesControlActions = "CategoriesControlActions";

        // Chat
        public const string CanViewServiceRequestChat = "CanViewServiceRequestChat";
        public const string CanViewChatReview = "CanViewChatReview";
        public const string ChatActions = "ChatActions";

        // Settings
        public const string CanViewAreasSettings = "CanViewAreasSettings";
        public const string AreasSettingsActions = "AreasSettingsActions";
        public const string CanViewSystemConfiguration = "CanViewSystemConfiguration";
        public const string SystemConfigurationActions = "SystemConfigurationActions";

        // Tech support
        public const string CanViewComplains = "CanViewComplains";
        public const string ComplainActions = "ComplainActions";
        public const string CanViewSuggestions = "CanViewSuggestions";
        public const string SuggestionActions = "SuggestionActions";

        // Data export / backup
        public const string CanExportData = "CanExportData";

        /// <summary>
        /// All permission names for listing in admin UI and seeding. Grouped by module.
        /// </summary>
        public static IReadOnlyList<PermissionDefinition> All { get; } =
        [
            new PermissionDefinition(FullControl, "Full control (bypass all permission checks)", "System"),
            new PermissionDefinition(CanManageRolePermissions, "Manage role permissions", "System"),
            new PermissionDefinition(CanViewAdminHome, "View admin home", "AdminHome"),
            new PermissionDefinition(CanViewProfile, "View profile", "Profile"),
            new PermissionDefinition(ProfileActions, "Profile actions", "Profile"),
            new PermissionDefinition(CanViewAllOrders, "View all orders", "Orders"),
            new PermissionDefinition(OrderActions, "Order actions", "Orders"),
            new PermissionDefinition(CanViewOrderDetails, "View order details", "Orders"),
            new PermissionDefinition(CanViewControlCaptainOrders, "View control captain orders", "Orders"),
            new PermissionDefinition(CanViewOrderTracking, "View order tracking", "Orders"),
            new PermissionDefinition(CanViewCaptains, "View captains", "Users"),
            new PermissionDefinition(CaptainActions, "Captain actions", "Users"),
            new PermissionDefinition(CanViewSystemUsers, "View system users", "Users"),
            new PermissionDefinition(SystemUserActions, "System user actions", "Users"),
            new PermissionDefinition(CanViewNewCaptainApplications, "View new captain applications", "Users"),
            new PermissionDefinition(NewCaptainActions, "New captain actions", "Users"),
            new PermissionDefinition(CanViewVehicles, "View vehicles", "Vehicles"),
            new PermissionDefinition(VehicleActions, "Vehicle actions", "Vehicles"),
            new PermissionDefinition(CanViewMainCategories, "View main categories", "Categories"),
            new PermissionDefinition(MainCategoryActions, "Main category actions", "Categories"),
            new PermissionDefinition(CanViewAssistantWorks, "View assistant works", "AssistantWorks"),
            new PermissionDefinition(AssistantWorkActions, "Assistant work actions", "AssistantWorks"),
            new PermissionDefinition(CanViewWalletCaptain, "View wallet captain", "Wallet"),
            new PermissionDefinition(WalletCaptainActions, "Wallet captain actions", "Wallet"),
            new PermissionDefinition(CanViewCustomerWalletTransactions, "View customer wallet transactions", "Wallet"),
            new PermissionDefinition(CustomerWalletTransactionsActions, "Customer wallet transaction actions", "Wallet"),
            new PermissionDefinition(CanViewCategoriesControl, "View categories control", "Categories"),
            new PermissionDefinition(CategoriesControlActions, "Categories control actions", "Categories"),
            new PermissionDefinition(CanViewServiceRequestChat, "View service request chat", "Chat"),
            new PermissionDefinition(CanViewChatReview, "View chat review", "Chat"),
            new PermissionDefinition(ChatActions, "Chat actions", "Chat"),
            new PermissionDefinition(CanViewAreasSettings, "View areas settings", "Settings"),
            new PermissionDefinition(AreasSettingsActions, "Areas settings actions", "Settings"),
            new PermissionDefinition(CanViewSystemConfiguration, "View system configuration", "Settings"),
            new PermissionDefinition(SystemConfigurationActions, "System configuration actions", "Settings"),
            new PermissionDefinition(CanViewComplains, "View complains", "TechSupport"),
            new PermissionDefinition(ComplainActions, "Complain actions", "TechSupport"),
            new PermissionDefinition(CanViewSuggestions, "View suggestions", "TechSupport"),
            new PermissionDefinition(SuggestionActions, "Suggestion actions", "TechSupport"),
            new PermissionDefinition(CanExportData, "Export data to Excel (backup)", "System"),
        ];
    }

    public record PermissionDefinition(string Name, string Description, string Module);
}
