using System;
using System.Collections.Generic;

namespace Databasprojekt.Models;

public partial class Salary
{
    public int? PositionId { get; set; }

    public decimal? SalaryAmount { get; set; }

    public virtual Position? Position { get; set; }
}
