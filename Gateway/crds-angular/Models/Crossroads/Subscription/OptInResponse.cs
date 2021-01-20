namespace crds_angular.Models.Crossroads.Subscription
{
    public class OptInResponse
    {
        // This class is a little overkill for the current story, but will support expanding
        // out to handle non-user email contacts for list management
        public bool UserAlreadySubscribed { get; set; }
        public bool ErrorInSignupProcess { get; set; }
    }
}