using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Marketplace.Db;
using Marketplace.Db.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace Marketplace.Escrow.DataProcessing
{
    public abstract class DataProcessingService<TModel> : BackgroundService where TModel : class, IDataToProcess
    {
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly ILogger _logger;

        public DataProcessingService(IServiceScopeFactory scopeFactory, ILogger logger)
        {
            _scopeFactory = scopeFactory;
            _logger = logger;
        }
        
        public async Task ScheduleInProgressRun(CancellationToken stoppingToken)
        {
            using var scope = _scopeFactory.CreateScope();
            var dbContext = scope.ServiceProvider.GetService<MarketplaceDbContext>();
            var dataToProcesses = await dbContext!.Set<TModel>().Where(t => t.Status == ProcessingDataStatus.InProgress).ToListAsync();
            var now = DateTime.UtcNow;
            foreach (var dataToProcess in dataToProcesses)
            {
#pragma warning disable 4014
                Task.Run(async () =>
#pragma warning restore 4014
                    {
                        var lockTime = dataToProcess.LockTime ?? now;
                        if (lockTime < now)
                        {
                            lockTime = now;
                        }

                        await Task.Delay(lockTime - now, stoppingToken);
                        stoppingToken.ThrowIfCancellationRequested();
                        await Run(stoppingToken);
                    }
                , stoppingToken);
            }
        }

        public async Task Run(CancellationToken stoppingToken)
        {
            while (true)
            {
                CancellationTokenSource refreshLockCancellationTokenSource = new();
                TModel? dataToProcess = null;
                try
                {
                    dataToProcess = await AcquireDataToProcess(stoppingToken);
                    if (dataToProcess == null || stoppingToken.IsCancellationRequested)
                    {
                        return;
                    }

#pragma warning disable 4014
                    Task.Run(() => RefreshLock(dataToProcess, refreshLockCancellationTokenSource.Token),
                        refreshLockCancellationTokenSource.Token);
#pragma warning restore 4014

                    stoppingToken.ThrowIfCancellationRequested();

                    await Process(dataToProcess);

                    await SaveDoneStatus(dataToProcess.Id);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "{ServiceName} failed", GetType().FullName);
                    if (dataToProcess != null)
                    {
                        await SaveFailedStatus(dataToProcess.Id, ex);
                    }
                }
                finally
                {
                    refreshLockCancellationTokenSource.Cancel();
                }

            }
        }

        private async Task SaveFailedStatus(Guid id, Exception exception)
        {
            var updated = false;
            do
            {
                try
                {
                    using var scope = _scopeFactory.CreateScope();
                    var dbContext = scope.ServiceProvider.GetService<MarketplaceDbContext>();
                    // ReSharper disable once MethodSupportsCancellation
                    var dataToProcess = await dbContext!.Set<TModel>().FirstAsync(t => t.Id == id);

                    dataToProcess.Status = ProcessingDataStatus.Error;
                    dataToProcess.ErrorMessage = exception.Message;
                    // ReSharper disable once MethodSupportsCancellation
                    await dbContext.SaveChangesAsync();
                    updated = true;
                }
                catch (DbUpdateConcurrencyException)
                {
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "{ServiceName} Failed to change status of processed data to error", GetType().FullName);
                    throw;
                }
            } while (!updated);

        }

        private async Task SaveDoneStatus(Guid id)
        {
            var updated = false;
            do
            {
                try
                {
                    using var scope = _scopeFactory.CreateScope();
                    var dbContext = scope.ServiceProvider.GetService<MarketplaceDbContext>();
                    // ReSharper disable once MethodSupportsCancellation
                    var dataToProcess = await dbContext!.Set<TModel>().FirstAsync(t => t.Id == id);

                    dataToProcess.Status = ProcessingDataStatus.Done;
                    // ReSharper disable once MethodSupportsCancellation
                    await dbContext.SaveChangesAsync();
                    updated = true;
                }
                catch (DbUpdateConcurrencyException)
                {
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "{ServiceName} Failed to change status of processed data to done", GetType().FullName);
                    throw;
                }
            } while (!updated);
        }

        public abstract Task Process(TModel dataToProcess);
        
        private async Task? RefreshLock(TModel transaction, CancellationToken token)
        {
            await Task.Delay(TimeSpan.FromMinutes(15), token);
            if (token.IsCancellationRequested)
            {
                return;
            }

            using var scope = _scopeFactory.CreateScope();
            var dbContext = scope.ServiceProvider.GetService<MarketplaceDbContext>();
            var refresh = await dbContext!.Set<TModel>().FirstOrDefaultAsync(t => t.Id == transaction.Id, token);
            token.ThrowIfCancellationRequested();
            refresh.LockTime = DateTime.UtcNow.AddMinutes(20);
            await dbContext.SaveChangesAsync(token);
#pragma warning disable 4014
            Task.Run(() => RefreshLock(transaction, token), token);
#pragma warning restore 4014
        }


        private async Task<TModel?> AcquireDataToProcess(CancellationToken cancellationToken)
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                var now = DateTime.UtcNow;
                try
                {
                    using var scope = _scopeFactory.CreateScope();
                    var dbContext = scope.ServiceProvider.GetService<MarketplaceDbContext>();
                    var dataToProcess = await dbContext!.Set<TModel>().FirstOrDefaultAsync(t =>
                            t.Status == ProcessingDataStatus.InProgress && t.LockTime == null || t.LockTime < now,
                        cancellationToken);
                    if (dataToProcess == null)
                    {
                        return null;
                    }

                    dataToProcess.LockTime = now.AddMinutes(20);
                    await dbContext.SaveChangesAsync(cancellationToken);
                    return dataToProcess;
                }
                catch (DbUpdateConcurrencyException)
                {
                }
            }

            return null;
        }
        
        
        protected void RunInterval(CancellationToken stoppingToken)
        {
            Task.Run(async () =>
            {
                if (stoppingToken.IsCancellationRequested)
                {
                    return;
                }

                try
                {
                    await Run(stoppingToken);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "{ServiceName} failed", GetType().FullName);
                }
                
                await Task.Delay(TimeSpan.FromMinutes(1), stoppingToken);
                RunInterval(stoppingToken);
            }, stoppingToken);
        }

    }
}