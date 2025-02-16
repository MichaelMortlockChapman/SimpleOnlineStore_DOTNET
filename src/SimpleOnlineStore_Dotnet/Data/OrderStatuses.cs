namespace SimpleOnlineStore_Dotnet.Data {
    public class OrderStatuses {
        public const string ORDERED = "ORDERED";
        public const string SHIPPED = "SHIPPED";
        public const string DELIVERY = "DELIVERY";
        public const string COMPLETE = "COMPLETE";
        public const string CANCELED = "CANCELED";

        public enum Statuses {
            ORDERED,
            SHIPPED,
            DELIVERY,
            COMPLETE,
            CANCELED,
        }

        public static bool IsActive(Statuses orderStatuses) {
            return !orderStatuses.Equals(Statuses.CANCELED) || !orderStatuses.Equals(Statuses.COMPLETE);
        }

        public static string ToString(Statuses orderStatuses) {
            switch (orderStatuses) {
                default:
                case Statuses.ORDERED:
                    return ORDERED;
                case Statuses.SHIPPED:
                    return SHIPPED;
                case Statuses.DELIVERY:
                    return DELIVERY;
                case Statuses.COMPLETE:
                    return COMPLETE;
                case Statuses.CANCELED:
                    return CANCELED;
            }
        }

        public static Statuses ToEnum(string orderStatuses) {
            switch (orderStatuses) {
                default:
                case ORDERED:
                    return Statuses.ORDERED;
                case SHIPPED:
                    return Statuses.SHIPPED;
                case DELIVERY:
                    return Statuses.DELIVERY;
                case COMPLETE:
                    return Statuses.COMPLETE;
                case CANCELED:
                    return Statuses.CANCELED;
            }
        }
    }
}
