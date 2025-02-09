using System;
using System.Collections.Generic;

namespace Databasprojekt.Models;

public partial class StudentGrade
{
    public int GradeId { get; set; }

    public string? GradeLevel { get; set; }

    public int? TeacherId { get; set; }

    public int? StudentId { get; set; }

    public int? SubjectId { get; set; }

    public DateOnly GradeDate { get; set; }

    public virtual Grade? GradeLevelNavigation { get; set; }

    public virtual Student? Student { get; set; }

    public virtual Subject? Subject { get; set; }

    public virtual Staff? Teacher { get; set; }
}
