﻿using BankApp.Models;
using BankApp.Data;
using Microsoft.EntityFrameworkCore;

namespace BankApp.Repositories;

public class AccountRepository
{
    private readonly ApplicationDbContext _context;

    public AccountRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task SaveAsync(Account account)
    {
        if (account.AccountId == 0)
            await _context.Accounts.AddAsync(account);
        else
            _context.Accounts.Update(account);

        await _context.SaveChangesAsync();
    }

    public async Task<Account> FindByIdAsync(long id)
    {
        return await _context.Accounts
            .FirstOrDefaultAsync(a => a.AccountId == id);
    }

    public async Task DeleteAsync(Account account)
    {
        var relatedTransactionIds = await _context.Transactions
            .Where(t => t.FromAccountId == account.AccountId || t.ToAccountId == account.AccountId)
            .Select(t => t.Id)
            .ToListAsync();

        _context.Accounts.Remove(account);
        await _context.SaveChangesAsync();

        var orphanedTransactions = await _context.Transactions
            .Where(t => relatedTransactionIds.Contains(t.Id))
            .Where(t =>
                !_context.Accounts.Any(a => a.AccountId == t.FromAccountId) &&
                !_context.Accounts.Any(a => a.AccountId == t.ToAccountId))
            .ToListAsync();

        if (orphanedTransactions.Any())
        {
            _context.Transactions.RemoveRange(orphanedTransactions);
            await _context.SaveChangesAsync();
        }
    }

    public async Task<bool> IsAccountOwnedByClientAsync(
        long accountId,
        long clientId
    )
    {
        return await _context.Accounts
            .AnyAsync(a => a.AccountId == accountId && a.ClientId == clientId);
    }
}