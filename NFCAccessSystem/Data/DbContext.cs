using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;
using Microsoft.EntityFrameworkCore;

namespace NFCAccessSystem.Data;

// TagUid needs to be unique
[Index(nameof(TagUid), IsUnique = true)]
public class User
{
    public int UserId { get; set; }

    [DisplayName("Tag UID")]
    [Required(AllowEmptyStrings = false, ErrorMessage = "UID is required.")]
    [NfcUid("Please input a valid 4-byte hexadecimal UID, such as '1234ABCD'.")]
    public string TagUid { get; set; }


    [Required(AllowEmptyStrings = false, ErrorMessage = "Username is required.")]
    [DisplayName("Username")]
    public string Name { get; set; }

    [DisplayName("TOTP Secret")]
    // TODO: set the password type
    // [DataType(DataType.Password)]
    // TODO: set this to read only
    [Required(AllowEmptyStrings = false, ErrorMessage = "TOTP Secret is required.")]
    public string TotpSecret { get; set; }

    [DisplayName("Authorization")] public bool Authorized { get; set; }
    [DisplayName("Admin")] public bool Admin { get; set; }
    [DisplayName("Offline Auth")] public bool OfflineAuth { get; set; }
}

public class AccessSystemContext : DbContext
{
    public DbSet<User> Users { get; set; }

    public string DbPath { get; }

    public AccessSystemContext()
    {
        var folder = Environment.SpecialFolder.LocalApplicationData;
        var path = Environment.GetFolderPath(folder);
        DbPath = System.IO.Path.Join(path, "acs.db");
    }

    // The following configures EF to create a Sqlite database file in the
    // special "local" folder for your platform.
    protected override void OnConfiguring(DbContextOptionsBuilder options)
        => options.UseSqlite($"Data Source={DbPath}");

    // The following configures EF to create a Sqlite database file in the
    // special "local" folder for your platform.
    public DbSet<NFCAccessSystem.Models.ErrorViewModel>? ErrorViewModel { get; set; }
}
// Class to validate UIDs, which is 
// https://stackoverflow.com/questions/45491623/c-sharp-asp-mvc-a-field-that-only-accepts-hexadecimal-values-for-colors

public class NfcUidAttribute : ValidationAttribute
{
    private string _errorMessage;

    public NfcUidAttribute(string errorMessage)
    {
        _errorMessage = errorMessage;
    }

    protected override ValidationResult IsValid(object value, ValidationContext validationContext)
    {
        var uidHexStr = (string) value;
        var valid = Regex.IsMatch(uidHexStr, "[0-9A-F]{8}");
        if (valid)
        {
            return ValidationResult.Success;
        }
        else
        {
            return new ValidationResult(_errorMessage);
        }
    }
}