using System;

namespace PutZige.Domain.Entities
{
    /// <summary>
    /// Abstract base class for all entities, providing common properties and soft delete support.
    /// </summary>
    public abstract class BaseEntity
    {
        /// <summary>
        /// Primary key identifier.
        /// </summary>
        public Guid Id { get; set; }

        /// <summary>
        /// UTC timestamp when the entity was created.
        /// </summary>
        public DateTime CreatedAt { get; set; }

        /// <summary>
        /// UTC timestamp when the entity was last updated.
        /// </summary>
        public DateTime? UpdatedAt { get; set; }

        /// <summary>
        /// UTC timestamp when the entity was soft deleted.
        /// </summary>
        public DateTime? DeletedAt { get; set; }

        /// <summary>
        /// Indicates whether the entity is soft deleted.
        /// </summary>
        public bool IsDeleted { get; set; }
    }
}
