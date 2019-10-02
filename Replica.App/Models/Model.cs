using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Replica.App.Models
{
    public class Model
    {
        [Key]
        public int Id { get; set; }
    }
}