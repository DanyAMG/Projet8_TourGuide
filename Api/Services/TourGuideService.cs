using GpsUtil.Location;
using Microsoft.Extensions.Logging;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Globalization;
using TourGuide.LibrairiesWrappers.Interfaces;
using TourGuide.Services.Interfaces;
using TourGuide.Users;
using TourGuide.Utilities;
using TripPricer;

namespace TourGuide.Services;

public class TourGuideService : ITourGuideService
{
    private readonly ILogger _logger;
    private readonly IGpsUtil _gpsUtil;
    private readonly IRewardsService _rewardsService;
    private readonly TripPricer.TripPricer _tripPricer;
    public Tracker Tracker { get; private set; }
    private readonly ConcurrentDictionary<string, User> _internalUserMap;
    private const string TripPricerApiKey = "test-server-api-key";
    private bool _testMode = true;

    public TourGuideService(ILogger<TourGuideService> logger, IGpsUtil gpsUtil, IRewardsService rewardsService, ILoggerFactory loggerFactory)
    {
        _logger = logger;
        _tripPricer = new();
        _gpsUtil = gpsUtil;
        _rewardsService = rewardsService;
        _internalUserMap = new ConcurrentDictionary<string, User>();

        CultureInfo.CurrentCulture = new CultureInfo("en-US");

        if (_testMode)
        {
            _logger.LogInformation("TestMode enabled");
            _logger.LogDebug("Initializing users");
            InitializeInternalUsers();
            _logger.LogDebug("Finished initializing users");
        }

        var trackerLogger = loggerFactory.CreateLogger<Tracker>();

        Tracker = new Tracker(this, trackerLogger);
        AddShutDownHook();
    }

    public async Task<List<UserReward>> GetUserRewardsAsync(User user)
    {
        return await Task.FromResult(user.UserRewards);
    }

    public async Task <VisitedLocation> GetUserLocationAsync(User user)
    {
        return user.VisitedLocations.Any() ? user.GetLastVisitedLocation() : await TrackUserLocationAsync(user);
    }

    public User GetUser(string userName)
    {
        var u = _internalUserMap.ContainsKey(userName) ? _internalUserMap[userName] : null;
        return u;
    }

    public List<User> GetAllUsers()
    {
        return _internalUserMap.Values.ToList();
    }

    public void AddUser(User user)
    {
        if (!_internalUserMap.ContainsKey(user.UserName))
        {
            _internalUserMap.TryAdd(user.UserName, user);
        }
    }

    public async Task<List<Provider>> GetTripDealsAsync(User user)
    {
        int cumulativeRewardPoints = user.UserRewards.Sum(i => i.RewardPoints);

        List<Provider> providers = await _tripPricer.GetPriceAsync(TripPricerApiKey, user.UserId,
            user.UserPreferences.NumberOfAdults, user.UserPreferences.NumberOfChildren,
            user.UserPreferences.TripDuration, cumulativeRewardPoints);
        user.TripDeals = providers;
        return providers;
    }

    public async Task<VisitedLocation> TrackUserLocationAsync(User user)
    {
        VisitedLocation visitedLocation = await _gpsUtil.GetUserLocationAsync(user.UserId);
        user.AddToVisitedLocations(visitedLocation);
        await _rewardsService.CalculateRewardsAsync(user);
        return visitedLocation;
    }

    public async Task<List<Attraction>> GetNearByAttractionsAsync(VisitedLocation visitedLocation)
    {
        List<(double distance, Attraction attraction)> nearbyAttractions = new ();

        var attractions = await _gpsUtil.GetAttractionsAsync();

        foreach (var attraction in attractions)
        {
            var distance = _rewardsService.GetDistance(attraction, visitedLocation.Location);

            nearbyAttractions.Add((distance, attraction));
        }

        return nearbyAttractions.OrderBy(o=>o.distance).Select(o=>o.attraction).Take(5).ToList();
    }

    private void AddShutDownHook()
    {
        AppDomain.CurrentDomain.ProcessExit += (sender, e) => Tracker.StopTracking();
    }

    /**********************************************************************************
    * 
    * Methods Below: For Internal Testing
    * 
    **********************************************************************************/

    private async Task InitializeInternalUsers()
    {
        var tasks = new List<Task>();

        for (int i = 0; i < InternalTestHelper.GetInternalUserNumber(); i++)
        {
            var userName = $"internalUser{i}";
            var user = new User(Guid.NewGuid(), userName, "000", $"{userName}@tourGuide.com");

            tasks.Add(Task.Run(() =>
            {
                GenerateUserLocationHistory(user);
                _internalUserMap[userName] = user;
            }));
        }

        await Task.WhenAll(tasks);

        _logger.LogDebug($"Created {InternalTestHelper.GetInternalUserNumber()} internal test users.");
    }

    private void GenerateUserLocationHistory(User user)
    {
        for (int i = 0; i < 3; i++)
        {
            var visitedLocation = new VisitedLocation(user.UserId, new Locations(GenerateRandomLatitude(), GenerateRandomLongitude()), GetRandomTime());
            user.AddToVisitedLocations(visitedLocation);
        }
    }

    private static readonly Random random = new Random();

    private double GenerateRandomLongitude()
    {
        return new Random().NextDouble() * (180 - (-180)) + (-180);
    }

    private double GenerateRandomLatitude()
    {
        return new Random().NextDouble() * (90 - (-90)) + (-90);
    }

    private DateTime GetRandomTime()
    {
        return DateTime.UtcNow.AddDays(-new Random().Next(30));
    }
}
