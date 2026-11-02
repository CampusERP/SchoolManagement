using Application.Common.Interfaces.Services;
using Application.Common.Models;
using Application.Features.Billing.Queries;
using Domain.Entities.Billing;
using Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistence.Services;

public class BillingReadService : IBillingReadService
{
    private readonly PlatformDbContext _db;

    public BillingReadService(PlatformDbContext db) => _db = db;

    public async Task<List<SubscriptionPlanDto>> GetSubscriptionPlansAsync(
        bool activeOnly = false, CancellationToken ct = default)
    {
        var query = _db.SubscriptionPlans.AsNoTracking();
        if (activeOnly) query = query.Where(p => p.IsActive);

        return await query
            .OrderBy(p => p.PriceMonthly)
            .Select(p => new SubscriptionPlanDto(
                p.Id, p.Name, p.DescriptionEn, p.DescriptionAr,
                p.Benefits
                    .OrderBy(b => b.SortOrder)
                    .Select(b => new SubscriptionPlanBenefitDto(
                        b.Id, b.DescriptionEn, b.DescriptionAr, b.SortOrder))
                    .ToList(),
                p.PriceMonthly, p.MaxStudents, p.MaxTeachers,
                p.HasParentPortal, p.HasExamModule, p.HasAnalytics, p.IsActive))
            .ToListAsync(ct);
    }

    public async Task<PagedResult<SubscriptionStatusDto>> GetSubscriptionsAsync(
        PaginationParams p, Guid? schoolId, string? search = null, CancellationToken ct = default)
    {
        var query = _db.Subscriptions.AsNoTracking()
            .Include(s => s.Invoices)
            .Where(s => !s.IsDeleted);

        if (schoolId.HasValue)
            query = query.Where(s => s.SchoolId == schoolId.Value);

        // One row per school — its most recent subscription.
        var all = await query.OrderByDescending(s => s.CreatedAtUtc).ToListAsync(ct);
        var latest = all.GroupBy(s => s.SchoolId).Select(g => g.First()).ToList();

        var schoolIds = latest.Select(s => s.SchoolId).Distinct().ToList();
        var schools = await _db.Schools.AsNoTracking()
            .Where(s => schoolIds.Contains(s.Id))
            .Select(s => new { s.Id, s.Name })
            .ToDictionaryAsync(s => s.Id, s => s.Name, ct);

        var planIds = latest.Select(s => s.SubscriptionPlanId).Distinct().ToList();
        var plans = await _db.SubscriptionPlans.AsNoTracking()
            .Include(pl => pl.Benefits)
            .Where(pl => planIds.Contains(pl.Id))
            .ToDictionaryAsync(pl => pl.Id, ct);

        var now = DateTime.UtcNow;
        var items = latest
            .Select(s => MapStatus(s, schools.GetValueOrDefault(s.SchoolId) ?? "Unknown school",
                plans.GetValueOrDefault(s.SubscriptionPlanId), now))
            .ToList();

        if (!string.IsNullOrWhiteSpace(search))
        {
            var term = search.Trim().ToLowerInvariant();
            items = items
                .Where(i => i.SchoolName.ToLowerInvariant().Contains(term) ||
                            i.PlanName.ToLowerInvariant().Contains(term) ||
                            i.Status.ToLowerInvariant().Contains(term))
                .ToList();
        }

        var total = items.Count;
        var pageItems = items
            .OrderByDescending(i => i.IsActive)
            .ThenBy(i => i.EndDate)
            .Skip(p.Skip).Take(p.PageSize)
            .ToList();

        return new PagedResult<SubscriptionStatusDto>(pageItems, total, p.Page, p.PageSize);
    }

    public async Task<PagedResult<InvoiceListDto>> GetInvoicesAsync(
        PaginationParams p, Guid? schoolId, string? search = null, string? status = null,
        CancellationToken ct = default)
    {
        var query = _db.Invoices.AsNoTracking()
            .Include(i => i.Payments)
            .Where(i => !i.IsDeleted);

        if (schoolId.HasValue)
            query = query.Where(i => i.SchoolId == schoolId.Value);

        var raw = await query.OrderByDescending(i => i.CreatedAtUtc).ToListAsync(ct);

        var schoolIds = raw.Select(i => i.SchoolId).Distinct().ToList();
        var schools = await _db.Schools.AsNoTracking()
            .Where(s => schoolIds.Contains(s.Id))
            .Select(s => new { s.Id, s.Name })
            .ToDictionaryAsync(s => s.Id, s => s.Name, ct);

        var now = DateTime.UtcNow;
        var items = raw
            .Select(i => MapInvoice(i, schools.GetValueOrDefault(i.SchoolId) ?? "Unknown school", now))
            .ToList();

        if (!string.IsNullOrWhiteSpace(status))
            items = items.Where(i => string.Equals(i.Status, status, StringComparison.OrdinalIgnoreCase)).ToList();

        if (!string.IsNullOrWhiteSpace(search))
        {
            var term = search.Trim().ToLowerInvariant();
            items = items
                .Where(i => i.SchoolName.ToLowerInvariant().Contains(term) ||
                            i.Id.ToString().Contains(term))
                .ToList();
        }

        var total = items.Count;
        var pageItems = items.Skip(p.Skip).Take(p.PageSize).ToList();
        return new PagedResult<InvoiceListDto>(pageItems, total, p.Page, p.PageSize);
    }

