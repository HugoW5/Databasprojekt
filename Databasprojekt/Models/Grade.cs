using System;
using System.Collections.Generic;

namespace Databasprojekt.Models;

public partial class Grade
{
    public string Grade1 { get; set; } = null!;

    public decimal? Credit { get; set; }

    public virtual ICollection<StudentGrade> StudentGrades { get; set; } = new List<StudentGrade>();
}
