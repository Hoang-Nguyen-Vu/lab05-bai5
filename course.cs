
#nullable enable
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace ProjectTeamLab
{
        internal static class Guard
    {
        public static string NonBlank(string? value, string paramName)
        {
            if (string.IsNullOrWhiteSpace(value))
                throw new ArgumentException($"{paramName} must not be empty", paramName);
            return value;
        }

        public static double NonNegative(double value, string paramName)
        {
            if (!double.IsFinite(value) || value < 0.0)
                throw new ArgumentOutOfRangeException(paramName, value, $"{paramName} must be a non-negative number");
            return value;
        }

        public static string Money(double value) => value.ToString("N2", CultureInfo.InvariantCulture);
    }
        public class Employee : IDisposable
    {
        private bool _disposed;
        public string Id { get; }
        public string FullName { get; }
        public double BaseSalary { get; private set; }
        public Employee() : this("UNKNOWN", "Unnamed employee", 0.0) { }
        public Employee(string id, string fullName) : this(id, fullName, 0.0) { }
        public Employee(string id, string fullName, double baseSalary)
        {
            Id = Guard.NonBlank(id, nameof(id));
            FullName = Guard.NonBlank(fullName, nameof(fullName));
            BaseSalary = Guard.NonNegative(baseSalary, nameof(baseSalary));
        }
        public void IncreaseSalary(double amount) => IncreaseSalary(amount, false);
        public void IncreaseSalary(double value, bool byPercentage)
        {
            if (!double.IsFinite(value) || value <= 0.0)
                throw new ArgumentOutOfRangeException(nameof(value), value, "increase value must be positive");

            double delta = byPercentage ? BaseSalary * value / 100.0 : value;
            double result = BaseSalary + delta;
            if (!double.IsFinite(result))
                throw new OverflowException("salary overflow");

            BaseSalary = result;
        }

        public virtual double CalculateMonthlyCost() => BaseSalary;
        public virtual void DisplayInfo()
        {
            Console.WriteLine(
                $"Employee          | {Id} | {FullName} | base = {Guard.Money(BaseSalary)} " +
                $"| monthly cost = {Guard.Money(CalculateMonthlyCost())}");
        }
        public bool IsDisposed => _disposed;

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
        protected virtual void Dispose(bool disposing)
        {
            if (_disposed) return;
            if (disposing) Console.WriteLine($"  [Dispose Employee] {Id} released");
            _disposed = true;
        }
    }
    public class SoftwareEngineer : Employee
    {
        public string PrimaryLanguage { get; }
        public double TechnicalAllowance { get; }
        public SoftwareEngineer(string id, string fullName, string primaryLanguage)
            : this(id, fullName, 0.0, primaryLanguage, 0.0) { }
        public SoftwareEngineer(string id, string fullName, double baseSalary,
                                string primaryLanguage, double technicalAllowance)
            : base(id, fullName, baseSalary)
        {
            PrimaryLanguage = Guard.NonBlank(primaryLanguage, nameof(primaryLanguage));
            TechnicalAllowance = Guard.NonNegative(technicalAllowance, nameof(technicalAllowance));
        }
        public override double CalculateMonthlyCost() => BaseSalary + TechnicalAllowance;
        public override void DisplayInfo()
        {
            Console.WriteLine(
                $"SoftwareEngineer  | {Id} | {FullName} | base = {Guard.Money(BaseSalary)} " +
                $"| lang = {PrimaryLanguage} | allowance = {Guard.Money(TechnicalAllowance)} " +
                $"| monthly cost = {Guard.Money(CalculateMonthlyCost())}");
        }
        protected override void Dispose(bool disposing)
        {
            if (disposing && !IsDisposed)
                Console.WriteLine($"  [Dispose SoftwareEngineer] {Id} released");
            base.Dispose(disposing);
        }
    }
    public class ProjectTeam : IDisposable
    {
        private readonly List<Employee> _members = new List<Employee>();
        private bool _disposed;
        public string ProjectCode { get; }
        public string ProjectName { get; }
        public Employee? Leader { get; private set; }           
        public IReadOnlyList<Employee> Members => _members;     
        public ProjectTeam(string projectCode, string projectName)
        {
            ProjectCode = Guard.NonBlank(projectCode, nameof(projectCode));
            ProjectName = Guard.NonBlank(projectName, nameof(projectName));
        }
        public ProjectTeam(string projectCode, string projectName, Employee leader)
            : this(projectCode, projectName)
        {
            if (leader == null) throw new ArgumentNullException(nameof(leader));
            _members.Add(leader);
            Leader = leader;
        }
        public bool AddMember(Employee employee)
        {
            if (employee == null) throw new ArgumentNullException(nameof(employee));
            if (Contains(employee.Id)) return false;
            _members.Add(employee);
            return true;
        }
        public bool AddMember(Employee employee, bool makeLeader)
        {
            if (!makeLeader) return AddMember(employee);
            return ChangeLeader(employee);
        }
        public bool RemoveMember(string employeeId)
        {
            int idx = IndexOf(employeeId);
            if (idx < 0) return false;
            if (ReferenceEquals(_members[idx], Leader)) return false;
            _members.RemoveAt(idx);
            return true;
        }
        public bool ChangeLeader(Employee employee)
        {
            if (employee == null) throw new ArgumentNullException(nameof(employee));
            int idx = IndexOf(employee.Id);
            if (idx >= 0 && !ReferenceEquals(_members[idx], employee)) return false;
            if (ReferenceEquals(Leader, employee)) return false;
            if (idx < 0) _members.Add(employee);
            Leader = employee;
            return true;
        }

        public bool Contains(string employeeId) => IndexOf(employeeId) >= 0;
        public double CalculateTotalMonthlyCost() => _members.Sum(m => m.CalculateMonthlyCost());
        public void DisplayTeam()
        {
            Console.WriteLine($"Project {ProjectCode} - {ProjectName}");
            Console.WriteLine("Leader: " + (Leader != null ? $"{Leader.Id} ({Leader.FullName})" : "<none>"));
            if (_members.Count == 0) Console.WriteLine("  (no members)");
            foreach (Employee e in _members)
            {
                Console.Write(ReferenceEquals(e, Leader) ? "  * " : "    ");
                e.DisplayInfo();   // dispatch da hinh
            }
            Console.WriteLine($"Total monthly cost: {Guard.Money(CalculateTotalMonthlyCost())}");
        }
        public bool InvariantsHold()
        {
            bool noNulls = _members.All(m => m != null);
            bool noDuplicateIds = _members.Select(m => m.Id).Distinct().Count() == _members.Count;
            bool leaderInTeam = Leader == null || _members.Any(m => ReferenceEquals(m, Leader));
            return noNulls && noDuplicateIds && leaderInTeam;
        }
        public void Dispose()
        {
            if (_disposed) return;
            Console.WriteLine($"  [Dispose ProjectTeam] {ProjectCode} released ({_members.Count} employee(s) left untouched)");
            _members.Clear();
            Leader = null;
            _disposed = true;
        }
        private int IndexOf(string employeeId) => _members.FindIndex(m => m.Id == employeeId);
    }
    internal static class Program
    {
        private static void Step(int n, string title) =>
            Console.WriteLine($"\n=== Buoc {n}: {title} ===");

        private static void Result(string what, bool ok) =>
            Console.WriteLine($"  {what} -> {(ok ? "OK (true)" : "REJECTED (false)")}");

        private static void ExpectThrow(string what, Action action)
        {
            try
            {
                action();
                Console.WriteLine($"  {what} -> KHONG nem ngoai le (SAI)");
            }
            catch (Exception ex)
            {
                string firstLine = ex.Message.Split('\n')[0];
                Console.WriteLine($"  {what} -> {ex.GetType().Name}: {firstLine}");
            }
        }

        private static int Main()
        {
            Console.WriteLine("########## BIEN: cac truong hop bien ##########");
            using (var def = new Employee())   
            {
                def.DisplayInfo();
                ExpectThrow("new Employee(\"\", \"A\")", () => { _ = new Employee("", "A"); });
                ExpectThrow("new Employee(\"E1\", \"   \")", () => { _ = new Employee("E1", "   "); });
                ExpectThrow("new Employee(\"E1\", \"A\", -1)", () => { _ = new Employee("E1", "A", -1); });
                ExpectThrow("new SoftwareEngineer(..., lang = \"\")",
                    () => { _ = new SoftwareEngineer("S9", "X", ""); });
                ExpectThrow("new SoftwareEngineer(..., allowance = -5)",
                    () => { _ = new SoftwareEngineer("S9", "X", 1000, "C#", -5); });
                ExpectThrow("IncreaseSalary(0)", () => def.IncreaseSalary(0));
                ExpectThrow("IncreaseSalary(-10, true)", () => def.IncreaseSalary(-10, true));
            }

            Console.WriteLine("\n########## KICH BAN CHINH ##########");

            Step(1, "Tao hai Employee bang hai constructor khac nhau");
            using var e1 = new Employee("E001", "Nguyen Van A");              
            using var e2 = new Employee("E002", "Tran Thi B", 12000000);      
            e1.DisplayInfo();
            e2.DisplayInfo();

            Step(2, "Tao hai SoftwareEngineer bang hai constructor khac nhau");
            using var se1 = new SoftwareEngineer("S001", "Le Van C", "C#");                          
            using var se2 = new SoftwareEngineer("S002", "Pham Thi D", 20000000, "Java", 3000000);   
            se1.DisplayInfo();
            se2.DisplayInfo();

            Step(3, "Tang luong bang so tien co dinh (e1 +5,000,000)");
            e1.IncreaseSalary(5000000);
            e1.DisplayInfo();

            Step(4, "Tang luong theo phan tram (e2 +10%)");
            e2.IncreaseSalary(10, true);
            e2.DisplayInfo();

            Step(5, "Tao nhom du an khong co truong nhom");
            using var team = new ProjectTeam("PRJ01", "Payroll System");
            team.DisplayTeam();

            Step(6, "AddMember(employee)");
            Result("AddMember(e1)", team.AddMember(e1));
            Result("AddMember(e2)", team.AddMember(e2));

            Step(7, "AddMember(engineer, true) -> dat lam truong nhom");
            Result("AddMember(se2, true)", team.AddMember(se2, true));
            Result("AddMember(se1)", team.AddMember(se1));

            Step(8, "Them lai thanh vien da ton tai");
            Result("AddMember(e1)         ", team.AddMember(e1));
            Result("AddMember(e1, false)  ", team.AddMember(e1, false));
            Result("AddMember(se2, true) (da la truong nhom)", team.AddMember(se2, true));
            Console.WriteLine($"  So thanh vien van la {team.Members.Count}");

            Step(9, "Hien thi danh sach (goi da hinh)");
            team.DisplayTeam();

            Step(10, "Tong chi phi nhan su hang thang");
            Console.WriteLine($"  Tong = {Guard.Money(team.CalculateTotalMonthlyCost())}");

            Step(11, "Thu xoa truong nhom hien tai");
            Result("RemoveMember(\"S002\")", team.RemoveMember("S002"));
            Result("RemoveMember(\"NOPE\") (khong ton tai)", team.RemoveMember("NOPE"));
            Console.WriteLine($"  Truong nhom van la: {team.Leader?.Id}");

            Step(12, "Doi truong nhom roi xoa nguoi tung la truong nhom");
            Result("ChangeLeader(e1)", team.ChangeLeader(e1));
            Result("RemoveMember(\"S002\")", team.RemoveMember("S002"));
            Result("Contains(\"S002\")", team.Contains("S002"));
            team.DisplayTeam();
            Console.WriteLine($"  Bat bien nhom 1: {(team.InvariantsHold() ? "dung" : "VI PHAM")}");

            Step(13, "Nhom thu hai chia se nhan su voi nhom thu nhat (ket tap nhieu nhom)");
            Step(14, "Huy nhom thu hai bang khoi lenh cuc bo");
            using (var team2 = new ProjectTeam("PRJ02", "Mobile App", e2))   // ctor co truong nhom
            {
                Result("team2.AddMember(e1) (e1 dang o PRJ01)", team2.AddMember(e1));
                Result("team2.AddMember(e1) lan nua", team2.AddMember(e1));
                team2.DisplayTeam();
                Console.WriteLine("  Sap ket thuc khoi lenh -> team2 bi huy");
            }
            Console.WriteLine("  Da ra khoi khoi lenh.");

            Step(15, "Nhan su cua nhom 2 van ton tai sau khi nhom 2 bi huy");
            e1.DisplayInfo();
            e2.DisplayInfo();
            Console.WriteLine($"  e1.IsDisposed = {e1.IsDisposed}, e2.IsDisposed = {e2.IsDisposed}");
            Console.WriteLine($"  team.Contains(\"E001\") = {team.Contains("E001")}");
            team.DisplayTeam();

            Console.WriteLine("\n########## KET THUC Main: cac doi tuong duoc Dispose theo thu tu nguoc ##########");
            return 0;
        }
    }
}