using System;

namespace crds_angular.Enum
{
    public sealed class BadgeType
    {
        private readonly String _name;
        private readonly int _value;

        public static readonly BadgeType LabelDefault = new BadgeType(1, "label-default");
        public static readonly BadgeType LabelSuccess = new BadgeType(2, "label-success");
        public static readonly BadgeType LabelWarning = new BadgeType(3, "label-warning");
        public static readonly BadgeType LabelInfo = new BadgeType(4, "label-info");

        private BadgeType(int value, String name)
        {
            this._name = name;
            this._value = value;
        }

        public override String ToString()
        {
            return _name;
        }
    }
}