using System;

namespace Domain.Common;

public abstract class EntityBase : IEntityBase
{
    public bool IsDeleted { get; set; }
    public DateTime CreatedDate { get; set; }
    public DateTime? LastModifiedDate { get; set; }
}