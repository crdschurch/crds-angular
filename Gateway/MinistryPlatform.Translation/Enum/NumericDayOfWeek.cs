namespace MinistryPlatform.Translation.Enum
{
    public sealed class NumericDayOfWeek
    {
        public static readonly NumericDayOfWeek Monday = new NumericDayOfWeek(1, "Monday");
        public static readonly NumericDayOfWeek Tuesday = new NumericDayOfWeek(2, "Tuesday");
        public static readonly NumericDayOfWeek Wednesday = new NumericDayOfWeek(3, "Wednesday");
        public static readonly NumericDayOfWeek Thursday = new NumericDayOfWeek(4, "Thursday");
        public static readonly NumericDayOfWeek Friday = new NumericDayOfWeek(5, "Friday");
        public static readonly NumericDayOfWeek Saturday = new NumericDayOfWeek(6, "Saturday");
        public static readonly NumericDayOfWeek Sunday = new NumericDayOfWeek(7, "Sunday");
        private static readonly NumericDayOfWeek[] numericDays = {Monday, Tuesday, Wednesday, Thursday, Friday, Saturday, Sunday};

        public readonly int id;
        public readonly string name; 
       
        private NumericDayOfWeek(int id, string name)
        {
          this.id = id;
          this.name = name;
        }
        
        public static int GetDayOfWeekID(string day)
        {
            foreach (var numericDay in numericDays)
            {
                if (numericDay.name == day)
                    return numericDay.id;
            }
            return Monday.id;
        }
        
    }
}