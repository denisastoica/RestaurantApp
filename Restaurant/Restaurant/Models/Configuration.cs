using System;
using System.Collections.Generic;

namespace Restaurant.Models;

public partial class Configuration
{
    public string Key { get; set; } = null!;

    public string Value { get; set; } = null!;
}
