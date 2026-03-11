namespace Application.Features.AdminSection.BackupFeature.Constants
{
    public static class BackupModuleKeys
    {
        public const string Orders = "Orders";
        public const string OrderPackages = "OrderPackages";
        public const string Vehicles = "Vehicles";
        public const string SystemUsers = "SystemUsers";
        public const string DeliveryMen = "DeliveryMen";
        public const string MainCategories = "MainCategories";
        public const string WalletTransactions = "WalletTransactions";
        public const string Complains = "Complains";
        public const string Suggestions = "Suggestions";
        public const string Notifications = "Notifications";
        public const string Regions = "Regions";
        public const string Cities = "Cities";
        public const string Neighborhoods = "Neighborhoods";
        public const string AssistantWorks = "AssistantWorks";

        public static readonly IReadOnlySet<string> All = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            Orders,
            OrderPackages,
            Vehicles,
            SystemUsers,
            DeliveryMen,
            MainCategories,
            WalletTransactions,
            Complains,
            Suggestions,
            Notifications,
            Regions,
            Cities,
            Neighborhoods,
            AssistantWorks
        };

        public static readonly IReadOnlySet<string> DateFilterable = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            Orders,
            OrderPackages,
            WalletTransactions,
            Complains,
            Suggestions,
            Notifications
        };
    }
}
