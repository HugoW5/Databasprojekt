using System;
using System.Collections.Generic;

namespace Databasprojekt.Models;

public partial class Staff
{
    public int StaffId { get; set; }

    public string FirstName { get; set; } = null!;

    public string LastName { get; set; } = null!;

    public string PersonalNumber { get; set; } = null!;

    public int Position { get; set; }

    public virtual ICollection<Class> Classes { get; set; } = new List<Class>();

    public virtual Position PositionNavigation { get; set; } = null!;

    public virtual ICollection<StudentGrade> StudentGrades { get; set; } = new List<StudentGrade>();
}
