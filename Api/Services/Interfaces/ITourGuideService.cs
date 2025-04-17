using GpsUtil.Location;
using TourGuide.Users;
using TourGuide.Utilities;
using TripPricer;

namespace TourGuide.Services.Interfaces
{
    public interface ITourGuideService
    {
        Tracker Tracker { get; }

        void AddUser(User user);
        List<User> GetAllUsers();
        Task<List<Attraction>> GetNearByAttractionsAsync(VisitedLocation visitedLocation);
        Task<List<Provider>> GetTripDealsAsync(User user);
        User GetUser(string userName);
        Task<VisitedLocation> GetUserLocationAsync(User user);
        Task<List<UserReward>> GetUserRewardsAsync(User user);
        Task<VisitedLocation> TrackUserLocationAsync(User user);
    }
}