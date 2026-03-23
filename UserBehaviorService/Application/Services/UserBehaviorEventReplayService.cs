using UserBehaviorService.Application.Interfaces;
using UserBehaviorService.Application.MessagingContracts;
using UserBehaviorService.Application.ReadModels;

namespace UserBehaviorService.Application.Services
{
    public class UserBehaviorEventReplayService : IUserBehaviorEventReplayService
    {
        private readonly IRabbitMqRpcClient _rpcClient;
        private readonly ILogger<UserBehaviorEventReplayService> _logger;

        public UserBehaviorEventReplayService(
            IRabbitMqRpcClient rpcClient,
            ILogger<UserBehaviorEventReplayService> logger)
        {
            _rpcClient = rpcClient;
            _logger = logger;
        }

        public async Task<UserBehaviorEventReplayDetailReadModel> GetDetailAsync(
    string userId,
    CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("Starting event replay for user {UserId}", userId);

            try
            {
                var authReplayTask =
                    _rpcClient.CallAsync<AuthReplayRequestMessage, AuthReplayResponseMessage>(
                        "auth.replay.request",
                        new AuthReplayRequestMessage
                        {
                            UserId = userId,
                            CorrelationId = Guid.NewGuid().ToString("N")
                        },
                        cancellationToken);

                var authRawTask =
                    _rpcClient.CallAsync<RawEventsRequestMessage, RawEventsResponseMessage>(
                        "auth.raw-events.request",
                        new RawEventsRequestMessage
                        {
                            UserId = userId,
                            CorrelationId = Guid.NewGuid().ToString("N")
                        },
                        cancellationToken);

                var profileReplayTask =
                    _rpcClient.CallAsync<UserProfileReplayRequestMessage, UserProfileReplayResponseMessage>(
                        "profile.replay.request",
                        new UserProfileReplayRequestMessage
                        {
                            UserId = userId,
                            CorrelationId = Guid.NewGuid().ToString("N")
                        },
                        cancellationToken);

                var profileRawTask =
                    _rpcClient.CallAsync<RawEventsRequestMessage, RawEventsResponseMessage>(
                        "profile.raw-events.request",
                        new RawEventsRequestMessage
                        {
                            UserId = userId,
                            CorrelationId = Guid.NewGuid().ToString("N")
                        },
                        cancellationToken);

                await Task.WhenAll(authReplayTask, authRawTask, profileReplayTask, profileRawTask);

                var authReplay = await authReplayTask;
                var authRaw = await authRawTask;
                var profileReplay = await profileReplayTask;
                var profileRaw = await profileRawTask;

                _logger.LogInformation("Finished event replay for user {UserId}", userId);

                var summary = new UserBehaviorEventReplaySummaryReadModel
                {
                    UserId = userId,
                    Email = authReplay.Email ?? profileReplay.Email,
                    CurrentStatus = authReplay.CurrentStatus,

                    RegisteredAt = authReplay.RegisteredAt,
                    VerifiedAt = authReplay.VerifiedAt,

                    LoginCount = authReplay.LoginCount,
                    FirstLoginAt = authReplay.FirstLoginAt,
                    LastLoginAt = authReplay.LastLoginAt,

                    LockedCount = authReplay.LockedCount,
                    UnlockedCount = authReplay.UnlockedCount,
                    LastLockedAt = authReplay.LastLockedAt,
                    LastUnlockedAt = authReplay.LastUnlockedAt,

                    ProfileUpdateCount = profileReplay.ProfileUpdateCount,
                    LastProfileUpdatedAt = profileReplay.LastProfileUpdatedAt,

                    PreferredLoginHour = authReplay.PreferredLoginHour,
                    MostActiveWeekday = authReplay.MostActiveWeekday,

                    AverageDaysBetweenLogins = authReplay.AverageDaysBetweenLogins,
                    EstimatedActiveDaysSpan = authReplay.EstimatedActiveDaysSpan,
                    AverageSessionDurationMinutes = authReplay.AverageSessionDurationMinutes,

                    DataSource = "event-replay-rabbitmq"
                };

                return new UserBehaviorEventReplayDetailReadModel
                {
                    UserId = userId,
                    AuthEvents = authRaw.Events,
                    ProfileEvents = profileRaw.Events,
                    Summary = summary
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Event replay failed for user {UserId}", userId);
                throw;
            }
        }
    }
}
