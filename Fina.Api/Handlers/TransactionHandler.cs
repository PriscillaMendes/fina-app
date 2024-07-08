using Fina.Api.Data;
using Fina.Core.Common;
using Fina.Core.Handlers;
using Fina.Core.Models;
using Fina.Core.Requests.Transactions;
using Fina.Core.Responses;
using Microsoft.EntityFrameworkCore;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;

namespace Fina.Api.Handlers;

public class TransactionHandler(AppDbContext context) : ITransactionHandler
{
    public async Task<Response<Transaction?>> CreateAsync(CreateTransactionRequest request)
    {
        try
        {
            if (request is { Type: Core.Enums.ETransactionsType.Withdraw, Amount: >= 0 })
            {
                request.Amount *= -1;
            }

            var transaction = new Transaction
            {
                UserId = request.UserId,
                CategoryId = request.CategoryId,
                CreatedAt = DateTime.Now,
                Amount = request.Amount,
                PaidOrReceivedAt = DateTime.Now,
                Title = request.Title,
                Type = request.Type
            };

            await context.Transactions.AddAsync(transaction);
            await context.SaveChangesAsync();

            return new Response<Transaction?>(transaction, 200, "Nova transação criada!");

        }catch
        {
            return new Response<Transaction?>(null, 200, "Erro ao criar transação!");
        }
        
    }

    public async Task<Response<Transaction?>> DeleteAsync(DeleteTransactionRequest request)
    {
        try
        {
            var transaction = await context
                                .Transactions
                                .FirstOrDefaultAsync(
                                    x => x.Id == request.Id &&
                                    x.UserId == request.UserId);

            if (transaction == null)
            {
                return new Response<Transaction?>(null, 404, "Transação não encontrada!");
            }

            context.Transactions.Remove(transaction);
            await context.SaveChangesAsync();

            return new Response<Transaction?>(transaction, 200, "Transação deletada com sucesso!");

        }
        catch
        {
            return new Response<Transaction?>(null, 500, "Erro ao remover transação!");
        }
    }

    public async Task<Response<Transaction?>> GetByIdAsync(GetTransactionByIdRequest request)
    {
        try
        {
            var transaction = await context
                                .Transactions
                                .FirstOrDefaultAsync(
                                    x => x.Id == request.Id &&
                                    x.UserId == request.UserId);

            if (transaction == null)
            {
                return new Response<Transaction?>(null, 404, "Transação não encontrada!");
            }


            return new Response<Transaction?>(transaction, 200, null);

        }
        catch
        {
            return new Response<Transaction?>(null, 500, "Erro ao localizar transação!");
        }
    }

    public async Task<PagedResponse<List<Transaction>?>> GetByPeriodAsync(GetTransactionByPeriodRequest request)
    {
        try
        {
            request.StartDate ??= DateTime.Now.GetFirstDay();
            request.EndDate ??= DateTime.Now.GetLastDay();

            var query = context
                            .Transactions
                            .AsNoTracking()
                            .Where(x => 
                                x.UserId == request.UserId && 
                                x.PaidOrReceivedAt >= request.StartDate && 
                                x.PaidOrReceivedAt <= request.EndDate)
                            .OrderBy(x => x.PaidOrReceivedAt);



            var transactions = await query
                                    .Skip((request.PageNumber - 1) * request.PageSize)
                                    .Take(request.PageSize)
                                    .ToListAsync();

            var count = await query.CountAsync();

            return new PagedResponse<List<Transaction>?>(
                    transactions,
                    count,
                    request.PageNumber,
                    request.PageSize);

        }
        catch
        {
            return new PagedResponse<List<Transaction>?>(null, 500, message:"Erro ao localizar transação!");
        }
    }

    public async Task<Response<Transaction?>> UpdateAsync(UpdateTransactionRequest request)
    {
        try
        {
            if (request is { Type: Core.Enums.ETransactionsType.Withdraw, Amount: >= 0 })
            {
                request.Amount *= -1;
            }

            var transaction = await context
                                .Transactions
                                .FirstOrDefaultAsync(
                                    x => x.Id == request.Id && 
                                      x.UserId == request.UserId);
            
            if(transaction == null)
            {
                return new Response<Transaction?>(null, 404, "Transação não encontrada!");
            }

            transaction.CategoryId = request.CategoryId;
            transaction.Amount = request.Amount;
            transaction.PaidOrReceivedAt = request.PaidOrReceivedAt;
            transaction.Title = request.Title;
            transaction.Type = request.Type;

            context.Transactions.Update(transaction);
            await context.SaveChangesAsync();

            return new Response<Transaction?>(transaction, 200, "Transação alterada com sucesso!");

        }
        catch
        {
            return new Response<Transaction?>(null, 500, "Erro ao alterar transação!");
        }

    }
}
