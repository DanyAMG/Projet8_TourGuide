﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TripPricer.Helpers;

namespace TripPricer;

public class TripPricer
{
    public async Task<List<Provider>> GetPriceAsync(string apiKey, Guid attractionId, int adults, int children, int nightsStay, int rewardsPoints)
    {
        List<Provider> providers = new List<Provider>();
        HashSet<string> providersUsed = new HashSet<string>();

        // Sleep to simulate some latency
        await Task.Delay(ThreadLocalRandom.Current.Next(1, 50));


        for (int i = 0; i < 10; i++)
        {
            int multiple = ThreadLocalRandom.Current.Next(100, 700);
            double childrenDiscount = children / 3.0;
            double price = multiple * adults + multiple * childrenDiscount * nightsStay + 0.99 - rewardsPoints;

            if (price < 0.0)
            {
                price = 0.0;
            }

            string provider = GetProviderName(apiKey, adults, i+1);


            if (!providersUsed.Contains(provider))
            {
                providersUsed.Add(provider);
                providers.Add(new Provider(attractionId, provider, price));
            }
        }
        return providers;
    }

    public string GetProviderName(string apiKey, int adults, int multiple)
    {

        return multiple switch
        {
            1 => "Holiday Travels",
            2 => "Enterprize Ventures Limited",
            3 => "Sunny Days",
            4 => "FlyAway Trips",
            5 => "United Partners Vacations",
            6 => "Dream Trips",
            7 => "Live Free",
            8 => "Dancing Waves Cruselines and Partners",
            9 => "AdventureCo",
            _ => "Cure-Your-Blues",
        };        
    }
}
