namespace DShop2024.EnumData
{
    public static class Menu
    {
        public enum Home
        {
            Home,
            Shop,
            Blog,
            Chat,
            Pages
        }

        public enum Admin
        {
            Dashboard,
            Order,
            Promotion,
            Information,
            Blog,
            Assignment,
            Catalog,
            CustomerService,
            Inventory,
            UserManagement
        }
    }

    public static class SubMenu
    {
        public enum Assignment
        {
            TaskAssignment,
            TaskManagement
        }

        public enum Catalog
        {
            Brand,
            Category,
            Product
        }

        public enum Order
        {
            OrderManage,
            Payment,
            Shipping,
            Return,
            Refund
        }

        public enum Inventory
        {
            StockIn
        }

        public enum CustomerService
        {
            Contact,
            Chat,
            Feedback,
            FAQ
        }

        public enum Blog
        {
            Subject,
            Post,
            Slider
        }

        public enum Information
        {
            Information,
            Branch,
            PrivacyPolicy
        }

        public enum Promotion
        {
            Promotion,
            Coupon,
            Sale
        }

        public enum UserManagement
        {
            User,
            Role
        }
    }
}
