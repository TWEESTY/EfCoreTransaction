using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EfCoreUnitOfWork.Entities
{
    public class NotificationEntity
    {
        [Key]
        public int Id { get; set; }
        public required string Text { get; set; }
    }
}
