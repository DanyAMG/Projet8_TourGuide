﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TripPricer;

public class TripPricerTask
{
    private readonly Guid _attractionId;
    private readonly string _apiKey;
    private readonly int _adults;
    private readonly int _children;
    private readonly int _nightsStay;

    public TripPricerTask(string apiKey, Guid attractionId, int adults, int children, int nightsStay)
    {
        _apiKey = apiKey;
        _attractionId = attractionId;
        _adults = adults;
        _children = children;
        _nightsStay = nightsStay;
    }
    public async Task<List<Provider>> ExecuteAsync()
    {
        var tripPricer = new TripPricer();
        return await Task.Run(() => tripPricer.GetPriceAsync(_apiKey, _attractionId, _adults, _children, _nightsStay, 5));
    }
}
