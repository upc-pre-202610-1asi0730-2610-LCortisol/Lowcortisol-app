using System;
using System.Collections.Generic;
using System.Linq;

namespace LowCortisol.Web.Services;

public class AppStateService
{
    public event Action? OnChange;

    public int WaterPercentage { get; private set; } = 74;
    public int GasPercentage { get; private set; } = 37;

    public List<DeviceState> Devices { get; private set; } = new()
    {
        new DeviceState
        {
            Id = Guid.NewGuid().ToString(),
            Name = "Main water valve",
            Type = "smart valve",
            Icon = "💧",
            Signal = 4,
            IsOnline = true
        },
        new DeviceState
        {
            Id = Guid.NewGuid().ToString(),
            Name = "Gas main",
            Type = "smart valve",
            Icon = "🔥",
            Signal = 4,
            IsOnline = true
        },
        new DeviceState
        {
            Id = Guid.NewGuid().ToString(),
            Name = "Kitchen sensor",
            Type = "leak sensor",
            Icon = "💧",
            Signal = 4,
            IsOnline = true
        },
        new DeviceState
        {
            Id = Guid.NewGuid().ToString(),
            Name = "Boiler temp",
            Type = "sensor",
            Icon = "🧪",
            Signal = 2,
            IsOnline = true
        },
        new DeviceState
        {
            Id = Guid.NewGuid().ToString(),
            Name = "Pressure gauge",
            Type = "sensor",
            Icon = "◔",
            Signal = 0,
            IsOnline = false
        },
        new DeviceState
        {
            Id = Guid.NewGuid().ToString(),
            Name = "Hub mini",
            Type = "hub",
            Icon = "◎",
            Signal = 4,
            IsOnline = true
        }
    };

    public List<ValveState> Valves { get; } = new()
    {
        new ValveState
        {
            Id = "water-main",
            Name = "Main water valve",
            Location = "Basement",
            Threshold = 12,
            Min = 0,
            Max = 30,
            Step = 1,
            Unit = "L/min",
            IsOn = true
        },
        new ValveState
        {
            Id = "kitchen-line",
            Name = "Kitchen line",
            Location = "Ground floor",
            Threshold = 6,
            Min = 0,
            Max = 20,
            Step = 1,
            Unit = "L/min",
            IsOn = true
        },
        new ValveState
        {
            Id = "gas-main",
            Name = "Gas main",
            Location = "Utility room",
            Threshold = 1.2,
            Min = 0,
            Max = 3,
            Step = 0.1,
            Unit = "bar",
            IsOn = true
        },
        new ValveState
        {
            Id = "garden-valve",
            Name = "Garden valve",
            Location = "Outdoor",
            Threshold = 8,
            Min = 0,
            Max = 20,
            Step = 1,
            Unit = "L/min",
            IsOn = false
        }
    };

    public void ToggleDevice(string id)
    {
        var device = Devices.FirstOrDefault(x => x.Id == id);
        if (device is null) return;

        device.IsOnline = !device.IsOnline;
        device.Signal = device.IsOnline ? Math.Max(device.Signal, 3) : 0;

        NotifyStateChanged();
    }

    public void AddDevice(DeviceState device)
    {
        Devices.Add(device);
        NotifyStateChanged();
    }

    public void ToggleValve(string id)
    {
        var valve = Valves.FirstOrDefault(x => x.Id == id);
        if (valve is null) return;

        valve.IsOn = !valve.IsOn;
        NotifyStateChanged();
    }

    public void UpdateValveThreshold(string id, double value)
    {
        var valve = Valves.FirstOrDefault(x => x.Id == id);
        if (valve is null) return;

        valve.Threshold = value;
        NotifyStateChanged();
    }

    public void SetWaterPercentage(int value)
    {
        WaterPercentage = Math.Clamp(value, 0, 100);
        NotifyStateChanged();
    }

    public void SetGasPercentage(int value)
    {
        GasPercentage = Math.Clamp(value, 0, 100);
        NotifyStateChanged();
    }

    public void EmergencyShutdown()
    {
        WaterPercentage = 0;
        GasPercentage = 0;

        foreach (var valve in Valves)
            valve.IsOn = false;

        foreach (var device in Devices)
        {
            device.IsOnline = false;
            device.Signal = 0;
        }

        NotifyStateChanged();
    }

    private void NotifyStateChanged() => OnChange?.Invoke();
}

public class DeviceState
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string Name { get; set; } = "";
    public string Type { get; set; } = "";
    public string Icon { get; set; } = "";
    public int Signal { get; set; } = 4;
    public bool IsOnline { get; set; } = true;
}

public class ValveState
{
    public string Id { get; set; } = "";
    public string Name { get; set; } = "";
    public string Location { get; set; } = "";
    public double Threshold { get; set; }
    public double Min { get; set; }
    public double Max { get; set; }
    public double Step { get; set; }
    public string Unit { get; set; } = "";
    public bool IsOn { get; set; }
}