    public async Task<PagedResult<PaymentDto>> GetPaymentsAsync(
        PaginationParams p, Guid? schoolId, CancellationToken ct = default)
    {
        var query = _db.Payments.AsNoTracking()
            .Join(_db.Invoices.AsNoTracking().Where(i => !i.IsDeleted),
                payment => payment.InvoiceId, invoice => invoice.Id,
                (payment, invoice) => new { payment, invoice })
            .Where(x => !x.payment.IsDeleted);

        if (schoolId.HasValue)
            query = query.Where(x => x.invoice.SchoolId == schoolId.Value);

        var total = await query.CountAsync(ct);
        var items = await query
            .OrderByDescending(x => x.payment.PaidAtUtc)
            .Skip(p.Skip).Take(p.PageSize)
            .Select(x => new PaymentDto(
                x.payment.Id, x.payment.Amount, x.payment.Method.ToString(),
                x.payment.Reference, x.payment.PaidAtUtc))
            .ToListAsync(ct);

        return new PagedResult<PaymentDto>(items, total, p.Page, p.PageSize);
    }

    public async Task<SubscriptionStatusDto?> GetSubscriptionStatusAsync(
        Guid schoolId, CancellationToken ct = default)
    {
        var subscription = await _db.Subscriptions.AsNoTracking()
            .Include(s => s.Invoices)
            .Where(s => s.SchoolId == schoolId && !s.IsDeleted)
            .OrderByDescending(s => s.CreatedAtUtc)
            .FirstOrDefaultAsync(ct);

        if (subscription is null) return null;

        var schoolName = await _db.Schools.AsNoTracking()
            .Where(s => s.Id == schoolId)
            .Select(s => s.Name)
            .FirstOrDefaultAsync(ct) ?? "Unknown school";

        var plan = await _db.SubscriptionPlans.AsNoTracking()
            .Include(pl => pl.Benefits)
            .FirstOrDefaultAsync(pl => pl.Id == subscription.SubscriptionPlanId, ct);

        return MapStatus(subscription, schoolName, plan, DateTime.UtcNow);
    }

    public async Task<InvoiceDetailDto?> GetInvoiceByIdAsync(Guid invoiceId, CancellationToken ct = default)
    {
        var invoice = await _db.Invoices.AsNoTracking()
            .Include(i => i.Payments)
            .FirstOrDefaultAsync(i => i.Id == invoiceId && !i.IsDeleted, ct);

        if (invoice is null) return null;

        var schoolName = await _db.Schools.AsNoTracking()
            .Where(s => s.Id == invoice.SchoolId)
            .Select(s => s.Name)
            .FirstOrDefaultAsync(ct) ?? "Unknown school";

        var planName = await (
            from subscription in _db.Subscriptions.AsNoTracking()
            join plan in _db.SubscriptionPlans.AsNoTracking()
                on subscription.SubscriptionPlanId equals plan.Id
            where subscription.Id == invoice.SubscriptionId
            select plan.Name)
            .FirstOrDefaultAsync(ct) ?? "Unknown plan";

        var now = DateTime.UtcNow;
        var status = ResolveInvoiceStatus(invoice, now);

        return new InvoiceDetailDto(
            invoice.Id, invoice.SubscriptionId, invoice.SchoolId, schoolName, planName,
            invoice.Amount, invoice.PaidAmount, invoice.BalanceDue,
            invoice.DueDate, status, invoice.Notes, invoice.CreatedAtUtc,
            invoice.Payments
                .OrderByDescending(p => p.PaidAtUtc)
                .Select(p => new PaymentDto(p.Id, p.Amount, p.Method.ToString(), p.Reference, p.PaidAtUtc))
                .ToList());
    }

    // ── Mapping helpers ─────────────────────────────────────────────

    private static SubscriptionStatusDto MapStatus(
        Subscription s, string schoolName, SubscriptionPlan? plan, DateTime now)
    {
        var planName = plan?.Name ?? "Unknown plan";
        var status = s.Status.ToString();

        if (s.Status == SubscriptionStatus.Active && now > s.EndDate)
            status = SubscriptionStatus.Expired.ToString();

        var daysRemaining = s.EndDate <= now ? 0 : (int)Math.Ceiling((s.EndDate - now).TotalDays);

        var outstanding = s.Invoices
            .Where(i => !i.IsDeleted && i.Status != InvoiceStatus.Paid && i.Status != InvoiceStatus.Cancelled)
            .Sum(i => i.BalanceDue);

        var planBenefits = plan?.Benefits
            .OrderBy(b => b.SortOrder)
            .Select(b => new SubscriptionPlanBenefitDto(
                b.Id, b.DescriptionEn, b.DescriptionAr, b.SortOrder))
            .ToList() ?? [];

        return new SubscriptionStatusDto(
            s.Id, s.SchoolId, schoolName,
            s.SubscriptionPlanId, planName,
            plan?.DescriptionEn, plan?.DescriptionAr, planBenefits,
            plan?.PriceMonthly ?? 0m,
            status, s.StartDate, s.EndDate, s.AutoRenew,
            plan?.MaxStudents ?? 0, plan?.MaxTeachers ?? 0,
            plan?.HasParentPortal ?? false, plan?.HasExamModule ?? false, plan?.HasAnalytics ?? false,
            daysRemaining,
            s.Status == SubscriptionStatus.Active && now <= s.EndDate,
            outstanding);
    }

    private static InvoiceListDto MapInvoice(Invoice i, string schoolName, DateTime now) =>
        new(i.Id, i.SubscriptionId, i.SchoolId, schoolName,
            i.Amount, i.PaidAmount, i.BalanceDue, i.DueDate,
            ResolveInvoiceStatus(i, now), i.CreatedAtUtc);

    private static string ResolveInvoiceStatus(Invoice i, DateTime now) =>
        i.Status == InvoiceStatus.Issued && i.DueDate < now
            ? InvoiceStatus.Overdue.ToString()
            : i.Status.ToString();
}
