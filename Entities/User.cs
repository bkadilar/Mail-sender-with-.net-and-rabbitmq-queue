using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
namespace Entities
{
    public class User
    {
        [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int id { get; set; }
        public string guid { get; set; } = Guid.NewGuid().ToString();
        public int active { get; set; } = 1;
        public string dealer_mail { get; set; }
        public string site_url { get; set; }
        public string secret_key { get; set; }
        public string user_name { get; set; }
        public string password { get; set; }
    }
}

