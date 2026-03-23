using AuthService.Application.ReadModels;
using AuthService.Domain.Events;

namespace AuthService.Application.Projectors
{
    public class AuthBehaviorProjector
    {
        public AuthBehaviorReadModel Build(string userId, IEnumerable<DomainEvent> events)
        {
            var model = new AuthBehaviorReadModel
            {
                UserId = userId
            };

            var loginPoints = new List<(DateTimeOffset LoginAt, int SessionDurationMinutes)>();

            foreach (var ev in events.OrderBy(x => x.OccurredOn))
            {
                switch (ev)
                {
                    case UserRegisteredEvent e:
                        model.Email = e.Email;
                        model.RegisteredAt = e.OccurredOn;
                        model.CurrentStatus = "Pending";
                        break;

                    case UserOtpVerifiedEvent e:
                        model.Email = e.Email;
                        model.VerifiedAt = e.OccurredOn;
                        model.CurrentStatus = "Active";
                        break;

                    case UserLoggedInEvent e:
                        {
                            var loginAt = ResolveLoginTime(e);

                            model.Email = e.Email;
                            model.LoginCount++;

                            if (!model.FirstLoginAt.HasValue || loginAt < model.FirstLoginAt.Value)
                                model.FirstLoginAt = loginAt;

                            if (!model.LastLoginAt.HasValue || loginAt > model.LastLoginAt.Value)
                                model.LastLoginAt = loginAt;

                            loginPoints.Add((loginAt, NormalizeSessionDuration(e.SessionDurationMinutes)));
                            break;
                        }

                    case UserLockedEvent:
                        model.LockedCount++;
                        model.LastLockedAt = ev.OccurredOn;
                        model.CurrentStatus = "Locked";
                        break;

                    case UserUnlockedEvent:
                        model.UnlockedCount++;
                        model.LastUnlockedAt = ev.OccurredOn;
                        model.CurrentStatus = "Active";
                        break;
                }
            }

            RecalculateLoginAnalytics(model, loginPoints);
            return model;
        }

        private static DateTimeOffset ResolveLoginTime(UserLoggedInEvent e)
        {
            return e.LoggedInAt == default ? e.OccurredOn : e.LoggedInAt;
        }

        private static int NormalizeSessionDuration(int minutes)
        {
            return minutes < 0 ? 0 : minutes;
        }

        private static void RecalculateLoginAnalytics(
            AuthBehaviorReadModel model,
            List<(DateTimeOffset LoginAt, int SessionDurationMinutes)> loginPoints)
        {
            if (!loginPoints.Any())
            {
                model.PreferredLoginHour = -1;
                model.MostActiveWeekday = null;
                model.AverageDaysBetweenLogins = 0;
                model.EstimatedActiveDaysSpan = 0;
                model.AverageSessionDurationMinutes = 0;
                return;
            }

            var ordered = loginPoints
                .OrderBy(x => x.LoginAt)
                .ToList();

            model.FirstLoginAt = ordered.First().LoginAt;
            model.LastLoginAt = ordered.Last().LoginAt;

            model.PreferredLoginHour = ordered
                .GroupBy(x => x.LoginAt.Hour)
                .OrderByDescending(g => g.Count())
                .ThenBy(g => g.Key)
                .First()
                .Key;

            model.MostActiveWeekday = ordered
                .GroupBy(x => x.LoginAt.DayOfWeek.ToString())
                .OrderByDescending(g => g.Count())
                .ThenBy(g => g.Key)
                .Select(g => g.Key)
                .FirstOrDefault();

            if (ordered.Count >= 2)
            {
                var diffs = new List<double>();

                for (int i = 1; i < ordered.Count; i++)
                {
                    diffs.Add((ordered[i].LoginAt - ordered[i - 1].LoginAt).TotalDays);
                }

                model.AverageDaysBetweenLogins = Math.Round(diffs.Average(), 2);
            }
            else
            {
                model.AverageDaysBetweenLogins = 0;
            }

            model.EstimatedActiveDaysSpan =
                (ordered.Last().LoginAt.Date - ordered.First().LoginAt.Date).Days;

            var validDurations = ordered
                .Select(x => x.SessionDurationMinutes)
                .Where(x => x > 0)
                .ToList();

            model.AverageSessionDurationMinutes = validDurations.Any()
                ? Math.Round(validDurations.Average(), 2)
                : 0;
        }
    }
}
