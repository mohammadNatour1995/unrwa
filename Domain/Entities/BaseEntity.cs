namespace Domain.Entities;

public class BaseEntity
{
    public string CreateBy { get; set; } = string.Empty;
    public DateTime CreateDate { get; set; }
    public bool IsDeleted { get; set; }
    public string? DeletedBy { get; set; }
    public DateTime? DeletedDate { get; set; }
    public string? ModifiedBy { get; set; }
    public DateTime? ModifiedDate { get; set; }
}
