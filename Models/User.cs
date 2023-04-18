#pragma warning disable CS8618

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WeddingPlanner.Models;

public class User
{
    [Key]
    public int UserId {get;set;}

    [Required]
    [MinLength(2, ErrorMessage = "First name must be at least two characters")]
    public string Firstname {get;set;}

    [Required]
    [MinLength(2, ErrorMessage = "Last name must be at least two characters")]
    public string Lastname {get;set;}

    [Required]
    [EmailAddress]
    [UniqueEmail]
    public string Email {get;set;}

    [Required]
    [MinLength(8, ErrorMessage = "Password must be at least eight characters")]
    [DataType(DataType.Password)]
    public string Password {get;set;}

    public DateTime CreatedAt {get;set;} = DateTime.Now;
    public DateTime UpdatedAt {get;set;} = DateTime.Now;

    //One to many connection
    public List<Wedding> CreatedWeddings {get;set;} = new List<Wedding>();

    //Many to many connection
    public List<Association> AttendingWeddings { get; set; } = new List<Association>();

    [NotMapped]
    [DataType(DataType.Password)]
    [Compare("Password")]
    [Display(Name = "Confirm Password")]
    public string Confirm { get; set; }
}

public class UniqueEmailAttribute : ValidationAttribute
{
    protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
    {
        if (value == null)
        {
            return new ValidationResult("Email is required!");
        }

        MyContext _context = (MyContext)validationContext.GetService(typeof(MyContext));

        if (_context.Users.Any(e => e.Email == value.ToString()))
        {
            return new ValidationResult("Email must be unique!");
        }
        else
        {
            return ValidationResult.Success;
        }
    }
}