using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Entities;
public class Mail
{
    [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int id { get; set; }
    public int status { get; set; } = 0;
    public string subject { get; set; }
    public string message { get; set; }
    public string to_email { get; set; }
    public string from_email { get; set; }
    public string from_password { get; set; }
    public string from_host { get; set; }
    public int from_port { get; set; }
    public bool from_enable_ssl { get; set; }
    public string? reason { get; set; }
    public string? dealer_mail { get; set; }
    public string? guid { get; set; }
    public string? transaction_id { get; set; }
    public int user_id { get; set; }
    public DateTime create_time { get; set; }
    public DateTime modify_time { get; set; }
}

