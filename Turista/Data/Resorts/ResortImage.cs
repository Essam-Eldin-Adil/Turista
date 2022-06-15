using System;
using Turista.Data.General;

namespace Turista.Data.Resorts
{
    public class ResortImage : Entity
    {
        public bool IsPrimary { get; set; }
        public Guid FileId { get; set; }
        public Guid ResortId { get; set; }
        public File File { get; set; }
        public Resort Resort { get; set; }
    }
}
