#pragma warning disable CS8618
using System.ComponentModel.DataAnnotations;
namespace WeddingPlanner.Models;

public class Wedding
{
    [Key]
    public int WeddingId {get;set;}
    [Required]
    public string WedderOne {get;set;}
    [Required]
    public string WedderTwo {get;set;}
    [Required]
    [CheckDate(1, ErrorMessage ="Wedding must be planned in the future.")]
    public DateTime WeddingDate {get;set;}
    [Required]
    public string WeddingAddress {get;set;}

    public DateTime CreatedAt {get; set;} = DateTime.Now;
    public DateTime UpdatedAt {get;set;} = DateTime.Now;

    //One to many connection
    public int UserId {get;set;}
    public User? Creator {get;set;}

    //Many to many connection
        public List<Association> WeddingAttendees { get; set; } = new List<Association>();

}

public class CheckDateAttribute : ValidationAttribute
{
    int _minimumDate;

    public CheckDateAttribute(int minimumDate)
    {
        _minimumDate = minimumDate;
    }

    public override bool IsValid(object value)
    {
        DateTime date;

        if (DateTime.TryParse(value.ToString(), out date))
        {
            return date.AddDays(_minimumDate) > DateTime.Now;
        }

        return false;
    }
}