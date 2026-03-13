using Microsoft.EntityFrameworkCore;
using PaymentService.Domain.Entities;
using PaymentService.Domain.Interfaces;
using PaymentService.Infrastructure.Data;
using System.Security.Cryptography;
using System.Text;

namespace PaymentService.Infrastructure.Repositories;

public class WalletRepository : IWalletRepository
{
    private readonly WalletDbContext _context;

    public WalletRepository(WalletDbContext context)
    {
        _context = context;
    }

    // === Account ===

    public async Task<Account?> GetAccountByIdAsync(Guid accountId)
    {
        return await _context.Accounts
            .Include(a => a.Wallet)
            .FirstOrDefaultAsync(a => a.Id == accountId);
    }

    public async Task<Account?> GetAccountByUsernameAsync(string username)
    {
        return await _context.Accounts
            .Include(a => a.Wallet)
            .FirstOrDefaultAsync(a => a.Username == username);
    }

    public async Task<Account?> GetAccountByEmailAsync(string email)
    {
        return await _context.Accounts
            .Include(a => a.Wallet)
            .FirstOrDefaultAsync(a => a.Email == email);
    }

    public async Task<Account> CreateAccountAsync(Account account)
    {
        await _context.Accounts.AddAsync(account);
        await _context.SaveChangesAsync();
        return account;
    }

    // === Wallet ===

    public async Task<Wallet?> GetWalletByAccountIdAsync(Guid accountId)
    {
        return await _context.Wallets
            .Include(w => w.Account)
            .FirstOrDefaultAsync(w => w.AccountId == accountId);
    }

    public async Task UpdateWalletAsync(Wallet wallet)
    {
        _context.Wallets.Update(wallet);
        await Task.CompletedTask;
    }

    // === Transactions ===

    public async Task<WalletTransaction> AddTransactionAsync(WalletTransaction transaction)
    {
        await _context.WalletTransactions.AddAsync(transaction);
        return transaction;
    }

    public async Task<IEnumerable<WalletTransaction>> GetTransactionsByWalletIdAsync(Guid walletId, int count = 20)
    {
        return await _context.WalletTransactions
            .Where(t => t.WalletId == walletId)
            .OrderByDescending(t => t.Timestamp)
            .Take(count)
            .ToListAsync();
    }

    // === Phát hiện bất thường ===

    public async Task<IEnumerable<WalletTransaction>> GetTransactionsInTimeRangeAsync(
        Guid walletId, DateTime from, DateTime to)
    {
        return await _context.WalletTransactions
            .Where(t => t.WalletId == walletId && t.Timestamp >= from && t.Timestamp <= to)
            .OrderByDescending(t => t.Timestamp)
            .ToListAsync();
    }

    public async Task<IEnumerable<WalletTransaction>> GetSuspiciousTransactionsAsync(int count = 20)
    {
        return await _context.WalletTransactions
            .Where(t => t.IsSuspicious)
            .OrderByDescending(t => t.Timestamp)
            .Take(count)
            .ToListAsync();
    }

    // === Event Sourcing ===

    public async Task AppendEventAsync(WalletEvent walletEvent)
    {
        // Truncate timestamp to microseconds (PostgreSQL precision)
        // để hash trước/sau khi lưu DB luôn giống nhau
        var ticks = walletEvent.Timestamp.Ticks;
        walletEvent.Timestamp = new DateTime(ticks - (ticks % 10), walletEvent.Timestamp.Kind);

        // Kiểm tra change tracker trước (cho trường hợp ghi nhiều events liên tiếp trước SaveChanges)
        var localLast = _context.ChangeTracker.Entries<WalletEvent>()
            .Where(e => e.Entity.AggregateId == walletEvent.AggregateId && e.State == EntityState.Added)
            .OrderByDescending(e => e.Entity.Timestamp)
            .Select(e => e.Entity)
            .FirstOrDefault();

        if (localLast != null)
        {
            // Có event chưa save → dùng hash của nó
            walletEvent.PreviousHash = localLast.Hash;
        }
        else
        {
            // Query DB cho event cuối cùng đã save
            var dbLast = await _context.WalletEvents
                .Where(e => e.AggregateId == walletEvent.AggregateId)
                .OrderByDescending(e => e.Timestamp)
                .FirstOrDefaultAsync();
            walletEvent.PreviousHash = dbLast?.Hash ?? "GENESIS";
        }

        walletEvent.Hash = ComputeHash(walletEvent);
        await _context.WalletEvents.AddAsync(walletEvent);
    }

    /// <summary>
    /// Tính SHA256 hash = SHA256(PreviousHash + EventType + EventData + Timestamp)
    /// Dùng format "yyyy-MM-ddTHH:mm:ss.ffffff" để khớp với PostgreSQL microsecond precision
    /// </summary>
    public static string ComputeHash(WalletEvent e)
    {
        var ts = e.Timestamp.ToString("yyyy-MM-ddTHH:mm:ss.ffffffZ");
        var raw = $"{e.PreviousHash}|{e.EventType}|{e.EventData}|{ts}";
        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(raw));
        return Convert.ToHexString(bytes);
    }

    public async Task<IEnumerable<WalletEvent>> GetEventsByWalletIdAsync(Guid walletId)
    {
        return await _context.WalletEvents
            .Where(e => e.AggregateId == walletId)
            .OrderBy(e => e.Timestamp)
            .ToListAsync();
    }

    // === Save ===

    public async Task SaveChangesAsync()
    {
        await _context.SaveChangesAsync();
    }
}
