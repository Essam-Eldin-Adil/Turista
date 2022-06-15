using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace Turista.Data
{
    public abstract class Entity
    {
        public Entity()
        {
            Id = new Guid();
        }
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public Guid Id { get; set; }
        public int? Order { get; set; }

        public DateTime CreatedDate { get; set; } = Convert.ToDateTime(DateTime.Now);
        public bool IsDeleted { get; set; } = false;
    }
}
