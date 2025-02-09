using Databasprojekt.Models;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Databasprojekt
{
	public static class CLI
	{
		private static readonly string connectionString = @"Data Source=THINKPAD_T14SG4;Integrated Security=True;Initial Catalog=School;Connect Timeout=30;Encrypt=False;Trust Server Certificate=False;Application Intent=ReadWrite;Multi Subnet Failover=False";

		private static readonly Dictionary<string, Delegate> actionMap = new()
		{
			{"1. Visa lärare på olika avdelningar", () => { ViewStaffPositions(); } },
			{"2. Information om alla elever", () => { ViewStudentInformation(); } },
			{"3. Aktiva kurser", () => {ViewActiveSubjects(); } },
			{"4. Personal Översikt", () => {StaffOverview(); } },
			{"5. Lägg till ny personal", () => {AddNewStaff(); } },
			{"6. Visa betyg för en elev", () => {SeeGradeInformation(); } },
			{"7. Hur mycket respektive avdelning ut i lön varje månad?", () => {DepartmentSalary(); } },
			{"8. Hur mycket är medellönen för de olika avdelningarna?", () => {AverageSalary(); } },
			{"9. Student Information", () => { StudentInformation(); } },
			{"10. Sätt betyg på en elev", () => {SetStudentGrade(); } },
			{"11. Avsluta", () => {Environment.Exit(0); } },
		};
		public static void StartMenu()
		{
			while (true)
			{
				Console.Clear();
				foreach (var action in actionMap.Keys)
				{
					Console.WriteLine(action);
				}
				try
				{
					Console.Write("Val: ");
					int choise = int.Parse(Console.ReadLine()!);
					if (choise < 0 || choise > actionMap.Count)
					{
						throw new Exception("Ogiltigt val");
					}
					var key = new List<string>(actionMap.Keys)[choise - 1];
					Console.Clear();
					actionMap[key].DynamicInvoke();
				}
				catch (Exception ex)
				{
					Console.WriteLine(ex.ToString());
				}
			}
		}
		private static void ViewStaffPositions()
		{
			using (var context = new SchoolContext())
			{
				var staffPerDepartment = context.Staff.Include(staff => staff.PositionNavigation)
					.GroupBy(staff => staff.PositionNavigation.PositionName);

				foreach (var department in staffPerDepartment)
				{
					Console.WriteLine(department.Key + "  \t " + department.Count().ToString());
				}
				Console.WriteLine("Visa ytterligare informaton on amnställda? [y]\n");


				string answer = Console.ReadLine();

				if (answer == "y")
				{
					var staffWithPosition = context.Staff.Include(staff => staff.PositionNavigation);
					foreach (var staff in staffWithPosition)
					{
						Console.WriteLine(staff.FirstName + " " + staff.LastName + "\t" + staff.PositionNavigation.PositionName);
					}
					Console.WriteLine("Tryck retur för att återgå hem");
					Console.ReadLine();
				}
			}
		}
		private static void ViewStudentInformation()
		{
			Console.WriteLine("Elever: ");

			using (var context = new SchoolContext())
			{
				var students = context.Students
					.Include(s => s.Class).ToList();

				foreach (var student in students)
				{
					string className = student.Class != null ? student.Class.ClassName : "N/A";
					Console.WriteLine($"{student.StudentId}\t{className}\t{student.FirstName} {student.LastName}");
				}
			}
			Console.WriteLine("Tryck retur för att återgå hem");
			Console.ReadLine();
		}
		private static void ViewActiveSubjects()
		{
			Console.WriteLine("Aktiva Kurser: ");
			using (var context = new SchoolContext())
			{
				var activeSubjects = context.Subjects.Where(x => x.Active == true).ToList();
				foreach (var subject in activeSubjects)
				{
					Console.WriteLine(subject.SubjectName);
				}
			}
			Console.WriteLine("Tryck retur för att återgå hem");
			Console.ReadLine();
		}
		private static void StaffOverview()
		{

			using (SqlConnection connection = new SqlConnection(connectionString))
			{
				connection.Open();

				string query = @"SELECT
    FirstName + ' ' + LastName AS 'Namn',
    PositionName AS 'Befattning',
    DATEDIFF(year, StartDate, GETDATE()) AS 'År'
FROM STAFF
JOIN
Positions
ON PositionId = [Position];";

				SqlCommand command = new SqlCommand(query, connection);

				using (SqlDataReader reader = command.ExecuteReader())
				{
					for (int i = 0; i < reader.FieldCount; i++)
					{
						Console.Write(reader.GetName(i) + "\t");
					}
					Console.WriteLine();
					Console.WriteLine();
					while (reader.Read())
					{
						for (int i = 0; i < reader.FieldCount; i++)
						{
							Console.Write(reader[i].ToString() + " \t");
						}
						Console.WriteLine();
					}
				}
				connection.Close();
				Console.WriteLine("Tryck retur för att återgå hem");
				Console.ReadLine();
			}
		}
		private static void AddNewStaff()
		{
			try
			{
				Console.Clear();
				Console.WriteLine("Lägg till ny anställd");

				Console.Write("\nFörnamn: ");
				string firstName = Console.ReadLine();
				if (string.IsNullOrWhiteSpace(firstName))
				{
					throw new Exception("Skriv in ett förnamn");
				}

				Console.Write("Efternamn: ");
				string lastName = Console.ReadLine();
				if (string.IsNullOrWhiteSpace(lastName))
				{
					throw new Exception("Skriv in ett efternamn");
				}

				Console.Write("Personnummer (10 siffror, t.ex. 990101-1234): ");
				string personalNumber = Console.ReadLine();
				if (personalNumber.Length != 11 || !personalNumber.Contains('-'))
				{
					throw new Exception("Personnumret skall vara 10 siffror med bindestreck");
				}

				using (SqlConnection conn = new SqlConnection(connectionString))
				{
					conn.Open();
					Console.WriteLine("Positioner:");
					string positionQuery = "SELECT PositionId, PositionName FROM Positions";
					using (SqlCommand cmd = new SqlCommand(positionQuery, conn))
					using (SqlDataReader reader = cmd.ExecuteReader())
					{
						int i = 1;
						while (reader.Read())
						{
							Console.WriteLine($"{i}. {reader["PositionName"]}");
							i++;
						}
					}

					Console.Write("Val: ");
					if (!int.TryParse(Console.ReadLine(), out int positionId))
					{
						throw new Exception("Ogiltig position");
					}
					string insertQuery = @"
                INSERT INTO Staff (FirstName, LastName, PersonalNumber, Position)
                VALUES (@FirstName, @LastName, @PersonalNumber, @Position)";

					using (SqlCommand cmd = new SqlCommand(insertQuery, conn))
					{
						cmd.Parameters.AddWithValue("@FirstName", firstName);
						cmd.Parameters.AddWithValue("@LastName", lastName);
						cmd.Parameters.AddWithValue("@PersonalNumber", personalNumber);
						cmd.Parameters.AddWithValue("@Position", positionId);

						int rowsAffected = cmd.ExecuteNonQuery();
						if (rowsAffected > 0)
						{
							Console.ForegroundColor = ConsoleColor.Green;
							Console.WriteLine("Lyckades!");
						}
						else
						{
							throw new Exception("Något gick fel vid insättning av personal.");
						}
					}
				}
			}
			catch (Exception ex)
			{
				Console.ForegroundColor = ConsoleColor.Red;
				Console.WriteLine($"Fel: {ex.Message}");
			}
			Console.ResetColor();
			Console.ReadKey();
		}
		private static void SeeGradeInformation()
		{
			using (SqlConnection connection = new SqlConnection(connectionString))
			{
				connection.Open();

				Console.Write("\nStudent ID:");
				string studentId = Console.ReadLine().Trim();

				string query = @$"
SELECT
Students.FirstName + ' ' + Students.LastName AS 'Namn',
SubjectName AS 'Ämne',
GradeLevel AS 'Betyg',
Staff.FirstName + ' ' + Staff.LastName AS 'Lärare',
GradeDate AS 'Betygs Datum'
FROM Student_Grades
JOIN Students on Students.StudentId = Student_Grades.StudentId
JOIN Subjects on Subjects.SubjectId = Student_Grades.SubjectId
JOIN Staff on Staff.StaffId = Student_Grades.TeacherId
WHERE Student_Grades.StudentId = {studentId}";

				SqlCommand command = new SqlCommand(query, connection);

				using (SqlDataReader reader = command.ExecuteReader())
				{
					if (reader.HasRows)
					{
						for (int i = 0; i < reader.FieldCount; i++)
						{
							Console.Write(reader.GetName(i) + "\t");
						}
						Console.WriteLine();
						Console.WriteLine();
						while (reader.Read())
						{
							for (int i = 0; i < reader.FieldCount; i++)
							{
								Console.Write(reader[i].ToString() + " \t");
							}
							Console.WriteLine();
						}
					}
					else
					{
						Console.WriteLine("Hittade inga resultat");
					}
				}
				connection.Close();
				Console.WriteLine("Tryck retur för att återgå hem");
				Console.ReadLine();
			}
		}
		private static void DepartmentSalary()
		{
			using (SqlConnection connection = new SqlConnection(connectionString))
			{
				connection.Open();

				string query = @"SELECT 
    PositionName AS Avdelning, 
    SUM(Salary)/12 AS TotalMånadslön
FROM Salaries
JOIN Staff ON Salaries.StaffId = Staff.StaffId
JOIN Positions ON Staff.Position = Positions.PositionId
GROUP BY PositionName;
";

				SqlCommand command = new SqlCommand(query, connection);

				using (SqlDataReader reader = command.ExecuteReader())
				{
					for (int i = 0; i < reader.FieldCount; i++)
					{
						Console.Write(reader.GetName(i) + "\t");
					}
					Console.WriteLine();
					Console.WriteLine();
					while (reader.Read())
					{
						for (int i = 0; i < reader.FieldCount; i++)
						{
							Console.Write(reader[i].ToString() + " \t");
						}
						Console.WriteLine();
					}
				}
				connection.Close();
				Console.WriteLine("Tryck retur för att återgå hem");
				Console.ReadLine();
			}
		}
		private static void AverageSalary()
		{
			using (SqlConnection connection = new SqlConnection(connectionString))
			{
				connection.Open();

				string query = @"SELECT 
    Positions.PositionName AS Avdelning, 
    AVG(Salaries.Salary) AS Medellön
FROM Salaries
JOIN Staff ON Salaries.StaffId = Staff.StaffId
JOIN Positions ON Staff.Position = Positions.PositionId
GROUP BY Positions.PositionName;

";

				SqlCommand command = new SqlCommand(query, connection);

				using (SqlDataReader reader = command.ExecuteReader())
				{
					for (int i = 0; i < reader.FieldCount; i++)
					{
						Console.Write(reader.GetName(i) + "\t");
					}
					Console.WriteLine();
					Console.WriteLine();
					while (reader.Read())
					{
						for (int i = 0; i < reader.FieldCount; i++)
						{
							Console.Write(reader[i].ToString() + " \t");
						}
						Console.WriteLine();
					}
				}
				connection.Close();
				Console.WriteLine("Tryck retur för att återgå hem");
				Console.ReadLine();
			}
		}
		private static void StudentInformation()
		{
			using (SqlConnection connection = new SqlConnection(connectionString))
			{
				connection.Open();
				Console.WriteLine("\nStudent ID: ");
				string studentID = Console.ReadLine().Trim();
				SqlCommand command = new SqlCommand("GetStudentInformation", connection);
				command.CommandType = CommandType.StoredProcedure;
				command.Parameters.Add(new SqlParameter("@StudentId", SqlDbType.Int)).Value = studentID;

				using (SqlDataReader reader = command.ExecuteReader())
				{
					while (reader.Read())
					{
						for (int i = 0; i < reader.FieldCount; i++)
						{
							Console.WriteLine($"{reader.GetName(i)}: {reader[i].ToString()}");
						}
					}
				}
				connection.Close();
				Console.WriteLine("\nTryck retur för att återgå hem");
				Console.ReadLine();
			}
		}
		private static void SetStudentGrade()
		{
			try
			{
				Console.Clear();
				Console.WriteLine("Sätt betyg på en elev");

				Console.Write("\nStudentens ID: ");
				if (!int.TryParse(Console.ReadLine(), out int studentId))
				{
					throw new Exception("Ogiltigt student-ID");
				}
				Console.Write("\nKursens ID: ");
				if (!int.TryParse(Console.ReadLine(), out int subjectId))
				{
					throw new Exception("Ogiltigt kurs-ID");
				}
				Console.Write("\nLärarens ID: ");
				if (!int.TryParse(Console.ReadLine(), out int teacherId))
				{
					throw new Exception("Ogiltigt lärar-ID");
				}
				Console.Write("\nBetyg (A-F): ");
				string tmpGrade = Console.ReadLine();
				char grade = tmpGrade.ToUpper()[0];
				Console.WriteLine();

				if (!"ABCDEF".Contains(grade.ToString().ToUpper()))
				{
					throw new Exception("Ogiltigt betyg. Ange A, B, C, D, E eller F.");
				}

				using (SqlConnection conn = new SqlConnection(connectionString))
				{
					conn.Open();
					SqlTransaction transaction = conn.BeginTransaction();

					try
					{
						if (!Utility.RecordExists(conn, transaction, "Students", "StudentId", studentId))
						{
							throw new Exception("Student-ID existerar inte.");
						}
						if (!Utility.RecordExists(conn, transaction, "Subjects", "SubjectId", subjectId))
						{
							throw new Exception("Kurs-ID existerar inte.");
						}

						if (!Utility.RecordExists(conn, transaction, "Staff", "StaffId", teacherId))
						{
							throw new Exception("Lärar-ID existerar inte.");
						}
						//check if a grade already is set
						string checkGradeQuery = @"
                    SELECT COUNT(*) FROM Student_Grades 
                    WHERE StudentId = @StudentId AND SubjectId = @SubjectId";

						using (SqlCommand checkCmd = new SqlCommand(checkGradeQuery, conn, transaction))
						{
							checkCmd.Parameters.AddWithValue("@StudentId", studentId);
							checkCmd.Parameters.AddWithValue("@SubjectId", subjectId);
							int gradeExists = (int)checkCmd.ExecuteScalar();

							if (gradeExists > 0)
							{
								string updateQuery = @"
                            UPDATE Student_Grades 
                            SET GradeLevel = @GradeLevel, GradeDate = GETDATE(), TeacherId = @TeacherId
                            WHERE StudentId = @StudentId AND SubjectId = @SubjectId";

								using (SqlCommand updateCmd = new SqlCommand(updateQuery, conn, transaction))
								{
									updateCmd.Parameters.AddWithValue("@GradeLevel", grade.ToString().ToUpper());
									updateCmd.Parameters.AddWithValue("@TeacherId", teacherId);
									updateCmd.Parameters.AddWithValue("@StudentId", studentId);
									updateCmd.Parameters.AddWithValue("@SubjectId", subjectId);
									updateCmd.ExecuteNonQuery();
								}
							}
							else
							{
								string insertQuery = @"
                            INSERT INTO Student_Grades (StudentId, SubjectId, GradeLevel, GradeDate, TeacherId)
                            VALUES (@StudentId, @SubjectId, @GradeLevel, GETDATE(), @TeacherId)";

								using (SqlCommand insertCmd = new SqlCommand(insertQuery, conn, transaction))
								{
									insertCmd.Parameters.AddWithValue("@GradeLevel", grade.ToString().ToUpper());
									insertCmd.Parameters.AddWithValue("@TeacherId", teacherId);
									insertCmd.Parameters.AddWithValue("@StudentId", studentId);
									insertCmd.Parameters.AddWithValue("@SubjectId", subjectId);
									insertCmd.ExecuteNonQuery();
								}
							}
						}

						transaction.Commit();
						Console.ForegroundColor = ConsoleColor.Green;
						Console.WriteLine("\nBetyget har registrerats!");
					}
					catch (Exception ex)
					{
						// show error and rollback
						transaction.Rollback();
						Console.ForegroundColor = ConsoleColor.Red;
						Console.WriteLine($"Fel vid betygsregistrering: {ex.Message}");
					}
				}
			}
			catch (Exception ex)
			{
				Console.ForegroundColor = ConsoleColor.Red;
				Console.WriteLine($"Fel: {ex.Message}");
			}
			finally
			{
				Console.ResetColor();
			}
			Console.WriteLine("\nTryck retur för att återgå hem");
			Console.ReadLine();
		}
	}
}