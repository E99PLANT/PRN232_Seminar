using UserBehaviorService.Application.Interfaces;
using UserBehaviorService.Application.ReadModels;
using UserBehaviorService.Domain.Entities;

namespace UserBehaviorService.Application.Services
{
    public class UserBehaviorAnalyticsService : IUserBehaviorAnalyticsService
    {
        private readonly IUserBehaviorRepository _behaviorRepository;
        private readonly IUserLoginHistoryRepository _loginHistoryRepository;
        private readonly IConsumedMessageRepository _consumedMessageRepository;

        public UserBehaviorAnalyticsService(
            IUserBehaviorRepository behaviorRepository,
            IUserLoginHistoryRepository loginHistoryRepository,
            IConsumedMessageRepository consumedMessageRepository)
        {
            _behaviorRepository = behaviorRepository;
            _loginHistoryRepository = loginHistoryRepository;
            _consumedMessageRepository = consumedMessageRepository;
        }

        public async Task HandleUserRegisteredAsync(string messageId, string userId, string email, DateTimeOffset occurredOn)
        {
            if (await _consumedMessageRepository.ExistsAsync(messageId)) return;

            var projection = await EnsureProjectionAsync(userId, email);
            projection.Email = email;
            projection.CurrentStatus = "Pending";
            projection.UpdatedAt = DateTimeOffset.UtcNow;

            await UpsertProjectionAsync(projection);
            await _consumedMessageRepository.AddAsync(messageId, "user.registered");
        }

        public async Task HandleUserVerifiedAsync(string messageId, string userId, string email, DateTimeOffset occurredOn)
        {
            if (await _consumedMessageRepository.ExistsAsync(messageId)) return;

            var projection = await EnsureProjectionAsync(userId, email);
            projection.Email = email;
            projection.CurrentStatus = "Active";
            projection.UpdatedAt = DateTimeOffset.UtcNow;

            await UpsertProjectionAsync(projection);
            await _consumedMessageRepository.AddAsync(messageId, "user.verified");
        }

        public async Task HandleUserLoggedInAsync(string messageId, string userId, string email, DateTimeOffset occurredOn)
        {
            if (await _consumedMessageRepository.ExistsAsync(messageId)) return;

            var projection = await EnsureProjectionAsync(userId, email);

            projection.Email = email;
            projection.LoginCount += 1;
            projection.LastLoginAt = occurredOn;
            projection.FirstLoginAt ??= occurredOn;
            projection.UpdatedAt = DateTimeOffset.UtcNow;

            await _loginHistoryRepository.AddAsync(new UserLoginHistory
            {
                UserId = userId,
                Email = email,
                LoggedInAt = occurredOn,
                LoginHour = occurredOn.Hour,
                Weekday = occurredOn.DayOfWeek.ToString(),
                DateOnlyUtc = occurredOn.UtcDateTime.Date
            });

            await RecalculateLoginAnalyticsAsync(projection, userId);
            await UpsertProjectionAsync(projection);
            await _consumedMessageRepository.AddAsync(messageId, "user.logged_in");
        }

        public async Task HandleUserLockedAsync(string messageId, string userId, DateTimeOffset occurredOn)
        {
            if (await _consumedMessageRepository.ExistsAsync(messageId)) return;

            var projection = await EnsureProjectionAsync(userId, null);
            projection.CurrentStatus = "Locked";
            projection.UpdatedAt = DateTimeOffset.UtcNow;

            await UpsertProjectionAsync(projection);
            await _consumedMessageRepository.AddAsync(messageId, "user.locked");
        }

        public async Task HandleUserUnlockedAsync(string messageId, string userId, DateTimeOffset occurredOn)
        {
            if (await _consumedMessageRepository.ExistsAsync(messageId)) return;

            var projection = await EnsureProjectionAsync(userId, null);
            projection.CurrentStatus = "Active";
            projection.UpdatedAt = DateTimeOffset.UtcNow;

            await UpsertProjectionAsync(projection);
            await _consumedMessageRepository.AddAsync(messageId, "user.unlocked");
        }

        public async Task HandleUserProfileCreatedAsync(string messageId, string userId, string email, DateTimeOffset occurredOn)
        {
            if (await _consumedMessageRepository.ExistsAsync(messageId)) return;

            var projection = await EnsureProjectionAsync(userId, email);
            projection.Email ??= email;
            projection.UpdatedAt = DateTimeOffset.UtcNow;

            await UpsertProjectionAsync(projection);
            await _consumedMessageRepository.AddAsync(messageId, "userprofile.created");
        }

        public async Task HandleUserProfileUpdatedAsync(string messageId, string userId, DateTimeOffset occurredOn)
        {
            if (await _consumedMessageRepository.ExistsAsync(messageId)) return;

            var projection = await EnsureProjectionAsync(userId, null);
            projection.ProfileUpdateCount += 1;
            projection.LastProfileUpdatedAt = occurredOn;
            projection.UpdatedAt = DateTimeOffset.UtcNow;

            await UpsertProjectionAsync(projection);
            await _consumedMessageRepository.AddAsync(messageId, "userprofile.updated");
        }

        public async Task<UserBehaviorReadModel?> GetByUserIdAsync(string userId)
        {
            var projection = await _behaviorRepository.GetByUserIdAsync(userId);
            if (projection == null) return null;

            return new UserBehaviorReadModel
            {
                UserId = projection.UserId,
                Email = projection.Email,
                CurrentStatus = projection.CurrentStatus,
                LoginCount = projection.LoginCount,
                FirstLoginAt = projection.FirstLoginAt,
                LastLoginAt = projection.LastLoginAt,
                ProfileUpdateCount = projection.ProfileUpdateCount,
                LastProfileUpdatedAt = projection.LastProfileUpdatedAt,
                PreferredLoginHour = projection.PreferredLoginHour,
                MostActiveWeekday = projection.MostActiveWeekday,
                AverageDaysBetweenLogins = projection.AverageDaysBetweenLogins,
                EstimatedActiveDaysSpan = projection.EstimatedActiveDaysSpan,
                AverageSessionDurationMinutes = projection.AverageSessionDurationMinutes
            };
        }

