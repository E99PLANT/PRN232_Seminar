using Microsoft.EntityFrameworkCore;
using PaymentService.Domain.Entities;
using PaymentService.Domain.Interfaces;
using PaymentService.Infrastructure.Data;

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
        await _context.WalletEvents.AddAsync(walletEvent);
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
