namespace DShop2024.EnumData
{
    public static class DateFilterConstantData
    {
        public const string LastMonth = "last_month";
        public const string ThisMonth = "this_month";

        public static List<FilterOption> GetDynamicStatisticsOptions()
        {
            int currentYear = DateTime.Now.Year;

            var options = new List<FilterOption>
        {
            new FilterOption { Value = LastMonth, Text = "Last month" },
            new FilterOption { Value = ThisMonth, Text = "This month" }
        };

            for (int i = -2; i <= 0; i++)
            {
                int year = currentYear + i;
                options.Add(new FilterOption
                {
                    Value = year.ToString(),
                    Text = year.ToString()
                });
            }

            return options;
        }

        public static List<FilterOption> GetDynamicImportExportOptions()
        {
            int currentYear = DateTime.Now.Year;

            var options = new List<FilterOption>{};

            for (int i = -2; i <= 0; i++)
            {
                int year = currentYear + i;
                options.Add(new FilterOption
                {
                    Value = year.ToString(),
                    Text = year.ToString()
                });
            }

            return options;
        }

    }
}