        private async Task<UserBehaviorProjection> EnsureProjectionAsync(string userId, string? email)
        {
            var projection = await _behaviorRepository.GetByUserIdAsync(userId);
            if (projection != null) return projection;

            return new UserBehaviorProjection
            {
                UserId = userId,
                Email = email,
                CurrentStatus = "Pending"
            };
        }

        private async Task UpsertProjectionAsync(UserBehaviorProjection projection)
        {
            var existed = await _behaviorRepository.GetByUserIdAsync(projection.UserId);
            if (existed == null)
                await _behaviorRepository.AddAsync(projection);
            else
                await _behaviorRepository.UpdateAsync(projection);
        }

        //private async Task RecalculateLoginAnalyticsAsync(UserBehaviorProjection projection, string userId)
        //{
        //    var logins = await _loginHistoryRepository.GetByUserIdAsync(userId);
        //    if (!logins.Any()) return;

        //    projection.PreferredLoginHour = logins
        //        .GroupBy(x => x.LoginHour)
        //        .OrderByDescending(g => g.Count())
        //        .ThenBy(g => g.Key)
        //        .First()
        //        .Key;

        //    projection.MostActiveWeekday = logins
        //        .GroupBy(x => x.Weekday)
        //        .OrderByDescending(g => g.Count())
        //        .Select(g => g.Key)
        //        .FirstOrDefault();

        //    if (projection.FirstLoginAt.HasValue && projection.LastLoginAt.HasValue)
        //    {
        //        projection.EstimatedActiveDaysSpan =
        //            (projection.LastLoginAt.Value.Date - projection.FirstLoginAt.Value.Date).Days;
        //    }

        //    if (logins.Count >= 2)
        //    {
        //        var diffs = new List<double>();
        //        for (int i = 1; i < logins.Count; i++)
        //        {
        //            diffs.Add((logins[i].LoggedInAt - logins[i - 1].LoggedInAt).TotalDays);
        //        }

        //        projection.AverageDaysBetweenLogins = diffs.Average();
        //    }
        //}

        private async Task RecalculateLoginAnalyticsAsync(UserBehaviorProjection projection, string userId)
        {
            var logins = await _loginHistoryRepository.GetByUserIdAsync(userId);
            if (!logins.Any())
            {
                projection.PreferredLoginHour = -1;
                projection.MostActiveWeekday = null;
                projection.AverageDaysBetweenLogins = 0;
                projection.EstimatedActiveDaysSpan = 0;
                projection.AverageSessionDurationMinutes = 0;
                return;
            }

            projection.PreferredLoginHour = logins
                .GroupBy(x => x.LoginHour)
                .OrderByDescending(g => g.Count())
                .ThenBy(g => g.Key)
                .First()
                .Key;

            projection.MostActiveWeekday = logins
                .GroupBy(x => x.Weekday)
                .OrderByDescending(g => g.Count())
                .ThenBy(g => g.Key)
                .Select(g => g.Key)
                .FirstOrDefault();

            if (projection.FirstLoginAt.HasValue && projection.LastLoginAt.HasValue)
            {
                projection.EstimatedActiveDaysSpan =
                    (projection.LastLoginAt.Value.Date - projection.FirstLoginAt.Value.Date).Days;
            }
            else
            {
                projection.EstimatedActiveDaysSpan = 0;
            }

            // AverageDaysBetweenLogins
            if (logins.Count >= 2)
            {
                var dayDiffs = new List<double>();

                for (int i = 1; i < logins.Count; i++)
                {
                    dayDiffs.Add((logins[i].LoggedInAt - logins[i - 1].LoggedInAt).TotalDays);
                }

                projection.AverageDaysBetweenLogins = Math.Round(dayDiffs.Average(), 2);
            }
            else
            {
                projection.AverageDaysBetweenLogins = 0;
            }

            // AverageSessionDurationMinutes (ước lượng đơn giản)
            // Logic:
            // - lấy khoảng cách từ login hiện tại đến login kế tiếp
            // - cap tối đa 30 phút để tránh phiên bị kéo quá dài không thực tế
            // - login cuối cùng: tạm tính mặc định 15 phút
            var estimatedSessionMinutes = new List<double>();

            if (logins.Count == 1)
            {
                estimatedSessionMinutes.Add(15);
            }
            else
            {
                for (int i = 0; i < logins.Count; i++)
                {
                    if (i < logins.Count - 1)
                    {
                        var diffMinutes = (logins[i + 1].LoggedInAt - logins[i].LoggedInAt).TotalMinutes;

                        // nếu 2 lần login quá gần nhau thì lấy đúng diff
                        // nếu quá xa thì cap 30 phút
                        var estimated = Math.Min(diffMinutes, 30);

                        // tránh số âm hoặc 0 bất thường
                        if (estimated <= 0)
                            estimated = 15;

                        estimatedSessionMinutes.Add(estimated);
                    }
                    else
                    {
                        // login cuối cùng không có login kế tiếp để suy ra
                        estimatedSessionMinutes.Add(15);
                    }
                }
            }

            projection.AverageSessionDurationMinutes = Math.Round(estimatedSessionMinutes.Average(), 2);
        }
    }
}
