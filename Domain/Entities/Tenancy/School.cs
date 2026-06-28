using Domain.Common;

namespace Domain.Entities.Tenancy;

/// <summary>
/// The tenant itself. Root of all tenant data.
/// it has no SchoolId because it IS the
/// tenant key everything else hangs off.
/// </summary>
public class School : AuditableEntity, IAggregateRoot
{
    public string Name { get; private set; } = default!;
    public string SubdomainCode { get; private set; } = default!;
    public string Status { get; private set; } = "Active";

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

    public void AddCampus(string name, string address)
    {
        _campuses.Add(Campus.Create(Id, name, address));
    }

    public void Suspend() => Status = "Suspended";
    public void Reactivate() => Status = "Active";
}
