using Domain.Common;

namespace Domain.Entities.Tenancy;

/// <summary>
/// Represents a school entity in the system, which serves as an aggregate root for related entities such as campuses.
/// </summary>
public class School : AuditableEntity, IAggregateRoot
{
    public string Name { get; private set; } = default!;
    public string SubdomainCode { get; private set; } = default!;
    public string Status { get; private set; } = "Active";
    public string? Address { get; private set; }
    public string? Phone { get; private set; }
    public string? Email { get; private set; }

    private readonly List<Campus> _campuses = new();
    public IReadOnlyCollection<Campus> Campuses => _campuses.AsReadOnly();

    private School() { } // EF Core

    private School(Guid id, string name, string subdomainCode) : base(id)
    {
        Name = name;
        SubdomainCode = subdomainCode;
        Status = "Active";
    }   

    public static School Create(string name, string subdomainCode)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("School name is required.", nameof(name));
        if (string.IsNullOrWhiteSpace(subdomainCode))
            throw new ArgumentException("Subdomain code is required.", nameof(subdomainCode));

        return new School(Guid.NewGuid(), name, subdomainCode.ToLowerInvariant());
    }

    public void Update(string name, string? address, string? phone, string? email)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Name is required.");
        Name = name;
        Address = address;
        Phone = phone;
        Email = email;
    }

    public Campus AddCampus(string name, string address)
    {
        var campus = Campus.Create(Id, name, address);
        _campuses.Add(campus);
        return campus;
    }

    public void Suspend() => Status = "Suspended";
    public void Reactivate() => Status = "Active";
}
