using System;

namespace PutZige.Domain.Entities
{
    /// <summary>
    /// Abstract base class for auditable entities, providing user tracking fields.
    /// </summary>
    public abstract class AuditableEntity : BaseEntity
    {
        /// <summary>
        /// User who created the entity.
        /// </summary>
        public Guid? CreatedBy { get; set; }

        /// <summary>
        /// User who last updated the entity.
        /// </summary>
        public Guid? UpdatedBy { get; set; }

        /// <summary>
        /// User who deleted the entity.
        /// </summary>
        public Guid? DeletedBy { get; set; }
    }
}
